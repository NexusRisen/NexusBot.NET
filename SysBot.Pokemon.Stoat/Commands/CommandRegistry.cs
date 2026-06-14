using PKHeX.Core;
using StoatSharp;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Stoat.Commands;

public class CommandRegistry<T> where T : PKM, new()
{
    private readonly Dictionary<string, Func<UserMessage, List<string>, Task>> _commands = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterCommands(object instance)
    {
        var type = instance.GetType();
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<StoatCommandAttribute>();
            if (attr != null)
            {
                var handler = (Func<UserMessage, List<string>, Task>)Delegate.CreateDelegate(typeof(Func<UserMessage, List<string>, Task>), instance, method);
                foreach (var alias in attr.Aliases)
                {
                    _commands[alias] = handler;
                }
            }
        }
    }

    public async Task<bool> TryExecuteCommandAsync(string command, UserMessage message, List<string> args)
    {
        if (_commands.TryGetValue(command, out var handler))
        {
            try
            {
                await handler(message, args);
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Error executing Stoat command '{command}': {ex.Message}", "CommandRegistry");
            }
            return true;
        }
        return false;
    }
}
