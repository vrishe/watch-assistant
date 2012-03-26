using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows;
using System.Threading;
using System.Windows.Shapes;

namespace watch_assistant.Themes
{
    class WindowResizer
    {
        public enum ElementResizeDirection
        {
            NS_VERTICAL,
            SN_VERTICAL,
            WE_HORIZONTAL,
            EW_HORIZONTAL,
            NWSE_DIAGONAL,
            SENW_DIAGONAL,
            NESW_DIAGONAL,
            SWNE_DIAGONAL
        }

        [Flags]
        private enum ResizeSide
        {
            NOTHING = 0,
            LEFT = 0x01,
            TOP = 0x02,
            RIGHT = 0x04,
            BOTTOM = 0x08
        }

        #region External call
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out PointAPI lpPoint);

        private struct PointAPI
        {
            public int X;
            public int Y;
        }
        #endregion //External call

        #region Fields

        private readonly Window _owner;

        private List<UIElement> leftElements = new List<UIElement>();
        private List<UIElement> rightElements = new List<UIElement>();
        private List<UIElement> topElements = new List<UIElement>();
        private List<UIElement> bottomElements = new List<UIElement>();

        private ResizeSide resizeSide = ResizeSide.NOTHING;
        private PointAPI resizePoint = new PointAPI();
        private Size resizeSize = new Size();
        private Point resizeWindowPoint = new Point();

        private delegate void RefreshDelegate();

        #endregion // Fields

        #region Methods

        ResizeSide SpotResizeSide(UIElement resizer)
        {
            ResizeSide result = ResizeSide.NOTHING;

            if (leftElements.Contains(resizer)) result |= ResizeSide.LEFT;
            if (rightElements.Contains(resizer)) result |= ResizeSide.RIGHT;
            if (topElements.Contains(resizer)) result |= ResizeSide.TOP;
            if (bottomElements.Contains(resizer)) result |= ResizeSide.BOTTOM;

            return result;
        }

        #region Resize components management
        private void connectMouseHandlers(UIElement element)
        {
            if (element == null) return;

            element.MouseLeftButtonDown += new MouseButtonEventHandler(element_MouseLeftButtonDown);
            element.MouseLeftButtonUp += new MouseButtonEventHandler((s, e) => { ((UIElement)s).ReleaseMouseCapture(); });
            element.MouseEnter += new MouseEventHandler(element_MouseEnter);
            element.MouseLeave += new MouseEventHandler((s, e) => { _owner.Cursor = Cursors.Arrow; });
        }

        public void AddResizer(string resizingElementName, ElementResizeDirection resizeDirection)
        {
            Rectangle element = (Rectangle)_owner.Template.FindName(resizingElementName, _owner);

            if (element == null) return;

            switch (resizeDirection)
            {
                case ElementResizeDirection.EW_HORIZONTAL:
                    if (!rightElements.Contains(element)) rightElements.Add(element);
                    break;

                case ElementResizeDirection.WE_HORIZONTAL:
                    if (!leftElements.Contains(element)) leftElements.Add(element);
                    break;

                case ElementResizeDirection.NS_VERTICAL:
                    if (!topElements.Contains(element)) topElements.Add(element);
                    break;

                case ElementResizeDirection.SN_VERTICAL:
                    if (!bottomElements.Contains(element)) bottomElements.Add(element);
                    break;

                case ElementResizeDirection.NWSE_DIAGONAL:
                    if (!leftElements.Contains(element)) leftElements.Add(element);
                    if (!topElements.Contains(element)) topElements.Add(element);
                    break;

                case ElementResizeDirection.SENW_DIAGONAL:
                    if (!rightElements.Contains(element)) rightElements.Add(element);
                    if (!bottomElements.Contains(element)) bottomElements.Add(element);
                    break;

                case ElementResizeDirection.NESW_DIAGONAL:
                    if (!rightElements.Contains(element)) rightElements.Add(element);
                    if (!topElements.Contains(element)) topElements.Add(element);
                    break;

                case ElementResizeDirection.SWNE_DIAGONAL:
                    if (!leftElements.Contains(element)) leftElements.Add(element);
                    if (!bottomElements.Contains(element)) bottomElements.Add(element);
                    break;
            }

            connectMouseHandlers(element);
        }
        #endregion // Resize components management

