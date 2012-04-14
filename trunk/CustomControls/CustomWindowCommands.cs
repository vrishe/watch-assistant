using System.Windows.Input;
namespace CustomControls
{
    public static class CustomWindowCommands
    {
        public static readonly RoutedUICommand ToggleMaximizeNormal = new RoutedUICommand("Toggles between normal/maximized states", "ToggleMaximizeNormal", typeof(CustomWindow));
        public static readonly RoutedUICommand Minimize = new RoutedUICommand("Minimize command", "Minimize", typeof(CustomWindow));
    }
}
