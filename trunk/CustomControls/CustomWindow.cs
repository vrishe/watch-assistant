﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace CustomControls
{
    public class CustomWindow : Window
    {
        #region External calls
        [StructLayout(LayoutKind.Sequential)]
        private struct Rect32
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr MonitorFromRect(ref Rect32 wndRect, uint flags);

        private struct MonitorInfoEx
        {
            public int cbSize;
            public Rect32 rcMonitor;
            public Rect32 rcWork;
            public uint dwFlags;           
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string szDevice;
        }
        [DllImport("user32.dll", EntryPoint="GetMonitorInfo", SetLastError=true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx monitorInfo);
        #endregion (External calls)

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

        private Control _frame;

        private readonly Dictionary<FrameworkElement, ResizeOperation> _resizeBorders = new Dictionary<FrameworkElement,ResizeOperation>();
        private ResizeAnchor _resizeAnchor;

        private Rect _restoreBounds;
        private Size _restoreMaxBounds;

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

        #endregion (Properties)

        #region Methods

        private void OnFrameCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == CustomWindowCommands.Minimize)
            {
                WindowState = WindowState.Minimized;
            } 
            else if (e.Command == CustomWindowCommands.ToggleMaximizeNormal)
            {
                //WindowState = 
                //    WindowState == WindowState.Maximized ? 
                //        WindowState.Normal : WindowState.Maximized;
                switch (WindowState)
                {
                    case WindowState.Normal:
                        Size maximizedBounds = GetMaximizedWindowWorkArea(this);
                        _restoreMaxBounds = new Size(MaxWidth, MaxHeight);
                        MaxWidth = maximizedBounds.Width;
                        MaxHeight = maximizedBounds.Height;
                        WindowState = WindowState.Maximized;
                        break;

                    case WindowState.Maximized:
                        MaxWidth = _restoreMaxBounds.Width;
                        MaxHeight = _restoreMaxBounds.Height;
                        WindowState = WindowState.Normal;
                        break;
                }
            }
            else if (e.Command == ApplicationCommands.Close)
            {
                this.Close();
            }
        }

        #region Frame styling

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            CommandBindings.Add(new CommandBinding(CustomWindowCommands.Minimize, OnFrameCommand));
            CommandBindings.Add(new CommandBinding(CustomWindowCommands.ToggleMaximizeNormal, OnFrameCommand));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnFrameCommand));

            _frame = (Control)Template.FindName("PART_CustomFrame", this);
            UpdateFrameStyle(LayoutName);
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
        private void UpdateFrameBehaviors()
        {
            // Attach event handlers for elements in new layout (CaptionBar dragging, sizing borders...)
            // (because we were called from a different thread, we must also marshall back to the UI thread)
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (ThreadStart)delegate
            {
                FrameworkElement titleBar = (FrameworkElement)_frame.Template.FindName("PART_TitleBar", _frame);
                if (titleBar != null)
                {
                    titleBar.MouseLeftButtonDown += titleBarMouseLeftButtonDown;
                    if (WindowState == WindowState.Maximized)
                    {
                        titleBar.MouseMove += titleBarMouseMove;
                        titleBar.MouseLeftButtonUp += titleBarMouseLeftButtonUp;
                    }
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

        #region Utility

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

        #endregion (Utility)

        #endregion (Frame styling)

        #region Behaviors 

        #region Moving handlers

        private void titleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
            else
            {
                if (WindowState != WindowState.Maximized && (sender as FrameworkElement).IsMouseDirectlyOver) DragMove();
            }
        }

        private void titleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed 
                && WindowState == WindowState.Maximized)
            {
                if (!(sender as FrameworkElement).IsMouseDirectlyOver)
                {
                    (e.Source as FrameworkElement).CaptureMouse();
                    return;
                }

                _restoreBounds = new Rect(e.GetPosition(this), _restoreBounds.Size);
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

                DragMove();
            }
        }

        private void titleBarMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (e.OriginalSource as FrameworkElement).ReleaseMouseCapture();
        }

        #endregion (Moving handlers)

        #region Sizing handlers

        private static Size GetMaximizedWindowWorkArea(Window window)
        {
            Rect32 rect = new Rect32();
            rect.Left = (int)window.Left;
            rect.Top = (int)window.Top;
            rect.Right = (int)(window.Left + window.ActualWidth);
            rect.Bottom = (int)(window.Top + window.ActualHeight);
            IntPtr monitor = MonitorFromRect(ref rect, 0x00000002);
            MonitorInfoEx monInfo = new MonitorInfoEx(); monInfo.cbSize = 104;
            if (GetMonitorInfo(monitor, ref monInfo)) 
                return new Size(monInfo.rcWork.Right - monInfo.rcWork.Left, monInfo.rcWork.Bottom - monInfo.rcWork.Top);
            return new Size(SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    _restoreBounds = RestoreBounds;
                    break;

                case WindowState.Normal:
                    Top = _restoreBounds.Top;
                    Left = _restoreBounds.Left;
                    Width = _restoreBounds.Width;
                    Height = _restoreBounds.Height;
                    break;
            }
            UpdateFrameStyle(LayoutName);
            base.OnStateChanged(e);
        }

        private void sizingBorderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Normal)
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

        #endregion (Behaviors)

        #endregion (Methods)

        #region Constructors

        static CustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));
        }

        #endregion (Constructors)
    }
}
