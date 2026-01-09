using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;
using System.Reflection;

namespace SysBot.Pokemon.WinForms.Helpers
{
    public enum PointerEncoding
    {
        /// <summary>
        /// Direct absolute address (rare in PIE/ASLR, but possible)
        /// </summary>
        Absolute,

        /// <summary>
        /// ADRP (Page Base) + ADD (Offset) pattern. Common for getting address of globals.
        /// </summary>
        AdrpAdd,

        /// <summary>
        /// ADRP (Page Base) + LDR (Offset) pattern. Common for loading globals.
        /// </summary>
        AdrpLdr
    }

    public enum PointerVerificationKind
    {
        None,
        Bool01,
        UIntRange,
        PointerSampleNonZero
    }

    public sealed class PointerVerificationResult
    {
        public bool IsValid { get; set; }
        public string Details { get; set; } = string.Empty;
        public ulong? ResolvedAddress { get; set; }
        public ulong? ReadValue { get; set; }
    }

    public class PointerSignature
    {
        /// <summary>
        /// Name of the pointer (e.g., "BoxStart", "Overworld")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Hex signature string (e.g., "AA BB ?? DD")
        /// </summary>
        public string Signature { get; set; } = string.Empty;

        /// <summary>
        /// Offset from the start of the signature match to the instruction(s) of interest.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// How the pointer is encoded in the instruction(s).
        /// </summary>
        public PointerEncoding Encoding { get; set; } = PointerEncoding.AdrpAdd;

        /// <summary>
        /// The list of offsets to apply after resolving the base address.
        /// </summary>
        public long[] RelativeOffsets { get; set; } = Array.Empty<long>();

        public PointerVerificationKind VerifyKind { get; set; } = PointerVerificationKind.None;
        public int VerifyReadSize { get; set; } = 4;
        public ulong VerifyMin { get; set; }
        public ulong VerifyMax { get; set; }
        public int VerifySampleSize { get; set; } = 0x40;
        public bool VerifyPreferHeap { get; set; }
    }

    public static class PointerScanner
    {
        /// <summary>
        /// Scans for a signature and attempts to resolve the base pointer from the ARM64 instructions.
        /// </summary>
        public static async Task<ulong?> FindPointerAddressAsync(
            ISwitchConnectionAsync connection, 
            PointerSignature sig, 
            CancellationToken token)
        {
            // 1. Scan for the signature pattern
            // We search the Main region (where code lives)
            ulong mainBase = await connection.GetMainNsoBaseAsync(token);
            // Limit scan to reasonable code size (e.g., 64MB)
            ulong scanSize = 0x4000000; 

            var matches = await MemoryScanner.ScanPatternAsync(connection, sig.Signature, mainBase, scanSize, token);

            if (matches.Count == 0) return null;
            if (matches.Count > 1) 
            {
                // Ambiguous match, log warning or pick first?
                // For now, pick first.
            }

            var matchAddress = matches[0].Address;
            var targetInstructionAddr = matchAddress + (ulong)sig.Offset;

            // 2. Read the instructions at the match + offset
            // We typically need 8 bytes (2 instructions) for ADRP+ADD/LDR
            byte[] instructions = await connection.ReadBytesAbsoluteAsync(targetInstructionAddr, 8, token);
            uint instr1 = BitConverter.ToUInt32(instructions, 0);
            uint instr2 = BitConverter.ToUInt32(instructions, 4);

            // 3. Decode based on type
            ulong resolvedBase = 0;

            switch (sig.Encoding)
            {
                case PointerEncoding.AdrpAdd:
                    resolvedBase = DecodeAdrpAdd(targetInstructionAddr, instr1, instr2);
                    break;
                case PointerEncoding.AdrpLdr:
                    resolvedBase = DecodeAdrpLdr(targetInstructionAddr, instr1, instr2);
                    break;
                case PointerEncoding.Absolute:
                    // Just return the match address + offset (uncommon for dynamic pointers)
                    resolvedBase = targetInstructionAddr; 
                    break;
            }

            // Return the relative offset from Main Base (this is what goes into the C# file)
            return resolvedBase - mainBase;
        }

