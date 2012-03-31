using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using System.Collections;
using System;

namespace CustomControls
{
    public class CustomWindow : Window
    {
        public enum Layout // Represents all available styles for this window
        { 
            OfficeButton, 
            RoundMonitor 
        }

        #region Fields
        
        private Control _frame;
        private Layout _frameLayout;

        private Point _cursorOffset;
        private FrameworkElement _sizingBorderTop;
        private FrameworkElement _sizingBorderRight;
        private FrameworkElement _sizingBorderLeft;
        private FrameworkElement _sizingBorderBottom;
        private FrameworkElement _sizingBorderTopRight;
        private FrameworkElement _sizingBorderBottomRight;
        private FrameworkElement _sizingBorderBottomLeft;
        private FrameworkElement _sizingBorderTopLeft;

        #endregion (Fields)

        #region Properties

        public Layout FrameLayout
        {
            get { return _frameLayout; }
            set
            {
                _frameLayout = value;

                UpdateFrameSettings();
            }
        }

        #endregion (Properties)

        #region Methods

        public override void OnApplyTemplate()
        { 
            base.OnApplyTemplate();

            CommandBindings.Add(new CommandBinding(CustomWindowCommands.Minimize, OnFrameCommand));
            CommandBindings.Add(new CommandBinding(CustomWindowCommands.Maximize, OnFrameCommand));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnFrameCommand));

            _frame = (Control)Template.FindName("PART_CustomFrame", this);

