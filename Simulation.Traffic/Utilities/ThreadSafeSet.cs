using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Simulation.Traffic.Utilities
{
    public class ThreadSafeSet<T> : ISet<T>
    {
        private ReaderWriterLock readerWriterLock = new ReaderWriterLock();

        private readonly ISet<T> internalSet;
        public ThreadSafeSet(ISet<T> internalSet)
        {
            this.internalSet = internalSet;
        }

        public int Count => internalSet.Count;

        public bool IsReadOnly => internalSet.IsReadOnly;

        public bool Add(T item)
        {
            readerWriterLock.AcquireWriterLock(0);
            try
            {
                return internalSet.Add(item);
            }
            finally
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public void Clear()
        {
            readerWriterLock.AcquireWriterLock(0);
            try
            {
                internalSet.Clear();
            }
            finally
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public bool Contains(T item)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                return internalSet.Contains(item);
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                internalSet.CopyTo(array, arrayIndex);
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            readerWriterLock.AcquireWriterLock(0);
            try
            {
                internalSet.ExceptWith(other);
            }
            finally
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            T[] copy;
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                copy = new T[Count];
                CopyTo(copy, 0);
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
            return ((IEnumerable<T>)copy).GetEnumerator();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            readerWriterLock.AcquireWriterLock(0);
            try
            {
                internalSet.IntersectWith(other);
            }
            finally
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                return internalSet.IsProperSubsetOf(other);
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                return internalSet.IsProperSupersetOf(other);
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                return internalSet.IsSubsetOf(other);
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                return internalSet.IsSupersetOf(other);
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                return internalSet.Overlaps(other);
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public bool Remove(T item)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                return internalSet.Remove(item);
            }
            finally
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                return internalSet.SetEquals(other);
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                internalSet.SymmetricExceptWith(other);
            }
            finally
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                internalSet.UnionWith(other);
            }
            finally
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        void ICollection<T>.Add(T item)
        {
            readerWriterLock.AcquireReaderLock(0);
            try
            {
                internalSet.Add(item);
            }
            finally
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
