using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.Data
{
    public class MultiDictionary<TKey, TValue> : ILookup<TKey, TValue>
    {
        private HashSet<TValue> values = new HashSet<TValue>();
        private Dictionary<TKey, ISet<TValue>> map = new Dictionary<TKey, ISet<TValue>>();

        public void Add(IEnumerable<TKey> keys, TValue value)
        {
            values.Add(value);
            foreach (var key in keys)
            {
                if (!map.TryGetValue(key, out var mapValues))
                    map[key] = mapValues = new HashSet<TValue>();
                mapValues.Add(value);
            }
        }

        public void Clear(IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
                map.Remove(key);
        }

        public bool Remove(IEnumerable<TKey> keys, TValue value)
        {
            bool removed = false;
            foreach (var key in keys)
            {
                if (map.TryGetValue(key, out var mapValues))
                    removed |= mapValues.Remove(value);
            }
            return removed;
        }

        public IEnumerable<TValue> this[TKey key] => map.TryGetValue(key, out var values) ? values : Enumerable.Empty<TValue>();

        public int Count => values.Count;

        public bool Contains(TKey key) => map.TryGetValue(key, out var values) ? values.Any() : false;

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() => map.Select<KeyValuePair<TKey, ISet<TValue>>, IGrouping<TKey, TValue>>(t => new Group(t)).GetEnumerator();


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Group : IGrouping<TKey, TValue>
        {
            private readonly KeyValuePair<TKey, ISet<TValue>> pair;

            public Group(KeyValuePair<TKey, ISet<TValue>> pair) : this()
            {
                this.pair = pair;
            }

            public TKey Key => pair.Key;

            public IEnumerator<TValue> GetEnumerator() => pair.Value.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
