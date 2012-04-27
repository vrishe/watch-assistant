using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace watch_assistant.ViewModel.Helpers
{
    // TODO: Make single list selection controller upgrade
    public class ListBoxHelper : DependencyObject
    {
        private class SelectedItemsReflector : IDisposable
        {
            #region Fields

            private ListBox _target;
            private IList _mirror;

            #endregion (Fields)

            #region Properties

            public ListBox Target { get { return _target; } }

            public IList Mirror { get { return _mirror; } }

            #endregion (Properties)

            #region Methods

            private void CollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (_target != null)
                {
                    if (e.Action != NotifyCollectionChangedAction.Reset)
                    {
                        if (e.NewItems != null)
                        {
                            foreach (Object item in e.NewItems) _target.SelectedItems.Add(item);
                        }

                        if (e.OldItems != null)
                        {
                            foreach (Object item in e.OldItems) _target.SelectedItems.Remove(item);
                        }
                    }
                    else
                    {
                        _target.SelectedItems.Clear();
                    }

                    CommandManager.InvalidateRequerySuggested();
                }
            }

            private void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e)
            {
                if (_mirror != null)
                {
                    if (e.RemovedItems != null)
                    {
                        foreach (var item in e.RemovedItems) _mirror.Remove(item);
                    }

                    if (e.AddedItems != null)
                    {
                        foreach (var item in e.AddedItems) _mirror.Add(item);
                    }
                    
                    CommandManager.InvalidateRequerySuggested();
                }
            }

            #endregion (Methods)

            #region Constructors

            public SelectedItemsReflector(ListBox target, IList mirror)
            {
                if ((_target = target) == null) throw new ArgumentNullException("'target' cannot be null");
                if ((_mirror = mirror) == null) throw new ArgumentNullException("'mirror' cannot be null");

                _target.SelectionChanged -= SelectionChangedEventHandler;
                _target.SelectedItems.Clear();
                foreach (var item in _mirror) _target.SelectedItems.Add(item);

                _target.SelectionChanged += SelectionChangedEventHandler;

                var observable = mirror as INotifyCollectionChanged;
                if (observable != null) observable.CollectionChanged += CollectionChangedEventHandler;
            }

            #endregion (Constructors)

            #region IDisposable Members

            public void Dispose()
            {
                _target.SelectionChanged -= SelectionChangedEventHandler;

                var observable = _mirror as INotifyCollectionChanged;
                if (observable != null) observable.CollectionChanged -= CollectionChangedEventHandler;
            }

            #endregion
        }

        #region Fields

        private static readonly List<SelectedItemsReflector> _reflectors = new List<SelectedItemsReflector>();

        #endregion (Fields)

        #region Properties

        public static IList GetSelectedItemsMirrorList(DependencyObject d)
        {
            return (IList)d.GetValue(SelectedItemsMirrorListProperty);
        }

        public static void SetSelectedItemsMirrorList(DependencyObject d, IList value)
        {
            d.SetValue(SelectedItemsMirrorListProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsMirrorListProperty =
            DependencyProperty.RegisterAttached("SelectedItemsMirrorList", typeof(IList), typeof(ListBoxHelper), 
            new FrameworkPropertyMetadata(new List<object>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedItemsMirrorListValueChanged));

        #endregion (Properties)

        #region Methods

        #region Event handlers

        private static void SelectedItemsMirrorListValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ListBox target = sender as ListBox;

            UnsubscribeListBoxSynchronisation(target);

            target.Unloaded += ListBoxUnloaded;
            _reflectors.Add(new SelectedItemsReflector(target, e.NewValue as IList));
        }

        private static void ListBoxUnloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeListBoxSynchronisation(sender as ListBox);
        }

        #endregion (Event handlers)

        private static void UnsubscribeListBoxSynchronisation(ListBox target)
        {
            SelectedItemsReflector reflector = (from SelectedItemsReflector finder in _reflectors where finder.Target == target select finder).FirstOrDefault();            
            if (reflector != null) { _reflectors.Remove(reflector); reflector.Dispose(); }
        }

        #endregion (Methods)
    }
}
