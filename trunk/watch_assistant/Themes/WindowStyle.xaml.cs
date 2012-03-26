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
    private WindowResizer wResizer;

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
        if (window == null) return;

        // Make sure we don't bump the max width and height of the desktop when maximized
        double Twelve = 12;
        window.MaxHeight = SystemParameters.WorkArea.Height + Twelve;
        window.MaxWidth = SystemParameters.WorkArea.Width + Twelve;
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
            window.Focus();
            window.WindowState = WindowState.Maximized;
        }
    }

    /// <summary>
    /// Called when the window gets resized.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Update window's contraints like max height and width.
        UpdateWindowConstraints(sender as Window);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Window _owner = (Window)(sender as FrameworkElement).TemplatedParent;

        wResizer = new WindowResizer(_owner);

        // Attach resizer
        wResizer.addResizerRight((Rectangle)_owner.Template.FindName("rightSizeGrip", _owner));
        wResizer.addResizerLeft((Rectangle)_owner.Template.FindName("leftSizeGrip", _owner));
        wResizer.addResizerUp((Rectangle)_owner.Template.FindName("topSizeGrip", _owner));
        wResizer.addResizerDown((Rectangle)_owner.Template.FindName("bottomSizeGrip", _owner));
        wResizer.addResizerLeftUp((Rectangle)_owner.Template.FindName("topLeftSizeGrip", _owner));
        wResizer.addResizerRightUp((Rectangle)_owner.Template.FindName("topRightSizeGrip", _owner));
        wResizer.addResizerLeftDown((Rectangle)_owner.Template.FindName("bottomLeftSizeGrip", _owner));
        wResizer.addResizerRightDown((Rectangle)_owner.Template.FindName("bottomRightSizeGrip", _owner));
    }

    /// <summary>
    /// Handles the MouseLeftButtonDown event. This event handler is used here to facilitate
    /// dragging of the Window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnBorderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Check if the control have been double clicked.
        if (e.ClickCount == 2)
        {
            // If double clicked then maximize the window.
            MaximizeWindow(sender, e);
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
                if (window.WindowState == WindowState.Maximized)
                {
                    Point cursorRelative = e.GetPosition(window);
                    Point newWindowLocation = e.GetPosition(null);
                    newWindowLocation.Offset(-cursorRelative.X * window.RestoreBounds.Width / (window.ActualWidth), -cursorRelative.Y);

                    window.WindowState = WindowState.Normal;

                    window.Left = newWindowLocation.X;
                    window.Top = newWindowLocation.Y;
                }
                window.DragMove();
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