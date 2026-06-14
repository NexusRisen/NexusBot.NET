using System;

namespace SysBot.Pokemon.Stoat.Commands;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class StoatCommandAttribute : Attribute
{
    public string[] Aliases { get; }

    public StoatCommandAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }
}
