using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using watch_assistant.View;

namespace watch_assistant.ViewModel
{
	public class WindowViewModel : ViewModelBase
    {
        #region Fields

        protected readonly Window _owner;

        #endregion (Fields)

        #region Constructors

        public WindowViewModel(Window owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
            _owner.DataContext = this;
        }

        #endregion (Constructors)
    }
}
