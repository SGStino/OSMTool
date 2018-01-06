using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic.Trees
{
    public class QuadTreeNode<T>
        where T : IBoundsObject2D
    {
        private readonly Rect bounds;
        private readonly Rect maxBounds;
        private int quadrant;
        private QuadTreeNode<T> parent;

        private Rect? actualBounds;

        public Rect ActualBounds => actualBounds ?? (actualBounds = CalculateBounds()).Value;

        private Rect CalculateBounds()
        {
            var rect = this.bounds;

            var children = this.children;
            if (children != null)
                foreach (var c in children)
                    rect = rect.Combine(c.ActualBounds);

            if (items.Count > 0)
                foreach (var item in items)
                    rect = rect.Combine(item.Bounds);

            return rect;
        }

        private void InvalidateBounds()
        {
            actualBounds = null;
            parent?.InvalidateBounds();
        }

        private QuadTreeNode<T>[] children;



        private TinyItemCollection<T> items = new TinyItemCollection<T>();

        private readonly QuadTree<T> tree;

        public QuadTreeNode(QuadTree<T> tree, Rect bounds) : this(tree, null, bounds, -1) { }
        private QuadTreeNode(QuadTree<T> tree, QuadTreeNode<T> parent, Rect bounds, int quadrant)
        {
            this.quadrant = quadrant;
            this.tree = tree;
            this.parent = parent;
            this.bounds = bounds;

            var halfSize = bounds.size / 2;
            this.maxBounds = Rect.MinMaxRect(bounds.xMin - halfSize.x, bounds.yMin - halfSize.y, bounds.xMax + halfSize.x, bounds.yMax + halfSize.y);
        }

        public IEnumerable<T> Query(Vector2 point, float radius)
        {
            var list = new List<T>(items.Count);
            list.AddRange(items.Where(t => t.Bounds.Overlaps(point, radius)));

            var children = this.children;
            if (children != null)
                for (int i = 0; i < 4; i++)
                {
                    var child = children[i];
                    if (child.ActualBounds.Overlaps(point, radius))
                        list.AddRange(child.Query(point, radius));
                }
            return list;
            //if (items.Count > 0)
            //    foreach (var item in items)
            //        if (item.Bounds.Overlaps(point, radius))
            //            yield return item;

            //foreach (var item in GetChildItems(t => t.Overlaps(point, radius), t => t.Query(point, radius)))
            //    yield return item;
        }

        public bool Remove(T item)
        {
            var result = items.Remove(item);

            if (result)
            {
                emptyCheck();
                InvalidateBounds();
            }
            return result;
        }

        private void emptyCheck()
        {
            if (items.Count > 0)
            {
                this.Clear();
                parent?.NotifyEmpty(this);
            }
        }

        private void NotifyEmpty(QuadTreeNode<T> quadTreeNode)
        {
            if (this.children?.All(t => t == quadTreeNode || !t.Items.Any()) == true)
            {
                this.Clear();
                parent?.NotifyEmpty(this);
            }
        }

        public IEnumerable<T> Query(Rect bounds)
        {
            var list = new List<T>(items.Count);
            list.AddRange(items.Where(t => t.Bounds.Overlaps(bounds)));


            var children = this.children;
            if (children != null)
                for (int i = 0; i < 4; i++)
                {
                    var child = children[i];
                    if (child.ActualBounds.Overlaps(bounds))
                        list.AddRange(child.Query(bounds));
                }
            return list;
            //if (items.Count > 0)
            //    foreach (var item in items)
            //        if (item?.Bounds.Overlaps(bounds) == true)
            //            yield return item;

            //foreach (var item in GetChildItems(t => t.Overlaps(bounds), t => t.Query(bounds)))
            //    yield return item;
        }

        public QuadTreeNode(QuadTree<T> tree, QuadTreeNode<T>[] children, Rect newBounds) : this(tree, newBounds)
        {
            this.children = children;
            for (int i = 0; i < 4; i++)
            {
                children[i].quadrant = i;
                children[i].parent = this;
            }
        }

        private QuadTreeNode<T>[] CreateChildren()
        {
            var rects = QuadTreeUtils.DevideQuads(Bounds);
            var children = new QuadTreeNode<T>[rects.Length];
            for (int i = 0; i < children.Length; i++)
                children[i] = new QuadTreeNode<T>(tree, this, rects[i], i);
            return children;
        }


        private object childrenLock = new object();
        internal QuadTreeNode<T> Add(T item)
        {
            var index = QuadTreeUtils.GetQuadIndex(Bounds, item.Bounds.center);

            if (bounds.size.x > tree.MinSize.x && bounds.size.y > tree.MinSize.y)
            {
                if (children == null)
                {
                    lock (childrenLock)
                        if (children == null)
                            children = CreateChildren();
                }

                if (children[index].Encapsulates(item.Bounds))
                    return children[index].Add(item);
            }
            this.items.Add(item);
            InvalidateBounds();
            return this;

        }

        internal bool Encapsulates(Rect bounds) => QuadTreeUtils.Encapsulates(maxBounds, bounds) && this.bounds.Contains(bounds.center);

        public IEnumerable<T> Items
        {
            get
            {
                var list = new List<T>(items.Count);
                list.AddRange(items);


                var children = this.children;
                if (children != null)
                    for (int i = 0; i < 4; i++)
                        list.AddRange(children[i].Items);

                if (list.Any(t => t == null))
                    throw new InvalidOperationException(" got nullz");

                return list;
                //if (items.Any())
                //    foreach (var item in items)
                //        yield return item;

                //foreach (var item in GetChildItems(t => true, t => t.Items))
                //    yield return item;
            }
        }


        public Rect Bounds => bounds;

        public int Quadrant { get => quadrant; }
        public QuadTreeNode<T> Parent { get => parent; }
        public QuadTreeNode<T>[] Children { get => children; }

        internal void Clear()
        {
            if (children != null)
                lock (childrenLock)
                {
                    if (children != null)
                        foreach (var child in children)
                            child.Destroy();
                    children = null;
                }
            items.Clear();
        }

        private void Destroy()
        {
            children = null;
            parent = null;
            quadrant = -1;

        }
    }
}
