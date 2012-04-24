using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Data;

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
        //public static readonly RoutedUICommand DetailsShowCommand = new RoutedUICommand("Runs 'details' window", "Details show", typeof(MainWindowViewModel));
        public static readonly RoutedUICommand UserListAddItemCommand = new RoutedUICommand("Adds an item to one of user lists", "User list add item", typeof(MainWindowViewModel));

        #endregion (Commands)

        #endregion (Fields)

        #region Properties

        public DataView SearchResultView
        {
            get { return (DataView)GetValue(SearchResultTableProperty); }
            private set { SetValue(SearchResultTablePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey SearchResultTablePropertyKey =
            DependencyProperty.RegisterReadOnly("SearchResultTable", typeof(DataView), typeof(MainWindowViewModel), new UIPropertyMetadata(null));
        public static readonly DependencyProperty SearchResultTableProperty = SearchResultTablePropertyKey.DependencyProperty;


        public Collection<DataTable> UserDataLists
        {
            get { return (Collection<DataTable>)GetValue(UserDataListsProperty); }
            private set { SetValue(UserDataListsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey UserDataListsPropertyKey =
            DependencyProperty.RegisterReadOnly("UserDataLists", typeof(Collection<DataTable>), typeof(MainWindowViewModel), new UIPropertyMetadata(null));
        public static readonly DependencyProperty UserDataListsProperty = UserDataListsPropertyKey.DependencyProperty;

        public IList<object> UserManipulationSelection
        {
            get { return (IList<object>)GetValue(UserManipulationSelectionProperty); }
            set { SetValue(UserManipulationSelectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserManipulationSelection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserManipulationSelectionProperty =
            DependencyProperty.Register("UserManipulationSelection", typeof(IList<object>), typeof(MainWindowViewModel), new UIPropertyMetadata(new List<object>()));

        #region Attached

        public static bool GetAreListBoxBehaviorsAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(AreListBoxBehaviorsAttached);
        }

        public static void SetAreListBoxBehaviorsAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(AreListBoxBehaviorsAttached, value);
        }

        public static readonly DependencyProperty AreListBoxBehaviorsAttached =
            DependencyProperty.RegisterAttached("AreListBoxBehaviorsAttached", typeof(bool), typeof(MainWindowViewModel),
            new UIPropertyMetadata(false, AreListBoxBehaviorsAttachedValueChanged));  

        #endregion (Attached)

        #endregion (Properties)

        #region Methods

        #region Property event handlers

        private static void AreListBoxBehaviorsAttachedValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == AreListBoxBehaviorsAttached)
            {
                ListBox lb = sender as ListBox;
                if ((bool)e.NewValue == true)
                {                   
                    lb.PreviewMouseLeftButtonDown += ListBoxMouseButtonEventHandler;
                }
                else
                {
                    lb.PreviewMouseLeftButtonDown -= ListBoxMouseButtonEventHandler;
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
            var list = sender as ListBox;
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (e.ButtonState == MouseButtonState.Pressed)
                    {
                        if (list != null)
                        {
                            if (e.ClickCount > 1)
                            {
                                if (list.SelectedItem != null) DetailsShowTask((list.SelectedItem as DataRowView).Row);
                            }
                            else
                            {
                                list.SelectedItem = null;
                            }
                        }
                    }
                    break;
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

                //_interviewer.InterviewResult.DefaultView.Sort = "Raiting DESC";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        private void SearchCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SearchResultView = _interviewer.InterviewResult.DefaultView;
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


        private static void DetailsShowTask(DataRow detailData)
        {
            new watch_assistant.ViewModel.DetailsWindow.DetailsWindowViewModel(
                new View.DetailsWindow.DetailsWindow() { Owner = Application.Current.MainWindow },
                detailData
            );
        }
        //private void CanExecuteDetailsShowTask (object sender, CanExecuteRoutedEventArgs e)
        //{
        //    ListBox lb = e.OriginalSource as ListBox;
        //    e.CanExecute = lb != null && lb.SelectedItems.Count == 1;
        //}
        //private void RunDetailsShowTask(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (e.Parameter != null) DetailsShowTask((e.Parameter as DataRowView).Row);
        //}


        private void CanExecuteUserListAddItemTask(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void RunUserListAddItemTask(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Lololo!");
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
            //_owner.CommandBindings.Add(new CommandBinding(DetailsShowCommand, RunDetailsShowTask, CanExecuteDetailsShowTask));
            _owner.CommandBindings.Add(new CommandBinding(UserListAddItemCommand, RunUserListAddItemTask, CanExecuteUserListAddItemTask));

            // Temporary list definitions
            _userData.Add(new DataTable() { TableName = "Favorites" });
            _userData.Add(new DataTable() { TableName = "Interest" });
        }

        #endregion (Constructors)
    }
}
