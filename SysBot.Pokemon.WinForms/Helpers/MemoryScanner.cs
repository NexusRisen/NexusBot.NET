using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace SysBot.Pokemon.WinForms.Helpers
{
    public class MemoryScanner
    {
        private readonly ISwitchConnectionAsync Connection;
        private const int ChunkSize = 0x100000; // 1MB chunks

        public MemoryScanner(ISwitchConnectionAsync connection)
        {
            Connection = connection;
        }

        public async Task<string> AnalyzeAddressAsync(ulong address)
        {
            try
            {
                ulong heapBase = await Connection.GetHeapBaseAsync(CancellationToken.None).ConfigureAwait(false);
                if (address >= heapBase && address < heapBase + 0x40000000) // Assuming 1GB heap for safety check
                {
                    return $"[Heap+0x{address - heapBase:X}]";
                }

                ulong mainBase = await Connection.GetMainNsoBaseAsync(CancellationToken.None).ConfigureAwait(false);
                if (address >= mainBase && address < mainBase + 0x8000000) // Assuming 128MB main
                {
                    return $"[Main+0x{address - mainBase:X}]";
                }

                return $"0x{address:X} (Unknown Region)";
            }
            catch
            {
                return $"0x{address:X}";
            }
        }

        public async Task<List<ulong>> ScanPattern(byte[] pattern, ulong startAddress, ulong length, IProgress<string> progress, CancellationToken token)
        {
            var results = new List<ulong>();
            ulong current = startAddress;
            ulong end = startAddress + length;

            progress?.Report($"Starting scan from 0x{startAddress:X} to 0x{end:X}...");

            while (current < end)
            {
                if (token.IsCancellationRequested) break;

                ulong remaining = end - current;
                int readSize = (int)Math.Min(remaining, (ulong)ChunkSize);

                try
                {
                    // Read chunk
                    byte[] buffer = await Connection.ReadBytesAbsoluteAsync(current, readSize, token).ConfigureAwait(false);

                    // Search in buffer
                    var matches = FindPatternInBuffer(buffer, pattern);
                    foreach (var offset in matches)
                    {
                        results.Add(current + (ulong)offset);
                        progress?.Report($"Found match at 0x{current + (ulong)offset:X}");
                    }

                    // Handle boundary issue: The pattern might span across two chunks.
                    // We should actually overlap reads by pattern.Length - 1, but for simplicity we'll skip for now 
                    // or implement a small backtrack.
                    if (pattern.Length > 1 && remaining > (ulong)readSize)
                    {
                        current -= (ulong)(pattern.Length - 1);
                    }
                }
                catch (Exception ex)
                {
                    progress?.Report($"Error reading at 0x{current:X}: {ex.Message}");
                }

                current += (ulong)readSize;
                
                // Report progress every chunk
                int percent = (int)((current - startAddress) * 100 / length);
                progress?.Report($"Scanning... {percent}% (Found {results.Count})");
            }

            return results;
        }

        private List<int> FindPatternInBuffer(byte[] buffer, byte[] pattern)
        {
            var matches = new List<int>();
            for (int i = 0; i <= buffer.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    // Support wildcard (optional)? For now strict match.
                    if (buffer[i + j] != pattern[j])
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
