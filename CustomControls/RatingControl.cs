using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace CustomControls
{
    public enum SelectionDirection
    {
        Forward,
        Backward
    }

    [ContentProperty("Content")]
    public class RatingItem : ComboBoxItem
    {
    #region Methods

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        e.Handled = true;
        var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
        if (rating != null)
            rating.HighlightItems(this);
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        e.Handled = true;
        var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
        if (rating != null)
            rating.SelectItems(this);
        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        e.Handled = true;
        var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
        if (rating != null)
            rating.DeselectItems(this);
        base.OnMouseDoubleClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
        {
            var select = e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.Return;
            var deselect = e.Key == Key.Escape;

            e.Handled = select || deselect;

            if (e.Handled)
            {
                var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
                if (rating != null)
                {
                    if (select)
                        rating.SelectItems(this);
                    else
                        rating.DeselectItems(this);
                }
            }
        }

        base.OnKeyDown(e);
    }

    internal void Highlight(bool state)
    {
        IsHighlighted = state;
    }

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

    [StyleTypedProperty(Property = "RatingItemStyle", StyleTargetType = typeof(RatingItem))]
    public class RatingControl : ItemsControl
    {
        private enum RatingItemUpdate
        {
            Highlight,
            Select,
            Rate,
            Skip
        }

        //#region Fields

        //#endregion (Fields)

        #region Properties


        // SelectionDirection
        public SelectionDirection SelectionDirection
        {
            get { return (SelectionDirection)GetValue(SelectionDirectionProperty); }
            set { SetValue(SelectionDirectionProperty, value); }
        }

        public static readonly DependencyProperty SelectionDirectionProperty =
            DependencyProperty.Register("SelectionDirection", typeof(SelectionDirection), typeof(RatingControl), 
            new UIPropertyMetadata(SelectionDirection.Forward));
        

        // RatingRangeMax
        public double RatingRangeMax
        {
            get { return (double)GetValue(RatingRangeMaxProperty); }
            set { SetValue(RatingRangeMaxProperty, value); }
        }

        public static readonly DependencyProperty RatingRangeMaxProperty =
            DependencyProperty.Register("RatingRangeMax", typeof(double), typeof(RatingControl), 
            new UIPropertyMetadata(0.0, RatingAttributesValueChanged, CoerceRatingRangeMaxValue));

        // RatingRangeMin
        public double RatingRangeMin
        {
            get { return (double)GetValue(RatingRangeMinProperty); }
            set { SetValue(RatingRangeMinProperty, value); }
        }

        public static readonly DependencyProperty RatingRangeMinProperty =
            DependencyProperty.Register("RatingRangeMin", typeof(double), typeof(RatingControl),
            new UIPropertyMetadata(10.0, RatingAttributesValueChanged, CoerceRatingRangeMinValue));

        // CommonRating
        public double CommonRating
        {
            get { return (double)GetValue(CommonRatingProperty); }
            set { SetValue(CommonRatingProperty, value); }
        }

        public static readonly DependencyProperty CommonRatingProperty =
            DependencyProperty.Register("CommonRating", typeof(double), typeof(RatingControl),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                RatingAttributesValueChanged, CoerceCommonRatingValue)
                );

        // x:Name
        public double PersonalRating
        {
            get { return (double)GetValue(PersonalRatingProperty); }
            set { SetValue(PersonalRatingProperty, value); }
        }

        public static readonly DependencyProperty PersonalRatingProperty =
            DependencyProperty.Register("PersonalRating", typeof(double), typeof(RatingControl),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                RatingAttributesValueChanged, CoercePersonalRatingValue)
                );

        //public int PersonalRating
        //{
        //    get { return (int)GetValue(ValueProperty); }
        //    set { SetValue(ValueProperty, value); }
        //}

        //public static readonly DependencyProperty ValueProperty =
        //        DependencyProperty.Register("PersonalRating", typeof(int), typeof(RatingControl),
        //            new FrameworkPropertyMetadata(0,
        //                FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        //                ValueValueChanged, CoerceValueValue, true, UpdateSourceTrigger.PropertyChanged));

        //private static void ValueValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var rating = (RatingControl)d;
        //    rating.UpdateItems(RatingItemUpdate.Rate, null);
        //}

        //private static object CoerceValueValue(DependencyObject d, object baseValue)
        //{
        //    var rating = (RatingControl)d;
        //    var value = (int)baseValue;
        //    value = Math.Max(0, value);
        //    if (rating.HasItems)
        //        value = Math.Min(rating.Items.Count, value);
        //    return value;
        //}

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

        private static object CoerceCommonRatingValue(DependencyObject obj, object baseValue)
        {
            var rating = (RatingControl)obj;
            return Math.Min(Math.Max((double)baseValue, rating.RatingRangeMin), rating.RatingRangeMax);
        }

        private static object CoercePersonalRatingValue(DependencyObject obj, object baseValue)
        {
            var rating = (RatingControl)obj;
            return Math.Floor(Math.Min(Math.Max((double)baseValue, rating.RatingRangeMin), rating.RatingRangeMax));
        }

        private static void RatingAttributesValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var rating = (RatingControl)obj;
                rating.UpdateItems(RatingItemUpdate.Rate, null);
            }
        }

        #endregion (Property event handlers)

        internal void HighlightItems(RatingItem ratingItem)
        {
            UpdateItems(RatingItemUpdate.Highlight, ratingItem);
        }

        internal void SelectItems(RatingItem ratingItem)
        {
            UpdateItems(RatingItemUpdate.Select, ratingItem);
        }

        internal void DeselectItems(RatingItem ratingItem)
        {
            var children = LogicalChildren;
            if (children != null && children.MoveNext() && children.Current == ratingItem)
                PersonalRating = 0;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            e.Handled = true;
            UpdateItems(RatingItemUpdate.Rate, null);
            base.OnMouseLeave(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            UpdateItems(RatingItemUpdate.Rate, null);
            base.OnInitialized(e);
        }

        private void UpdateItems(RatingItemUpdate action, RatingItem keyItem)
        {
            var children = LogicalChildren;
            if (children != null)
            {
                var update = action;
                var curValue = PersonalRating;
                var newValue = 0;
                var index = 0;

                while (children.MoveNext())
                {
                    var item = children.Current as RatingItem;
                    if (item != null)
                    {
                        ++index;
                        if (update == RatingItemUpdate.Rate)
                        {
                            item.Highlight(false);
                            item.IsSelected = index <= curValue;

                        }
                        else
                        {
                            item.Highlight(update == RatingItemUpdate.Highlight);
                            item.IsSelected = update == RatingItemUpdate.Select;
                            if (update == RatingItemUpdate.Select)
                                ++newValue;
                        }
                    }
                    if (children.Current == keyItem)
                        update = RatingItemUpdate.Skip;
                }

                if (action == RatingItemUpdate.Select)
                    PersonalRating = newValue;
            }
        }

        #endregion (Methods)

        #region Constructors

        static RatingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingControl), new FrameworkPropertyMetadata(typeof(RatingControl)));
        }

        #endregion (Constructuctors)
    }

    //[ContentProperty("Content")]
    //public class RatingItem : ComboBoxItem
    //{
    //    #region Methods

    //    protected override void OnMouseEnter(MouseEventArgs e)
    //    {
    //        e.Handled = true;
    //        var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
    //        if (rating != null)
    //            rating.HighlightItems(this);
    //        base.OnMouseEnter(e);
    //    }

    //    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    //    {
    //        e.Handled = true;
    //        var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
    //        if (rating != null)
    //            rating.SelectItems(this);
    //        base.OnMouseLeftButtonDown(e);
    //    }

    //    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    //    {
    //        e.Handled = true;
    //        var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
    //        if (rating != null)
    //            rating.DeselectItems(this);
    //        base.OnMouseDoubleClick(e);
    //    }

    //    protected override void OnKeyDown(KeyEventArgs e)
    //    {
    //        if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
    //        {
    //            var select = e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.Return;
    //            var deselect = e.Key == Key.Escape;

    //            e.Handled = select || deselect;

    //            if (e.Handled)
    //            {
    //                var rating = ItemsControl.ItemsControlFromItemContainer(this) as RatingControl;
    //                if (rating != null)
    //                {
    //                    if (select)
    //                        rating.SelectItems(this);
    //                    else
    //                        rating.DeselectItems(this);
    //                }
    //            }
    //        }

    //        base.OnKeyDown(e);
    //    }

    //    internal void Highlight(bool state)
    //    {
    //        IsHighlighted = state;
    //    }

    //    #endregion (Methods)

    //    #region Constructors

    //    static RatingItem()
    //    {
    //        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
    //            typeof(RatingItem), new FrameworkPropertyMetadata(typeof(RatingItem))
    //        );
    //    }

    //    #endregion (Constructuctors)
    //}

    //[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(RatingItem))]
    //public class RatingControl : ItemsControl
    //{
    //    private enum RatingItemUpdate
    //    {
    //        Highlight,
    //        Select,
    //        Rate,
    //        Skip
    //    }

    //    //#region Fields

    //    //#endregion (Fields)

    //    #region Properties

    //    public int PersonalRating
    //    {
    //        get { return (int)GetValue(ValueProperty); }
    //        set { SetValue(ValueProperty, value); }
    //    }

    //    public static readonly DependencyProperty ValueProperty =
    //            DependencyProperty.Register("PersonalRating", typeof(int), typeof(RatingControl),
    //                new FrameworkPropertyMetadata(0,
    //                    FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
    //                    ValueValueChanged, CoerceValueValue, true, UpdateSourceTrigger.PropertyChanged));

    //    private static void ValueValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var rating = (RatingControl)d;
    //        rating.UpdateItems(RatingItemUpdate.Rate, null);
    //    }

    //    private static object CoerceValueValue(DependencyObject d, object baseValue)
    //    {
    //        var rating = (RatingControl)d;
    //        var value = (int)baseValue;
    //        value = Math.Max(0, value);
    //        if (rating.HasItems)
    //            value = Math.Min(rating.Items.Count, value);
    //        return value;
    //    }

    //    #endregion (Properties)

    //    #region Methods

    //    internal void HighlightItems(RatingItem ratingItem)
    //    {
    //        UpdateItems(RatingItemUpdate.Highlight, ratingItem);
    //    }

    //    internal void SelectItems(RatingItem ratingItem)
    //    {
    //        UpdateItems(RatingItemUpdate.Select, ratingItem);
    //    }

    //    internal void DeselectItems(RatingItem ratingItem)
    //    {
    //        var children = LogicalChildren;
    //        if (children != null && children.MoveNext() && children.Current == ratingItem)
    //            PersonalRating = 0;
    //    }

    //    protected override void OnMouseLeave(MouseEventArgs e)
    //    {
    //        e.Handled = true;
    //        UpdateItems(RatingItemUpdate.Rate, null);
    //        base.OnMouseLeave(e);
    //    }

    //    protected override void OnInitialized(EventArgs e)
    //    {
    //        UpdateItems(RatingItemUpdate.Rate, null);
    //        base.OnInitialized(e);
    //    }

    //    private void UpdateItems(RatingItemUpdate action, RatingItem keyItem)
    //    {
    //        var children = LogicalChildren;
    //        if (children != null)
    //        {
    //            var update = action;
    //            var curValue = PersonalRating;
    //            var newValue = 0;
    //            var index = 0;

    //            while (children.MoveNext())
    //            {
    //                var item = children.Current as RatingItem;
    //                if (item != null)
    //                {
    //                    ++index;
    //                    if (update == RatingItemUpdate.Rate)
    //                    {
    //                        item.Highlight(false);
    //                        item.IsSelected = index <= curValue;
    //                    }
    //                    else
    //                    {
    //                        item.Highlight(update == RatingItemUpdate.Highlight);
    //                        item.IsSelected = update == RatingItemUpdate.Select;
    //                        if (update == RatingItemUpdate.Select)
    //                            ++newValue;
    //                    }
    //                }
    //                if (children.Current == keyItem)
    //                    update = RatingItemUpdate.Skip;
    //            }

    //            if (action == RatingItemUpdate.Select)
    //                PersonalRating = newValue;
    //        }
    //    }

    //    #endregion (Methods)

    //    #region Constructors

    //    static RatingControl()
    //    {
    //        DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingControl), new FrameworkPropertyMetadata(typeof(RatingControl)));
    //    }

    //    #endregion (Constructuctors)
    //}
}
