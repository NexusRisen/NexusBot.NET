using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Helpers;

public static class NetUtil
{
    public static readonly HttpClient HttpClient = new();

    public static async Task<byte[]> DownloadFromUrlAsync(string url)
    {
        return await HttpClient.GetByteArrayAsync(url).ConfigureAwait(false);
    }
}
