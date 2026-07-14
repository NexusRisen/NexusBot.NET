using PKHeX.Core;
using System.Linq;
using System.Text;

namespace SysBot.Pokemon.Helpers;

/// <summary>
/// Simplified legality feedback that focuses on extracting data from LegalityAnalysis.Results
/// </summary>
public static class SimpleLegalityFeedback
{
    public static bool IsEffectivelyLegal(PKM pkm, LegalityAnalysis la)
    {
        if (la.Valid)
            return true;

        if (pkm is PA9)
        {
            var invalidChecks = la.Results.Where(r => !r.Valid).ToList();
            
            var localizationSet = LegalityLocalizationSet.GetLocalization(GameLanguage.DefaultLanguage);
            var context = LegalityLocalizationContext.Create(la, localizationSet);
            
            foreach (var issue in invalidChecks)
            {
                var comment = context.Humanize(issue).ToLower();
                
                // Ignore Unreleased Encounter
                if (issue.Identifier == CheckIdentifier.Encounter && 
                   (comment.Contains("unreleased") || comment.Contains("cannot verify") || comment.Contains("mystery gift")))
                    continue;
                    
                // Ignore Unreleased Form/GameOrigin
                if ((issue.Identifier == CheckIdentifier.Form || issue.Identifier == CheckIdentifier.GameOrigin) && 
                   (comment.Contains("unreleased") || comment.Contains("origin")))
                    continue;

                // Ignore Unreleased Move Transfers
                if ((issue.Identifier == CheckIdentifier.CurrentMove || issue.Identifier == CheckIdentifier.RelearnMove) &&
                    (comment.Contains("transfer") || comment.Contains("unreleased")))
                    continue;
                    
                // If we reach here, it's a real legality issue (e.g. impossible stats, illegal ability)
                return false;
            }
            
            // All invalid checks were due to the game being unreleased
            return true;
        }

        return false;
    }

    public static string GetLegalityReport(PKM pkm, LegalityAnalysis la, string speciesName)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"**Legality Analysis for {speciesName}**");
        sb.AppendLine($"Status: {(la.Valid ? "✅ Legal" : "❌ Illegal")}");

        if (!la.Valid)
        {
            // Get all invalid checks from the Results list
            var invalidChecks = la.Results.Where(r => !r.Valid).ToList();

            if (invalidChecks.Count > 0)
            {
                sb.AppendLine("\n**Issues Found:**");

                // Group by identifier for better organization
                var groupedIssues = invalidChecks.GroupBy(r => r.Identifier);

                // Create localization context to convert CheckResult to human-readable messages
                var localizationSet = LegalityLocalizationSet.GetLocalization(GameLanguage.DefaultLanguage);
                var context = LegalityLocalizationContext.Create(la, localizationSet);

                foreach (var group in groupedIssues)
                {
                    sb.AppendLine($"\n{GetCategoryIcon(group.Key)} **{GetCategoryName(group.Key)}:**");

                    foreach (var issue in group)
                    {
                        // Clean up the comment for display
                        var cleanComment = context.Humanize(issue)
                            .Replace("Invalid:", "")
                            .Replace("Fishy:", "Warning:")
                            .Trim();

                        sb.AppendLine($"  • {cleanComment}");
                    }
                }
            }

            // Add basic move analysis
            var moveIssues = invalidChecks.Where(r => r.Identifier == CheckIdentifier.CurrentMove).ToList();
            if (moveIssues.Count > 0)
            {
                sb.AppendLine("\n**Move Tips:**");
                sb.AppendLine("  • Check if moves are available in the target generation");
                sb.AppendLine("  • Verify move combinations are legal together");
                sb.AppendLine("  • Some moves are event-exclusive");
            }
        }
        else
        {
            sb.AppendLine($"\n✨ Your {speciesName} passed all legality checks!");
            if (la.EncounterOriginal != null)
            {
                sb.AppendLine($"Encounter: {la.EncounterOriginal.LongName}");
            }
        }

        return sb.ToString();
    }

    private static string GetCategoryIcon(CheckIdentifier identifier) => identifier switch
    {
        CheckIdentifier.CurrentMove => "🎯",
        CheckIdentifier.Ability => "⚡",
        CheckIdentifier.Ball => "🏀",
        CheckIdentifier.Level => "📊",
        CheckIdentifier.Shiny => "✨",
        CheckIdentifier.Form => "🔄",
        CheckIdentifier.GameOrigin => "🎮",
        CheckIdentifier.Encounter => "📍",
        _ => "🔸"
    };

    public static string GetCategoryName(CheckIdentifier identifier) => identifier switch
    {
        CheckIdentifier.CurrentMove => "Moves",
        CheckIdentifier.RelearnMove => "Relearn Moves",
        CheckIdentifier.Ability => "Ability",
        CheckIdentifier.Ball => "Poké Ball",
        CheckIdentifier.Level => "Level",
        CheckIdentifier.Shiny => "Shiny Status",
        CheckIdentifier.Form => "Form",
        CheckIdentifier.GameOrigin => "Game Origin",
        CheckIdentifier.Encounter => "Encounter",
        CheckIdentifier.IVs => "IVs",
        CheckIdentifier.EVs => "EVs",
        CheckIdentifier.Nature => "Nature",
        CheckIdentifier.Gender => "Gender",
        _ => identifier.ToString()
    };
}
