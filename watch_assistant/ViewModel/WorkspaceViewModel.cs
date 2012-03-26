using System;
using System.Windows.Input;

namespace watch_assistant.ViewModel
{
    /// <summary>
    /// This ViewModelBase subclass requests to be removed 
    /// from the UI when its CloseCommand executes.
    /// This class is abstract.
    /// </summary>
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        #region Commands

        private RelayCommand _closeCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to remove this workspace from the user interface.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return _closeCommand; }
        }

        #endregion // Commands

        #region Events

        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;

        void OnRequestClose()
        {
            if (RequestClose != null) RequestClose(this, EventArgs.Empty);
        }

        #endregion // Events

        #region Constructors

        protected WorkspaceViewModel()
        {
            _closeCommand = new RelayCommand(param => this.OnRequestClose());
        }

        #endregion // Constructors
    }
}