using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using watch_assistant.Model.Dictionary;
using System.Windows.Controls;
using System.Windows.Input;

namespace dictionary_manager.ViewModel
{
    class MainWindowViewModel : watch_assistant.ViewModel.ViewModelBase
    {
        #region Fields

        private readonly Window _owner;
        private Thesaurus _thesaurus = new Thesaurus();
        private Dictionary<string, IEnumerable<string>> _thesaurusView = new Dictionary<string, IEnumerable<string>>();
        #endregion (Fields)

        #region Properties

        public IEnumerable<string> Keys
        {
            get { return (IEnumerable<string>)GetValue(KeysProperty); }
            set { SetValue(KeysProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Keys.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeysProperty =
            DependencyProperty.Register("Keys", typeof(IEnumerable<string>), typeof(MainWindowViewModel), new UIPropertyMetadata(null));

        public IEnumerable<string> ActiveDefinition
        {
            get { return (IEnumerable<string>)GetValue(ActiveDefinitionProperty); }
            set { SetValue(ActiveDefinitionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveDefinition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveDefinitionProperty =
            DependencyProperty.Register("ActiveDefinition", typeof(IEnumerable<string>), typeof(MainWindowViewModel), new UIPropertyMetadata(null));

        

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
            DependencyProperty.Register("TextEntered", typeof(string), typeof(MainWindowViewModel), new UIPropertyMetadata(String.Empty));      

        
        #endregion (Properties)

        #region Methods
        private static void ComboBoxSelectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MainWindowViewModel vm = sender as MainWindowViewModel;
           
            IEnumerable<string> temp;
            if ( vm._thesaurus.TryGetDefinition(vm.TextEntered, out temp) ) vm.ActiveDefinition = temp;
        }
        #endregion (Methods)

        #region Constructors
        public MainWindowViewModel(Window owner)
        {
            _owner = owner;
            DisplayName = "Dictionary management";
        }
        #endregion (Constructors)
    }
}
