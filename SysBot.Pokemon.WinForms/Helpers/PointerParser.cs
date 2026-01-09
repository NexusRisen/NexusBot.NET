using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SysBot.Pokemon.WinForms.Helpers
{
    public static class PointerParser
    {
        /// <summary>
        /// Parses a pointer string input (e.g. "[main] + 0x123 + 0x10" or "0x123 0x10") into a jump chain.
        /// </summary>
        /// <param name="input">The input string to parse.</param>
        /// <returns>A tuple containing the list of jumps and whether it starts from heap (false = main/absolute).</returns>
        public static (IEnumerable<long> Jumps, bool IsHeap) Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (new List<long>(), false);

            var isHeap = input.IndexOf("[heap]", StringComparison.OrdinalIgnoreCase) >= 0;

            var clean = input.Replace("0x", "", StringComparison.OrdinalIgnoreCase);
            clean = clean.Replace("[heap]", " ", StringComparison.OrdinalIgnoreCase)
                         .Replace("[main]", " ", StringComparison.OrdinalIgnoreCase)
                         .Replace("[abs]", " ", StringComparison.OrdinalIgnoreCase)
                         .Replace("[absolute]", " ", StringComparison.OrdinalIgnoreCase);

            clean = clean.Replace("+", " ")
                         .Replace("-", " ")
                         .Replace(",", " ")
                         .Replace(";", " ")
                         .Replace("[", " ")
                         .Replace("]", " ");

            var parts = clean.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var jumps = new List<long>();
            
            foreach (var part in parts)
            {
                if (long.TryParse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long val))
                {
                    jumps.Add(val);
                }
            }

            return (jumps, isHeap);
        }
    }
}
