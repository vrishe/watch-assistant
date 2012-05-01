using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Media.Imaging;
using CustomControls;
using System.Threading;

namespace watch_assistant.View.DetailsWindow
{
    /// <summary>
    /// Логика взаимодействия для MagnifiedImageWidget.xaml
    /// </summary>
    public partial class MagnifiedImageWidget : CustomWindow
    {
        private double _ratio;
        private BitmapImage _bmp;

        public BitmapImage Bitmap { get { return _bmp; } }

        public MagnifiedImageWidget(BitmapImage bmp)
        {
            _bmp = bmp;
            
            Width = MaxWidth = _bmp.Width;
            Height = MaxHeight = _bmp.Height;

            _ratio = Height / Width;

            InitializeComponent();
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            double zoom = 1.0 + Math.Sign(e.Delta) * 0.03;

            double height = Math.Min(Height * zoom, MaxHeight);
            Width = Math.Max(height / _ratio, MinWidth);
            Height = Width * _ratio;

            base.OnMouseWheel(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            Close();
        }
        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            Close();
        }
    }
}
