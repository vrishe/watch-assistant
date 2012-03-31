using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CustomControls
{
    public static class CustomWindowCommands
    {
        public static readonly RoutedUICommand Maximize = new RoutedUICommand();
        public static RoutedUICommand Minimize = new RoutedUICommand();
        public static RoutedUICommand DisplaySystemButtons = new RoutedUICommand();
    }
}
