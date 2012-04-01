using System.Windows.Input;
namespace CustomControls
{
    public static class CustomWindowCommands
    {
        public static readonly RoutedUICommand Maximize = new RoutedUICommand("Maximize command", "Maximize", typeof(CustomWindow));
        public static readonly RoutedUICommand Minimize = new RoutedUICommand("Minimize command", "Minimize", typeof(CustomWindow));
        //public static RoutedUICommand DisplaySystemButtons = new RoutedUICommand();
    }
}
