using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using watch_assistant.Model.Dictionary;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace dictionary_manager.ViewModel
{
    class MainWindowViewModel : watch_assistant.ViewModel.ViewModelBase
    {
        #region Fields

        private readonly Window _owner;
        private Thesaurus _thesaurus = new Thesaurus();
        private Dictionary<string, IEnumerable<string>> _thesaurusView = new Dictionary<string, IEnumerable<string>>();

        public static RoutedUICommand AddDefinitionCommand = new RoutedUICommand("Adds new thesaurus definition", "AddDefinition", typeof(MainWindowViewModel));
        public static RoutedUICommand RemoveDefinitionCommand = new RoutedUICommand("Removes thesaurus definition", "RemoveDefinition", typeof(MainWindowViewModel));
        #endregion (Fields)

        #region Properties

        public IEnumerable<string> Keys
        {
            get { return (IEnumerable<string>)GetValue(KeysProperty); }
            set { SetValue(KeysProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Keys.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeysProperty =
            DependencyProperty.Register("Keys", typeof(IEnumerable<string>), typeof(MainWindowViewModel), new UIPropertyMetadata(new List<string>()));

        public string ActiveDefinition
        {
            get { return (string)GetValue(ActiveDefinitionProperty); }
            set { SetValue(ActiveDefinitionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveDefinition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveDefinitionProperty =
            DependencyProperty.Register("ActiveDefinition", typeof(string), typeof(MainWindowViewModel), new UIPropertyMetadata(String.Empty));

        

        public string SelectedKey
        {
            get { return (string)GetValue(SelectedKeyProperty); }
            set { SetValue(SelectedKeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedKeyProperty =
            DependencyProperty.Register("SelectedKey", typeof(string), typeof(MainWindowViewModel), new UIPropertyMetadata(String.Empty, ComboBoxSelectionChanged));


        public string TextEntered
        {
            get { return (string)GetValue(TextEnteredProperty); }
            set { SetValue(TextEnteredProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextEntered.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextEnteredProperty =
            DependencyProperty.Register("TextEntered", typeof(string), typeof(MainWindowViewModel), new UIPropertyMetadata(String.Empty, ComboBoxTextChanged));      

        
        #endregion (Properties)

        #region Methods
        private static void ComboBoxSelectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MainWindowViewModel vm = sender as MainWindowViewModel;
            vm.TextEntered = vm.SelectedKey;
        }

        private static void ComboBoxTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MainWindowViewModel vm = sender as MainWindowViewModel;

            if (!String.IsNullOrEmpty((string)e.NewValue))
            {
                IEnumerable<string> temp;
                if (vm._thesaurus.TryGetDefinition((string)e.NewValue, out temp))
                {
                    StringBuilder sb = new StringBuilder();
                    string separator = ", ";
                    foreach (string str in temp)
                    {
                        sb.Append(String.Format("{0}{1}", str, separator));
                    }
                    vm.ActiveDefinition = sb.Remove(sb.Length - separator.Length, separator.Length).ToString();
                    return;
                }
            }
            vm.ActiveDefinition = (string)ActiveDefinitionProperty.DefaultMetadata.DefaultValue;
        }
        #endregion (Methods)

        #region Constructors
        public MainWindowViewModel(Window owner)
        {
            _owner = owner;
            DisplayName = "Dictionary management";

            _owner.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) => { _owner.Close(); }));
            _owner.CommandBindings.Add(new CommandBinding(
                ApplicationCommands.Open, (s, e) => 
                {
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.CheckFileExists = true;
                    dlg.Multiselect = false;
                    if ( dlg.ShowDialog(_owner) == true )
                    {
                        _thesaurus.Deserialize(dlg.FileName);
                        Keys = new List<string>(_thesaurus.Keys);
                        SelectedKey = Keys.First();
                    }
                }
            ));
            _owner.CommandBindings.Add(new CommandBinding(
                ApplicationCommands.Save, (s, e) =>
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.CheckPathExists = true;
                    dlg.OverwritePrompt = true;
                    if (dlg.ShowDialog(_owner) == true)
                    {
                        _thesaurus.Serialize(dlg.FileName);
                    }
                },
                (s, e) => { e.CanExecute = _thesaurus.Count > 0; }
            ));

            _owner.CommandBindings.Add(new CommandBinding(
                AddDefinitionCommand,
                (s, e) => 
                {
                    try
                    {
                        string separator = "\t\n, ";
                        string[] definitionSplit = ActiveDefinition.Split(separator.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                        _thesaurus.SetDefinition(TextEntered, definitionSplit, false);
                        Keys = new List<string>(_thesaurus.Keys);
                        SelectedKey = TextEntered;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                },
                (s, e) => { e.CanExecute = !(String.IsNullOrEmpty(TextEntered) || String.IsNullOrEmpty(ActiveDefinition)); }
            ));
            _owner.CommandBindings.Add(new CommandBinding(
                RemoveDefinitionCommand, (s, e) =>
                {
                    try
                    {
                        _thesaurus.RemoveDefinition(TextEntered, false);
                        Keys = new List<string>(_thesaurus.Keys);
                        SelectedKey = Keys.First();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                },
                (s, e) => { e.CanExecute = Keys != null && Keys.Contains(TextEntered); }
            ));
        }
        #endregion (Constructors)
    }
}
