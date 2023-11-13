using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.Text.Json;
using Launch.Exception;

namespace Launch;

internal static class Program
{
    private static ShortcutContainer _container = null!;
    
    public static async Task<int> Main(params string[] args)
    {
        var rootCommand = new RootCommand(description: "Quickly launch apps from the command line.");
        var targetArgument = new Argument<string?>(name: "target", description: "The app to launch", getDefaultValue: () => null);
        var createOption = new Option<string?>(name: "--create", description: "Create a new launch shortcut", getDefaultValue: () => null);
        var deleteOption = new Option<string?>(name: "--delete", description: "Delete launch shortcut", getDefaultValue: () => null);
        var editOption = new Option<string?>(name: "--edit", description: "Create a new launch shortcut", getDefaultValue: () => null);
        var typeOption = new Option<string>(name: "--type", description: "Set the type of app to launch");
        var valueOption = new Option<string>(name: "--value", description: "The launch string");
        var listOption = new Option<bool>(name: "--list", description: "List all shortcuts");
        var addAliasOption = new Option<string?>(name: "--add-alias", description: "Add an alias", getDefaultValue: () => null);
        var removeAliasOption = new Option<string?>(name: "--remove-alias", description: "Remove an alias", getDefaultValue: () => null);
        var targetShortcutOption = new Option<string?>(name: "--shortcut", description: "Shortcut to alias", getDefaultValue: () => null);

        rootCommand.AddArgument(targetArgument);
        rootCommand.AddOption(createOption);
        rootCommand.AddOption(editOption);
        rootCommand.AddOption(typeOption);
        rootCommand.AddOption(deleteOption);
        rootCommand.AddOption(valueOption);
        rootCommand.AddOption(listOption);
        rootCommand.AddOption(addAliasOption);
        rootCommand.AddOption(removeAliasOption);
        rootCommand.AddOption(targetShortcutOption);
        _container = await ShortcutContainer.LoadAsync();
        rootCommand.Handler = CommandHandler.Create(async (CommandLineArgs cla) =>
        {
            try
            {
                await cla.RunCommand();
            }
            // No prefix for expected exception types.
            catch (System.Exception ex) when (ex is NoSuchShortcutException or AmbiguousShortcutException or ArgumentException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (System.Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("(" + ex.GetType() + ") ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        });
        return await rootCommand.InvokeAsync(args);
    }

    public static void ShowList(string? filter = null)
    {
        foreach (var kvp in _container)
        {
            foreach (var shortcut in _container[kvp.Key])
            {
                if (filter != null &&
                    (filter.Contains(':') ? !$"{kvp.Key}:{shortcut.Name}".Contains(filter) : !kvp.Key.Contains(filter)))
                {
                    continue;
                }

                var fullName = $"{kvp.Key}:{shortcut.Name}";
                var aliasList = shortcut.Aliases.Count == 0 ? "" : "(" + string.Join(", ", shortcut.Aliases) + ") ";
                Console.WriteLine($"{fullName,-30} {shortcut.Type,8} {aliasList}{shortcut.Value}");
            }
        }
    }

    internal static async Task Launch(string name)
    {
        await new ShortcutRunner(_container.Get(name)).RunAsync();
    }

    internal static async Task Modify(string fullName, string? type, string? value, bool overwrite)
    {
        var (ns, name) = ShortcutContainer.UnpackName(fullName);
        if (ns is null)
            throw new ArgumentException("Specify a namespace when creating or editing a shortcut");
        
        if (overwrite)
        {
            var toDelete = _container.Get(fullName, true);
            type ??= toDelete.Type;
            value ??= toDelete.Value;
            _container[ns].Remove(toDelete);
        }
        else
        {
            if (_container.ContainsNamespace(ns) && _container[ns].Exists(s => s.Name == name))
            {
                throw new ArgumentException("Shortcut already exists. Use --edit to modify instead.");
            }
        }

        if (type is null)
            throw new ArgumentNullException(nameof(type), "--type must be specified");
        if (value is null)
            throw new ArgumentNullException(nameof(value), "--value must be specified");
        
        if (!new [] { "url", "file", "command" }.Contains(type))
        {
            throw new ArgumentException("Invalid value for --type. Must be one of url, file, command");
        }

        

        if (!_container.ContainsNamespace(ns))
            _container[ns] = new List<Shortcut>();

        _container[ns].Add(new Shortcut
        {
            Name = name,
            Type = type,
            Value = value
        });

        await _container.WriteAsync();
    }

    internal static async Task Delete(string name)
    {
        var shortcut = _container.Get(name, true);
        var (ns, _) = ShortcutContainer.UnpackName(name);
        _container[ns!].Remove(shortcut);
        await _container.WriteAsync();
    }

    internal static async Task AddAlias(string alias, string shortcut)
    {
        if (_container.ShortcutExists(alias))
            throw new ArgumentException("Alias already exists");
        _container.Get(shortcut, true).Aliases.Add(alias);
        await _container.WriteAsync();
    }

    internal static async Task RemoveAlias(string alias)
    {
        _container.Get(alias).Aliases.Remove(alias);
        await _container.WriteAsync();
    }
}