using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace watch_assistant.Utility
{
    static class WindowMover : DependencyObject
    {
        #region Fields

        private static Dictionary<UIElement, Window> visualWindowAssociations { get; }

        #endregion // Fields

        #region Properties

        public static bool GetIsBehaviorAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachedBehaviorProperty);
        }

        public static void SetIsBehaviorAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachedBehaviorProperty, value);
        }

        // Using a DependencyProperty as the backing store for Apply.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AttachedBehaviorProperty =
            DependencyProperty.RegisterAttached(
                "IsBehaviorAttached", 
                typeof(bool), 
                typeof(WindowMover), 
                new UIPropertyMetadata(
                    false
                    )
            );

        #endregion // Properties

        #region Methods

        #region Event handlers

        private static void AttachedBehaviorePropertyChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // Attach
            }
            else
            {
                // Detach
            }
        }

        #endregion // Event handlers

        //private static Window GetControl

        private void FrameworkElementMouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            var element = sender as FrameworkElement;

            if (e.LeftButton == MouseButtonState.Pressed && element.IsMouseDirectlyOver)
            {
                var window = element;

                if (window != null)
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
            }
        }

        #endregion // Methods


    }
}
