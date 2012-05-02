using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CustomControls
{
    public class CustomWindow : Window
    {
        private delegate void ResizeProcedure(Window owner, Point mousePosition, Point mouseOffset);
        private delegate Point AnchorProcedure(Window owner, Point mousePosition);

        private struct ResizeOperation
        {
            private ResizeProcedure _resizer;
            private AnchorProcedure _binder;

            public ResizeProcedure Resizer { get { return _resizer; } }
            public AnchorProcedure Binder { get { return _binder; } }
            public ResizeOperation(ResizeProcedure resizer, AnchorProcedure binder)
            {
                _resizer = resizer;
                _binder = binder;
            }
        }
        private struct ResizeAnchor
        {
            private ResizeProcedure _resizer;
            private Point _anchor;

            public ResizeProcedure Resizer { get { return _resizer; } }
            public Point Anchor { get { return _anchor; } }
            public ResizeAnchor(Window owner, Point mousePosition, ResizeOperation op)
            {
                 _resizer = op.Resizer;
                _anchor = op.Binder(owner, mousePosition);
            }
        }

        #region Fields

        private ResourceDictionary _activeLayoutDictionary;

        private Control _frame;

        private readonly Dictionary<FrameworkElement, ResizeOperation> _resizeBorders = new Dictionary<FrameworkElement,ResizeOperation>();
        private ResizeAnchor _resizeAnchor;

        #endregion (Fields)

        #region Properties

        public string LayoutName
        {
            get { return (string)GetValue(LayoutNameProperty); }
            set { SetValue(LayoutNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayoutName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayoutNameProperty =
            DependencyProperty.Register("LayoutName", typeof(string), typeof(CustomWindow), new UIPropertyMetadata(
                    "Default", (s, e) => { ((CustomWindow)s).UpdateFrameStyle(e.NewValue as string); }
                )
            );

        private bool CanResize
        {
            get
            {
                return ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
            }
        }

        #endregion (Properties)

        #region Methods

        #region Commands

        private static void OnFrameCommandCanExcecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var commander = sender as CustomWindow;
            if (e.Command == SystemCommands.MinimizeWindowCommand)
            {
                e.CanExecute = commander.CanResize || commander.ResizeMode == ResizeMode.CanMinimize;
            }
            else if (e.Command == SystemCommands.ToggleMaximizeWindowCommand)
            {
                e.CanExecute = commander.CanResize;
            }
            else if (e.Command == SystemCommands.CloseWindowCommand)
            {
                e.CanExecute = true;
            }            
        }
        private static void OnFrameCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Window commander = sender as Window;
            if (e.Command == SystemCommands.MinimizeWindowCommand)
            {
                commander.WindowState = WindowState.Minimized;              
            } 
            else if (e.Command == SystemCommands.ToggleMaximizeWindowCommand)
            {
                commander.WindowState = commander.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
            else if (e.Command == SystemCommands.CloseWindowCommand)
            {
                commander.Close();
            }
        }

        #endregion (Commands)

        #region Resizer utility

        private FrameworkElement GetResizeBorder(string borderSegmentID)
        {
            FrameworkElement borderSegment = (FrameworkElement)_frame.Template.FindName(borderSegmentID, _frame);

            if (borderSegment != null)
            {
                borderSegment.MouseLeftButtonDown += sizingBorderMouseLeftButtonDown;
                borderSegment.MouseLeftButtonUp += sizingBorderMouseLeftButtonUp;
                borderSegment.MouseMove += sizingBorderMouseMove;
            }

            return borderSegment;
        }
        private void ReleaseResizeBorder(FrameworkElement borderSegment)
        {
            if (borderSegment != null)
            {
                borderSegment.MouseLeftButtonDown -= sizingBorderMouseLeftButtonDown;
                borderSegment.MouseLeftButtonUp -= sizingBorderMouseLeftButtonUp;
                borderSegment.MouseMove -= sizingBorderMouseMove;
            }
        }

        #endregion (Resizer utility)

        #region Frame styling

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState != WindowState.Minimized) UpdateFrameStyle(LayoutName);
            base.OnStateChanged(e);
        }

        public override void OnApplyTemplate()
        {
            _frame = (Control)Template.FindName("PART_CustomFrame", this);
            UpdateFrameStyle(LayoutName);
            base.OnApplyTemplate();
        }

        private void UpdateFrameStyle(string styleName)
        {
            if (_frame != null)
            {
                string format = "{0}{1}{2}.xaml";
                string strFilePath = "/CustomControls;component/Themes/Skins/";
                string strFrameStyle = styleName + "/resFrameStyle";

                if (WindowState == WindowState.Normal)
                {
                    UpdateFrameAppearance(String.Format(format, strFilePath, strFrameStyle, String.Empty));
                }
                else
                {
                    try
                    {
                        UpdateFrameAppearance(String.Format(format, strFilePath, strFrameStyle, "Max"));
                    }
                    catch (System.IO.IOException)
                    {
                        UpdateFrameAppearance(String.Format(format, strFilePath, strFrameStyle, String.Empty));
                    }
                }
                new Thread(() => { UpdateFrameBehaviors(); }).Start();
            }
        }
        private void UpdateFrameAppearance(string strResourceFile)
        {
            var loadedDictionary = Application.LoadComponent(new Uri(strResourceFile, UriKind.Relative)) as ResourceDictionary;

            if (_activeLayoutDictionary != null) Resources.MergedDictionaries.Remove(_activeLayoutDictionary);
            Resources.MergedDictionaries.Add(loadedDictionary);

            _activeLayoutDictionary = loadedDictionary;
        }
        private void UpdateFrameBehaviors()
        {
            // Attach event handlers for elements in new layout (CaptionBar dragging, sizing borders...)
            // (because we were called from a different thread, we must also marshall back to the UI thread)
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (ThreadStart)delegate
            {
                // TODO: Change the WindowFrame layout to setup visual tree in its right state.
                FrameworkElement titleBar = (FrameworkElement)_frame.Template.FindName("PART_TitleBar", _frame);
                if (titleBar != null)
                {
                    titleBar.MouseLeftButtonDown += titleBarMouseLeftButtonDown;
                    if (WindowState == WindowState.Maximized) titleBar.MouseMove += titleBarMouseMove;
                }

                if (WindowState == System.Windows.WindowState.Normal)
                {
                    foreach (KeyValuePair<FrameworkElement, ResizeOperation> item in _resizeBorders)
                    {
                        ReleaseResizeBorder(item.Key);
                    }

                    _resizeBorders.Clear();
                    FrameworkElement border;
                    if ((border = GetResizeBorder("PART_ResizeBorderLeft")) != null) _resizeBorders.Add(border, new ResizeOperation(ResizeLeft, GetOffsetLeft));
                    if ((border = GetResizeBorder("PART_ResizeBorderTopLeft")) != null) _resizeBorders.Add(border, new ResizeOperation(ResizeTopLeft, GetOffsetTopLeft));
                    if ((border = GetResizeBorder("PART_ResizeBorderTop")) != null) _resizeBorders.Add(border, new ResizeOperation(ResizeTop, GetOffsetTop));
                    if ((border = GetResizeBorder("PART_ResizeBorderTopRight")) != null) _resizeBorders.Add(border, new ResizeOperation(ResizeTopRight, GetOffsetTopRight));
                    if ((border = GetResizeBorder("PART_ResizeBorderRight")) != null) _resizeBorders.Add(border, new ResizeOperation(ResizeRight, GetOffsetRight));
                    if ((border = GetResizeBorder("PART_ResizeBorderBottomRight")) != null) _resizeBorders.Add(border, new ResizeOperation(ResizeBottomRight, GetOffsetBottomRight));
                    if ((border = GetResizeBorder("PART_ResizeBorderBottom")) != null) _resizeBorders.Add(border, new ResizeOperation(ResizeBottom, GetOffsetBottom));
                    if ((border = GetResizeBorder("PART_ResizeBorderBottomLeft")) != null) _resizeBorders.Add(border, new ResizeOperation(ResizeBottomLeft, GetOffsetBottomLeft));
                }
            });
        }

        #endregion (Frame styling)

        #region Behaviors

        #region Sizing handlers

        #region WinAPI interop

        internal struct POINT
        {
            public int x;
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential)]
        internal class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public static readonly RECT Empty = new RECT();

            public int Width
            {
                get { return Math.Abs(right - left); }
            }
            public int Height
            {
                get { return bottom - top; }
            }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }
            public override string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:/* WM_GETMINMAXINFO */
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }
        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            uint MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        private static void OnLoadedWinApiInteropInitialization(object sender, RoutedEventArgs e)
        {
            IntPtr handle = (new System.Windows.Interop.WindowInteropHelper(sender as Window)).Handle;
            System.Windows.Interop.HwndSource.FromHwnd(handle).AddHook(new System.Windows.Interop.HwndSourceHook(WindowProc));
        }

        #endregion (WinAPI interop)

        private void sizingBorderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Normal && CanResize)
            {
                FrameworkElement borderSegment = (FrameworkElement)sender;

                ResizeOperation op;
                if (_resizeBorders.TryGetValue(borderSegment, out op))
                {
                    _resizeAnchor = new ResizeAnchor(this, e.GetPosition(this), op);
                    borderSegment.CaptureMouse();
                }
            }
        }
        private static void sizingBorderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement borderSegment = (FrameworkElement)sender;
            borderSegment.ReleaseMouseCapture();
        }
        private void sizingBorderMouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement borderSegment = (FrameworkElement)sender;
            if (borderSegment.IsMouseCaptured)
            {
                _resizeAnchor.Resizer(this, e.GetPosition(this), _resizeAnchor.Anchor);
            }
        }

        #region Resize binders
        private static Point GetOffsetLeft(Window owner, Point mousePosition)
        {
            return new Point() { X = mousePosition.X };
        }

        private static Point GetOffsetTopLeft(Window owner, Point mousePosition)
        {
            return new Point() { X = mousePosition.X, Y = mousePosition.Y };
        }

        private static Point GetOffsetTop(Window owner, Point mousePosition)
        {
            return new Point() { Y = mousePosition.Y };
        }

        private static Point GetOffsetTopRight(Window owner, Point mousePosition)
        {
            return new Point() { X = owner.Width - mousePosition.X, Y = mousePosition.Y };
        }

        private static Point GetOffsetRight(Window owner, Point mousePosition)
        {
            return new Point() { X = owner.Width - mousePosition.X };
        }

        private static Point GetOffsetBottomRight(Window owner, Point mousePosition)
        {
            return new Point() { X = owner.Width - mousePosition.X, Y = owner.Height - mousePosition.Y };
        }

        private static Point GetOffsetBottom(Window owner, Point mousePosition)
        {
            return new Point() { Y = owner.Height - mousePosition.Y };
        }

        private static Point GetOffsetBottomLeft(Window owner, Point mousePosition)
        {
            return new Point() { X = mousePosition.X, Y = owner.Height - mousePosition.Y };
        }
        #endregion (Resize binders)

        #region Resizers

        private static void ResizeLeft(Window owner, Point mousePosition, Point mouseOffset)
        {
            double offset = mousePosition.X - mouseOffset.X;
            double horizontalPreview = owner.Width - offset;

            if (horizontalPreview > owner.MinWidth && horizontalPreview < owner.MaxWidth)
            {
                owner.Left += offset;
                owner.Width -= offset;
            }
            else if (horizontalPreview < owner.MinWidth)
            {
                owner.Left += (owner.Width - owner.MinWidth);
                owner.Width = owner.MinWidth;
            }
            else if (horizontalPreview > owner.MaxWidth)
            {
                owner.Left -= (owner.Width - owner.MaxWidth);
                owner.Width = owner.MaxWidth;
            }
        }

        private static void ResizeTopLeft(Window owner, Point mousePosition, Point mouseOffset)
        {
            ResizeLeft(owner, mousePosition, mouseOffset);
            ResizeTop(owner, mousePosition, mouseOffset);
        }

        private static void ResizeTop(Window owner, Point mousePosition, Point mouseOffset)
        {
            double offset = mousePosition.Y - mouseOffset.Y;
            double verticalPreview = owner.Height - offset;

            if (verticalPreview > owner.MinHeight && verticalPreview < owner.MaxHeight)
            {
                owner.Top += offset;
                owner.Height -= offset;
            }
            else if (verticalPreview < owner.MinHeight)
            {
                owner.Top += (owner.Height - owner.MinHeight);
                owner.Height = owner.MinHeight;
            }
            else if (verticalPreview > owner.MaxHeight)
            {
                owner.Left -= (owner.Height - owner.MaxHeight);
                owner.Height = owner.MaxHeight;
            }
        }

        private static void ResizeTopRight(Window owner, Point mousePosition, Point mouseOffset)
        {
            ResizeRight(owner, mousePosition, mouseOffset);
            ResizeTop(owner, mousePosition, mouseOffset);
        }

        private static void ResizeRight(Window owner, Point mousePosition, Point mouseOffset)
        {
            double horizontalPreview = mousePosition.X + mouseOffset.X;
            if (horizontalPreview > owner.MinWidth && horizontalPreview < owner.MaxWidth)
            {
                owner.Width = horizontalPreview;
            }
            else if (horizontalPreview <= owner.MinWidth)
            {
                owner.Width = owner.MinWidth;
            }
            else if (horizontalPreview >= owner.MaxWidth)
            {
                owner.Width = owner.MaxWidth;
            }
        }

        private static void ResizeBottomRight(Window owner, Point mousePosition, Point mouseOffset)
        {
            ResizeRight(owner, mousePosition, mouseOffset);
            ResizeBottom(owner, mousePosition, mouseOffset);
        }

        private static void ResizeBottom(Window owner, Point mousePosition, Point mouseOffset)
        {
            double verticalPreview = mousePosition.Y + mouseOffset.Y;
            if (verticalPreview > owner.MinHeight && verticalPreview < owner.MaxHeight)
            {
                owner.Height = mousePosition.Y + mouseOffset.Y;
            }
            else if (verticalPreview <= owner.MinHeight)
            {
                owner.Height = owner.MinHeight;
            }
            else if (verticalPreview >= owner.MaxHeight)
            {
                owner.Height = owner.MaxHeight;
            }
        }

        private static void ResizeBottomLeft(Window owner, Point mousePosition, Point mouseOffset)
        {
            ResizeLeft(owner, mousePosition, mouseOffset);
            ResizeBottom(owner, mousePosition, mouseOffset);
        }

        #endregion (Resizers)

        #endregion (Sizing handlers)

        #region Moving handlers

        private void titleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                if (CanResize) WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
            else
            {
                if (WindowState != WindowState.Maximized) DragMove();
            }
        }
        private void titleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed 
                && WindowState == WindowState.Maximized)
            {
                if (!(sender as FrameworkElement).IsMouseDirectlyOver) return;

                Rect _restoreBounds = new Rect(e.GetPosition(this), RestoreBounds.Size);
                double restoreOffsetX = _restoreBounds.X / Width;
                double restoreOffsetY = _restoreBounds.Y / Height;

                if (restoreOffsetX >= .45 && restoreOffsetX <= .55)
                {
                    restoreOffsetX = _restoreBounds.Width * .5;
                }
                else
                {
                    double tempOffsetX = _restoreBounds.X;
                    if (restoreOffsetX < .45)
                    {
                        restoreOffsetX = Math.Min(_restoreBounds.Width * .5, tempOffsetX);
                    }
                    else
                    {
                        restoreOffsetX = Math.Max(_restoreBounds.Width * .5, _restoreBounds.Width - ActualWidth + tempOffsetX);
                    }
                }

                if (restoreOffsetY >= .45 && restoreOffsetY <= .55)
                {
                    restoreOffsetY = _restoreBounds.Height * .5;
                }
                else
                {
                    double tempOffsetY = _restoreBounds.Y;
                    if (restoreOffsetY < .45)
                    {
                        restoreOffsetY = Math.Min(_restoreBounds.Height * .5, tempOffsetY);
                    }
                    else
                    {
                        restoreOffsetY = Math.Max(_restoreBounds.Height * .5, _restoreBounds.Height - ActualHeight + tempOffsetY);
                    }
                }
                _restoreBounds.Offset(-restoreOffsetX, -restoreOffsetY);
                
                WindowState = WindowState.Normal;
                Top = _restoreBounds.Top;
                Left = _restoreBounds.Left;
                Width = _restoreBounds.Width;
                Height = _restoreBounds.Height;

                DragMove();
            }
        }

        #endregion (Moving handlers)

        #endregion (Behaviors)

        #endregion (Methods)

        #region Constructors

        static CustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));

            EventManager.RegisterClassHandler(typeof(CustomWindow), CustomWindow.LoadedEvent, new RoutedEventHandler(OnLoadedWinApiInteropInitialization));

            CommandManager.RegisterClassCommandBinding(typeof(CustomWindow), new CommandBinding(SystemCommands.MinimizeWindowCommand, OnFrameCommand, OnFrameCommandCanExcecute));
            CommandManager.RegisterClassCommandBinding(typeof(CustomWindow), new CommandBinding(SystemCommands.ToggleMaximizeWindowCommand, OnFrameCommand, OnFrameCommandCanExcecute));
            CommandManager.RegisterClassCommandBinding(typeof(CustomWindow), new CommandBinding(SystemCommands.CloseWindowCommand, OnFrameCommand, OnFrameCommandCanExcecute));
        }

        #endregion (Constructors)
    }
}
