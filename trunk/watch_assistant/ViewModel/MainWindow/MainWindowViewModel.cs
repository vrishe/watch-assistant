using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Data;
using System.Windows.Data;
using System;
using System.Diagnostics;
using System.Net;

namespace watch_assistant.ViewModel.MainWindow
{
    class FormatStringConverter : IValueConverter
    {
        #region Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value.ToString();
            return parameter != null ? String.Format((string)parameter, val) : val;
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value.ToString();
            return parameter != null ? val.Replace((string)parameter, String.Empty) : val;
        }

        #endregion
    }

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

        public DataTable SearchResultTable
        {
            get { return (DataTable)GetValue(SearchResultTableProperty); }
            set { SetValue(SearchResultTableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchResultList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchResultTableProperty =
            DependencyProperty.Register("SearchResultTable", typeof(DataTable), typeof(MainWindowViewModel), new UIPropertyMetadata(null));

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
                        _interviewer.ClearInterviewResults();
                        string[] tmp = new string[] { /*"Genshiken", */(string)e.Parameter };
                        _interviewer.ConductInterview(tmp);
                        SearchResultTable = _interviewer.InterviewResult;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            ));
        }

        #endregion (Constructors)

    }
}
