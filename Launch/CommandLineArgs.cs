using System.Text;

namespace Launch;

public class CommandLineArgs
{
    public string? Target { private get; set; }
    public string? Create { private get; set; }
    public string? Delete { private get; set; }
    public string? Edit { private get; set; }
    public string? Type { private get; set; }
    public string? Value { private get; set; }
    public bool List { private get; set; } = false;
    public string? AddAlias { private get; set; }
    public string? RemoveAlias { private get; set; }
    public string? Shortcut { private get; set; }

    private bool NotNull(params object?[] objects) => objects.All(s => s is not null);
    
    private bool Null(params object?[] objects) => objects.All(s => s is null);

    public string Vars {
        get
        {
            var sb = new StringBuilder();
            sb.AppendLine("Target: " + Target);
            sb.AppendLine("Create: " + Create);
            sb.AppendLine("Delete: " + Delete);
            sb.AppendLine("Edit: " + Edit);
            sb.AppendLine("Type: " + Type);
            sb.AppendLine("Value: " + Value);
            sb.AppendLine("ShowList: " + List);
            sb.AppendLine("AddAlias: " + AddAlias);
            sb.AppendLine("RemoveAlias: " + RemoveAlias); 
            sb.AppendLine("TargetShortcut: " + Shortcut);
            return sb.ToString();
        }
    }

    public async Task RunCommand()
    {
        //Console.WriteLine(Vars);
        //> launch [name/alias]
        if (NotNull(Target) && Null(Create, Edit, Delete, Type, Value, AddAlias, RemoveAlias, Shortcut) && !List)
            await Program.Launch(Target!);

        //> launch --create ns:name --type url/file/command --value value
        else if (NotNull(Create, Type, Value) && Null(Target, Edit, Delete, AddAlias, RemoveAlias, Shortcut) && !List)
            await Program.Modify(Create!, Type!, Value!, false);

        //> launch --edit ns:name [--type url/file/command] [--value value]
        else if (NotNull(Edit) && Null(Target, Create, Delete, AddAlias, RemoveAlias, Shortcut) && !List)
            await Program.Modify(Create!, Type, Value, true);

        //> launch --delete ns:name
        else if (NotNull(Delete) && Null(Target, Create, Edit, Type, Value, AddAlias, RemoveAlias, Shortcut) &&
                 !List)
            await Program.Delete(Delete!);

        //> launch --add-alias alias --shortcut ns:name
        else if (NotNull(AddAlias, Shortcut) && Null(Target, Create, Edit, Delete, Type, Value, RemoveAlias) && !List)
            await Program.AddAlias(AddAlias!, Shortcut!);

        //> launch --remove-alias alias
        else if (NotNull(RemoveAlias) && Null(Target, Create, Edit, Delete, Type, Value, AddAlias, Shortcut) && !List)
            await Program.RemoveAlias(RemoveAlias!);

        //> launch --list
        else if (List && Null(Create, Edit, Delete, Type, Value, AddAlias, RemoveAlias, Shortcut))
            Program.ShowList(Target);

        else
            throw new ArgumentException("Invalid arguments");
    }
}