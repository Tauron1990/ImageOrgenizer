using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImageOrganizer.Views.Models;
using Tauron.Application;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public abstract class EditorItemBase : ObservableObject, IEditorItem
    {
        public event Action<IEditorItem> ChangedEvent;
        public bool IsNew { get; protected set; }

        protected void OnChangedEvent(IEditorItem obj) => ChangedEvent?.Invoke(obj);

        private Dictionary<string, object> _values = new Dictionary<string, object>();
        private bool _updateMode;

        protected IDisposable EnterUpdateMode()
        {
            _updateMode = true;
            return new ActionDispose(() => _updateMode = false);
        }

        protected TValue GetValue<TValue>([CallerMemberName] string name = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (_values.TryGetValue(name, out var value)) return (TValue) value;

            return default;
        }

        protected void SetValue<TValue>(TValue value, [CallerMemberName] string name = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (GetValue<TValue>(name)?.Equals(value) ?? true) return;

            lock(this)
                _values[name] = value;
            OnPropertyChanged(name);

            if (_updateMode)
                return;

            OnChangedEvent(this);
        }
    }
}