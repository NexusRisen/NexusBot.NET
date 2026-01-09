using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace SysBot.Pokemon.WinForms.Helpers
{
    public static class MemoryScanner
    {
        public class ScanResult
        {
            public ulong Address { get; set; }
            public ulong OffsetFromBase { get; set; }
            public string Region { get; set; } = string.Empty;
        }

        public static async Task<List<ScanResult>> ScanPatternAsync(
            ISwitchConnectionAsync connection,
            string patternStr,
            ulong startAddress,
            ulong length,
            CancellationToken token,
            IProgress<float>? progress = null)
        {
            var results = new List<ScanResult>();
            var (pattern, mask) = ParsePattern(patternStr);
            if (pattern.Length == 0) return results;

            const int chunkSize = 0x10000; // 64KB chunks
            // Overlap by pattern length - 1 to catch patterns crossing chunk boundaries
            int overlap = pattern.Length - 1;
            
            ulong currentAddress = startAddress;
            ulong endAddress = startAddress + length;
            ulong totalBytes = length;
            ulong bytesRead = 0;

            byte[]? buffer = null;

            while (currentAddress < endAddress)
            {
                if (token.IsCancellationRequested) break;

                // Calculate read size
                int readSize = chunkSize;
                if (currentAddress + (ulong)readSize > endAddress)
                {
                    readSize = (int)(endAddress - currentAddress);
                }

                // Read memory
                // We use ReadBytesAbsoluteAsync. If startAddress is MainBase, it's effectively reading Main.
                try 
                {
                    buffer = await connection.ReadBytesAbsoluteAsync(currentAddress, readSize, token).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // If read fails (e.g. invalid memory), skip this chunk? 
                    // Or maybe we hit a gap. For now, let's just advance.
                    currentAddress += (ulong)readSize;
                    bytesRead += (ulong)readSize;
                    progress?.Report((float)bytesRead / totalBytes);
                    continue;
                }

                // Search in buffer
                var matches = SearchInBuffer(buffer, pattern, mask);
                foreach (var offset in matches)
                {
                    var matchAddr = currentAddress + (ulong)offset;
                    results.Add(new ScanResult
                    {
                        Address = matchAddr,
                        OffsetFromBase = matchAddr - startAddress,
                        Region = "Main" // Assumption for now
                    });
                }

                // Advance
                // If we found matches, we still advance.
                // We overlap to ensure we don't miss patterns on the boundary.
                // But simple approach: just advance by readSize, but we need to handle the overlap.
                // If we simply read chunks, patterns crossing 64KB boundary won't match.
                // Correct approach: Next read starts at currentAddress + chunkSize - overlap.
                // BUT, to avoid re-reading too much, we can just handle it carefully.
                // For simplicity/performance trade-off in this MVP: 
                // We will advance by (readSize - overlap) to ensure coverage.
                
                if (readSize < overlap) break; // Should not happen given chunk size

                ulong advance = (ulong)(readSize - overlap);
                if (advance == 0) advance = 1; // Prevent infinite loop if pattern is huge (unlikely)

                currentAddress += advance;
                bytesRead += advance;
                
                progress?.Report((float)bytesRead / totalBytes);
            }

            return results;
        }

        public static (byte[] pattern, bool[] mask) ParsePattern(string input)
        {
            var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var pattern = new byte[parts.Length];
            var mask = new bool[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "??" || parts[i] == "?")
                {
                    mask[i] = false; // Wildcard
                    pattern[i] = 0;
                }
                else
                {
                    if (byte.TryParse(parts[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
                    {
                        mask[i] = true;
                        pattern[i] = b;
                    }
                    else
                    {
                        // Invalid format treatment? Treat as wildcard or fail?
                        // Let's treat as wildcard for safety or error?
                        // Better to treat as wildcard if unsure, or throw.
                        mask[i] = false;
                    }
                }
            }
            return (pattern, mask);
        }

        public static List<int> SearchInBuffer(byte[] buffer, byte[] pattern, bool[] mask)
        {
            var matches = new List<int>();
            int end = buffer.Length - pattern.Length;

            for (int i = 0; i <= end; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (mask[j] && buffer[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    matches.Add(i);
                }
            }
            return matches;
        }
    }
}
