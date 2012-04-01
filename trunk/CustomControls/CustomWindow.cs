﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

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

        private Control _frame;

        private readonly Dictionary<FrameworkElement, ResizeOperation> _resizeBorders = new Dictionary<FrameworkElement,ResizeOperation>();
        private ResizeAnchor _resizeAnchor;


        // Using a DependencyProperty as the backing store for LayoutName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayoutNameProperty =
            DependencyProperty.Register("LayoutName", typeof(string), typeof(CustomWindow), new UIPropertyMetadata(
                    "Default", (s, e) => { ((CustomWindow)s).UpdateFrameStyle(e.NewValue as string); }
                )
            );

        #endregion (Fields)

        #region Properties

        public string LayoutName
        {
            get { return (string)GetValue(LayoutNameProperty); }
            set { SetValue(LayoutNameProperty, value); }
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
            UpdateFrameStyle(LayoutName);
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == WindowStateProperty) UpdateFrameStyle(LayoutName); return;
        }

        private void OnFrameCommand(object sender, ExecutedRoutedEventArgs args)
        {
            if (args.Command == CustomWindowCommands.Minimize)
            {
                WindowState = WindowState.Minimized;
            } 
            else if (args.Command == CustomWindowCommands.Maximize)
            {
                WindowState = 
                    WindowState == WindowState.Maximized ? 
                        WindowState.Normal : WindowState.Maximized;
            }
            else if (args.Command == ApplicationCommands.Close)
            {
                this.Close();
            }
        }

        #region Frame styling

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

                    // Make ASYNCHRONOUS call to method that can react to new style
                    // (new settings are NOT detected when you call the method directly)
                    new Thread(() => { UpdateFrameBehaviors(); }).Start();
                }

                else
                {
                    try
                    {
                        // Check for special frame style to support MAXIMIZED Window state
                        UpdateFrameAppearance(String.Format(format, strFilePath, strFrameStyle, "Max"));
                    }
                    catch (System.IO.IOException)
                    {
                        // Revert to standard frame style if no special style was defined
                        UpdateFrameAppearance(String.Format(format, strFilePath, strFrameStyle, String.Empty));
                    }
                }
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
                if (titleBar != null) titleBar.MouseLeftButtonDown += (s, e) => { if ((s as FrameworkElement).IsMouseDirectlyOver) DragMove(); };

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
            });
        }

        #endregion (Frame styling)

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

        #region Sizing handlers

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
        private  void sizingBorderMouseMove(object sender, MouseEventArgs e)
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
            double widthPreview = owner.Width - offset;

            if (widthPreview > owner.MinWidth && widthPreview < owner.MaxWidth)
            {
                owner.Left += offset;
                owner.Width -= offset;
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
            double heightPreview = owner.Height - offset;

            if (heightPreview > owner.MinHeight && heightPreview < owner.MaxHeight)
            {
                owner.Top += offset;
                owner.Height -= offset;
            }
        }

        private static void ResizeTopRight(Window owner, Point mousePosition, Point mouseOffset)
        {
            ResizeRight(owner, mousePosition, mouseOffset);
            ResizeTop(owner, mousePosition, mouseOffset);
        }

        private static void ResizeRight(Window owner, Point mousePosition, Point mouseOffset)
        {
            double widthPreview = mousePosition.X + mouseOffset.X;
            if ( widthPreview > owner.MinWidth && widthPreview < owner.MaxWidth) 
                owner.Width = widthPreview;
        }

        private static void ResizeBottomRight(Window owner, Point mousePosition, Point mouseOffset)
        {
            ResizeRight(owner, mousePosition, mouseOffset);
            ResizeBottom(owner, mousePosition, mouseOffset);
        }

        private static void ResizeBottom(Window owner, Point mousePosition, Point mouseOffset)
        {
            double heightPreview = mousePosition.Y + mouseOffset.Y;
            if (heightPreview > owner.MinHeight && heightPreview < owner.MaxHeight)
                owner.Height = mousePosition.Y + mouseOffset.Y;
        }

        private static void ResizeBottomLeft(Window owner, Point mousePosition, Point mouseOffset)
        {
            ResizeLeft(owner, mousePosition, mouseOffset);
            ResizeBottom(owner, mousePosition, mouseOffset);
        }

        #endregion (Resizers)

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
