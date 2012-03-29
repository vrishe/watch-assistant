using System.Windows;
using System.Windows.Input;

namespace watch_assistant.ViewModel.DetailsWindow
{
    class DetailsWindowViewModel : WindowViewModel
    {
        
        #region Constructors

        public DetailsWindowViewModel(Window owner)
            : base(owner)
        {
            base._owner.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) => { owner.Close(); }));
        }

        #endregion // Constructors

    }
}
