using System;
using System.Collections.ObjectModel;
using System.Data;
using CustomControls;

namespace watch_assistant.View.MainWindow
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CustomWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            new watch_assistant.ViewModel.MainWindow.MainWindowViewModel(this, (ObservableCollection<DataTable>)AppDomain.CurrentDomain.GetData("userListsData"));
        }
    }
}
