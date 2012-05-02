using System;
using System.Windows;

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
