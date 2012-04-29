using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using watch_assistant.Model.ExternalDataManager;
using watch_assistant.Model.RatingSystem;

namespace watch_assistant.ViewModel.MainWindow
{
    public class DataTableToMenuItemCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var command = parameter as ICommand;
            if (command == null) throw new ArgumentException(
                String.Format("Parameter must be a descendant of ICommand ('{0}')", parameter != null ? parameter.GetType().ToString() : "null")
                );

            var incoming = value as IEnumerable;
            if (incoming == null ) return incoming;

            var result = new Collection<MenuItem>();
            foreach (DataTable table in value as IEnumerable)
            {
                result.Add(new MenuItem() 
                { 
                    Header = table.TableName, 
                    Command = MainWindowViewModel.UserListAddItemCommand, 
                    CommandParameter = table
                });
            }
            CommandManager.InvalidateRequerySuggested();
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MainWindowViewModel : WindowViewModel
    {
        #region Fields

        private static readonly ExternalUserRatingTableData _userRatingTableData = (ExternalUserRatingTableData)AppDomain.CurrentDomain.GetData("userRatingTableData");

        private readonly Model.Dictionary.Thesaurus _thesaurus = new Model.Dictionary.Thesaurus("thesaurus.dic");
        private readonly Model.Search.IInterviewers.InterviewAggregator _interviewer = new Model.Search.IInterviewers.InterviewAggregator();
        private BackgroundWorker _bgInterview = new BackgroundWorker();

        #region Commands

        public static readonly RoutedUICommand SearchCommand = new RoutedUICommand("Activates searching process", "Search", typeof(MainWindowViewModel));
        public static readonly RoutedUICommand DetailsShowCommand = new RoutedUICommand("Runs 'details' window", "Details show", typeof(MainWindowViewModel));
        public static readonly RoutedUICommand UserListAddItemCommand = new RoutedUICommand("Adds an item to one of user lists", "User list add item", typeof(MainWindowViewModel));
        public static readonly RoutedUICommand UserListRemoveItemCommand = new RoutedUICommand("Removes an item to one of user lists", "User list remove item", typeof(MainWindowViewModel));

        #endregion (Commands)

        #endregion (Fields)

        #region Properties

        public DataTable SearchResultTable
        {
            get { return (DataTable)GetValue(SearchResultTableProperty); }
            private set { SetValue(SearchResultTablePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey SearchResultTablePropertyKey =
            DependencyProperty.RegisterReadOnly("SearchResultTable", typeof(DataTable), typeof(MainWindowViewModel), new UIPropertyMetadata(new DataTable() { TableName = "<Empty>" }));
        public static readonly DependencyProperty SearchResultTableProperty = SearchResultTablePropertyKey.DependencyProperty;


        public ObservableCollection<DataTable> UserListsData
        {
            get { return (ObservableCollection<DataTable>)GetValue(UserListsDataProperty); }
            set { SetValue(UserListsDataProperty, value); }
        }

        public static readonly DependencyProperty UserListsDataProperty =
            DependencyProperty.Register("UserListsData", typeof(ObservableCollection<DataTable>), typeof(MainWindowViewModel), new UIPropertyMetadata(_userRatingTableData.UserListsData));

        public ObservableCollection<DataRowView> SearchManipulationSelection
        {
            get { return (ObservableCollection<DataRowView>)GetValue(SearchManipulationSelectionProperty); }
            set { SetValue(SearchManipulationSelectionProperty, value); }
        }

        public static readonly DependencyProperty SearchManipulationSelectionProperty =
            DependencyProperty.Register("SearchManipulationSelection", typeof(ObservableCollection<DataRowView>), typeof(MainWindowViewModel), new UIPropertyMetadata(new ObservableCollection<DataRowView>()));

        // TEMPORARY

        public ObservableCollection<DataRowView> UserManipulationSelection
        {
            get { return (ObservableCollection<DataRowView>)GetValue(UserManipulationSelectionProperty); }
            set { SetValue(UserManipulationSelectionProperty, value); }
        }

        public static readonly DependencyProperty UserManipulationSelectionProperty =
            DependencyProperty.Register("UserManipulationSelection", typeof(ObservableCollection<DataRowView>), typeof(MainWindowViewModel), new UIPropertyMetadata(new ObservableCollection<DataRowView>()));      

        // TEMPORARY END

        #region Attached

        public static bool GetAreListBoxBehaviorsAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(AreListBoxBehaviorsAttachedProperty);
        }

        public static void SetAreListBoxBehaviorsAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(AreListBoxBehaviorsAttachedProperty, value);
        }

        public static readonly DependencyProperty AreListBoxBehaviorsAttachedProperty =
            DependencyProperty.RegisterAttached("AreListBoxBehaviorsAttached", typeof(bool), typeof(MainWindowViewModel),
            new UIPropertyMetadata(false, AttachedPropertyValueChanged));

        #endregion (Attached)

        #endregion (Properties)

        #region Methods

        #region Property event handlers

        private static void AttachedPropertyValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == AreListBoxBehaviorsAttachedProperty)
            {
                ListBox lb = sender as ListBox;
                if ((bool)e.NewValue == true)
                {
                    lb.MouseDown += ListBoxMouseButtonEventHandler;
                }
                else
                {
                    lb.MouseDown -= ListBoxMouseButtonEventHandler;
                }
            }
            // else if () { }
        }

