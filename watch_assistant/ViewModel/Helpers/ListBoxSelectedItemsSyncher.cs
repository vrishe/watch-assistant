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
    public class ListBoxSelectedItemsSyncher : DependencyObject
    {
        #region Private types

        private class Syncher : IDisposable
        {

            private ListBox _listbox;
            public ListBox ListBox { get { return _listbox; } }

            private IList _listToSync;
            public IList SynchronizedList
            {
                get { return _listToSync; }

                set
                {
                    detachTheSynchronizedListh();
                    _listToSync = value;
                    attachTheSynchronizedListh();
                }
            }

            public Syncher(ListBox listbox, IList listToSync)
            {
                _listbox = listbox;
                _listToSync = listToSync;
                attachTheSynchronizedListh();
            }

            void collectionChangedList_CollectionChanged(object sender,
                                    NotifyCollectionChangedEventArgs e)
            {
                //Add new items   
                if (e.NewItems != null)
                {
                    foreach (Object item in e.NewItems)
                    {
                        _listbox.SelectedItems.Add(item);
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (Object item in e.OldItems)
                    {
                        _listbox.SelectedItems.Remove(item);
                    }
                }

                if (e.Action == NotifyCollectionChangedAction.Reset)
                    _listbox.SelectedItems.Clear();

                CommandManager.InvalidateRequerySuggested();
            }

            void _list_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (_listToSync != null)
                {
                    //Add new items                   
                    foreach (Object item in e.AddedItems)
                    {
                        _listToSync.Add(item);
                    }
                    foreach (Object item in e.RemovedItems)
                    {
                        _listToSync.Remove(item);
                    }
                    CommandManager.InvalidateRequerySuggested();
                }
            }

            #region IDisposable Members
            public void Dispose()
            {
                if (_listbox == null) return;
                _listbox.SelectionChanged -= _list_SelectionChanged;
                detachTheSynchronizedListh();
                _listbox = null;
            }
            #endregion

            #region private methods

            private void attachTheSynchronizedListh()
            {
                _listbox.SelectionChanged -= _list_SelectionChanged;
                if (_listToSync == null) return;

                INotifyCollectionChanged collectionChangedList = null;
                if ((collectionChangedList = _listToSync as INotifyCollectionChanged) != null)
                    collectionChangedList.CollectionChanged
                  += new NotifyCollectionChangedEventHandler(collectionChangedList_CollectionChanged);

                //Update the selection with the new list
                _listbox.SelectedItems.Clear();

                foreach (var item in _listToSync)
                    _listbox.SelectedItems.Add(item);

                _listbox.SelectionChanged
                     += new SelectionChangedEventHandler(_list_SelectionChanged);
            }

            private void detachTheSynchronizedListh()
            {
                INotifyCollectionChanged collectionChangedList = null;
                if ((collectionChangedList = _listToSync as INotifyCollectionChanged) != null)
                    collectionChangedList.CollectionChanged -= collectionChangedList_CollectionChanged;
            }
            #endregion
        }

        #endregion (Private types)

        #region Fields

        private static List<Syncher> _synchers = new List<Syncher>();

        #endregion (Fields)

        #region Properties

        #region Attached

        /// <summary>
        /// Gets the SynchronizedList property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static IList GetSynchronizedList(DependencyObject d)
        {
            return (IList)d.GetValue(SynchronizedListProperty);
        }

        /// <summary>
        /// Sets the SynchronizedList property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetSynchronizedList(DependencyObject d, IList value)
        {
            d.SetValue(SynchronizedListProperty, value);
        }

        /// <summary>
        /// SynchronizedList Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty SynchronizedListProperty = 
            DependencyProperty.RegisterAttached("SynchronizedList", typeof(IList), typeof(ListBoxSelectedItemsSyncher), 
            new FrameworkPropertyMetadata(new List<object>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSynchronizedListChanged));

        #endregion (Attached)

        #endregion (Properties)


        private static void OnSynchronizedListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListBox listBox = d as ListBox;
 
            if(!(d is  ListBox))
                throw new  ArgumentException("ListBoxSelectedItemsSyncher is only applyable to Listbox");
 
            Syncher synch = (from Syncher syncher 
	                        in _synchers where syncher.ListBox == listBox select syncher)
                                        .FirstOrDefault();
            if (synch != null)
            {
                synch.SynchronizedList = e.NewValue as IList;
            } 
            else
            {
                synch = new  Syncher(listBox, e.NewValue as IList);
                _synchers.Add(synch);
                listBox.Unloaded += new  RoutedEventHandler(listBox_Unloaded);
            }
        }

        static void listBox_Unloaded(object sender, RoutedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            Syncher synch = (from Syncher syncher in _synchers where syncher.ListBox == listBox select syncher).FirstOrDefault();
            if (synch != null)
            {
                _synchers.Remove(synch);
                synch.Dispose();
                synch = null;
            }
        }

    }
}
