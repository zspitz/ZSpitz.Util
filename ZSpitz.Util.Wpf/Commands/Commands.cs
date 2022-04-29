using System.Windows.Input;

namespace ZSpitz.Util.Wpf; 

public static class Commands {
    public static ICommand LaunchUrlOrFileCommand = new LaunchUrlOrFileCommand();
}
