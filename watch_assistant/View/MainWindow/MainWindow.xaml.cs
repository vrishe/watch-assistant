using System.Windows;
using CustomControls;
using System.Data;
using System;
using watch_assistant.Model.Search.IVideoInfoGrabers;

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

            DataContext = new watch_assistant.ViewModel.MainWindow.MainWindowViewModel(this);
        }

        // Delete this when DetailsWindow is seted up
        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DetailsWindow.DetailsWindow tempPreview = new DetailsWindow.DetailsWindow();
            tempPreview.ShowDialog();
        }
    }
}
