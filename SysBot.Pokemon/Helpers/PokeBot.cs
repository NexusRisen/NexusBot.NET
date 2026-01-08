namespace SysBot.Pokemon.Helpers
{
    public static class PokeBot
    {
        public const string Attribution = "https://github.com/NexusRisen/PokeBot";

        public const string ConfigPath = "config.json";

        public static string Version
        {
            get
            {
                var v = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
                return v != null ? $"v{v.Major}.{v.Minor}.{v.Build}" : "vUnknown";
            }
        }
    }
}