        #endregion (Property event handlers)

        #region UI behaviors

        private static void ListBoxMouseButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListBox; 
            if (list != null) list.SelectedItem = null;
        }

        #endregion (UI behaviors)

        #region Command logic

        private void SearchTask(object sender, DoWorkEventArgs e)
        {
            try
            {
                var strings = _thesaurus.GetPhrasePermutations((string)e.Argument);
                _interviewer.ConductInterview(strings);
                RatingDBMS.AssignItemsPriority(_interviewer.InterviewResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        private void SearchCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SearchResultTable = _interviewer.InterviewResult.DefaultView.Table.Copy();
            SearchResultTable.DefaultView.Sort = "Rating DESC";
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
        private void RunDetailsShowTask(object sender, ExecutedRoutedEventArgs e)
        {
            DetailsShowTask(((DataRowView)e.Parameter).Row);
        }


        private static bool IsItemNonExistent(DataTable table, DataRow row)
        {
            foreach (DataRow rowCurrent in table.Rows)
            {
                if (rowCurrent["Name"].Equals(row["Name"])) return false;

                var hRefsIncoming = (Dictionary<string, string>)row["HRefs"];
                var hRefsOwn = new Dictionary<string, string>((Dictionary<string, string>)rowCurrent["HRefs"]);
                foreach (var hRef in hRefsIncoming) if (hRefsOwn.ContainsKey(hRef.Key)) return false;
            }
            return true;
        }
        private void CanExecuteUserListAddItemTask(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SearchManipulationSelection.Count > 0;
        }
        private void RunUserListAddItemTask(object sender, ExecutedRoutedEventArgs e)
        {
            var table = e.Parameter as DataTable;
            if (table == null) throw new ArgumentException(
                String.Format("UserListAddItemTask command failed: '{0}'", e.Parameter != null ? e.Parameter.ToString() : "null")
                );

            DataRowView[] addedArray = new DataRowView[SearchManipulationSelection.Count];
            SearchManipulationSelection.CopyTo(addedArray, 0);

            if (table.Rows.Count == 0) table.Merge(addedArray[0].Row.Table.Clone(), true, MissingSchemaAction.Add);
            foreach (DataRowView rowView in addedArray)
            {
                if (IsItemNonExistent(table, rowView.Row))
                {
                    watch_assistant.Model.Search.VideoInfoGraber.GetInfo(rowView.Row);
                    table.ImportRow(rowView.Row);
                }
                else
                {
                    // TODO: Verify existent item's data
                }
            }
            SearchManipulationSelection.Clear();
        }

        private void CanExecuteUserListRemoveItemTask(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = UserManipulationSelection.Count > 0;
        }
        private void RunUserListRemoveItemTask(object sender, ExecutedRoutedEventArgs e)
        {
            var table = e.Parameter as DataTable;
            if (table == null) throw new ArgumentException(
                String.Format("UserListAddItemTask command failed: '{0}'", e.Parameter != null ? e.Parameter.ToString() : "null")
                );

            DataRowView[] removalArray = new DataRowView[UserManipulationSelection.Count];
            UserManipulationSelection.CopyTo(removalArray, 0);

            foreach (DataRowView rowView in removalArray)
            {
                table.Rows.Remove(rowView.Row);
            }
            if (table.Rows.Count == 0) table.Reset();
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

            // Command bindings
            _owner.CommandBindings.Add(new CommandBinding(SearchCommand, RunSearchTask, CanExecuteSearchTask));
            _owner.CommandBindings.Add(new CommandBinding(DetailsShowCommand, RunDetailsShowTask/*, CanExecuteDetailsShowTask*/));
            _owner.CommandBindings.Add(new CommandBinding(UserListAddItemCommand, RunUserListAddItemTask, CanExecuteUserListAddItemTask));
            _owner.CommandBindings.Add(new CommandBinding(UserListRemoveItemCommand, RunUserListRemoveItemTask, CanExecuteUserListRemoveItemTask));
            
            //// REMOVE THIS
            //_thesaurus = new Model.Dictionary.Thesaurus("thesaurus.dic");
            //// REMOVE THIS END
        }
        
        #endregion (Constructors)
    }
}