        public static async Task<PointerVerificationResult?> VerifyFoundOffsetAsync(
            ISwitchConnectionAsync connection,
            PointerSignature sig,
            ulong mainBase,
            ulong heapBase,
            ulong offsetFromMain,
            CancellationToken token)
        {
            if (sig.VerifyKind == PointerVerificationKind.None)
                return null;

            if (sig.VerifyKind == PointerVerificationKind.PointerSampleNonZero || sig.RelativeOffsets.Length > 0)
            {
                var jumps = BuildJumps(offsetFromMain, sig.RelativeOffsets);
                var resolved = await connection.PointerAll(jumps, token).ConfigureAwait(false);

                if (resolved == 0)
                {
                    return new PointerVerificationResult
                    {
                        IsValid = false,
                        Details = "Resolved address is 0",
                        ResolvedAddress = resolved
                    };
                }

                if (sig.VerifyPreferHeap && resolved < heapBase)
                {
                    return new PointerVerificationResult
                    {
                        IsValid = false,
                        Details = $"Resolved address not in heap (0x{resolved:X})",
                        ResolvedAddress = resolved
                    };
                }

                byte[] sample;
                try
                {
                    sample = await connection.ReadBytesAbsoluteAsync(resolved, sig.VerifySampleSize, token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return new PointerVerificationResult
                    {
                        IsValid = false,
                        Details = $"Read failed: {ex.Message}",
                        ResolvedAddress = resolved
                    };
                }

                bool allZero = true;
                bool allFF = true;
                for (int i = 0; i < sample.Length; i++)
                {
                    if (sample[i] != 0) allZero = false;
                    if (sample[i] != 0xFF) allFF = false;
                    if (!allZero && !allFF) break;
                }

                bool ok = !allZero && !allFF;
                return new PointerVerificationResult
                {
                    IsValid = ok,
                    Details = ok ? $"Resolved 0x{resolved:X} sample looks valid" : $"Resolved 0x{resolved:X} sample looks empty",
                    ResolvedAddress = resolved
                };
            }

            var absolute = mainBase + offsetFromMain;
            byte[] data;
            try
            {
                data = await connection.ReadBytesAbsoluteAsync(absolute, sig.VerifyReadSize, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new PointerVerificationResult
                {
                    IsValid = false,
                    Details = $"Read failed: {ex.Message}",
                    ResolvedAddress = absolute
                };
            }

            ulong value = sig.VerifyReadSize switch
            {
                1 => data[0],
                2 => BitConverter.ToUInt16(data, 0),
                4 => BitConverter.ToUInt32(data, 0),
                8 => BitConverter.ToUInt64(data, 0),
                _ => BitConverter.ToUInt32(data, 0)
            };

            bool inRange = true;
            if (sig.VerifyKind == PointerVerificationKind.Bool01)
                inRange = value <= 1;
            else if (sig.VerifyKind == PointerVerificationKind.UIntRange)
                inRange = value >= sig.VerifyMin && value <= sig.VerifyMax;

            return new PointerVerificationResult
            {
                IsValid = inRange,
                Details = $"Read 0x{absolute:X} value={value}",
                ResolvedAddress = absolute,
                ReadValue = value
            };
        }

        private static long[] BuildJumps(ulong baseOffsetFromMain, long[] relativeOffsets)
        {
            var jumps = new long[1 + relativeOffsets.Length];
            jumps[0] = unchecked((long)baseOffsetFromMain);
            for (int i = 0; i < relativeOffsets.Length; i++)
                jumps[i + 1] = relativeOffsets[i];
            return jumps;
        }

        // --- ARM64 Decoding Helpers ---
        // Reference: ARMv8 Instruction Set Architecture

        private static ulong DecodeAdrp(ulong pc, uint instruction)
        {
            // ADRP: Address of 4KB page at a PC-relative offset
            // Opcode: 1 0010000 (immlo:2) (immhi:19) (Rd:5)
            // 31 30 29 28 27 26 25 24 23 ... 5 4 ... 0
            
            // Check if it's ADRP (1001 0000 ...)
            if ((instruction & 0x9F000000) != 0x90000000) return 0;

            // Extract immlo (bits 29-30) and immhi (bits 5-23)
            uint immlo = (instruction >> 29) & 0x3;
            uint immhi = (instruction >> 5) & 0x7FFFF;
            
            // Combine: imm = immhi:immlo:Zeros(12)
            long imm = (long)((immhi << 2) | immlo) << 12;

            // Sign extend if necessary (21-bit signed integer shifted left by 12)
            // The 21 bits are at the top of a 33-bit value effectively? 
            // Actually immhi:immlo is 21 bits.
            // If bit 20 of combined (bit 18 of immhi) is 1, it's negative.
            if ((imm & 0x100000000) != 0) // Check sign bit of 33-bit result
            {
                // Sign extension logic can be tricky, but for simple forward/backward references:
                // Let's assume standard C# handling.
                // Re-calculating cleanly:
                long val = (long)((immhi << 2) | immlo);
                if ((val & 0x100000) != 0) // 21st bit
                {
                    val |= unchecked((long)0xFFFFFFFFFFE00000); // Sign extend top
                }
                imm = val << 12;
            }

            // PC is aligned to 4KB page
            ulong pcPage = pc & ~0xFFFUL;
            return (ulong)((long)pcPage + imm);
        }

        internal static ulong DecodeAdrpAdd(ulong pc, uint instr1, uint instr2)
        {
            // Instr1: ADRP Xd, Page
            ulong page = DecodeAdrp(pc, instr1);
            if (page == 0) return 0; // Failed decode

            // Instr2: ADD Xd, Xn, #imm
            // Opcode: 10010001 (sf=1) ...
            // ADD (immediate)
            // 31 (sf) 30 (op) 29 (S) 28-24 (10010) ...
            
            // We assume standard ADD Xd, Xn, #imm12
            // bits 10-21 = imm12
            uint imm12 = (instr2 >> 10) & 0xFFF;
            
            return page + imm12;
        }

        internal static ulong DecodeAdrpLdr(ulong pc, uint instr1, uint instr2)
        {
            // Instr1: ADRP Xd, Page
            ulong page = DecodeAdrp(pc, instr1);
            if (page == 0) return 0;

            // Instr2: LDR Xt, [Xn, #imm]
            // LDR (immediate, unsigned offset)
            // 10111001 01 ...
            // bits 10-21 = imm12
            // For 64-bit LDR, the immediate is scaled by 8
            
            uint imm12 = (instr2 >> 10) & 0xFFF;
            
            // Determine scale based on opcode size (usually 64-bit/8 bytes for pointers)
            // bit 30 = 1 for 64-bit
            bool is64 = (instr2 & 0x40000000) != 0;
            int scale = is64 ? 3 : 2; // 3 = shift 3 (mul 8), 2 = shift 2 (mul 4)

            // LDR unsigned offset doesn't scale imm12 in the instruction field itself for standard LDR?
            // Wait, for "LDR (unsigned immediate)", the imm12 IS scaled by the transfer size.
            // But verify specific opcode. 
            // 39 40 ... -> LDRB (unsigned)
            // 79 40 ... -> LDRH
            // B9 40 ... -> LDR (32)
            // F9 40 ... -> LDR (64)
            
            // Assuming F9 (64-bit load)
            if ((instr2 & 0xFFC00000) == 0xF9400000)
            {
                return page + (ulong)(imm12 * 8);
            }
            
            // Assuming B9 (32-bit load)
            if ((instr2 & 0xFFC00000) == 0xB9400000)
            {
                return page + (ulong)(imm12 * 4);
            }

            return 0; // Unknown LDR variant
        }

        /// <summary>
        /// Dumps the Main NSO (executable memory) to a file for analysis.
        /// </summary>
        public static async Task DumpMainToDiskAsync(ISwitchConnectionAsync connection, string path, IProgress<float> progress, CancellationToken token)
        {
            // Typical Main size is 30-100MB.
            ulong mainBase = await connection.GetMainNsoBaseAsync(token);
            const int chunkSize = 0x100000; // 1MB chunks
            
            // Heuristic: Read until we hit a large block of zeros or a max size (e.g. 128MB)
            // For safety, let's read 64MB which covers most Pokemon games (SwSh is ~50MB, SV ~80MB?)
            // Better yet, just read 96MB to be safe.
            ulong totalSize = 0x6000000; // 96MB
            
            using var fs = new System.IO.FileStream(path, System.IO.FileMode.Create);
            
            byte[] buffer;
            ulong current = 0;
            
            while (current < totalSize)
            {
                if (token.IsCancellationRequested) break;
                
                // Read
                try 
                {
                    buffer = await connection.ReadBytesAbsoluteAsync(mainBase + current, chunkSize, token);
                }
                catch 
                {
                    break; // Stop on read error (end of memory?)
                }
                
                await fs.WriteAsync(buffer, 0, buffer.Length, token);
                
                current += (ulong)buffer.Length;
                progress?.Report((float)current / totalSize);
            }
        }

        /// <summary>
        /// Scans the Main memory to find instructions that reference a specific offset.
        /// Useful for generating signatures from a known pointer.
        /// </summary>
        public static async Task<List<PointerSignature>> FindSignaturesForOffsetAsync(
            ISwitchConnectionAsync connection, 
            ulong targetOffsetFromMain, 
            IProgress<float> progress,
            CancellationToken token)
        {
            var results = new List<PointerSignature>();
            
            ulong mainBase = await connection.GetMainNsoBaseAsync(token);
            ulong targetAbsolute = mainBase + targetOffsetFromMain;
            
            // Scan 64MB of code
            ulong scanSize = 0x4000000; 
            const int chunkSize = 0x40000; // 256KB chunks for processing
            
            byte[] buffer;
            ulong currentOffset = 0;
            
            while (currentOffset < scanSize)
            {
                if (token.IsCancellationRequested) break;
                
                int readSize = chunkSize;
                try
                {
                    buffer = await connection.ReadBytesAbsoluteAsync(mainBase + currentOffset, readSize, token);
                }
                catch
                {
                    break;
                }
                
                // Process buffer (step by 4 bytes)
                // We need at least 8 bytes for a pair
                for (int i = 0; i < buffer.Length - 8; i += 4)
                {
                    uint instr1 = BitConverter.ToUInt32(buffer, i);
                    uint instr2 = BitConverter.ToUInt32(buffer, i + 4);
                    
                    ulong pc = mainBase + currentOffset + (ulong)i;
                    ulong resolved = 0;
                    PointerEncoding enc = PointerEncoding.Absolute;
                    
                    // Try ADRP+ADD
                    resolved = DecodeAdrpAdd(pc, instr1, instr2);
                    if (resolved == targetAbsolute)
                    {
                        enc = PointerEncoding.AdrpAdd;
                        goto Found;
                    }
                    
                    // Try ADRP+LDR
                    resolved = DecodeAdrpLdr(pc, instr1, instr2);
                    if (resolved == targetAbsolute)
                    {
                        enc = PointerEncoding.AdrpLdr;
                        goto Found;
                    }
                    
                    continue;
                    
                    Found:
                    // Create signature
                    // Convert bytes to hex string
                    // Wildcard the ADRP immediate (bits 5-23 and 29-30)
                    // For simplicity in this helper, we'll just wildcard the whole 2nd and 3rd byte of ADRP?
                    // ADRP Opcode: 1 0010000 ... 
                    // 90xxxxxx is typical for ADRP?
                    // Let's just create a raw byte signature for now and let the user refine if needed.
                    // Or smarter: "xx xx xx 90" (Little Endian) -> Wildcard the immediate
                    
                    // Helper to format bytes
                    var b1 = BitConverter.GetBytes(instr1);
                    var b2 = BitConverter.GetBytes(instr2);
                    
                    // Wildcard strategy: 
                    // Instr1 is ADRP. It changes heavily with position. 
                    // We should wildcard the immediate fields.
                    // A simple heuristic: Wildcard the 3 bytes that contain the immediate.
                    // ADRP is 0x90... 
                    // The high byte (0x90 | 0x9F etc) contains opcode bits.
                    // The low bytes contain the immediate.
                    
                    string sig = $"{b1[0]:X2} {b1[1]:X2} {b1[2]:X2} {b1[3]:X2} {b2[0]:X2} {b2[1]:X2} {b2[2]:X2} {b2[3]:X2}";
                    
                    // Add a "smart" version with wildcards for ADRP
                    // ADRP mask: 1001 0000 (0x90) is fixed.
                    // Immediates are everywhere else.
                    // Let's wildcard the first 3 bytes of the ADRP instruction (Little Endian)
                    // b1[0], b1[1], b1[2] are mostly immediate. b1[3] is 0x90 + high bits.
                    string smartSig = $"?? ?? ?? {b1[3]:X2} {b2[0]:X2} {b2[1]:X2} {b2[2]:X2} {b2[3]:X2}";
                    
                    results.Add(new PointerSignature
                    {
                        Name = $"Auto_{targetOffsetFromMain:X}_{results.Count}",
                        Signature = smartSig,
                        Offset = 0,
                        Encoding = enc,
                        RelativeOffsets = new long[] { } // Base pointer found
                    });
                }
                
                currentOffset += (ulong)readSize;
                progress?.Report((float)currentOffset / scanSize);
            }
            
            return results;
        }
        /// <summary>
        /// Scans a local NSO file for a list of known signatures and resolves their targets.
        /// This mimics Ghidra script functionality.
        /// </summary>
        public static async Task<Dictionary<string, ulong>> ScanFileForSignaturesAsync(
            string nsoPath, 
            List<PointerSignature> signatures, 
            IProgress<float> progress, 
            CancellationToken token)
        {
            var results = new Dictionary<string, ulong>();
            
            // 1. Load the entire NSO into memory (usually < 100MB)
            byte[] nsoData;
            using (var fs = new System.IO.FileStream(nsoPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                nsoData = new byte[fs.Length];
                await fs.ReadAsync(nsoData, 0, (int)fs.Length, token);
            }

            // 2. Scan for each signature
            float count = 0;
            foreach (var sig in signatures)
            {
                if (token.IsCancellationRequested) break;

                var (pattern, mask) = MemoryScanner.ParsePattern(sig.Signature);
                var matches = MemoryScanner.SearchInBuffer(nsoData, pattern, mask);

                if (matches.Count > 0)
                {
                    // Use first match
                    int matchOffset = matches[0];
                    int targetInstrOffset = matchOffset + sig.Offset;

                    if (targetInstrOffset + 8 <= nsoData.Length)
                    {
                        uint instr1 = BitConverter.ToUInt32(nsoData, targetInstrOffset);
                        uint instr2 = BitConverter.ToUInt32(nsoData, targetInstrOffset + 4);
                        ulong pc = (ulong)targetInstrOffset;

                        ulong resolved = 0;
                        switch (sig.Encoding)
                        {
                            case PointerEncoding.AdrpAdd:
                                resolved = DecodeAdrpAdd(pc, instr1, instr2);
                                break;
                            case PointerEncoding.AdrpLdr:
                                resolved = DecodeAdrpLdr(pc, instr1, instr2);
                                break;
                            case PointerEncoding.Absolute:
                                resolved = (ulong)targetInstrOffset; // Or read the value at offset?
                                break;
                        }

                        if (resolved != 0)
                        {
                            results[sig.Name] = resolved;
                        }
                    }
                }

                count++;
                progress?.Report(count / signatures.Count);
            }

            return results;
        }

        public static string GeneratePokeDataOffsetsCode(Dictionary<string, ulong> resolvedPointers)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// Auto-generated by SysBot Pointer Scanner");
            sb.AppendLine("// Copy this into your PokeDataOffsets class");
            sb.AppendLine();
            
            foreach (var kvp in resolvedPointers)
            {
                sb.AppendLine($"        public IReadOnlyList<long> {kvp.Key} {{ get; }} = [0x{kvp.Value:X}, 0x0]; // TODO: Check offsets");
            }
            
            return sb.ToString();
        }

        // --- Known Signatures Database ---
        // These are examples. The user needs to populate this with real signatures found via "Find Sig".
        public static List<PointerSignature> GetKnownSignatures()
        {
            return new List<PointerSignature>
            {
                // Example: BoxStart (Hypothetical Signature)
                new PointerSignature 
                { 
                    Name = "BoxStartPokemonPointer", 
                    Signature = "E0 03 1F 2A ?? ?? ?? 90", // Example pattern
                    Encoding = PointerEncoding.AdrpAdd,
                    Offset = 4 // Offset to the ADRP instruction relative to match start
                },
                new PointerSignature 
                { 
                    Name = "OverworldPointer", 
                    Signature = "E1 03 1F 2A ?? ?? ?? 90", 
                    Encoding = PointerEncoding.AdrpLdr 
                },
                 new PointerSignature 
                { 
                    Name = "MyStatusPointer", 
                    Signature = "F3 03 1F 2A ?? ?? ?? 90", 
                    Encoding = PointerEncoding.AdrpLdr 
                }
            };
        }

        public static async Task<string> VerifyLoadedOffsetsAsync(
            ISwitchConnectionAsync connection,
            string gameVersion,
            IProgress<string> status,
            CancellationToken token)
        {
            var sb = new StringBuilder();
            
            // 1. Identify the class
            var safeVersion = gameVersion.Replace(".", "");
            if (safeVersion == "PLZA" || safeVersion == "200") safeVersion = "PLZA";
            var className = $"PokeDataOffsets{safeVersion}";
            
            // Try to find the type
            Type? offsetType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType($"SysBot.Pokemon.{className}");
                if (t != null) 
                {
                    offsetType = t;
                    break;
                }
            }
            
            if (offsetType == null)
                return $"Could not find class 'SysBot.Pokemon.{className}'. Ensure it exists and is loaded.";
            
            sb.AppendLine($"Verifying offsets in {offsetType.Name}...");
            
            // 2. Get Bases
            ulong mainBase = await connection.GetMainNsoBaseAsync(token);
            ulong heapBase = await connection.GetHeapBaseAsync(token);
            
            // 3. Create Instance
            object? instance = null;
            try
            {
                instance = Activator.CreateInstance(offsetType);
            }
            catch
            {
                sb.AppendLine("Could not create instance of offset class.");
                return sb.ToString();
            }

            // Check Properties (Lists)
            foreach (var prop in offsetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (typeof(IEnumerable<long>).IsAssignableFrom(prop.PropertyType))
                {
                    var val = prop.GetValue(instance) as IEnumerable<long>;
                    if (val != null)
                    {
                        var list = new List<long>(val);
                        if (list.Count > 0)
                        {
                            status.Report($"Verifying {prop.Name}...");
                            var baseOffset = (ulong)list[0];
                            var offsets = list.GetRange(1, list.Count - 1).ToArray();
                            
                            var sig = new PointerSignature 
                            { 
                                Name = prop.Name, 
                                RelativeOffsets = offsets,
                                VerifyKind = PointerVerificationKind.PointerSampleNonZero,
                                VerifyPreferHeap = true
                            };
                            
                            var result = await VerifyFoundOffsetAsync(connection, sig, mainBase, heapBase, baseOffset, token);
                            
                            sb.AppendLine($"{prop.Name}: {(result?.IsValid == true ? "OK" : "FAIL")}");
                            if (result?.IsValid != true)
                            {
                                sb.AppendLine($"  -> {result?.Details}");

                                // Attempt Auto-Fix
                                status.Report($"Attempting to resolve {prop.Name} via signatures...");
                                var signatures = PointerSignatures.GetSignaturesForGame(gameVersion);
                                var matchingSig = signatures.FirstOrDefault(s => s.Name == prop.Name);
                                
                                // Basic check to ensure signature is not just wildcards or empty
                                bool isUsefulSig = matchingSig != null && !string.IsNullOrWhiteSpace(matchingSig.Signature) && matchingSig.Signature.Replace("?", "").Replace(" ", "").Length > 0;
                                
                                if (matchingSig != null && isUsefulSig)
                                {
                                    // Check if it's the default placeholder
                                    if (matchingSig.Signature == "?? ?? ?? ?? ?? ?? ?? ??")
                                    {
                                         sb.AppendLine($"  -> Signature is a placeholder (??). Cannot scan.");
                                    }
                                    else
                                    {
                                        var newAddr = await FindPointerAddressAsync(connection, matchingSig, token);
                                        if (newAddr.HasValue)
                                        {
                                             ulong newOffset = newAddr.Value - mainBase;
                                             sb.AppendLine($"  -> [AUTO-FIX] Found updated offset: 0x{newOffset:X}");
                                             sb.AppendLine($"  -> Please update {className}.cs with this value.");
                                        }
                                        else
                                        {
                                             sb.AppendLine($"  -> Signature scan returned no matches.");
                                        }
                                    }
                                }
                                else
                                {
                                     sb.AppendLine($"  -> No valid signature definition found for auto-fix.");
                                }
                            }
                        }
                    }
                }
            }
            
            // Check Fields (Consts)
            foreach (var field in offsetType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(uint) || field.FieldType == typeof(int) || field.FieldType == typeof(ulong))
                {
                    var valObj = field.GetValue(null);
                    ulong val = 0;
                    if (valObj is uint u) val = u;
                    else if (valObj is int i) val = (ulong)i;
                    else if (valObj is ulong ul) val = ul;
                    
                    if (val > 0x1000 && field.Name.EndsWith("Offset"))
                    {
                         status.Report($"Verifying {field.Name}...");
                         var sig = new PointerSignature 
                         { 
                             Name = field.Name, 
                             RelativeOffsets = [], 
                             VerifyReadSize = 4,
                             VerifyKind = (field.Name.Contains("Connected") || field.Name.Contains("Overworld")) 
                                ? PointerVerificationKind.Bool01 
                                : PointerVerificationKind.UIntRange,
                             VerifyMax = 0xFFFFFFFF
                         };
                         
                         var result = await VerifyFoundOffsetAsync(connection, sig, mainBase, heapBase, val, token);
                         sb.AppendLine($"{field.Name}: {(result?.IsValid == true ? "OK" : "FAIL")}");
                         if (result?.IsValid != true)
                             sb.AppendLine($"  -> {result?.Details}");
                    }
                }
            }
            
            return sb.ToString();
        }
    }

