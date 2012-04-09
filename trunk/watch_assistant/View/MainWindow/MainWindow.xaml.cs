using System.Windows;
using CustomControls;
using System.Data;
using System;

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

        // Delete this when AOSInterviewer is seted up
        Model.Search.ASeeInterviewer testSpamer = new Model.Search.ASeeInterviewer();
        private void bSearch_Click(object sender, RoutedEventArgs e)
        {
            testSpamer.ClearInterviewResults();
            try
            {
                testSpamer.InterviewSite(tbSearch.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }   

            searchView.Items.Clear();
            foreach (DataRow tmp in testSpamer.InterviewResult.Rows) 
            {
                searchView.Items.Add(tmp.ItemArray[0].ToString());
            }
        }
    }
}
