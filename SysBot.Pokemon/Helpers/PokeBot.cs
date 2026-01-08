using System.Reflection;

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
                var assembly = System.Reflection.Assembly.GetEntryAssembly();
                if (assembly == null) return "vUnknown";

                var attr = assembly.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>();
                if (attr != null)
                {
                    var infoVer = attr.InformationalVersion;
                    int plusIndex = infoVer.IndexOf('+');
                    return "v" + (plusIndex > 0 ? infoVer.Substring(0, plusIndex) : infoVer);
                }

                var v = assembly.GetName().Version;
                return v != null ? $"v{v.Major}.{v.Minor}.{v.Build}" : "vUnknown";
            }
        }
    }
}
