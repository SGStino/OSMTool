using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic.Trees
{
    public delegate void BoundsChangedEventHandler(IBoundsObject2D item, Rect oldBounds, Rect newBounds);
    public interface IBoundsObject2D
    {
        Rect Bounds { get; }
    }

    public class QuadTree<T> : ISet<T>
        where T : IBoundsObject2D
    {
        private readonly Vector2 minSize;

        public QuadTreeNode<T> CurrentRoot => rootNode;

        public int Count => containmentDictionary.Count;

        public bool IsReadOnly => false;

        public Vector2 MinSize => minSize;

        public IEnumerable<T> Query(Vector2 point, float radius)
        {
            var root = rootNode;
            if (QuadTreeUtils.Overlaps(root.Bounds, point, radius))
            {
                return root.Query(point, radius);
            }
            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> Query(Rect bounds)
        {
            var root = rootNode;
            if (bounds.Overlaps(root.Bounds))
            {
                return root.Query(bounds);
            }
            return Enumerable.Empty<T>();
        }


        public IEnumerable<T> Items => rootNode.Items;

        public QuadTree(Rect bounds) : this(bounds, new Vector2(0.5f, 0.5f)) { }
        public QuadTree(Rect bounds, Vector2 minSize)
        {
            this.minSize = minSize;
            this.bounds = bounds;

            this.rootNode = new QuadTreeNode<T>(this, bounds);
        }
        private Dictionary<T, QuadTreeNode<T>> containmentDictionary = new Dictionary<T, QuadTreeNode<T>>();

        private QuadTreeNode<T> rootNode;
        private readonly Rect bounds;

        public bool Remove(T item)
        {
            lock (containmentDictionary)
            {
                if (containmentDictionary.TryGetValue(item, out var node))
                {
                    var result = node.Remove(item);
                    containmentDictionary.Remove(item);
                    return result;
                }
            }
            return false;
        }


        public QuadTreeNode<T> Update(T item)
        {
            if (containmentDictionary.TryGetValue(item, out var node))
            {
                if (!node.Bounds.Encapsulates(item.Bounds))
                {
                    node.Remove(item);
                    return AddInternal(item);
                }
            }
            return null;
        }

        public QuadTreeNode<T> Add(T item)
        {
            return Add(item, out var already);
        }
        public QuadTreeNode<T> Add(T item, out bool alreadyContained)
        {
            lock (containmentDictionary)
            {
                if (containmentDictionary.TryGetValue(item, out var node))
                {
                    alreadyContained = true;
                    return node;// already contained
                }
            }
            alreadyContained = false;
            return AddInternal(item);
        }

        private SemaphoreSlim syncGrow = new SemaphoreSlim(1);
        private QuadTreeNode<T> AddInternal(T item)
        {
            syncGrow.Wait();
            var node = rootNode;
            try
            {
                while (!node.Encapsulates(item.Bounds))
                {
                    node = Grow(node, item.Bounds);
                }
                rootNode = node;
            }
            finally
            {
                syncGrow.Release();
            }
            var containementNode = node.Add(item);
            lock (containmentDictionary)
                containmentDictionary.Add(item, containementNode);
            return containementNode;
        }

        private QuadTreeNode<T> Grow(QuadTreeNode<T> rootNode, Rect bounds)
        {
            var newBounds = QuadTreeUtils.GrowQuad(rootNode.Bounds, bounds.center, out int sourceIndex, out int index);


            var quads = QuadTreeUtils.DevideQuads(newBounds);

            var children = new QuadTreeNode<T>[4];
            for (int i = 0; i < 4; i++)
            {
                if (i == index)
                    children[i] = rootNode;
                else
                    children[i] = new QuadTreeNode<T>(this, quads[i]);
            }

            return new QuadTreeNode<T>(this, children, newBounds);
        }

        bool ISet<T>.Add(T item)
        {
            Add(item, out bool already);
            return already;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var item in other)
                Add(item);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other)
                Remove(item);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            var set = new HashSet<T>(other);
            return set.SetEquals(containmentDictionary.Keys);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return other.Intersect(containmentDictionary.Keys).Any();
        }

        void ICollection<T>.Add(T item) => Add(item);

        public void Clear()
        {
            rootNode.Clear();
            containmentDictionary.Clear();
        }

        public bool Contains(T item)
        {
            return containmentDictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in Items)
            {
                array[arrayIndex++] = item;
                if (arrayIndex >= array.Length) break;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
