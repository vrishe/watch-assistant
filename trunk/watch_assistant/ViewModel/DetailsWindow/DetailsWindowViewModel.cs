using System.Data;
using System.Windows;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace watch_assistant.ViewModel.DetailsWindow
{
    class VideoItemRef
    {
        string Dub { get; set; }
        List<string> HRefs { get; set; }

        public override string ToString()
        {
            return Dub;
        }
    }
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

        // Combo boxes container reference
        public Dictionary<string, List<string>> Dubs
        {
            get { return (Dictionary<string, List<string>>)GetValue(DubsProperty); }
            set { SetValue(DubsProperty, value); }
        }

        public static readonly DependencyProperty DubsProperty =
            DependencyProperty.Register("Dubs", typeof(Dictionary<string, List<string>>), typeof(DetailsWindowViewModel), new UIPropertyMetadata(null));
        
        #endregion (Properties)

        #region Constructors

        public DetailsWindowViewModel(Window owner, DataRow details)
            : base(owner)
        {
            try
            {
                watch_assistant.Model.Search.VideoInfoGraber.GetInfo(details);
                Details = details;
                Dubs = FillDubs();

                _owner.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion (Constructors)

        #region Methods

        private Dictionary<string, List<string>> FillDubs()
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            foreach (var item in (List<KeyValuePair<string,string>>)Details["HRefs"])
            {
                if (!result.ContainsKey(item.Value))
                {
                    List<string> tmp = new List<string>();
                    tmp.Add(item.Key);
                    result.Add(item.Value, tmp);
                }
                else
                    result[item.Value].Add(item.Key);
            }
            return result;
        }

        #endregion (Methods)
    }
}