    public class PointerChainScanner
    {
        public class MemoryDump
        {
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public ulong StartAddress { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public static async Task<MemoryDump> DumpMemoryAsync(ISwitchConnectionAsync connection, ulong start, ulong size, string name, IProgress<float>? progress, CancellationToken token)
        {
            var dump = new MemoryDump { StartAddress = start, Name = name };
            dump.Data = new byte[size];
            
            const int chunkSize = 0x100000; // 1MB
            ulong current = 0;
            
            while (current < size)
            {
                if (token.IsCancellationRequested) break;
                
                int readSize = chunkSize;
                if (current + (ulong)readSize > size) readSize = (int)(size - current);
                
                byte[] chunk = await connection.ReadBytesAbsoluteAsync(start + current, readSize, token);
                Array.Copy(chunk, 0, dump.Data, (int)current, chunk.Length);
                
                current += (ulong)chunk.Length;
                progress?.Report((float)current / size);
            }
            
            return dump;
        }

        public static List<long[]> FindPointerChains(MemoryDump heapDump, MemoryDump mainDump, ulong targetAddress, int maxDepth, int offsetRange)
        {
            var results = new List<long[]>();
            
            // Phase 1: Search Heap for intermediate pointers (Heap -> Target)
            var level1 = ScanForValue(heapDump, targetAddress, offsetRange);
            
            // Phase 2: Search Main for pointers to Level 1 (Main -> HeapAddr)
            // Or Main -> Target directly
            
            // 2a. Direct Main -> Target
            var directMain = ScanForValue(mainDump, targetAddress, offsetRange);
            foreach (var m in directMain)
            {
                long baseOffset = (long)m.Address - (long)mainDump.StartAddress;
                long finalOffset = (long)targetAddress - (long)m.Value;
                results.Add(new long[] { baseOffset, finalOffset });
            }
            
            // 2b. Main -> Heap -> Target (Depth 2)
            foreach (var l1 in level1)
            {
                var level2 = ScanForValue(mainDump, l1.Address, offsetRange);
                foreach (var l2 in level2)
                {
                    long baseOffset = (long)l2.Address - (long)mainDump.StartAddress;
                    long offset1 = (long)l1.Address - (long)l2.Value;
                    long offset2 = (long)targetAddress - (long)l1.Value;
                    results.Add(new long[] { baseOffset, offset1, offset2 });
                }
            }
            
            return results;
        }

        public static string FormatChainAsCode(long[] chain, string name)
        {
            var sb = new StringBuilder();
            sb.Append($"        public IReadOnlyList<long> {name} {{ get; }} = [");
            
            for (int i = 0; i < chain.Length; i++)
            {
                sb.Append($"0x{chain[i]:X}");
                if (i < chain.Length - 1) sb.Append(", ");
            }
            
            sb.Append($"]; // Depth {chain.Length}");
            return sb.ToString();
        }

        private struct ScanMatch
        {
            public ulong Address;
            public ulong Value;
        }

        private static List<ScanMatch> ScanForValue(MemoryDump dump, ulong target, int range)
        {
            var matches = new List<ScanMatch>();
            for (int i = 0; i < dump.Data.Length - 8; i += 8)
            {
                ulong val = BitConverter.ToUInt64(dump.Data, i);
                
                if (val >= target - (ulong)range && val <= target + (ulong)range)
                {
                    matches.Add(new ScanMatch { Address = dump.StartAddress + (ulong)i, Value = val });
                }
            }
            return matches;
        }
        
        /// <summary>
        /// Fully automated scan: Finds known signatures, resolves them, verifies them, and generates the PokeDataOffsets class.
        /// </summary>
        public static async Task<string> AutoScanAndGenerateAsync(
            ISwitchConnectionAsync connection, 
            string gameVersion,
            IProgress<string> status, 
            CancellationToken token)
        {
            var sb = new StringBuilder();
            
            // Generate Class Header
            var safeVersion = gameVersion.Replace(".", "");
            var className = $"PokeDataOffsets{safeVersion}"; // e.g. PokeDataOffsetsPLZA
            if (gameVersion == "PLZA" || gameVersion == "2.0.0") className = "PokeDataOffsetsPLZA";
            
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace SysBot.Pokemon;");
            sb.AppendLine();
            sb.AppendLine($"/// <summary>");
            sb.AppendLine($"/// Automated offsets for {gameVersion}");
            sb.AppendLine($"/// Generated: {DateTime.Now}");
            sb.AppendLine($"/// </summary>");
            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");

            // 1. Get Base Addresses
            status.Report("Getting Memory Bases...");
            ulong heapBase = await connection.GetHeapBaseAsync(token);
            ulong mainBase = await connection.GetMainNsoBaseAsync(token);
            
            // 2. Dump Main for Scanning (We don't need Heap dump for signature matching, only Main)
            status.Report("Dumping Main (64MB) for signature scanning...");
            var mainDump = await DumpMemoryAsync(connection, mainBase, 0x4000000, "Main", null, token);

            // 3. Scan for Known Signatures
            status.Report("Scanning for Known Signatures...");
            var sigs = PointerSignatures.GetSignaturesForGame(gameVersion);
            
            if (sigs.Count == 0)
            {
                sb.AppendLine("    // No signatures found for this version.");
                sb.AppendLine("}");
                return sb.ToString();
            }

            foreach (var sig in sigs)
            {
                status.Report($"Processing {sig.Name}...");
                
                // Search in Main Dump
                var (pattern, mask) = MemoryScanner.ParsePattern(sig.Signature);
                var matches = MemoryScanner.SearchInBuffer(mainDump.Data, pattern, mask);
                
                bool found = false;
                ulong resolvedOffset = 0;
                PointerVerificationResult? verification = null;

                if (matches.Count > 0)
                {
                    // Resolve the pointer from the instruction
                    int offset = matches[0];
                    ulong pc = mainDump.StartAddress + (ulong)offset;
                    ulong targetInstrAddr = pc + (ulong)sig.Offset;
                    
                    if (offset + sig.Offset + 8 <= mainDump.Data.Length)
                    {
                        uint instr1 = BitConverter.ToUInt32(mainDump.Data, offset + sig.Offset);
                        uint instr2 = BitConverter.ToUInt32(mainDump.Data, offset + sig.Offset + 4);
                        
                        ulong resolvedTarget = 0;
                        switch (sig.Encoding)
                        {
                            case PointerEncoding.AdrpAdd:
                                resolvedTarget = PointerScanner.DecodeAdrpAdd(pc, instr1, instr2);
                                break;
                            case PointerEncoding.AdrpLdr:
                                resolvedTarget = PointerScanner.DecodeAdrpLdr(pc, instr1, instr2);
                                break;
                            case PointerEncoding.Absolute:
                                resolvedTarget = targetInstrAddr; 
                                break;
                        }

                        if (resolvedTarget != 0)
                        {
                            resolvedOffset = resolvedTarget - mainBase;
                            
                            // Verify
                            status.Report($"  Verifying {sig.Name}...");
                            verification = await PointerScanner.VerifyFoundOffsetAsync(connection, sig, mainBase, heapBase, resolvedOffset, token);
                            
                            if (verification != null && verification.IsValid)
                            {
                                found = true;
                            }
                            else
                            {
                                status.Report($"  Verification failed: {verification?.Details}");
                            }
                        }
                    }
                }

                if (found)
                {
                    // Format Output
                    sb.AppendLine($"    // Verification: OK - {verification?.Details}");
                    
                    if (sig.RelativeOffsets.Length == 0)
                    {
                        // Direct Offset (const uint)
                        sb.AppendLine($"    public const uint {sig.Name} = 0x{resolvedOffset:X};");
                    }
                    else
                    {
                        // Pointer Chain
                        sb.Append($"    public IReadOnlyList<long> {sig.Name} {{ get; }} = [0x{resolvedOffset:X}");
                        foreach (var off in sig.RelativeOffsets)
                        {
                            sb.Append($", 0x{off:X}");
                        }
                        sb.AppendLine("];");
                    }
                }
                else
                {
                    sb.AppendLine($"    // {sig.Name} - Scan Failed or Verification Failed");
                    if (verification != null)
                        sb.AppendLine($"    // Verification Error: {verification.Details}");
                }
                sb.AppendLine();
            }
            
            sb.AppendLine("}");
            return sb.ToString();
        }
    }

}
