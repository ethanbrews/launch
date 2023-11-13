using System.Collections;
using System.Reflection;
using System.Text.Json;
using Launch.Exception;

namespace Launch;

public class ShortcutContainer : IEnumerable<KeyValuePair<string, List<Shortcut>>>
{
    private static Dictionary<string, List<Shortcut>> _shortcuts = null!;
    
    private static readonly string ShortcutsFileName =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".shortcuts");

    private ShortcutContainer(Dictionary<string, List<Shortcut>> shortcuts)
    {
        _shortcuts = shortcuts;
    }
    
    public static async Task<ShortcutContainer> LoadAsync()
    {
        if (File.Exists(ShortcutsFileName))
        {
            return new ShortcutContainer(
                JsonSerializer.Deserialize<Dictionary<string, List<Shortcut>>>(await File.ReadAllTextAsync(ShortcutsFileName)) ??
                new Dictionary<string, List<Shortcut>>()
            );
        }
        return new ShortcutContainer(new Dictionary<string, List<Shortcut>>());
    }

    public async Task WriteAsync()
    {
        await File.WriteAllTextAsync(ShortcutsFileName, JsonSerializer.Serialize(_shortcuts));
    }

    public Shortcut Get(string name, bool strict = false)
    {
        var (ns, ext) = UnpackName(name);
        if (ns is null && strict)
            throw new ArgumentException("Namespace is required in strict mode");
        var shortcuts = _shortcuts
            .Where(c => ns is null || ns == c.Key)
            .SelectMany(c => c.Value)
            .Where(s => s.Name == ext || s.Aliases?.Contains(ext) == true)
            .ToList();
        if (!shortcuts.Any())
            throw new NoSuchShortcutException(name);
        if (shortcuts.Count > 1)
            throw new AmbiguousMatchException(name);
        return shortcuts.First();
    }

    public bool ShortcutExists(string name, bool strict = false)
    {
        try
        {
            Get(name, strict);
        }
        catch (NoSuchShortcutException)
        {
            return false;
        }

        return true;
    }
    
    public static (string?, string) UnpackName(string name)
    {
        var parts = name.Split(':');
        return parts.Length switch
        {
            1 => (null, name),
            2 => (parts[0], parts[1]),
            _ => throw new ArgumentException("Unexpected ':' parsing shortcut name")
        };
    }

    public bool ContainsNamespace(string ns) => _shortcuts.ContainsKey(ns);

    public List<Shortcut> this[string key]
    {
        get => _shortcuts[key];
        set => _shortcuts[key] = value;
    }
    public IEnumerator<KeyValuePair<string, List<Shortcut>>> GetEnumerator()
    {
        return _shortcuts.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}