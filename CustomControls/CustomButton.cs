using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;


namespace CustomControls
{
    public class BrightnessToColorConverter : IValueConverter
    {
        #region Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte brightness = (byte)value;

            return Color.FromArgb(brightness, 255, 255, 255);
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class ColorToAlphaColorConverter : IValueConverter
    {
        #region Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush brush = (SolidColorBrush)value;

            if (brush != null)
            {
                Color color = brush.Color;
                color.A = byte.Parse(parameter.ToString());

                return color;
            }

            return Colors.Black; // make error obvious
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class HighlightCornerRadiusConverter : IValueConverter
    {
        #region Implementation

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CornerRadius corners = (CornerRadius)value;

            if (corners != null)
            {
                corners.BottomLeft = 0;
                corners.BottomRight = 0;
                return corners;
            }

            return null;
        }


        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }

    public class CustomButton : ToggleButton
    {
        public enum Highlight { Elliptical, Diffuse }

        #region Fields

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(typeof(CustomButton));

        public static readonly DependencyProperty OuterBorderBrushProperty =
            DependencyProperty.Register("OuterBorderBrush", typeof(Brush), typeof(CustomButton));

        public static readonly DependencyProperty OuterBorderThicknessProperty =
            DependencyProperty.Register("OuterBorderThickness", typeof(Thickness), typeof(CustomButton));

        public static readonly DependencyProperty InnerBorderBrushProperty =
            DependencyProperty.Register("InnerBorderBrush", typeof(Brush), typeof(CustomButton));

        public static readonly DependencyProperty InnerBorderThicknessProperty =
            DependencyProperty.Register("InnerBorderThickness", typeof(Thickness), typeof(CustomButton));

        public static readonly DependencyProperty GlowColorProperty =
            DependencyProperty.Register("GlowColor", typeof(SolidColorBrush), typeof(CustomButton));

        public static readonly DependencyProperty HighlightAppearanceProperty =
            DependencyProperty.Register("HighlightAppearance", typeof(ControlTemplate), typeof(CustomButton));

        public static readonly DependencyProperty HighlightMarginProperty =
            DependencyProperty.Register("HighlightMargin", typeof(Thickness), typeof(CustomButton));

        public static readonly DependencyProperty HighlightBrightnessProperty =
            DependencyProperty.Register("HighlightBrightness", typeof(byte), typeof(CustomButton));

        public static readonly DependencyProperty HighlightStyleProperty =
            DependencyProperty.Register("HighlightStyle", typeof(Highlight), typeof(CustomButton),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHighlightStyleChanged)));

        #endregion (Fields)

        #region Properties

        public Brush GlowColor
        {
            get { return (SolidColorBrush)GetValue(GlowColorProperty); }
            set { SetValue(GlowColorProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public Brush OuterBorderBrush
        {
            get { return (Brush)GetValue(OuterBorderBrushProperty); }
            set { SetValue(OuterBorderBrushProperty, value); }
        }

        public Thickness OuterBorderThickness
        {
            get { return (Thickness)GetValue(OuterBorderThicknessProperty); }
            set { SetValue(OuterBorderThicknessProperty, value); }
        }

        public Brush InnerBorderBrush
        {
            get { return (Brush)GetValue(InnerBorderBrushProperty); }
            set { SetValue(InnerBorderBrushProperty, value); }
        }

        public Thickness InnerBorderThickness
        {
            get { return (Thickness)GetValue(InnerBorderThicknessProperty); }
            set { SetValue(InnerBorderThicknessProperty, value); }
        }

        // Force clients to pass enum value to HighlightStyle by hiding this accessor
        public ControlTemplate HighlightAppearance
        {
            get { return (ControlTemplate)GetValue(HighlightAppearanceProperty); }
            set { SetValue(HighlightAppearanceProperty, value); }
        }

        public Thickness HighlightMargin
        {
            get { return (Thickness)GetValue(HighlightMarginProperty); }
            set { SetValue(HighlightMarginProperty, value); }
        }

        public byte HighlightBrightness
        {
            get { return (byte)GetValue(HighlightBrightnessProperty); }
            set { SetValue(HighlightBrightnessProperty, value); }
        }

        public Highlight HighlightStyle
        {
            get { return (Highlight)GetValue(HighlightStyleProperty); }
            set { SetValue(HighlightStyleProperty, value); }
        }

        #endregion (Properties)

        #region Constructors

        static CustomButton()
        {
            // Override defautl layout and behavior with template definitions located in Themes\Generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(typeof(CustomButton)));
        }

        #endregion (Constructors)

        #region Event handlers

        private static void OnHighlightStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomButton btn = (CustomButton)d;

            Highlight highlight = (Highlight)e.NewValue;

            // Assign style associated with user-selected enum value
            btn.Style = (Style)btn.TryFindResource(new ComponentResourceKey(btn.GetType(), highlight.ToString()));
        }

        #endregion (Event handlers)
    }
}
