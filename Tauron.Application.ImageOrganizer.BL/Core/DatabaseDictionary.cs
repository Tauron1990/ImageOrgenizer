using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tauron.Application.ImageOrganizer.BL.Core
{
    public sealed class DatabaseDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly Action<TKey, TValue, DatabaseAction> _changeAction;
        private readonly IDictionary<TKey, TValue> _dictionary;

        /// <inheritdoc />
        public DatabaseDictionary([NotNull] Action<TKey, TValue, DatabaseAction> changeAction, [CanBeNull] IDictionary<TKey, TValue> dic)
        {
            _changeAction = changeAction;
            _dictionary = dic ?? new Dictionary<TKey, TValue>();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item.Key, item.Value);
            _changeAction(item.Key, item.Value, DatabaseAction.Add);
        }

        public void Clear()
        {
            foreach (var value in _dictionary)
                _changeAction(value.Key, value.Value, DatabaseAction.Remove);
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotSupportedException();

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!_dictionary.Remove(item)) return false;

            _changeAction(item.Key, item.Value, DatabaseAction.Remove);
            return true;
        }

        public int Count => _dictionary.Count;
        public bool IsReadOnly => _dictionary.IsReadOnly;

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            _changeAction(key, value, DatabaseAction.Add);
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var value)) return false;
            if (!_dictionary.Remove(key)) return false;

            _changeAction(key, value, DatabaseAction.Remove);
            return true;

        }

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                _changeAction(key, value, TryGetValue(key, out _) ? DatabaseAction.Update : DatabaseAction.Add);
                _dictionary[key] = value;
            }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
    }
}