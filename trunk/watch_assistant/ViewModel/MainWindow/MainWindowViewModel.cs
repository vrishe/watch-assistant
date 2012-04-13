using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Data;

namespace watch_assistant.ViewModel.MainWindow
{
    class MainWindowViewModel : WindowViewModel
    {
        #region Fields
        private readonly Model.Search.InterviewAggregator _interviewer = new Model.Search.InterviewAggregator();
        private readonly Dictionary<string, DataTable> _userLists = new Dictionary<string, DataTable>();

        #region Commands
        public static readonly RoutedUICommand SearchCommand = new RoutedUICommand("Activates searching process", "Search", typeof(MainWindowViewModel));
        #endregion (Commands)

        #endregion (Fields)

        #region Properties

        public DataTable SearchResultList
        {
            get { return (DataTable)GetValue(SearchResultListProperty); }
            set { SetValue(SearchResultListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchResultList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchResultListProperty =
            DependencyProperty.Register("SearchResultList", typeof(DataTable), typeof(MainWindowViewModel), new UIPropertyMetadata(null));

        public DataTable ActiveUserList
        {
            get { return (DataTable)GetValue(ActiveUserListProperty); }
            set { SetValue(ActiveUserListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveUserList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveUserListProperty =
            DependencyProperty.Register("ActiveUserList", typeof(DataTable), typeof(MainWindowViewModel), new UIPropertyMetadata(null));       

        #endregion (Properties)

        #region Constructors

        public MainWindowViewModel(Window owner)
            : base(owner)
        {
            _owner.CommandBindings.Add(new CommandBinding(
                SearchCommand, (s, e) => 
                {
                    try
                    {
                        _interviewer.ConductInterview((string)e.Parameter);
                    }
                    catch { }
                }
            ));
        }

        #endregion (Constructors)

    }
}
