using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using System;

namespace watch_assistant.ViewModel.Helpers
{
    public class PropertyBindingProxy : FrameworkElement
    {
        public static readonly DependencyProperty InProperty;
        public static readonly DependencyProperty OutProperty;

        public PropertyBindingProxy()
        {
            Visibility = Visibility.Collapsed;
        }

        static PropertyBindingProxy()
        {
            FrameworkPropertyMetadata inMetadata = new FrameworkPropertyMetadata(
                delegate(DependencyObject p, DependencyPropertyChangedEventArgs args)
                {
                    if (BindingOperations.GetBinding(p, OutProperty) != null) (p as PropertyBindingProxy).Out = args.NewValue;
                }
            );

            inMetadata.BindsTwoWayByDefault = false;
            inMetadata.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            InProperty = DependencyProperty.Register("In", typeof(object), typeof(PropertyBindingProxy), inMetadata);

            FrameworkPropertyMetadata outMetadata = new FrameworkPropertyMetadata(
                delegate(DependencyObject p, DependencyPropertyChangedEventArgs args)
                {
                    ValueSource source = DependencyPropertyHelper.GetValueSource(p, args.Property);

                    if (source.BaseValueSource != BaseValueSource.Local)
                    {
                        PropertyBindingProxy proxy = p as PropertyBindingProxy;
                        if (!object.ReferenceEquals(args.NewValue, proxy.In))
                        {
                            Dispatcher.CurrentDispatcher.BeginInvoke(
                                new Action(delegate() { proxy.In = proxy.Out; }), 
                                DispatcherPriority.DataBind
                            );
                        }
                    }
                }
            );

            outMetadata.BindsTwoWayByDefault = true;
            outMetadata.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            OutProperty = DependencyProperty.Register("Out", typeof(object), typeof(PropertyBindingProxy), outMetadata);
        }

        public object In
        {
            get { return this.GetValue(InProperty); }
            set { this.SetValue(InProperty, value); }
        }

        public object Out
        {
            get { return this.GetValue(OutProperty); }
            set { this.SetValue(OutProperty, value); }
        }
    }
}
