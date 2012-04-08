using System.Windows;
using CustomControls;
using System.Data;

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
        Model.Search.AOSInterviewer testSpamer = new Model.Search.AOSInterviewer();
        private void bSearch_Click(object sender, RoutedEventArgs e)
        {
            testSpamer.ClearInterviewResults();
            testSpamer.InterviewSite(tbSearch.Text);

            searchView.Items.Clear();
            foreach (DataRow tmp in testSpamer.InterviewResult.Rows) 
            {
                searchView.Items.Add(tmp.ItemArray[0].ToString());
            }
        }
    }
}
