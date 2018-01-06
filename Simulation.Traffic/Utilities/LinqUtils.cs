using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation.Traffic.Utilities
{
    public static class LinqUtils
    {
        public static IEnumerable<V> CombineWith<T, U, V>(this IEnumerable<T> first, IEnumerable<U> second, Func<T, U, V> combine)
        {
            foreach (var item1 in first)
                foreach (var item2 in second)
                    yield return combine(item1, item2);
        }

        public static IDictionary<TKey, TValue> WithDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TKey, TValue> value)
        {
            return new DefaultDictionary<TKey, TValue>(dictionary, value);
        }
    }

    internal class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IDictionary<TKey, TValue> dictionary;
        private Func<TKey, TValue> value;

        public DefaultDictionary(IDictionary<TKey, TValue> dictionary, Func<TKey, TValue> value)
        {
            this.dictionary = dictionary;
            this.value = value;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (dictionary.TryGetValue(key, out value))
                    return value;
                return dictionary[key] = this.value(key);
            }

            set { dictionary[key] = value; }
        }

        public ICollection<TKey> Keys => dictionary.Keys;

        public ICollection<TValue> Values => dictionary.Values;

        public int Count => dictionary.Count;

        public bool IsReadOnly => dictionary.IsReadOnly;

        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            dictionary.Add(item);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            dictionary.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            return dictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Remove(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }
}
