using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public abstract class EditorItemBase : ObservableObject, IEditorItem
    {
        private struct ValueHolder
        {
            public object Value { get; }

            public ValueHolder(object value) => Value = value;

            public override bool Equals(object obj)
            {
                switch (Value)
                {
                    case null when obj == null:
                        return true;
                    case null:
                        return false;
                    default:
                        return Value.Equals(obj);
                }
            }

            public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        }

        public event Action<IEditorItem> ChangedEvent;
        public bool IsNew { get; protected set; }

        protected void OnChangedEvent(IEditorItem obj) => ChangedEvent?.Invoke(obj);

        private Dictionary<string, ValueHolder> _values = new Dictionary<string, ValueHolder>();
        private bool _updateMode;
        private object _lock = new object();

        protected IDisposable EnterUpdateMode()
        {
            _updateMode = true;
            return new ActionDispose(() => _updateMode = false);
        }

        protected TValue GetValue<TValue>([CallerMemberName] string name = null)
        {
            lock (_lock)
            {
                var holder = GetValueImpl(name);
                return holder.Value is TValue value ? value : default;
            }
        }

        private ValueHolder GetValueImpl(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _values.TryGetValue(name, out var value) ? value : default;
        }

        protected void SetValue<TValue>(TValue value, [CallerMemberName] string name = null)
        {
            lock (_lock)
            {
                if (name == null) throw new ArgumentNullException(nameof(name));

                if (GetValueImpl(name).Equals(value)) return;

                lock(this)
                    _values[name] = new ValueHolder(value);
                OnPropertyChanged(name);

                if (_updateMode)
                    return;
            }

            OnChangedEvent(this);
        }
    }
}