using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public sealed class LinkedCollection<TData> : IList<TData>, INotifyCollectionChanged, INotifyPropertyChanged, IList
    {
        private readonly ObservableCollection<TData> _linked;

        public LinkedCollection(ObservableCollection<TData> linked) => _linked = linked;

        public IEnumerator<TData> GetEnumerator() => _linked.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _linked).GetEnumerator();

        public void Add(TData item) => _linked.Add(item);

        public int Add(object value)
        {
            return ((IList) _linked).Add(value);
        }

        public bool Contains(object value)
        {
            return ((IList) _linked).Contains(value);
        }

        public void Clear() => _linked.Clear();
        public int IndexOf(object value)
        {
            return ((IList) _linked).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList) _linked).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList) _linked).Remove(value);
        }

        public bool Contains(TData item) => _linked.Contains(item);

        public void CopyTo(TData[] array, int arrayIndex) => _linked.CopyTo(array, arrayIndex);

        public bool Remove(TData item) => _linked.Remove(item);

        public void CopyTo(Array array, int index)
        {
            ((ICollection) _linked).CopyTo(array, index);
        }

        public int Count => _linked.Count;
        public object SyncRoot => ((ICollection) _linked).SyncRoot;

        public bool IsSynchronized => ((ICollection) _linked).IsSynchronized;

        public bool IsReadOnly => false;
        public bool IsFixedSize => ((IList) _linked).IsFixedSize;

        public int IndexOf(TData item) => _linked.IndexOf(item);

        public void Insert(int index, TData item) => _linked.Insert(index, item);

        public void RemoveAt(int index) => _linked.RemoveAt(index);
        object IList.this[int index]
        {
            get => ((IList) _linked)[index];
            set => ((IList) _linked)[index] = value;
        }

        public TData this[int index]
        {
            get => _linked[index];
            set => _linked[index] = value;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => _linked.CollectionChanged += value;
            remove => _linked.CollectionChanged -= value;
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => ((INotifyPropertyChanged) _linked).PropertyChanged += value;
            remove => ((INotifyPropertyChanged) _linked).PropertyChanged -= value;
        }
    }
}