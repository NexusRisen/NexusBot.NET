using System.ComponentModel;

namespace SysBot.Pokemon;

public class DatabaseSettings
{
    [Description("The host address for the remote database (e.g. 'localhost' or an IP). Set this to your Shockhosting DB server IP or hostname. Defaults to genacnh.com.")]
    public string DatabaseHost { get; set; } = "144.208.125.199";

    [Description("Port for the remote database.")]
    public uint DatabasePort { get; set; } = 3306;
}
