namespace Launch.Exception;

public class NoSuchShortcutException : System.Exception
{
    public NoSuchShortcutException(string name) : base("No such shortcut: " + name)
    {
    }
}