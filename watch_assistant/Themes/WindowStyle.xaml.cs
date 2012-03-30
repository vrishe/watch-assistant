using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using watch_assistant.ViewModel;

namespace watch_assistant.Themes
{
    public partial class WindowStyle : ResourceDictionary
    {
        //private WindowResizer wResizer;
        private bool _restorationBlock = false;

        public WindowStyle()
        {
            InitializeComponent();

            // Load the OS dependent styles
  
            ResourceDictionary osDependantResources = new ResourceDictionary();

            if (Environment.OSVersion.Version.Major <= 5)
            {
                osDependantResources.Source = new Uri(@"Themes\WindowStyleXP.xaml", UriKind.Relative);
            }
            else
            {
                osDependantResources.Source = new Uri(@"Themes\WindowStyleWin7.xaml", UriKind.Relative);
            }    
            this.MergedDictionaries.Add(osDependantResources);
        }

        /// <summary>
        /// Updates the window constraints based on its state.
        /// For instance, the max width and height of the window is set to prevent overlapping over the taskbar.
        /// </summary>
        /// <param name="window">Window to set properties</param>
        private void UpdateWindowConstraints(Window window)
        {
            if (window != null)
            {
                // Make sure we don't bump the max width and height of the desktop when maximized
                GridLength borderWidth = (GridLength)window.FindResource("BorderWidth");
                if (borderWidth != null)
                {
                    window.MaxHeight = SystemParameters.WorkArea.Height + borderWidth.Value * 2;
                    window.MaxWidth = SystemParameters.WorkArea.Width + borderWidth.Value * 2;
                }
            }
        }

        /// <summary>
        /// Fires when the user clicks the minimize button on the window's custom title bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Fires when the user clicks the maximize button on the window's custom title bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            // Check the current state of the window. If the window is currently maximized, return the
            // window to it's normal state when the maximize button is clicked, otherwise maximize the window.
            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
            }
            else
            {
                window.WindowState = WindowState.Maximized;
                window.Focus();
            }
        }

        /// <summary>
        /// Called when the window gets resized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            // Update window's contraints like max height and width.
            UpdateWindowConstraints(window);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //Window window = (Window)(sender as FrameworkElement).TemplatedParent;
        
            //UpdateWindowConstraints(window);

            //wResizer = new WindowResizer(window);

            //// Attach resizers
            //wResizer.AddResizer("rightSizeGrip", WindowResizer.ElementResizeDirection.EW_HORIZONTAL);
            //wResizer.AddResizer("leftSizeGrip", WindowResizer.ElementResizeDirection.WE_HORIZONTAL);
            //wResizer.AddResizer("topSizeGrip", WindowResizer.ElementResizeDirection.NS_VERTICAL);
            //wResizer.AddResizer("bottomSizeGrip", WindowResizer.ElementResizeDirection.SN_VERTICAL);
            //wResizer.AddResizer("topLeftSizeGrip", WindowResizer.ElementResizeDirection.NWSE_DIAGONAL);
            //wResizer.AddResizer("topRightSizeGrip", WindowResizer.ElementResizeDirection.NESW_DIAGONAL);
            //wResizer.AddResizer("bottomLeftSizeGrip", WindowResizer.ElementResizeDirection.SWNE_DIAGONAL);
            //wResizer.AddResizer("bottomRightSizeGrip", WindowResizer.ElementResizeDirection.SENW_DIAGONAL);
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event. This event handler is used here to facilitate
        /// dragging of the Window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBorderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeWindow(sender, e);
                _restorationBlock = true;
            }
        }

        /// <summary>
        /// Called when the user drags the title bar when maximized.
        /// </summary>
        private void OnBorderMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var window = (Window)((FrameworkElement)sender).TemplatedParent;

                if (window != null)
                {
                    if (!_restorationBlock)
                    {
                        if (window.WindowState == WindowState.Maximized)
                        {
                            Point cursorRelative = e.GetPosition(window);
                            Point newWindowLocation = e.GetPosition(null);
                            newWindowLocation.Offset(
                                -cursorRelative.X * window.RestoreBounds.Width / (window.ActualWidth),
                                -cursorRelative.Y * window.RestoreBounds.Height / (window.ActualHeight)
                            );

                            window.WindowState = WindowState.Normal;

                            window.Left = newWindowLocation.X;
                            window.Top = newWindowLocation.Y;
                        }
                        window.DragMove();
                    }
                    else
                    {
                        _restorationBlock = false;
                    }
                }
            }
        }

        //private void OnThemeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    string themeXamlFileName = @"";

        //    // Get combo box item tag name
        //    if (e.AddedItems.Count > 0)
        //    {
        //        ComboBoxItem selectedItem = (ComboBoxItem)e.AddedItems[0];
        //        themeXamlFileName = (string)selectedItem.Tag;
        //        ResourceDictionary skin = new ResourceDictionary();
        //        skin.Source = new Uri(@"Resources\Skins\" + themeXamlFileName + ".xaml", UriKind.Relative);

        //        Application.Current.Resources.MergedDictionaries[0] = skin;
        //    }
        //}
    }
}