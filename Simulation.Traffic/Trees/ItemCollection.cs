using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation.Traffic.Trees
{
    public class TinyItemCollection<T> : IReadOnlyCollection<T>
    {
        private static T[] empty = new T[0];
        private T[] array = empty;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public int Count => array.Length;

        public bool Add(T item)
        {
            try
            {
                semaphore.Wait();


                var index = Array.IndexOf(array, item);

                if (index < 0)
                {
                    var newArray = new T[array.Length + 1];
                    Array.Copy(array, newArray, array.Length);
                    newArray[array.Length] = item;
                    array = newArray;
                    if(array.Length > 100)
                    {
                        return true;

                    }
                    return true;
                }
                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)array).GetEnumerator();

        public bool Remove(T item)
        {
            try
            {
                semaphore.Wait();

                int index = Array.IndexOf(array, item);
                if (index >= 0)
                {
                    var newArray = new T[array.Length - 1];

                    if (index > 0)
                        Array.Copy(array, newArray, index);
                    int remainder = array.Length - index - 1;
                    if (remainder > 0)
                        Array.Copy(array, index + 1, newArray, index, remainder);
                    array = newArray;
                    return true;
                }
                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void Clear()
        {
            try
            {
                semaphore.Wait();
                array = empty;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
