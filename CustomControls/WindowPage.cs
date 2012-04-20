using System.Windows;
using System.Windows.Controls;

namespace CustomControls
{
    public class WindowPage : UserControl
    {
        // TODO: CaptionHeight DepProp - For caption height dynamic customization.
        static WindowPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowPage), new FrameworkPropertyMetadata(typeof(WindowPage)));
        }

        #region depProps

        // REFACTOR: Rename properties according to their semantic purposes. 
        public object LeftHeaderCell
        {
            get { return (object)GetValue(LeftHeaderCellProperty); }
            set { SetValue(LeftHeaderCellProperty, value); }
        }
        public static readonly DependencyProperty LeftHeaderCellProperty =
            DependencyProperty.Register("LeftHeaderCell", typeof(object), typeof(WindowPage), new UIPropertyMetadata(null));

        public object MidHeaderCell
        {
            get { return (object)GetValue(MidHeaderCellProperty); }
            set { SetValue(MidHeaderCellProperty, value); }
        }
        public static readonly DependencyProperty MidHeaderCellProperty =
            DependencyProperty.Register("MidHeaderCell", typeof(object), typeof(WindowPage), new UIPropertyMetadata(null));

        public object RightHeaderCell
        {
            get { return (object)GetValue(RightHeaderCellProperty); }
            set { SetValue(RightHeaderCellProperty, value); }
        }
        public static readonly DependencyProperty RightHeaderCellProperty =
            DependencyProperty.Register("RightHeaderCell", typeof(object), typeof(WindowPage), new UIPropertyMetadata(null));

        #endregion // depProps
    }
}
