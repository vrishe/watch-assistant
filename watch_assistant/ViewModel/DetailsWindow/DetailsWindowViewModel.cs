using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using watch_assistant.View.DetailsWindow;

namespace watch_assistant.ViewModel.DetailsWindow
{
    public class DubRefsAssociation
    {
        public string Dub { get; set; }
        public List<string> HRefs { get; set; }

        public DubRefsAssociation(string dub, List<string> hrefs)
        {
            Dub = dub;
            HRefs = hrefs;
        }
    }

    public class DetailsWindowViewModel : WindowViewModel
    {
        #region Commands

        public static readonly RoutedUICommand PlayCommand = new RoutedUICommand("Opens player window", "Play", typeof(DetailsWindowViewModel));
        public static readonly RoutedUICommand MagnifyCommand = new RoutedUICommand("Magnifies something", "Magnify", typeof(DetailsWindowViewModel));

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
        public List<DubRefsAssociation> DubsAssociation
        {
            get { return (List<DubRefsAssociation>)GetValue(DubsAssociationProperty); }
            set { SetValue(DubsAssociationProperty, value); }
        }

        public static readonly DependencyProperty DubsAssociationProperty =
            DependencyProperty.Register("DubsAssociation", typeof(List<DubRefsAssociation>), typeof(DetailsWindowViewModel), new UIPropertyMetadata(null));

        #endregion (Properties)

        #region Methods

        private static void RunPlayerWindow(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo((string)e.Parameter));
        }

        private List<DubRefsAssociation> DubsAssociatedListInitialize()
        {
            List<DubRefsAssociation> result = new List<DubRefsAssociation>();
            foreach (var item in (Dictionary<string, string>)Details["HRefs"])
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
                    result.Add(new DubRefsAssociation(item.Value, tmp));
                }
            }
            return result;
        }

        private static void WindowClosedEventHandler(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window != null)
            {
                window.Owner.Activate();
                window.Owner.Focus();
            }
        }

        #endregion (Methods)

        #region Constructors

        static DetailsWindowViewModel()
        {
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(PlayCommand, RunPlayerWindow));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(MagnifyCommand, (s, e) => 
            {
                MagnifiedImageWidget imageWidget = new MagnifiedImageWidget(e.Parameter as string) { Owner = s as Window, ShowActivated = true };
                imageWidget.Closed += WindowClosedEventHandler;

                imageWidget.Show();
            }));
        }

        public DetailsWindowViewModel(Window owner, DataRow details)
            : base(owner)
        {
            try
            {
                watch_assistant.Model.Search.VideoInfoGraber.GetInfo(details);
                Details = details;
                DubsAssociation = DubsAssociatedListInitialize();

                _owner.Closed += WindowClosedEventHandler;
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
