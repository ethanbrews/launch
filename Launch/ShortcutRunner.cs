using System.Diagnostics;

namespace Launch;

public class ShortcutRunner
{
    private readonly Shortcut _shortcut;
    
    public ShortcutRunner(Shortcut shortcut)
    {
        _shortcut = shortcut;
    }

    public async Task RunAsync()
    {
        switch (_shortcut.Type)
        {
            case "url":
                LaunchUrl(_shortcut.Value!);
                break;
            case "file":
                LaunchFile(_shortcut.Value!);
                break;
            case "command":
                await LaunchCommandLine(_shortcut.Value!);
                break;
            default:
                throw new ArgumentException("Invalid shortcut data");
        }
    }
    
    private static void LaunchUrl(string value)
    {
        var process = new Process();
        process.StartInfo.UseShellExecute = true; 
        process.StartInfo.FileName = value;
        process.Start();
    }
    
    private static void LaunchFile(string value)
    {
        var process = new Process();
        process.EnableRaisingEvents = false;
        process.StartInfo.FileName = value;
        process.Start();
    }
    
    private static async Task LaunchCommandLine(string value)
    {
        var tmp = Path.GetTempPath() + Guid.NewGuid() + ".ps1";
        await File.WriteAllTextAsync(tmp, value);
        
        var scriptArguments = "-ExecutionPolicy Bypass -File \"" + tmp + "\"";
        var processStartInfo = new ProcessStartInfo("powershell.exe", scriptArguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = new Process();
        process.StartInfo = processStartInfo;
        process.Start();
        await process.WaitForExitAsync();
        File.Delete(tmp);
    }
}