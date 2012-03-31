using System.Windows;
using CustomControls;

namespace watch_assistant.View.DetailsWindow
{
    /// <summary>
    /// Логика взаимодействия для DetailsWindow.xaml
    /// </summary>
    public partial class DetailsWindow : CustomWindow
    {
        public DetailsWindow()
        {
            InitializeComponent();

            DataContext = new watch_assistant.ViewModel.DetailsWindow.DetailsWindowViewModel(this);
            //this.PosterGridColumn
        }
    }
}
