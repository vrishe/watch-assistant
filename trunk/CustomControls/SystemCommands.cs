using System.Windows.Input;

namespace CustomControls
{
    public static class SystemCommands
    {
        public static readonly RoutedUICommand MinimizeWindowCommand = new RoutedUICommand("Window minimization command", "MinimizeWindowCommand", typeof(CustomWindow));
        public static readonly RoutedUICommand ToggleMaximizeWindowCommand = new RoutedUICommand("Toggles between normal/maximized states", "ToggleMaximizeWindowCommand", typeof(CustomWindow));
        public static readonly RoutedUICommand CloseWindowCommand = ApplicationCommands.Close;
    }
}
