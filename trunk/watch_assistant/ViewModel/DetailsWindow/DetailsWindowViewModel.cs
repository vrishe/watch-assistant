using System.Data;
using System.Windows;
using System.Windows.Input;
using System;

namespace watch_assistant.ViewModel.DetailsWindow
{
    class DetailsWindowViewModel : WindowViewModel
    {
        #region Properties

        // Details container reference
        public DataRow Details
        {
            get { return (DataRow)GetValue(DetailsProperty); }
            set { SetValue(DetailsProperty, value); }
        }

        public static readonly DependencyProperty DetailsProperty =
            DependencyProperty.Register("Details", typeof(DataRow), typeof(DetailsWindowViewModel), new UIPropertyMetadata(null));       

        #endregion (Properties)

        #region Constructors

        public DetailsWindowViewModel(Window owner, DataRow details)
            : base(owner)
        {
            try
            {
                watch_assistant.Model.Search.VideoInfoGraber.GetInfo(details);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Details = details;
        }

        #endregion // Constructors

    }
}
