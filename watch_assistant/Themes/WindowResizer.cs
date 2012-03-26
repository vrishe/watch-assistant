using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows;
using System.Threading;

namespace watch_assistant.Themes
{
    class WindowResizer
    {
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

        private Dictionary<UIElement, short> leftElements = new Dictionary<UIElement, short>();
        private Dictionary<UIElement, short> rightElements = new Dictionary<UIElement, short>();
        private Dictionary<UIElement, short> topElements = new Dictionary<UIElement, short>();
        private Dictionary<UIElement, short> bottomElements = new Dictionary<UIElement, short>();

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

            if (leftElements.ContainsKey(resizer)) result |= ResizeSide.LEFT;
            if (rightElements.ContainsKey(resizer)) result |= ResizeSide.RIGHT;
            if (topElements.ContainsKey(resizer)) result |= ResizeSide.TOP;
            if (bottomElements.ContainsKey(resizer)) result |= ResizeSide.BOTTOM;

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

        public void addResizerRight(UIElement element)
        {
            if (element == null) return;

            connectMouseHandlers(element);
            rightElements.Add(element, 0);
        }

        public void addResizerLeft(UIElement element)
        {
            if (element == null) return;

            connectMouseHandlers(element);
            leftElements.Add(element, 0);
        }

        public void addResizerUp(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            topElements.Add(element, 0);
        }

        public void addResizerDown(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            bottomElements.Add(element, 0);
        }

        public void addResizerRightDown(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            rightElements.Add(element, 0);
            bottomElements.Add(element, 0);
        }

        public void addResizerLeftDown(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            leftElements.Add(element, 0);
            bottomElements.Add(element, 0);
        }

        public void addResizerRightUp(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            rightElements.Add(element, 0);
            topElements.Add(element, 0);
        }

        public void addResizerLeftUp(UIElement element)
        {
            if (element == null)
                return;

            connectMouseHandlers(element);
            leftElements.Add(element, 0);
            topElements.Add(element, 0);
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

        void element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void updateSizeLoop()
        {
            try
            {
                while (resizeSide != ResizeSide.NOTHING)
                {
                    _owner.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new RefreshDelegate(updateSize));
                    _owner.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new RefreshDelegate(updateMouseDown));
                    //Thread.Sleep(10);
                }

                //_owner.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new RefreshDelegate(
                //    delegate() { if (resizeSide.HasFlag(ResizeSide.NOTHING)) _owner.Cursor = Cursors.Arrow; })
                //);
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

        //private void setWECursor(object sender, MouseEventArgs e)
        //{
        //    _owner.Cursor = Cursors.SizeWE;
        //}

        //private void setNSCursor(object sender, MouseEventArgs e)
        //{
        //    _owner.Cursor = Cursors.SizeNS;
        //}

        //private void setNESWCursor(object sender, MouseEventArgs e)
        //{
        //    _owner.Cursor = Cursors.SizeNESW;
        //}

        //private void setNWSECursor(object sender, MouseEventArgs e)
        //{
        //    _owner.Cursor = Cursors.SizeNWSE;
        //}

        //private void setArrowCursor(object sender, MouseEventArgs e)
        //{
        //    if (!resizeDown && !resizeLeft && !resizeRight && !resizeUp)
        //    {
        //        _owner.Cursor = Cursors.Arrow;
        //    }
        //}

        //private void setArrowCursor()
        //{
        //    _owner.Cursor = Cursors.Arrow;
        //}
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
