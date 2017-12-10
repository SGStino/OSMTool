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
        private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        private readonly ISet<T> internalSet;
        public ThreadSafeSet(ISet<T> internalSet)
        {
            this.internalSet = internalSet;
        }

        public int Count => internalSet.Count;

        public bool IsReadOnly => internalSet.IsReadOnly;

        public bool Add(T item)
        {
            readerWriterLock.EnterWriteLock();
            try
            {
                return internalSet.Add(item);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            readerWriterLock.EnterWriteLock();
            try
            {
                internalSet.Clear();
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return internalSet.Contains(item);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                internalSet.CopyTo(array, arrayIndex);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            readerWriterLock.EnterWriteLock();
            try
            {
                internalSet.ExceptWith(other);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            T[] copy;
            readerWriterLock.EnterReadLock();
            try
            {
                copy = new T[internalSet.Count];
                internalSet.CopyTo(copy, 0);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
            return ((IEnumerable<T>)copy).GetEnumerator();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            readerWriterLock.EnterWriteLock();
            try
            {
                internalSet.IntersectWith(other);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return internalSet.IsProperSubsetOf(other);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return internalSet.IsProperSupersetOf(other);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return internalSet.IsSubsetOf(other);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return internalSet.IsSupersetOf(other);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return internalSet.Overlaps(other);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public bool Remove(T item)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return internalSet.Remove(item);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return internalSet.SetEquals(other);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                internalSet.SymmetricExceptWith(other);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                internalSet.UnionWith(other);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        void ICollection<T>.Add(T item)
        {
            readerWriterLock.EnterReadLock();
            try
            {
                internalSet.Add(item);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
