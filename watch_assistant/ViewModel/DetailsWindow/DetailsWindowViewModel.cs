using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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

        public DataRow Details
        {
            get { return (DataRow)GetValue(DetailsProperty); }
            private set { SetValue(DetailsPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey DetailsPropertyKey = 
            DependencyProperty.RegisterReadOnly("Details", typeof(DataRow), typeof(DetailsWindowViewModel), new UIPropertyMetadata(null, DetailsPropertyValueChanged));
        public static readonly DependencyProperty DetailsProperty = DetailsPropertyKey.DependencyProperty;

        public List<DubRefsAssociation> DubsAssociation
        {
            get { return (List<DubRefsAssociation>)GetValue(DubsAssociationProperty); }
            private set { SetValue(DubsAssociationPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey DubsAssociationPropertyKey =
            DependencyProperty.RegisterReadOnly("DubsAssociation", typeof(List<DubRefsAssociation>), typeof(DetailsWindowViewModel), new UIPropertyMetadata(null));
        public static readonly DependencyProperty DubsAssociationProperty = DubsAssociationPropertyKey.DependencyProperty;

        public BitmapImage PosterBitmap
        {
            get { return (BitmapImage)GetValue(PosterBitmapProperty); }
            private set { SetValue(PosterBitmapPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey PosterBitmapPropertyKey =
            DependencyProperty.RegisterReadOnly("PosterBitmap", typeof(BitmapImage), typeof(DetailsWindowViewModel), new UIPropertyMetadata(null));
        public static readonly DependencyProperty PosterBitmapProperty = PosterBitmapPropertyKey.DependencyProperty;

        #endregion (Properties)

        #region Methods

        #region Property event handlers

        private static void DetailsPropertyValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var target = sender as DetailsWindowViewModel;

            List<DubRefsAssociation> result = new List<DubRefsAssociation>();
            foreach (var item in (Dictionary<string, string>)target.Details["HRefs"])
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

            target.DubsAssociation = result;
            target.PosterBitmap = new BitmapImage(new Uri(target.Details["Poster"] as string));
        }

        #endregion (Property event handlers)

        private static void RunPlayerWindow(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo((string)e.Parameter));
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
                MagnifiedImageWidget imageWidget = new MagnifiedImageWidget(e.Parameter as BitmapImage) { Owner = s as Window, ShowActivated = true };
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
