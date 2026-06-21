using System.ComponentModel;

namespace SysBot.Pokemon;

public sealed class GitHubSettings
{
    [Description("The owner of the GitHub repository (e.g. 'NexusRisen').")]
    public string RepositoryOwner { get; set; } = string.Empty;

    [Description("The name of the GitHub repository (e.g. 'DudeBot.NET').")]
    public string RepositoryName { get; set; } = string.Empty;
}
