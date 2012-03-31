using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


namespace CustomControls
{
    public class CustomSystemButton : ToggleButton
    {
        public enum Highlight { Elliptical, Diffuse }

        #region Fields

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(typeof(CustomSystemButton));

        public static readonly DependencyProperty OuterBorderBrushProperty =
            DependencyProperty.Register("OuterBorderBrush", typeof(Brush), typeof(CustomSystemButton));

        public static readonly DependencyProperty OuterBorderThicknessProperty =
            DependencyProperty.Register("OuterBorderThickness", typeof(Thickness), typeof(CustomSystemButton));

        public static readonly DependencyProperty InnerBorderBrushProperty =
            DependencyProperty.Register("InnerBorderBrush", typeof(Brush), typeof(CustomSystemButton));

        public static readonly DependencyProperty InnerBorderThicknessProperty =
            DependencyProperty.Register("InnerBorderThickness", typeof(Thickness), typeof(CustomSystemButton));

        public static readonly DependencyProperty GlowColorProperty =
            DependencyProperty.Register("GlowColor", typeof(SolidColorBrush), typeof(CustomSystemButton));

        public static readonly DependencyProperty HighlightAppearanceProperty =
            DependencyProperty.Register("HighlightAppearance", typeof(ControlTemplate), typeof(CustomSystemButton));

        public static readonly DependencyProperty HighlightMarginProperty =
            DependencyProperty.Register("HighlightMargin", typeof(Thickness), typeof(CustomSystemButton));

        public static readonly DependencyProperty HighlightBrightnessProperty =
            DependencyProperty.Register("HighlightBrightness", typeof(byte), typeof(CustomSystemButton));

        public static readonly DependencyProperty HighlightStyleProperty =
            DependencyProperty.Register("HighlightStyle", typeof(Highlight), typeof(CustomSystemButton),
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

        static CustomSystemButton()
        {
            // Override defautl layout and behavior with template definitions located in Themes\Generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomSystemButton), new FrameworkPropertyMetadata(typeof(CustomSystemButton)));
        }

        #endregion (Constructors)

        #region Event handlers

        private static void OnHighlightStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomSystemButton btn = (CustomSystemButton)d;

            Highlight highlight = (Highlight)e.NewValue;

            // Assign style associated with user-selected enum value
            btn.Style = (Style)btn.TryFindResource(new ComponentResourceKey(btn.GetType(), highlight.ToString()));
        }

        #endregion (Event handlers)
    }
}
