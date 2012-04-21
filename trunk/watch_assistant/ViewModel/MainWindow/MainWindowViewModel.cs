using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

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
        private readonly Model.Dictionary.Thesaurus _thesaurus = new Model.Dictionary.Thesaurus();
        private readonly Model.Search.IInterviewers.InterviewAggregator _interviewer = new Model.Search.IInterviewers.InterviewAggregator();
        private BackgroundWorker _bgInterview = new BackgroundWorker();
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

        public static readonly DependencyProperty SearchResultTableProperty =
            DependencyProperty.Register("SearchResultTable", typeof(DataTable), typeof(MainWindowViewModel), new UIPropertyMetadata(null));

        public DataTable ActiveUserTable
        {
            get { return (DataTable)GetValue(ActiveUserTableProperty); }
            set { SetValue(ActiveUserTableProperty, value); }
        }

        public static readonly DependencyProperty ActiveUserTableProperty =
            DependencyProperty.Register("ActiveUserTable", typeof(DataTable), typeof(MainWindowViewModel), new UIPropertyMetadata(null));



        public static bool GetAttachDetailsDoubleClickOpen(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachDetailsDoubleClickOpenProperty);
        }

        public static void SetAttachDetailsDoubleClickOpen(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachDetailsDoubleClickOpenProperty, value);
        }

        public static readonly DependencyProperty AttachDetailsDoubleClickOpenProperty =
            DependencyProperty.RegisterAttached("AttachDetailsDoubleClickOpen", typeof(bool), typeof(MainWindowViewModel), new UIPropertyMetadata(0));

        

        #endregion (Properties)

        #region Constructors

        public MainWindowViewModel(Window owner)
            : base(owner)
        {
            InitializeBGInterview();
            _owner.CommandBindings.Add(new CommandBinding(
                SearchCommand, (s, e) => 
                {
                    _interviewer.ClearInterviewResults();
                    if (!_bgInterview.IsBusy)
                        _bgInterview.RunWorkerAsync(e.Parameter);
                    else
                        System.Media.SystemSounds.Beep.Play();
                }
            ));
        }

        #endregion (Constructors)

        #region Methods

        private void InitializeBGInterview()
        {
            _bgInterview.DoWork += new DoWorkEventHandler((s, e) =>
            {
                string[] tmp = _thesaurus.GetPhrasePermutations((string)e.Argument);
                try
                {
                    _interviewer.ConductInterview((string[])tmp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            });
            _bgInterview.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
            {
                SearchResultTable = _interviewer.InterviewResult;
            });
            //_bgInterview.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
        }

        #endregion (Methods)
    }
}
