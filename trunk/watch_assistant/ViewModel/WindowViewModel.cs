using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace watch_assistant.ViewModel
{
	public class WindowViewModel : ViewModelBase
    {
        #region Fields

        protected readonly Window _owner;

        #endregion // Fields

        #region Properties

        //public Dictionary<string, ICommand> Commands { get; private set; }

        #endregion

        #region Constructors

        public WindowViewModel(Window owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            this._owner = owner;
            //this.RequestClose += new EventHandler((s, e) => { owner.Close(); });

            //Commands = new Dictionary<string, ICommand>();
            //Commands.Add("CloseWindow", this.CloseCommand);
        }

        #endregion // Constructors
    }
}
