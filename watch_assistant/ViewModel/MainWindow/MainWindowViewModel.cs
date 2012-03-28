using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace watch_assistant.ViewModel.MainWindow
{
    class MainWindowViewModel : WindowViewModel
    {

        #region Properties
        public static int f;
        #endregion // Properties

        #region Constructors

        public MainWindowViewModel(Window owner)
            : base(owner)
        {
            base._owner.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) => { owner.Close(); }));
        }

        #endregion // Constructors

    }
}
