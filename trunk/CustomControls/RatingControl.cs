using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace CustomControls
{

    // TODO: RatingControl improvements pending...
    public class DoublesToRectConverter : IMultiValueConverter
    {
        #region IMultiValueConverter members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values) if (value == DependencyProperty.UnsetValue) return new Rect();
            if (values.Length == 2) return new Rect(0, 0, (double)(values[0]), (double)(values[1]));
            if (values.Length == 4) return new Rect((double)values[0], (double)values[1], (double)values[2], (double)values[3]);
            return new Rect();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("DoublesToRectConverter.ConvertBack");
        }

        #endregion (IMultiValueConverter members)
    }

    public enum SelectionDirection
    {
        FirstToLast,
        LastToFirst
    }
    internal enum PreviewState
    {
        PreviewHighlight,
        PreviewUnrated,
        PreviewPersonalRating,
        PreviewCommonRating,
    }

    //public class RatingItemData : DependencyObject
    //{
    //    #region Properties

    //    public Brush UnratedFillColor
    //    {
    //        get { return (Brush)GetValue(UnratedFillColorProperty); }
    //        set { SetValue(UnratedFillColorProperty, value); }
    //    }
    //    public static readonly DependencyProperty UnratedFillColorProperty =
    //        DependencyProperty.Register("UnratedFillColor", typeof(Brush), typeof(RatingItemData), new UIPropertyMetadata(new SolidColorBrush(Colors.Gray)));

    //    public Brush UnratedStrokeColor
    //    {
    //        get { return (Brush)GetValue(UnratedStrokeColorProperty); }
    //        set { SetValue(UnratedStrokeColorProperty, value); }
    //    }
    //    public static readonly DependencyProperty UnratedStrokeColorProperty =
    //        DependencyProperty.Register("UnratedStrokeColor", typeof(Brush), typeof(RatingItemData), new UIPropertyMetadata(new SolidColorBrush(Colors.LightGray)));

    //    public Brush CommonRatedFillColor
    //    {
    //        get { return (Brush)GetValue(CommonRatedFillColorProperty); }
    //        set { SetValue(CommonRatedFillColorProperty, value); }
    //    }
    //    public static readonly DependencyProperty CommonRatedFillColorProperty =
    //        DependencyProperty.Register("CommonRatedFillColor", typeof(Brush), typeof(RatingItemData), new UIPropertyMetadata(new SolidColorBrush(Colors.Goldenrod)));

    //    public Brush CommonRatedStrokeColor
    //    {
    //        get { return (Brush)GetValue(CommonRatedStrokeColorProperty); }
    //        set { SetValue(CommonRatedStrokeColorProperty, value); }
    //    }
    //    public static readonly DependencyProperty CommonRatedStrokeColorProperty =
    //        DependencyProperty.Register("CommonRatedStrokeColor", typeof(Brush), typeof(RatingItemData), new UIPropertyMetadata(new SolidColorBrush(Colors.Goldenrod) { Opacity = .5 }));

    //    public Brush PersonalRatedFillColor
    //    {
    //        get { return (Brush)GetValue(PersonalRatedFillColorProperty); }
    //        set { SetValue(PersonalRatedFillColorProperty, value); }
    //    }
    //    public static readonly DependencyProperty PersonalRatedFillColorProperty =
    //        DependencyProperty.Register("PersonalRatedFillColor", typeof(Brush), typeof(RatingItemData), new UIPropertyMetadata(new SolidColorBrush(Colors.Gold)));

    //    public Brush PersonalRatedStrokeColor
    //    {
    //        get { return (Brush)GetValue(PersonalRatedStrokeColorProperty); }
    //        set { SetValue(PersonalRatedStrokeColorProperty, value); }
    //    }
    //    public static readonly DependencyProperty PersonalRatedStrokeColorProperty =
    //        DependencyProperty.Register("PersonalRatedStrokeColor", typeof(Brush), typeof(RatingItemData), new UIPropertyMetadata(new SolidColorBrush(Colors.Goldenrod)));

    //    public Brush HighlitedFillColor
    //    {
    //        get { return (Brush)GetValue(HighlitedFillColorProperty); }
    //        set { SetValue(HighlitedFillColorProperty, value); }
    //    }
    //    public static readonly DependencyProperty HighlitedFillColorProperty =
    //        DependencyProperty.Register("HighlitedFillColor", typeof(Brush), typeof(RatingItemData), new UIPropertyMetadata(new SolidColorBrush(Colors.LightGoldenrodYellow)));

    //    public Brush HighlightedStrokeColor
    //    {
    //        get { return (Brush)GetValue(HighlightedStrokeColorProperty); }
    //        set { SetValue(HighlightedStrokeColorProperty, value); }
    //    }
    //    public static readonly DependencyProperty HighlightedStrokeColorProperty =
    //        DependencyProperty.Register("HighlightedStrokeColor", typeof(Brush), typeof(RatingItemData), new UIPropertyMetadata(new SolidColorBrush(Colors.Goldenrod)));

    //    public PathGeometry PathData
    //    {
    //        get { return (PathGeometry)GetValue(PathDataProperty); }
    //        set { SetValue(PathDataProperty, value); }
    //    }
    //    public static readonly DependencyProperty PathDataProperty =
    //        DependencyProperty.Register("PathData", typeof(PathGeometry), typeof(RatingItemData), new UIPropertyMetadata(new PathGeometry()));

    //    #endregion (Properties)
    //}

    public class RatingItem : Control
    {
        #region Properties

        internal double Fraction
        {
            get { return (double)GetValue(FractionProperty); }
            set { SetValue(FractionProperty, value); }
        }

        internal static readonly DependencyProperty FractionProperty =
            DependencyProperty.Register("Fraction", typeof(double), typeof(RatingItem), 
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceFractionValue));

        internal PreviewState PreviewState
        {
            get { return (PreviewState)GetValue(PreviewStateProperty); }
            set { SetValue(PreviewStateProperty, value); }
        }

        public static readonly DependencyProperty PreviewStateProperty =
            DependencyProperty.Register("PreviewState", typeof(PreviewState), typeof(RatingItem), 
            new FrameworkPropertyMetadata(PreviewState.PreviewUnrated, FrameworkPropertyMetadataOptions.AffectsRender, null, CoercePreviewStateValue));

        //public RatingItemData Data
        //{
        //    get { return (RatingItemData)GetValue(DataProperty); }
        //    set { SetValue(DataProperty, value); }
        //}
        //public static readonly DependencyProperty DataProperty =
        //    DependencyProperty.Register("Data", typeof(RatingItemData), typeof(RatingItem), new UIPropertyMetadata(new RatingItemData()));

        #endregion (Properties)

        #region Methods

        #region Property event handlers

        private static object CoerceFractionValue(DependencyObject obj, object baseValue)
        {
            if (baseValue == DependencyProperty.UnsetValue) return (double)FractionProperty.DefaultMetadata.DefaultValue;
            return Math.Max(Math.Min((double)baseValue, (double)FractionProperty.DefaultMetadata.DefaultValue), 0.0);
        }

        private static object CoercePreviewStateValue(DependencyObject obj, object baseValue)
        {
            PreviewState previewState = (PreviewState)baseValue;
            if ((previewState == PreviewState.PreviewCommonRating || previewState == PreviewState.PreviewPersonalRating) && !((obj as RatingItem).Fraction > .0)) return PreviewState.PreviewUnrated;
            return baseValue;
        }

        #endregion (Property event handlers)

        #region UI behaviors

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            e.Handled = true;

            var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
            if (rating != null) rating.HighlightItems(this);

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;

            var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
            if (rating != null) rating.SelectItems(this);

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;

            var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
            if (rating != null) rating.DeselectItems();

            base.OnMouseRightButtonDown(e);
        }

        #endregion (UI behaviors)

        #endregion (Methods)

        #region Constructors

        static RatingItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RatingItem), new FrameworkPropertyMetadata(typeof(RatingItem))
            );
        }

    #endregion (Constructuctors)
    }

    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(RatingItem))]
    public class RatingControl : ItemsControl
    {
        private enum RatingItemUpdate
        {
            PreviewRate,
            SelectRate,
            SetRate,
            SkipRate
        }

        #region Properties

        // Orientation
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(RatingControl), new UIPropertyMetadata(Orientation.Horizontal));        

        // SelectionDirection
        public SelectionDirection SelectionDirection
        {
            get { return (SelectionDirection)GetValue(SelectionDirectionProperty); }
            set { SetValue(SelectionDirectionProperty, value); }
        }

        public static readonly DependencyProperty SelectionDirectionProperty =
            DependencyProperty.Register("SelectionDirection", typeof(SelectionDirection), typeof(RatingControl), 
            new UIPropertyMetadata(SelectionDirection.FirstToLast));


        // RatingRangeMax
        public double RatingRangeMax
        {
            get { return (double)GetValue(RatingRangeMaxProperty); }
            set { SetValue(RatingRangeMaxProperty, value); }
        }

        public static readonly DependencyProperty RatingRangeMaxProperty =
            DependencyProperty.Register("RatingRangeMax", typeof(double), typeof(RatingControl),
            new UIPropertyMetadata(10.0, RatingAttributesValueChanged, CoerceRatingRangeMaxValue));

        // RatingRangeMin
        public double RatingRangeMin
        {
            get { return (double)GetValue(RatingRangeMinProperty); }
            set { SetValue(RatingRangeMinProperty, value); }
        }

        public static readonly DependencyProperty RatingRangeMinProperty =
            DependencyProperty.Register("RatingRangeMin", typeof(double), typeof(RatingControl),
            new UIPropertyMetadata(0.0, RatingAttributesValueChanged, CoerceRatingRangeMinValue));

        // CommonRating
        public double CommonRating
        {
            get { return (double)GetValue(CommonRatingProperty); }
            set { SetValue(CommonRatingProperty, value); }
        }

        public static readonly DependencyProperty CommonRatingProperty =
            DependencyProperty.Register("CommonRating", typeof(double), typeof(RatingControl),
            new FrameworkPropertyMetadata(
                RatingRangeMinProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender,
                RatingAttributesValueChanged, CoerceRatingValue)
                );

        // PersonalRating
        public double PersonalRating
        {
            get { return (double)GetValue(PersonalRatingProperty); }
            set { SetValue(PersonalRatingProperty, value); }
        }

        public static readonly DependencyProperty PersonalRatingProperty =
            DependencyProperty.Register("PersonalRating", typeof(double), typeof(RatingControl),
            new FrameworkPropertyMetadata(RatingRangeMinProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, 
                RatingAttributesValueChanged, CoerceRatingValue)
                );

        // PersonalRatingPreview
        public double PersonalRatingPreview
        {
            get { return (double)GetValue(PersonalRatingPreviewProperty); }
            set { SetValue(PersonalRatingPreviewProperty, value); }
        }

        public static readonly DependencyProperty PersonalRatingPreviewProperty =
            DependencyProperty.Register("PersonalRatingPreview", typeof(double), typeof(RatingControl), 
            new UIPropertyMetadata(PersonalRatingProperty.DefaultMetadata.DefaultValue));      

        public double RatingRangeDelta { get { return RatingRangeMax - RatingRangeMin; } }

        #endregion (Properties)

        #region Methods

        #region Property event handlers

        private static object CoerceRatingRangeMaxValue(DependencyObject obj, object baseValue)
        {
            var rating = (RatingControl)obj;
            var value = (double)baseValue;         
            return value > rating.RatingRangeMin && Math.Floor(value - rating.RatingRangeMin) >= rating.Items.Count ? value : rating.RatingRangeMax;
        }

        private static object CoerceRatingRangeMinValue(DependencyObject obj, object baseValue)
        {
            var rating = (RatingControl)obj;
            var value = (double)baseValue;
            return value < rating.RatingRangeMax ? value : rating.RatingRangeMin;
        }

        private static object CoerceRatingValue(DependencyObject obj, object baseValue)
        {
            var rating = (RatingControl)obj;
            return Math.Max(Math.Min((double)baseValue, rating.RatingRangeMax), rating.RatingRangeMin);
        }

        private static void RatingAttributesValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var rating = (RatingControl)obj;
            rating.UpdateItems(RatingItemUpdate.SetRate, null);
        }

        #endregion (Property event handlers)

        #region Rating interface

        internal void HighlightItems(RatingItem ratingItem)
        {
            UpdateItems(RatingItemUpdate.PreviewRate, ratingItem);
        }

        internal void SelectItems(RatingItem ratingItem)
        {
            UpdateItems(RatingItemUpdate.SelectRate, ratingItem);
        }

        internal void DeselectItems()
        {
            PersonalRating = (double)PersonalRatingProperty.DefaultMetadata.DefaultValue;
        }

        #endregion (Rating interface)

        #region UI behaviors

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            e.Handled = true;
            UpdateItems(RatingItemUpdate.SetRate, null);
            base.OnMouseLeave(e);
        }

        #endregion (UI behabiors)

        private void UpdateItems(RatingItemUpdate action, RatingItem keyItem)
        {
            lock (this)
            {
                if (ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                {
                    bool commonRating = PersonalRating == (double)PersonalRatingProperty.DefaultMetadata.DefaultValue;

                    var gaugeValue = commonRating ? CommonRating : PersonalRating;
                    var itemValue = RatingRangeDelta / Items.Count;
                    var currentValue = RatingRangeMin;

                    var index = 0; var indexMax = Items.Count;
                    var increment = 1;
                    if (SelectionDirection == CustomControls.SelectionDirection.LastToFirst)
                    {
                        var temp = index; index = indexMax - 1; indexMax = temp - 1;
                        increment = -1;
                    }
                    for (int order = 1; indexMax.CompareTo(index) == increment; index += increment, order++)
                    {
                        var item = ItemContainerGenerator.ContainerFromIndex(index) as RatingItem;
                        if (item != null)
                        {
                            item.Fraction = (double)RatingItem.FractionProperty.DefaultMetadata.DefaultValue;
                            item.PreviewState = PreviewState.PreviewUnrated;
                            switch (action)
                            {
                                case RatingItemUpdate.SetRate:
                                    var itemFraction = (double)RatingItem.FractionProperty.DefaultMetadata.DefaultValue;
                                    if (order * itemValue > gaugeValue)
                                    {
                                        var delta = gaugeValue - ((order - 1) * itemValue);
                                        itemFraction = delta > 0 ? delta / itemValue : 0.0;
                                    }
                                    item.Fraction = itemFraction;
                                    item.PreviewState = commonRating ? PreviewState.PreviewCommonRating : PreviewState.PreviewPersonalRating;
                                    break;

                                case RatingItemUpdate.PreviewRate:
                                    item.PreviewState = PreviewState.PreviewHighlight;
                                    PersonalRatingPreview = Math.Round(order * itemValue, 2);
                                    break;

                                case RatingItemUpdate.SelectRate:                                 
                                    currentValue += itemValue;
                                    break;
                            }
                            if (item == keyItem)
                            {
                                if (action == RatingItemUpdate.SelectRate) PersonalRating = currentValue;
                                action = RatingItemUpdate.SkipRate;
                            }
                        }
                    }
                }
            }
        }

        #endregion (Methods)

        #region Constructors

        public RatingControl()
        {
            ItemContainerGenerator.StatusChanged += new EventHandler((s, e) => { UpdateItems(RatingItemUpdate.SetRate, null); });
        }

        static RatingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingControl), new FrameworkPropertyMetadata(typeof(RatingControl)));
        }

        #endregion (Constructuctors)
    }
}
