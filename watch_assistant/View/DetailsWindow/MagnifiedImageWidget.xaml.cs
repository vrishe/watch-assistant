using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Media.Imaging;
using CustomControls;
using System.Threading;
using System.Windows.Media;

namespace watch_assistant.View.DetailsWindow
{
    /// <summary>
    /// Логика взаимодействия для MagnifiedImageWidget.xaml
    /// </summary>
    public partial class MagnifiedImageWidget : CustomWindow
    {
        private double _scale;
        private double _maxScale;
        private double _minScale;

        private Point _mousePosition;

        private BitmapImage _bmp;

        public BitmapImage Bitmap { get { return _bmp; } }

        public MagnifiedImageWidget(BitmapImage bmp)
        {
            VisualBitmapScalingMode = BitmapScalingMode.Fant;

            _bmp = bmp;

            _scale = 1.0;
            _maxScale = MaxWidth / _bmp.Width; if (Double.IsInfinity(_maxScale)) _maxScale = 1.0;
            _minScale = MinWidth / _bmp.Width; if (_minScale == .0) _minScale = .3;

            InitializeComponent();
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            double zoom = Math.Sign(e.Delta) * 0.03;

            _scale = Math.Max(Math.Min(_scale + zoom, _maxScale), _minScale);

            if (_scale != _minScale && _scale != _maxScale)
            {
                VisualTransform = new ScaleTransform(_scale, _scale, _mousePosition.X, _mousePosition.Y);
            }
            else
            {
                _mousePosition = e.GetPosition(this);
            }

            base.OnMouseWheel(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            Close();
        }
        protected override void OnMouseRightButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            Close();
        }
    }
}
