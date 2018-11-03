using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Application;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class LiveDatCollection<TData, TEditorItem> : UIObservableCollection<TEditorItem>
        where TData : IEquatable<TData> where TEditorItem : IEditorItem<TData>, new()
    {
        private readonly object _gate = new object();
        private readonly IOperationManager<TData, TEditorItem> _operationManager;
        private readonly UIObservableCollection<TData> _dataCollection = new UIObservableCollection<TData>();

        public event EventHandler<InsertCheckEventArgs<TEditorItem>> CheckInsertEvent;

        public ICollection<TData> DataCollection
        {
            get
            {
                lock(_gate)
                    return _dataCollection;
            }
        }

        public LinkedCollection<TEditorItem> Collection => new LinkedCollection<TEditorItem>(this);

        public LiveDatCollection(IOperationManager<TData, TEditorItem> operationManager) => _operationManager = operationManager;

        public LiveDatCollection(IOperationManager<TData, TEditorItem> operationManager, [NotNull] IEnumerable<TData> enumerable)
            : base(enumerable.Select(operationManager.CreatEditorItem)) => _operationManager = operationManager;

        public void ReplaceOrAdd(TData data)
        {
            var index = IndexOf(data);
            if (index == -1)
                Add(CreateItem(data));
            else
                SetItem(index, CreateItem(data));
        }

        public int IndexOf(TData data)
        {
            for (var i = 0; i < Count; i++)
            {
                var ele = this[i];
                if (ele?.Equals(data) ?? data == null) return i;
            }

            return -1;
        }

        public void Add(TData data) => Add(CreateItem(data));

        public void AddRange(IEnumerable<TData> data) => AddRange(data.Select(_operationManager.CreatEditorItem));

        public TEditorItem Get(TData data)
        {
            int index = IndexOf(data);
            return index == -1 ? default : this[index];
        }

        public bool Remove(TData data)
        {
            int index = IndexOf(data);
            if (index == -1)
                return false;

            RemoveAt(index);

            return true;
        }

        public bool Contains(TData data) => IndexOf(data) != -1;

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            if (item.IsNew)
            {
                RemoveInternal(item);
                lock (_gate)
                {
                    base.RemoveItem(index);
                    _dataCollection.RemoveAt(index);
                }
                return;
            }

            _operationManager.RemoveFromDatabase(item).ContinueWith(t =>
            {
                if (!t.Result) return;

                RemoveInternal(item);
                lock (_gate)
                {
                    base.RemoveItem(index);
                    _dataCollection.RemoveAt(index);
                }
            });

        }

        protected override void InsertItem(int index, TEditorItem item)
        {
            if (item.IsNew)
            {
                var args = new InsertCheckEventArgs<TEditorItem>(item);
                if (args.OverrideAdd)
                    _operationManager.FetchFromDatabase(item).ContinueWith(item.Update);
                else
                    _operationManager.SendToDatabase(item).ContinueWith(t => Add(t.Result));
            }

            if (index == Count)
                lock (_gate)
                {
                    base.InsertItem(index, item);
                    _dataCollection.Insert(index, item.Create());
                }
            else
                SetItem(index, item);
        }

        protected override void SetItem(int index, TEditorItem item)
        {
            var item2 = this[index];

            _operationManager.FetchFromDatabase(item).ContinueWith(t =>
            {
                if (t.Result == null) return;

                RemoveInternal(item2);
                lock (_gate)
                {
                    base.SetItem(index, CreateItem(t.Result));
                    _dataCollection[index] = t.Result;
                }
            });
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
                RemoveInternal(item);

            lock (_gate)
            {
                base.ClearItems();
                _dataCollection.Clear();
            }
        }

        public void ForceUpdateAsync(TData data) => Task.Run(() => ForceUpdate(data));

        private void ForceUpdate(TData data)
        {
            var index = IndexOf(data);
            if(index == -1) return;

            lock (_gate)
            {
                this[index].Update(Task.FromResult(data));
                _dataCollection[index] = data;
            }
        }

        private TEditorItem CreateItem(TData data)
        {
            var item = _operationManager.CreatEditorItem(data);
            item.ChangedEvent += OnChangedEvent;
            return item;
        }

        private void RemoveInternal(TEditorItem item) => item.ChangedEvent -= OnChangedEvent;

        private void OnChangedEvent(IEditorItem editorItem)
        {
            var item = (TEditorItem) editorItem;
            if (item.IsNew)
                _operationManager.SendToDatabase(item).ContinueWith(t =>
                {
                    Remove(item);
                    Add(t.Result);
                });
            else
            {
                var task = _operationManager.SendToDatabase((TEditorItem) editorItem);
                task.ContinueWith(t =>
                {
                    var index = IndexOf((TEditorItem)editorItem);
                    lock (_gate)
                        _dataCollection[index] = t.Result;
                });
                item.Update(task);
            }
        }

        protected void OnCheckInsertEvent(InsertCheckEventArgs<TEditorItem> e) => CheckInsertEvent?.Invoke(this, e);
    }
}