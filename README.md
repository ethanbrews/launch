# Launch

Tool to launch programs from the command line.

Usage:

```powershell
launch.exe
  [namespace:name]                                                                      // Launch app
  [name]                                                                                // Launch app
  --create namespace:name --type [url/file/command] --value ["url or file or command"]  // Create shortcut
  --edit namespace:name --type [url/file/command] --value ["url or file or command"]    // Edit a shortcut
  --add-alias [alias] --shortcut [namespace:name]                                       // Create a unique alias for the shortcut
  --remove-alias [alias]                                                                // Delete an alias for the shortcut
  --delete [namespace:name]                                                             // Delete a shortcut
  ```

Shortcut configurations are stored in `~\.shortcuts` using JSON format.
