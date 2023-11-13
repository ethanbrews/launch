namespace Launch.Exception;

public class AmbiguousShortcutException : System.Exception
{
    public AmbiguousShortcutException(string name)
        : base($"The name '{name}' matches more than one shortcut")
    {
    }
}