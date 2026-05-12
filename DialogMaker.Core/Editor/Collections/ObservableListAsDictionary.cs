using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Editor.Collections
{
    public class ObservableListAsDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged
    {
        public ObservableListAsDictionary()
        {
            ValuesCollection.CollectionChanged += OnValuesCollectionCollectionChanged;
        }
        ~ObservableListAsDictionary()
        {
            ValuesCollection.CollectionChanged -= OnValuesCollectionCollectionChanged;
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public EditableCollection<KeyValuePair<TKey, TValue>> ValuesCollection { get; } = [];
        public TValue this[TKey key]
        {
            get
            {
                foreach (var pair in ValuesCollection)
                {
                    if (Equals(pair.Key, key))
                    {
                        return pair.Value;
                    }
                }

                throw new ArgumentException("Неизвестный ключ", nameof(key));
            }
            set
            {
                for (int i = 0; i < ValuesCollection.Count; i++)
                {
                    var pair = ValuesCollection[i];

                    if (Equals(pair.Key, key))
                    {
                        ValuesCollection[i] = new(key, value);
                        return;
                    }
                }

                throw new ArgumentException("Неизвестный ключ", nameof(key));
            }
        }
        public Collection<TKey> Keys
        {
            get
            {
                field ??= new KeyCollection(ValuesCollection);
                return field;
            }
        }
        public Collection<TValue> Values
        {
            get
            {
                field ??= new ValueCollection(ValuesCollection);
                return field;
            }
        }
        public int Count => ValuesCollection.Count;
        public bool IsReadOnly => false;
        
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        #region Управление

        public bool ContainsKey(TKey key)
        {
            foreach (var pair in ValuesCollection)
            {
                if (Equals(pair.Key, key))
                {
                    return true;
                }
            }

            return false;
        }
        public bool Contains(KeyValuePair<TKey, TValue> item) => ValuesCollection.Contains(item);

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException("Ключ уже занят", nameof(key));
            }

            ValuesCollection.Add(new(key, value));
        }
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (ContainsKey(item.Key))
            {
                throw new ArgumentException("Ключ уже занят", nameof(item));
            }

            ValuesCollection.Add(item);
        }

        public void Clear()
        {
            ValuesCollection.Clear();
        }
        public bool Remove(TKey key)
        {
            for (int i = 0; i < ValuesCollection.Count; i++)
            {
                if (Equals(ValuesCollection[i].Key, key))
                {
                    ValuesCollection.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ValuesCollection.Remove(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ValuesCollection.CopyTo(array, arrayIndex);
        }

        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue value)
        {
            value = default!;

            foreach (var pair in ValuesCollection)
            {
                if (Equals(pair.Key, key))
                {
                    value = pair.Value!;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Перечисление

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ValuesCollection.GetEnumerator();
        }

        #endregion

        #region События

        private void OnValuesCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        #endregion

        #region Классы

        public abstract class Collection<T> : ICollection<T>
        {
            public abstract int Count { get; }
            public bool IsReadOnly => true;

            #region Управление

            public abstract bool Contains(T item);
            public abstract void CopyTo(T[] array, int arrayIndex);

            public void Add(T item)
            {
                throw new InvalidOperationException("Невозможно изменить список");
            }
            public void Clear()
            {
                throw new InvalidOperationException("Невозможно изменить список");
            }
            public bool Remove(T item)
            {
                throw new InvalidOperationException("Невозможно изменить список");
            }

            public int IndexOf(T item)
            {
                int i = 0;

                foreach (var element in this)
                {
                    if (Equals(element, item))
                    {
                        return i;
                    }

                    i++;
                }

                return -1;
            }

            #endregion

            #region Перечисление

            public abstract IEnumerator<T> GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }
        private class KeyCollection(EditableCollection<KeyValuePair<TKey, TValue>> values) : Collection<TKey>
        {
            public override int Count => values.Count;

            public override bool Contains(TKey item)
            {
                foreach (var pair in values)
                {
                    if (Equals(pair.Key, item))
                    {
                        return true;
                    }
                }

                return false;
            }
            public override void CopyTo(TKey[] array, int arrayIndex)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    array[arrayIndex] = values[i].Key;
                    arrayIndex++;

                    if (arrayIndex >= array.Length)
                    {
                        return;
                    }
                }
            }

            public override IEnumerator<TKey> GetEnumerator()
            {
                foreach (var pair in values)
                {
                    yield return pair.Key;
                }
            }
        }
        private class ValueCollection(EditableCollection<KeyValuePair<TKey, TValue>> values) : Collection<TValue>
        {
            public override int Count => values.Count;

            public override bool Contains(TValue item)
            {
                foreach (var pair in values)
                {
                    if (Equals(pair.Value, item))
                    {
                        return true;
                    }
                }

                return false;
            }
            public override void CopyTo(TValue[] array, int arrayIndex)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    array[arrayIndex] = values[i].Value;
                    arrayIndex++;

                    if (arrayIndex >= array.Length)
                    {
                        return;
                    }
                }
            }

            public override IEnumerator<TValue> GetEnumerator()
            {
                foreach (var pair in values)
                {
                    yield return pair.Value;
                }
            }
        }

        #endregion
    }
}