            UpdateFrameSettings();
        }

        private void OnFrameCommand(object sender, ExecutedRoutedEventArgs args)
        {
            if (args.Command == CustomWindowCommands.Minimize)
            {
                WindowState = WindowState.Minimized;
            } 
            else if (args.Command == CustomWindowCommands.Maximize)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

                UpdateFrameSettings();
            }
            else if (args.Command == ApplicationCommands.Close)
            {
                this.Close();
            }
        }

        private void UpdateFrameSettings()
        {
            if (_frame != null)
            {
                if (WindowState == WindowState.Normal)
                {
                    UpdateFrameAppearance(GetResourceDictionaryFileName(WindowState.Normal));

                    // Make ASYNCHRONOUS call to method that can react to new style
                    // (new settings are NOT detected when you call the method directly)
                    new Thread(() => { UpdateFrameBehavior(); }).Start();
                }

                else
                {
                    try
                    {
                        // Check for special frame style to support MAXIMIZED Window state
                        UpdateFrameAppearance(GetResourceDictionaryFileName(WindowState.Maximized));
                    }
                    catch (System.IO.IOException)
                    {
                        // Revert to standard frame style if no special style was defined
                        UpdateFrameAppearance(GetResourceDictionaryFileName(WindowState.Normal));
                    }
                }
            }
        }

        private string GetResourceDictionaryFileName(WindowState state)
        {
            string strFilePath = "/CustomControls;component/Themes/";
            string strFrameStyle = "resFrameStyle" + _frameLayout.ToString();
            string strMaximized = (state == WindowState.Maximized ? "Max" : String.Empty);

            return string.Format("{0}{1}{2}.xaml", strFilePath, strFrameStyle, strMaximized);
        }

        private void UpdateFrameAppearance(string strResourceFile)
        {
            ResourceDictionary currentResources = new ResourceDictionary();

            foreach (DictionaryEntry entry in Resources)
            {
                currentResources[entry.Key] = entry.Value;
            }

            Uri uri = new Uri(strResourceFile, UriKind.Relative);
            ResourceDictionary fromFile = Application.LoadComponent(uri) as ResourceDictionary;
            currentResources.MergedDictionaries.Add(fromFile);

            Resources = currentResources;
        }


        private void UpdateFrameBehavior()
        {
            // Attach event handlers for elements in new layout (CaptionBar dragging, sizing borders...)
            // (because we were called from a different thread, we must also marshall back to the UI thread)

            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (ThreadStart)delegate
            {
                FrameworkElement titleBar = (FrameworkElement)_frame.Template.FindName("PART_TitleBar", _frame);
                if (titleBar != null) titleBar.MouseLeftButtonDown += (sender, args) => { DragMove(); };

                _sizingBorderLeft = GetSizableBorder("PART_ResizeBorderLeft");
                _sizingBorderTopLeft = GetSizableBorder("PART_ResizeBorderTopLeft");
                _sizingBorderTop = GetSizableBorder("PART_ResizeBorderTop");
                _sizingBorderTopRight = GetSizableBorder("PART_ResizeBorderTopRight");
                _sizingBorderRight = GetSizableBorder("PART_ResizeBorderRight");
                _sizingBorderBottomRight = GetSizableBorder("PART_ResizeBorderBottomRight");
                _sizingBorderBottom = GetSizableBorder("PART_ResizeBorderBottom");
                _sizingBorderBottomLeft = GetSizableBorder("PART_ResizeBorderBottomLeft");

            });
        }

        private Path GetSizableBorder(string borderSegmentID)
        {
            Path sizingBorderSegment = (Path)_frame.Template.FindName(borderSegmentID, _frame);

            if (sizingBorderSegment != null)
            {
                sizingBorderSegment.MouseLeftButtonDown += (sender, args) =>
                {
                    if (WindowState != WindowState.Maximized)
                    {
                        Path borderSegment = (Path)sender;
                        _cursorOffset = GetCursorOffset(args.GetPosition(this), borderSegment);
                        borderSegment.CaptureMouse();
                    }
                };

                sizingBorderSegment.MouseLeftButtonUp += (sender, args) =>
                {
                    Path borderSegment = (Path)sender;
                    borderSegment.ReleaseMouseCapture();
                };

                sizingBorderSegment.MouseMove += (sender, args) =>
                {
                    Path borderSegment = (Path)sender;

                    if (borderSegment.IsMouseCaptured)
                    {
                        PerformResize(args.GetPosition(this), borderSegment);
                    }
                };
            }

            return sizingBorderSegment;
        }

        private Point GetCursorOffset(Point ptMousePosition, Path borderSegment)
        {
            Point ptOffset = new Point(0, 0);

            if (borderSegment == _sizingBorderLeft)
                ptOffset.X = ptMousePosition.X;

            else if (borderSegment == _sizingBorderTopLeft)
            {
                ptOffset.Y = ptMousePosition.Y;
                ptOffset.X = ptMousePosition.X;
            }

            else if (borderSegment == _sizingBorderTop)
                ptOffset.Y = ptMousePosition.Y;

            else if (borderSegment == _sizingBorderTopRight)
            {
                ptOffset.Y = ptMousePosition.Y;
                ptOffset.X = (Width - ptMousePosition.X);
            }

            else if (borderSegment == _sizingBorderRight)
                ptOffset.X = (Width - ptMousePosition.X);

            else if (borderSegment == _sizingBorderBottomRight)
            {
                ptOffset.X = (Width - ptMousePosition.X);
                ptOffset.Y = (Height - ptMousePosition.Y);
            }

            else if (borderSegment == _sizingBorderBottom)
                ptOffset.Y = Height - ptMousePosition.Y;

            else if (borderSegment == _sizingBorderBottomLeft)
            {
                ptOffset.Y = (Height - ptMousePosition.Y);
                ptOffset.X = ptMousePosition.X;
            }

            return ptOffset;
        }

        private void PerformResize(Point ptMousePosition, Path borderSegment)
        {
            if (borderSegment == _sizingBorderLeft)
            {
                Left += (ptMousePosition.X - _cursorOffset.X);
                Width -= (ptMousePosition.X - _cursorOffset.X);
            }

            else if (borderSegment == _sizingBorderTopLeft)
            {
                Left += (ptMousePosition.X - _cursorOffset.X);
                Width -= (ptMousePosition.X - _cursorOffset.X);
                Top += (ptMousePosition.Y - _cursorOffset.Y);
                Height -= (ptMousePosition.Y - _cursorOffset.Y);
            }

            else if (borderSegment == _sizingBorderTop)
            {
                Top += (ptMousePosition.Y - _cursorOffset.Y);
                Height -= (ptMousePosition.Y - _cursorOffset.Y);
            }

            else if (borderSegment == _sizingBorderTopRight)
            {
                Width = ptMousePosition.X + _cursorOffset.X;
                Top += (ptMousePosition.Y - _cursorOffset.Y);
                Height -= (ptMousePosition.Y - _cursorOffset.Y);
            }

            else if (borderSegment == _sizingBorderRight)
                Width = ptMousePosition.X + _cursorOffset.X;

            else if (borderSegment == _sizingBorderBottomRight)
            {
                Width = ptMousePosition.X + _cursorOffset.X;
                Height = ptMousePosition.Y + _cursorOffset.Y;
            }

            else if (borderSegment == _sizingBorderBottom)
                Height = ptMousePosition.Y + _cursorOffset.Y;

            else if (borderSegment == _sizingBorderBottomLeft)
            {
                Left += (ptMousePosition.X - _cursorOffset.X);
                Width -= (ptMousePosition.X - _cursorOffset.X);
                Height = ptMousePosition.Y + _cursorOffset.Y;
            }
        }

        #region Sizing handlers

        private void sizingBorderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                Path borderSegment = (Path)sender;
                _cursorOffset = GetCursorOffset(e.GetPosition(this), borderSegment);
                borderSegment.CaptureMouse();
            }
        }

        private static void sizingBorderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Path borderSegment = (Path)sender;
            borderSegment.ReleaseMouseCapture();
        }

        private  void sizingBorderMouseMove(object sender, MouseEventArgs e)
        {
            Path borderSegment = (Path)sender;
            if (borderSegment.IsMouseCaptured)
            {
                this.PerformResize(e.GetPosition(this), borderSegment);
            }
        }

        #endregion (Sizing handlers)

        #endregion (Methods)

        #region Constructors

        static CustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));
        }

        #endregion (Constructors)
    }
}
