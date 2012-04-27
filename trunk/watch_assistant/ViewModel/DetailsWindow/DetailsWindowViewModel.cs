using System.Data;
using System.Windows;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Diagnostics;

namespace watch_assistant.ViewModel.DetailsWindow
{
    public class DubRefsHolder
    {
        public string Dub { get; set; }
        public List<string> HRefs { get; set; }

        public DubRefsHolder(string dub, List<string> hrefs)
        {
            Dub = dub;
            HRefs = hrefs;
        }
    }

    public class DetailsWindowViewModel : WindowViewModel
    {
        #region Commands

        public static readonly RoutedUICommand PlayCommand = new RoutedUICommand("Opens player window", "Play", typeof(DetailsWindowViewModel));

        #endregion (Commands)

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
        public List<DubRefsHolder> Dubs
        {
            get { return (List<DubRefsHolder>)GetValue(DubsProperty); }
            set { SetValue(DubsProperty, value); }
        }

        public static readonly DependencyProperty DubsProperty =
            DependencyProperty.Register("Dubs", typeof(List<DubRefsHolder>), typeof(DetailsWindowViewModel), new UIPropertyMetadata(null));

        #endregion (Properties)

        #region Methods

        private static void RunPlayerWindow(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo((string)e.Parameter));
        }

        private List<DubRefsHolder> FillDubs()
        {
            List<DubRefsHolder> result = new List<DubRefsHolder>();
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
                    result.Add(new DubRefsHolder(item.Value, tmp));
                }
            }
            return result;
        }

        #endregion (Methods)

        #region Constructors

        static DetailsWindowViewModel()
        {
            CommandManager.RegisterClassCommandBinding(typeof(DetailsWindowViewModel), new CommandBinding(PlayCommand, RunPlayerWindow));
        }

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
    }
}