        #region Resize handlers
        private void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).CaptureMouse();

            GetCursorPos(out resizePoint);
            resizeSize = new Size(_owner.Width, _owner.Height);
            resizeWindowPoint = new Point(_owner.Left, _owner.Top);

            resizeSide = SpotResizeSide((UIElement)sender);

            Thread t = new Thread(new ThreadStart(updateSizeLoop));
            t.Name = "Mouse Position Poll Thread";
            t.Start();
        }

        private void updateSizeLoop()
        {
            try
            {
                while (resizeSide != ResizeSide.NOTHING)
                {
                    _owner.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new RefreshDelegate(updateSize));
                    _owner.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new RefreshDelegate(updateMouseDown));
                }
            }
            catch (Exception)
            {
            }
        }

        #region Updaters
        private void updateSize()
        {
            PointAPI p = new PointAPI();
            GetCursorPos(out p);

            if (resizeSide.HasFlag(ResizeSide.RIGHT))
            {
                _owner.Width = Math.Max(0, this.resizeSize.Width - (resizePoint.X - p.X));
            }

            if (resizeSide.HasFlag(ResizeSide.BOTTOM))
            {
                _owner.Height = Math.Max(0, resizeSize.Height - (resizePoint.Y - p.Y));
            }

            if (resizeSide.HasFlag(ResizeSide.LEFT))
            {
                _owner.Width = Math.Max(0, resizeSize.Width + (resizePoint.X - p.X));
                _owner.Left = Math.Max(0, resizeWindowPoint.X - (resizePoint.X - p.X));
            }

            if (resizeSide.HasFlag(ResizeSide.TOP))
            {
                _owner.Height = Math.Max(0, resizeSize.Height + (resizePoint.Y - p.Y));
                _owner.Top = Math.Max(0, resizeWindowPoint.Y - (resizePoint.Y - p.Y));
            }
        }

        private void updateMouseDown()
        {
            if (Mouse.LeftButton == MouseButtonState.Released) resizeSide = ResizeSide.NOTHING;
        }
        #endregion // Updaters
        #endregion  // Resize handlers

        #region Cursor updaters
        private void element_MouseEnter(object sender, MouseEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            if (window != null && window.WindowState == WindowState.Maximized) return;

            resizeSide = SpotResizeSide((UIElement)sender);

            if (resizeSide.HasFlag(ResizeSide.LEFT | ResizeSide.BOTTOM) 
                || resizeSide.HasFlag(ResizeSide.RIGHT | ResizeSide.TOP))
            {
                _owner.Cursor = Cursors.SizeNESW;
                return;
            }
            if (resizeSide.HasFlag(ResizeSide.RIGHT | ResizeSide.BOTTOM) 
                || resizeSide.HasFlag(ResizeSide.LEFT | ResizeSide.TOP))
            {
                _owner.Cursor = Cursors.SizeNWSE;
                return;
            }
            if (resizeSide.HasFlag(ResizeSide.LEFT) || resizeSide.HasFlag(ResizeSide.RIGHT))
            {
                _owner.Cursor = Cursors.SizeWE;
                return;
            }
            if (resizeSide.HasFlag(ResizeSide.BOTTOM) || resizeSide.HasFlag(ResizeSide.TOP))
            {
                _owner.Cursor = Cursors.SizeNS;
                return;
            }
        }
        #endregion // Cursor updaters

        #endregion // Methods

        #region Constructors

        public WindowResizer(Window owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
        }

        #endregion
    }
}
