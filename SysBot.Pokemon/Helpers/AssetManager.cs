using System;

namespace SysBot.Pokemon.Helpers;

public static class AssetManager
{
    private const string BaseUrl = "https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/";

    /// <summary>
    /// Gets the full centralized URL for a specific GitHub repository asset.
    /// </summary>
    /// <param name="relativePath">The path within the Nexus-Risen-Edition-Sprite-Images repository (e.g. Assets/Bot/DM/dm-uhoherror.gif)</param>
    /// <param name="raw">Whether to append ?raw=true to the URL</param>
    /// <param name="width">Optional Discord resize parameter</param>
    /// <param name="height">Optional Discord resize parameter</param>
    /// <returns>The complete raw github URL with built-in cache busting logic</returns>
    public static string GetAssetUrl(string relativePath, bool raw = false, int? width = null, int? height = null)
    {
        string url = $"{BaseUrl}{relativePath}";
        bool hasParams = relativePath.Contains("?");

        if (raw)
        {
            url += $"{(hasParams ? "&" : "?")}raw=true";
            hasParams = true;
        }

        if (width.HasValue && height.HasValue)
        {
            url += $"{(hasParams ? "&" : "?")}width={width.Value}&height={height.Value}";
            hasParams = true;
        }

        // Use the current date and hour to automatically bust the cache without manual version bumps
        string hourlyCacheBuster = DateTime.UtcNow.ToString("yyyyMMddHH");
        url += $"{(hasParams ? "&" : "?")}v={hourlyCacheBuster}";

        return url;
    }

    /// <summary>
    /// Gets the URL for a Pokémon held item sprite.
    /// </summary>
    public static string GetItemUrl(string itemName)
    {
        return $"https://serebii.net/itemdex/sprites/{itemName}.png";
    }

}
