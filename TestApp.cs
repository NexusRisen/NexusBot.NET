using System;
using PKHeX.Core;
using AutoModPlugins;
using SysBot.Pokemon;

class Program
{
    static void Main()
    {
        EncounterEvent.IgnoreTestEvents = true;
        GameInfo.Initialize("en");
        APILegality.SetAllLegalRibbons = false;
        
        var set = new ShowdownSet("Pikachu");
        var template = new RegenTemplate(set);
        var sav = SaveUtil.GetBlankSAV(GameVersion.ZA, "SysBot");
        sav.TID16 = 12345;
        sav.SID16 = 54321;
        sav.Language = 2; // English
        
        var pkm = sav.GetLegal(template, out string result);
        Console.WriteLine($"Result: {result}");
        if (pkm != null)
        {
            var la = new LegalityAnalysis(pkm);
            Console.WriteLine($"Valid: {la.Valid}");
            if (!la.Valid)
            {
                Console.WriteLine($"Report: {la.Report()}");
            }
        }
    }
}
