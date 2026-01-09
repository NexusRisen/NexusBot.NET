using System.Collections.Generic;

namespace SysBot.Pokemon.WinForms.Helpers
{
    public static class PointerSignatures
    {
        public static List<PointerSignature> GetSignaturesForGame(string gameVersion)
        {
            var sigs = new List<PointerSignature>();

            if (gameVersion == "PLZA" || gameVersion == "2.0.0")
            {
                sigs.Add(new PointerSignature
                {
                    Name = "BoxStartPokemonPointer",
                    Signature = "1F 20 03 D5 1F 20 03 D5", // Placeholder: NOP NOP
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [0xB0, 0x978],
                    VerifyKind = PointerVerificationKind.PointerSampleNonZero,
                    VerifyPreferHeap = true
                });

                sigs.Add(new PointerSignature
                {
                    Name = "LinkTradeCodeLengthPointer",
                    Signature = "1F 20 03 D5 1F 20 03 D5", // Placeholder
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [0x52],
                    VerifyKind = PointerVerificationKind.PointerSampleNonZero,
                    VerifyPreferHeap = true
                });

                sigs.Add(new PointerSignature
                {
                    Name = "LinkTradeCodePointer",
                    Signature = "1F 20 03 D5 1F 20 03 D5", // Placeholder
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [0x30, 0x0],
                    VerifyKind = PointerVerificationKind.PointerSampleNonZero,
                    VerifyPreferHeap = true
                });

                sigs.Add(new PointerSignature
                {
                    Name = "LinkTradePartnerDataPointer",
                    Signature = "1F 20 03 D5 1F 20 03 D5", // Placeholder
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [0x1D8, 0x30, 0xA0, 0x0],
                    VerifyKind = PointerVerificationKind.PointerSampleNonZero,
                    VerifyPreferHeap = true
                });

                sigs.Add(new PointerSignature
                {
                    Name = "LinkTradePartnerPokemonPointer",
                    Signature = "1F 20 03 D5 1F 20 03 D5", // Placeholder
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [0x128, 0x30, 0x0],
                    VerifyKind = PointerVerificationKind.PointerSampleNonZero,
                    VerifyPreferHeap = true
                });

                sigs.Add(new PointerSignature
                {
                    Name = "MyStatusPointer",
                    Signature = "1F 20 03 D5 1F 20 03 D5", // Placeholder
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [0xA0, 0x40],
                    VerifyKind = PointerVerificationKind.PointerSampleNonZero,
                    VerifyPreferHeap = true
                });

                sigs.Add(new PointerSignature
                {
                    Name = "TradePartnerBackupNIDPointer",
                    Signature = "1F 20 03 D5 1F 20 03 D5", // Placeholder
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [0x108],
                    VerifyKind = PointerVerificationKind.PointerSampleNonZero,
                    VerifyPreferHeap = true
                });

                sigs.Add(new PointerSignature
                {
                    Name = "TradePartnerStatusPointer",
                    Signature = "1F 20 03 D5 1F 20 03 D5", // Placeholder
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [0x134],
                    VerifyKind = PointerVerificationKind.PointerSampleNonZero,
                    VerifyPreferHeap = true
                });
                
                sigs.Add(new PointerSignature
                {
                    Name = "OverworldOffset",
                    Signature = "?? ?? ?? ?? ?? ?? ?? ??",
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [],
                    VerifyKind = PointerVerificationKind.Bool01,
                    VerifyReadSize = 4
                });
                
                sigs.Add(new PointerSignature
                {
                    Name = "MenuOffset",
                    Signature = "?? ?? ?? ?? ?? ?? ?? ??",
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [],
                    VerifyKind = PointerVerificationKind.UIntRange,
                    VerifyReadSize = 4,
                    VerifyMin = 0,
                    VerifyMax = 10
                });
                
                sigs.Add(new PointerSignature
                {
                    Name = "ConnectedOffset",
                    Signature = "?? ?? ?? ?? ?? ?? ?? ??",
                    Offset = 0,
                    Encoding = PointerEncoding.AdrpLdr,
                    RelativeOffsets = [],
                    VerifyKind = PointerVerificationKind.Bool01,
                    VerifyReadSize = 4
                });
            }
            
            return sigs;
        }
    }
}
