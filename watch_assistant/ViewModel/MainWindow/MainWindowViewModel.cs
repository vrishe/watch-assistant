using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace watch_assistant.ViewModel.MainWindow
{
    class MainWindowViewModel : WindowViewModel
    {
        #region Fields

        private readonly Model.Dictionary.Thesaurus _thesaurus = new Model.Dictionary.Thesaurus();
        private readonly Model.Search.IInterviewers.InterviewAggregator _interviewer = new Model.Search.IInterviewers.InterviewAggregator();
        private BackgroundWorker _bgInterview = new BackgroundWorker();

        private readonly ObservableCollection<DataTable> _userData = new ObservableCollection<DataTable>();

        #region Commands
        public static readonly RoutedUICommand SearchCommand = new RoutedUICommand("Activates searching process", "Search", typeof(MainWindowViewModel));
        #endregion (Commands)

        #endregion (Fields)

        #region Properties

        public DataTable SearchResultTable
        {
            get { return (DataTable)GetValue(SearchResultTableProperty); }
            private set { SetValue(SearchResultTablePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey SearchResultTablePropertyKey =
            DependencyProperty.RegisterReadOnly("SearchResultTable", typeof(DataTable), typeof(MainWindowViewModel), new UIPropertyMetadata(null));
        public static readonly DependencyProperty SearchResultTableProperty = SearchResultTablePropertyKey.DependencyProperty;


        public Collection<DataTable> UserDataLists
        {
            get { return (Collection<DataTable>)GetValue(UserDataListsProperty); }
            private set { SetValue(UserDataListsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey UserDataListsPropertyKey =
            DependencyProperty.RegisterReadOnly("UserDataLists", typeof(Collection<DataTable>), typeof(MainWindowViewModel), new UIPropertyMetadata(null));
        public static readonly DependencyProperty UserDataListsProperty = UserDataListsPropertyKey.DependencyProperty;

        #region Attached

        public static bool GetAttachDetailsDoubleClickOpen(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachDetailsDoubleClickOpenProperty);
        }

        public static void SetAttachDetailsDoubleClickOpen(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachDetailsDoubleClickOpenProperty, value);
        }

        public static readonly DependencyProperty AttachDetailsDoubleClickOpenProperty =
            DependencyProperty.RegisterAttached("AttachDetailsDoubleClickOpen", typeof(bool), typeof(MainWindowViewModel),
            new UIPropertyMetadata(false, AttachDetailsDoubleClickOpenValueChanged));  

        #endregion (Attached)

        #endregion (Properties)

        #region Methods

        #region Property event handlers

        private static void AttachDetailsDoubleClickOpenValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == AttachDetailsDoubleClickOpenProperty)
            {
                if ((bool)e.NewValue == true)
                {
                    (sender as ListBox).PreviewMouseLeftButtonDown += ListBoxMouseButtonEventHandler;
                }
                else
                {
                    (sender as ListBox).PreviewMouseLeftButtonDown -= ListBoxMouseButtonEventHandler;
                }
            }
            //else if ...
        }

        #endregion (Property event handlers)

        #region Arbitary event handlers

        private void UserDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UserDataLists = new Collection<DataTable>(sender as ObservableCollection<DataTable>);
        }

        #endregion (Arbitary event handlers)

        #region UI behaviors

        private static void ListBoxMouseButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var list = sender as ListBox;
                if (list != null)
                {
                    if (e.ClickCount > 1)
                    {
                        if (list.SelectedItem != null) RunDetailsWindow(list.SelectedItem as DataRow);
                    }
                    else
                    {
                        list.SelectedItem = null;
                    }
                }
            }
        }

        #endregion (UI behaviors)

        #region Command logic

        private void SearchTask(object sender, DoWorkEventArgs e)
        {
            try
            {
                var strings = _thesaurus.GetPhrasePermutations((string)e.Argument);
                _interviewer.ConductInterview(strings);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        private void SearchCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SearchResultTable = _interviewer.InterviewResult;
            _userData.Add(SearchResultTable);
            CommandManager.InvalidateRequerySuggested();
        }
        private void CanExecuteSearchTask(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_bgInterview.IsBusy;
        }
        private void RunSearchTask(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_bgInterview.IsBusy)
            {
                _interviewer.ClearInterviewResults();
                _bgInterview.RunWorkerAsync(e.Parameter);
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private static void RunDetailsWindow(DataRow detailData)
        {
            new watch_assistant.ViewModel.DetailsWindow.DetailsWindowViewModel(
                new View.DetailsWindow.DetailsWindow() { Owner = Application.Current.MainWindow }, 
                detailData
            );
        }

        #endregion (Command logic)

        #endregion (Methods)

        #region Constructors

        public MainWindowViewModel(Window owner)
            : base(owner)
        {
            _bgInterview.DoWork += SearchTask;
            _bgInterview.RunWorkerCompleted += SearchCompleted;
            // _bgInterview.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            _userData.CollectionChanged += UserDataCollectionChanged;

            // Command bindings
            _owner.CommandBindings.Add(new CommandBinding(SearchCommand, RunSearchTask, CanExecuteSearchTask));

            // Temporary list definitions
            _userData.Add(new DataTable() { TableName = "Favorites" });
            _userData.Add(new DataTable() { TableName = "Interest" });
        }

        #endregion (Constructors)
    }
}
