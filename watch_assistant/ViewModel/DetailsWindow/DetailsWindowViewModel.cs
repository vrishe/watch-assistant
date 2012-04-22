using System.Data;
using System.Windows;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace watch_assistant.ViewModel.DetailsWindow
{
    class RefsForDub
    {
        public string Dub { get; set; }
        public List<string> HRefs { get; set; }

        public RefsForDub(string dub, List<string> hrefs)
        {
            Dub = dub;
            HRefs = hrefs;
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
        public List<RefsForDub> Dubs
        {
            get { return (List<RefsForDub>)GetValue(DubsProperty); }
            set { SetValue(DubsProperty, value); }
        }

        public static readonly DependencyProperty DubsProperty =
            DependencyProperty.Register("Dubs", typeof(List<RefsForDub>), typeof(DetailsWindowViewModel), new UIPropertyMetadata(null));
        
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

        private List<RefsForDub> FillDubs()
        {
            List<RefsForDub> result = new List<RefsForDub>();
            foreach (var item in (List<KeyValuePair<string, string>>)Details["HRefs"])
            {
                bool aded = false;
                foreach (var dub in result)
                {
                    if (dub.Dub == item.Value)
                    {
                        dub.HRefs.Add(item.Key);
                        aded = true;
                    }
                }
                if (!aded)
                {
                    List<string> tmp = new List<string>();
                    tmp.Add(item.Key);
                    result.Add(new RefsForDub(item.Value, tmp));
                }
            }
            return result;
        }

        #endregion (Methods)
    }
}
