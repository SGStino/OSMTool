using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.Utilities
{
 
    public class Sequence<T> : IList<T>
    {

        private T currentItem;
        private int currentIndex = 0;
        private readonly IList<T> path;

        public Sequence(IList<T> path)
        {
            this.currentItem = path.FirstOrDefault();
            this.currentIndex = 0;
            this.path = path;
        }

        public T this[int index]
        {
            get { return path[index]; }
            set { path[index] = value; }
        }

        public T CurrentItem => currentItem;

        public int Count => path.Count;

        public bool IsReadOnly => path.IsReadOnly;

        public void Add(T item)
        {
            path.Add(item);
        }

        public void Clear()
        {
            path.Clear();
        }

        public bool Contains(T item)
        {
            return path.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            path.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return path.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return path.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (index <= currentIndex)
                SetIndex(currentIndex + 1);

            path.Insert(index, item);
        }

        public bool Next()
        {
            if (currentIndex + 1 < path.Count)
            {
                SetIndex(currentIndex + 1);
                return true;
            }
            return false;
        }
        public bool Previous()
        {
            if (currentIndex > 0)
            {
                SetIndex(currentIndex - 1);
                return true;
            }
            return false;
        }

        public bool Remove(T item)
        {
            int index = path.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            path.RemoveAt(index);
            if (index < currentIndex)
                SetIndex(currentIndex - 1);
            else if (index == currentIndex)
                SetIndex(index);
        }

        private void SetIndex(int index)
        {
            if (index < 0) index = 0;
            if (index >= Count)
                index = Count - 1;
            currentIndex = index;
            currentItem = this[currentIndex];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return path.GetEnumerator();
        }

        public bool IsFirst() => currentIndex == 0;
        public bool IsLast() => currentIndex == Count - 1;
    }
}
