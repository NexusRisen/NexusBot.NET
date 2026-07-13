using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;

namespace SysBot.Pokemon.Helpers
{
    public static class AutoOTHelper
    {
        /// <summary>
        /// Converts TradePartner details to an ITrainerInfo and applies AutoOT.
        /// </summary>
        public static void ApplyAutoOT(PKM pk, string otName, int gender, int language, uint tid, uint sid)
        {
            var tr = new SimpleTrainerInfo(pk.Version)
            {
                OT = otName,
                Gender = (byte)gender,
                Language = language,
                TID16 = (ushort)(tid & 0xFFFF),
                SID16 = (ushort)(sid & 0xFFFF)
            };

            // Call the extension method from PKHeX.Core.AutoMod
            // Note: overwriteOT is conditionally set if we want to overwrite standard OT
            // Usually, NexusBot handles "ignoreAutoOT" externally or checks if it's a fixed-OT mystery gift
            // We'll let PKHeX.Core.AutoMod handle the mutation securely.
            bool isFixedOT = pk.FatefulEncounter && pk.OriginalTrainerName != "ALM";
            pk.ApplyAutoOT(tr, overwriteOT: !isFixedOT);
        }
    }
}
