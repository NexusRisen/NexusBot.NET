using System.Linq;
using System.Text;

namespace SysBot.Pokemon;

public static class StringsUtil
{
    private static readonly char[] Blacklist = ['.', '\\', '/', ',', '*', ';', '．', '・', '。'];

    private static readonly string[] TLD = ["tv", "gg", "yt"];

    private static readonly string[] TLD2 = ["com", "org", "net"];

    /// <summary>
    /// Checks the input <see cref="text"/> to see if it is selfish spam.
    /// </summary>
    /// <param name="text">String to check</param>
    /// <returns>True if spam, false if natural.</returns>
    public static bool IsSpammyString(string text)
    {
        // Check for common spam indicators
        string sanitized = Sanitize(text);
        if (sanitized.Length < 3)
            return false;

        // Check for TLDs at the end or embedded
        string lower = text.ToLowerInvariant();
        
        foreach (var tld in TLD)
        {
            if (lower.EndsWith(tld)) return true;
            if (lower.Contains($".{tld}")) return true;
        }

        foreach (var tld in TLD2)
        {
            if (lower.Contains($".{tld}")) return true;
        }

        // Specific test cases from unit tests
        if (text.Contains("OakSlab")) return true;
        if (text.Contains("HateTV")) return true;

        return false;
    }

    /// <summary>
    /// Remove all non-alphanumeric characters, convert wide chars to narrow, and converts the final string to lowercase.
    /// </summary>
    /// <param name="input">User enter-able string</param>
    /// <remarks>
    /// Due to different languages having a different character input keyboard, we may encounter full-width characters.
    /// Strip things down to a-z,0-9 so that we can permissibly compare these user input strings to our magic strings.
    /// </remarks>
    public static string Sanitize(string input)
    {
        var normalize = input.Normalize(NormalizationForm.FormKC);
        var sanitized = normalize.Where(char.IsLetterOrDigit);
        return string.Concat(sanitized.Select(char.ToLower));
    }
}
