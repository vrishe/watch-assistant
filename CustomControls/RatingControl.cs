using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;

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
    
    enum PreviewState
    {
        Unrated,
        Highlighted,
        PersonalRated,
        CommonRated
    }

    public delegate void RatingItemHotSpotChangedEventHandler(object sender, EventArgs e);
    class RatingItem : Control
    {
        #region Properties

        public double Fraction
        {
            get { return (double)GetValue(FractionProperty); }
            set { SetValue(FractionProperty, value); }
        }

        public static readonly DependencyProperty FractionProperty =
            DependencyProperty.Register("Fraction", typeof(double), typeof(RatingItem), 
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public PreviewState PreviewState
        {
            get { return (PreviewState)GetValue(PreviewStateProperty); }
            set { SetValue(PreviewStateProperty, value); }
        }

        public static readonly DependencyProperty PreviewStateProperty =
            DependencyProperty.Register("PreviewState", typeof(PreviewState), typeof(RatingItem), 
            new FrameworkPropertyMetadata(PreviewState.Unrated, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion (Properties)

        #region Events

        public event RatingItemHotSpotChangedEventHandler RatingItemHotSpotChangedEvent = new RatingItemHotSpotChangedEventHandler((s, e) => { }); 

        #endregion (Events)

        #region Methods

        #region Property event handlers

        private static object CoerceFractionValue(DependencyObject sender, object baseValue)
        {
            if (baseValue == DependencyProperty.UnsetValue) return (double)FractionProperty.DefaultMetadata.DefaultValue;
            return Math.Max(Math.Min((double)baseValue, (double)FractionProperty.DefaultMetadata.DefaultValue), .0);
        }

        private static object CoercePreviewStateValue(DependencyObject sender, object baseValue)
        {
            PreviewState previewState = (PreviewState)baseValue;
            if ((previewState == PreviewState.CommonRated || previewState == PreviewState.PersonalRated) && (sender as RatingItem).Fraction == .0) return PreviewState.Unrated;
            return baseValue;
        }

        private static void PreviewStateValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RatingItem item = (RatingItem)sender;

            double itemFraction = item.Fraction; 
            switch ((PreviewState)e.NewValue)
            {
                case PreviewState.Highlighted:
                    itemFraction = (double)FractionProperty.DefaultMetadata.DefaultValue;
                    break;
                case PreviewState.Unrated:
                    itemFraction = .0;
                    break;
            }
            item.Fraction = itemFraction;
        }

        #endregion (Property event handlers)

        #region UI behaviors

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            e.Handled = true;

            RatingItemHotSpotChangedEvent(this, new EventArgs());

            base.OnMouseEnter(e);
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

    [DefaultProperty("RatingItemsCount")]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(RatingItem))]
    [ContentProperty]
    public class RatingControl : ItemsControl
    {
        private enum RatingItemUpdateAction
        {
            Clear,
            Preview,
            Confirm,
            Reset
        }

        #region Fields

        private RatingItem _hotSpottedItem;

        #endregion (Fields)

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
                    FrameworkPropertyMetadataOptions.AffectsRender, 
                    RatingAttributesValueChanged, CoerceRatingValue)
                );

        // PersonalRatingPreview
        public double HighlightedRating
        {
            get { return (double)GetValue(HighlightedRatingProperty); }
            private set { SetValue(HighlightedRatingPropertyKey, value); }
        }

        public static readonly DependencyPropertyKey HighlightedRatingPropertyKey =
            DependencyProperty.RegisterReadOnly("HighlightedRating", typeof(double), typeof(RatingControl), 
            new UIPropertyMetadata(PersonalRatingProperty.DefaultMetadata.DefaultValue));
        public static readonly DependencyProperty HighlightedRatingProperty = HighlightedRatingPropertyKey.DependencyProperty;

        // IsHotSpotted
        public bool IsHotSpotted
        {
            get { return (bool)GetValue(IsHotSpottedProperty); }
            private set { SetValue(IsHotSpottedPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsHotSpottedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsHotSpotted", typeof(bool), typeof(RatingControl), new UIPropertyMetadata(false));
        public static readonly DependencyProperty IsHotSpottedProperty = IsHotSpottedPropertyKey.DependencyProperty;

        // HotSpottedItem
        private RatingItem HotSpottedItem
        {
            get { return _hotSpottedItem; }
            set
            {
                if (!Items.Contains(value)) value = null;
                _hotSpottedItem = value;

                IsHotSpotted = _hotSpottedItem != null;
            }
        }
        
        // RatingItemsCount
        [DesignOnly(true)]
        public int RatingItemsCount
        {
            get { return Items.Count; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(String.Format(@"{0}: Invalid number of rating items was specified to create. Is: {1}, must not be less than 1", Name, value));

                ManageItemCollectionHandlers(this, Items, true);

                Items.Clear();
                while (value-- > 0) Items.Add(new RatingItem());

                ManageItemCollectionHandlers(this, Items, false);
            }
        }

        public double RatingRangeDelta { get { return RatingRangeMax - RatingRangeMin; } }

        #endregion (Properties)

        #region Events

        public static readonly RoutedEvent PersonalRatingChanged = EventManager.RegisterRoutedEvent("PersonalRatingChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RatingControl));
        public static readonly RoutedEvent CommonRatingChanged = EventManager.RegisterRoutedEvent("CommonRatingChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RatingControl));

        #region Handlers

        public event RoutedEventHandler PersonalRatingChangedEventHandler
        {
            add { AddHandler(PersonalRatingChanged, value); }
            remove { RemoveHandler(PersonalRatingChanged, value); }
        }

        public event RoutedEventHandler CommonRatingChangedEventHandler
        {
            add { AddHandler(CommonRatingChanged, value); }
            remove { RemoveHandler(CommonRatingChanged, value); }
        }

        #endregion (Handlers)

        #endregion (Events)

        #region Methods

        #region Property event handlers

        private static object CoerceRatingRangeMaxValue(DependencyObject sender, object baseValue)
        {
            var rating = (RatingControl)sender;
            var value = (double)baseValue;         
            return value > rating.RatingRangeMin && Math.Floor(value - rating.RatingRangeMin) >= rating.Items.Count ? value : rating.RatingRangeMax;
        }

        private static object CoerceRatingRangeMinValue(DependencyObject sender, object baseValue)
        {
            var rating = (RatingControl)sender;
            var value = (double)baseValue;
            return value < rating.RatingRangeMax ? value : rating.RatingRangeMin;
        }

        private static object CoerceRatingValue(DependencyObject sender, object baseValue)
        {
            var rating = (RatingControl)sender;
            return Math.Max(Math.Min((double)baseValue, rating.RatingRangeMax), rating.RatingRangeMin);
        }

        private static void RatingAttributesValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var rating = (RatingControl)sender;

            // else ...
            RoutedEvent risen = e.Property == PersonalRatingProperty ? PersonalRatingChanged : CommonRatingChanged;               

            rating.RaiseEvent(new RoutedEventArgs(risen));

        }

        #endregion (Property event handlers)

        #region UI behaviors

        private void OnRatingItemHotSpotChanged(object sender, EventArgs e)
        {
            HotSpottedItem = sender as RatingItem;
            RatingControl.UpdateItems(this, RatingItemUpdateAction.Preview);
        }

        private void OnRatingControlMouseAction(MouseEventArgs e)
        {
            e.Handled = true;

            if (e.RoutedEvent == UIElement.MouseLeftButtonUpEvent) 
            {
                RatingControl.UpdateItems(this, RatingItemUpdateAction.Confirm);
            }
            else if (e.RoutedEvent == UIElement.MouseRightButtonUpEvent) 
            {
                RatingControl.UpdateItems(this, RatingItemUpdateAction.Clear);
            }
            else if (e.RoutedEvent == UIElement.MouseLeaveEvent)
            {
                RatingControl.UpdateItems(this, RatingItemUpdateAction.Reset);
            }
        }

        #region Local overloads

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            OnRatingControlMouseAction(e);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            OnRatingControlMouseAction(e);
            base.OnMouseRightButtonDown(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            OnRatingControlMouseAction(e);
            base.OnMouseRightButtonUp(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            OnRatingControlMouseAction(e);
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            OnRatingControlMouseAction(e);
            base.OnMouseLeftButtonUp(e);
        }

        #endregion (Local overloads)

        #endregion (UI behabiors)

        #region Helpers

        private static void UpdateItems(RatingControl rating, RatingItemUpdateAction action)
        {
            if (rating == null) return;

            if (rating.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                // Initialization

                RatingItem hotSpottedItem = rating.HotSpottedItem;
              
                double gaugeValue = rating.PersonalRating;
                PreviewState itemPreviewState = PreviewState.PersonalRated;

                switch (action)
                {
                    case RatingItemUpdateAction.Preview:
                        if (hotSpottedItem == null) action = RatingItemUpdateAction.Reset;
                        break;
                    case RatingItemUpdateAction.Reset:
                        if (rating.PersonalRating == rating.RatingRangeMin)
                        {
                            gaugeValue = rating.CommonRating;
                            itemPreviewState = PreviewState.CommonRated;
                        }
                        break;
                }
                   
                // Items state recompution
                double itemValue = rating.RatingRangeDelta / rating.Items.Count;

                int index = 0, order = 1, indexMax = rating.Items.Count, increment = 1;
                if (rating.SelectionDirection == CustomControls.SelectionDirection.LastToFirst)
                {
                    var temp = index; index = indexMax - 1; indexMax = temp - 1;
                    increment = -1;
                }
                for (RatingItemUpdateAction selector = action; indexMax.CompareTo(index) == increment; index += increment)
                {
                    var item = rating.ItemContainerGenerator.ContainerFromIndex(index) as RatingItem;
                    if (item != null)
                    {
                        switch (selector)
                        {
                            case RatingItemUpdateAction.Clear:
                                item.PreviewState = PreviewState.Unrated;
                                continue;

                            case RatingItemUpdateAction.Preview:
                                item.PreviewState = PreviewState.Highlighted;
                                if (item == hotSpottedItem) selector = RatingItemUpdateAction.Clear;
                                break;

                            case RatingItemUpdateAction.Confirm:
                                item.PreviewState = PreviewState.PersonalRated;
                                if (item == hotSpottedItem) selector = RatingItemUpdateAction.Clear;
                                break;

                            case RatingItemUpdateAction.Reset:
                                double itemFraction = (double)RatingItem.FractionProperty.DefaultMetadata.DefaultValue;
                                if (order * itemValue > gaugeValue)
                                {
                                    var delta = gaugeValue - ((order - 1) * itemValue);
                                    itemFraction = delta > .0 ? delta / itemValue : .0;
                                    if (itemFraction == .0)
                                    {
                                        selector = RatingItemUpdateAction.Clear;
                                        index -= increment;
                                        continue;
                                    }
                                }
                                item.Fraction = itemFraction;
                                item.PreviewState = itemPreviewState;
                                break;
                        }
                        order++;
                    }
                }

                double tempRating = rating.RatingRangeMin + Math.Round((order - 1) * itemValue, 2);

                RatingItem ratingHotSpottedItem = null;
                double ratingHighlightedRating = (double)HighlightedRatingProperty.DefaultMetadata.DefaultValue;
                double ratingPersonalRating = rating.PersonalRating; 
                switch (action)
                {
                    case RatingItemUpdateAction.Clear:
                        ratingPersonalRating = (double)rating.RatingRangeMin;
                        break;

                    case RatingItemUpdateAction.Preview:
                        ratingHighlightedRating = tempRating;
                        ratingHotSpottedItem = hotSpottedItem;
                        break;

                    case RatingItemUpdateAction.Confirm:
                        ratingPersonalRating = tempRating;
                        break;
                }
                rating.PersonalRating = ratingPersonalRating;
                rating.HighlightedRating = ratingHighlightedRating;
                rating.HotSpottedItem = ratingHotSpottedItem;
            }
        }

        private static void ManageItemCollectionHandlers(RatingControl rating, IList items, bool remove)
        {
            if (rating == null && items == null) return;
            foreach (object obj in items)
            {
                RatingItem item = obj as RatingItem;
                if (item != null)
                {
                    if (!remove)
                    {
                        item.RatingItemHotSpotChangedEvent += rating.OnRatingItemHotSpotChanged;
                    }
                    else
                    {
                        item.RatingItemHotSpotChangedEvent -= rating.OnRatingItemHotSpotChanged;
                    }
                }
            }
        }

        private static void UpdateItemCollectionHandlers(RatingControl rating, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null) ManageItemCollectionHandlers(rating, e.NewItems, false);
            if (e.OldItems != null) ManageItemCollectionHandlers(rating, e.OldItems, true);
        }

        #region Local overloads

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            RatingControl.UpdateItemCollectionHandlers(this, e);

            base.OnItemsChanged(e);
        }

        #endregion (Local overloads)

        #endregion (Helpers)

        #endregion (Methods)

        #region Constructors

        public RatingControl()
        {
            ItemContainerGenerator.StatusChanged += new EventHandler((s, e) => { UpdateItems(this, RatingItemUpdateAction.Reset); });
        }

        static RatingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingControl), new FrameworkPropertyMetadata(typeof(RatingControl)));
        }

        #endregion (Constructuctors)
    }
}
