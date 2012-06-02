using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CustomControls;
using System.Timers;

namespace watch_assistant.View.DetailsWindow
{
    /// <summary>
    /// Логика взаимодействия для MagnifiedImageWidget.xaml
    /// </summary>
    public partial class MagnifiedImageWidget : CustomWindow
    {
        #region Fields

        private BitmapImage _bmp;

        private double _scale;
        private double _maxScale;
        private double _minScale;

        private Point? _mousePosition;

        #endregion (Fields)

        #region Properties

        public BitmapImage Bitmap { get { return _bmp; } }

        #endregion (Properties)

        #region Methods

        public MagnifiedImageWidget(BitmapImage bmp)
        {
            VisualBitmapScalingMode = BitmapScalingMode.Fant;

            _bmp = bmp;
            _mousePosition = null;

            _scale = 1.0;
            _maxScale = MaxWidth / _bmp.Width; if (Double.IsInfinity(_maxScale)) _maxScale = 1.0;
            _minScale = MinWidth / _bmp.Width; if (_minScale == .0) _minScale = .3;

            InitializeComponent();
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            double zoom = Math.Sign(e.Delta) * 0.03;

            _scale = Math.Max(Math.Min(_scale + zoom, _maxScale), _minScale);

            var lastScaleState = VisualTransform as ScaleTransform;
            if (lastScaleState == null || _scale != lastScaleState.ScaleX)
            {
                if (_mousePosition == null) _mousePosition = e.GetPosition(this); 
                VisualTransform = new ScaleTransform(_scale, _scale, _mousePosition.Value.X, _mousePosition.Value.Y);
            }

            if (_scale <= _minScale || _scale >= _maxScale) _mousePosition = null;

            base.OnMouseWheel(e);
        }

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            Close();
        }

        #endregion (Methods)
    }
}
