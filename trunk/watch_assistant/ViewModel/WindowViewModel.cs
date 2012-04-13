using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Interactivity;
using watch_assistant.View;

namespace watch_assistant.ViewModel
{
    public class WindowMoveBehavior : Behavior<Window>
    {
        #region Attach/Detach

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewMouseMove += OnPreviewMouseMoveEventHandler;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PreviewMouseMove -= OnPreviewMouseMoveEventHandler;
        }

        #endregion (Attach/Detach)

        private static void OnPreviewMouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            if ( e.LeftButton == MouseButtonState.Pressed && !(sender as Window).IsMouseCaptured ) e.Handled = true;
        }

    }

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

            this._owner = owner;
            this._owner.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) => { _owner.Close(); }));
        }

        #endregion (Constructors)
    }
}
