using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Data.Trees
{
    public class QuadTreeNode<T> : IObservable<QuadTreeEvent<T>>, IDisposable
        where T : IBoundsObject2D
    {
        private readonly Subject<QuadTreeEvent<T>> localEvents = new Subject<QuadTreeEvent<T>>();


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
                foreach (var item in items.Keys)
                    rect = rect.Combine(item.Bounds);

            return rect;
        }

        private void InvalidateBounds()
        {
            actualBounds = null;
            parent?.InvalidateBounds();
        }

        private QuadTreeNode<T>[] children;



        //private TinyItemCollection<T> items = new TinyItemCollection<T>();

        private Dictionary<T, IDisposable> items = new Dictionary<T, IDisposable>();

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
            list.AddRange(items.Keys.Where(t => t.Bounds.Overlaps(point, radius)));

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
            if (items.TryGetValue(item, out var disp))
            {
                items.Remove(item);
                disp?.Dispose();
                notifyEvent(QuadTreeEvent<T>.Removed(item));
                emptyCheck();
                InvalidateBounds();
                return true;
            }
            return false;
        }
        private bool isEmpty()
        {
            if (isSelfEmpty()) return false;
            var children = this.children;
            if (children == null) return true;
            for (int i = 0; i < 4; i++)
                if (!children[i].isEmpty()) return false;
            return true;
        }

        private bool isSelfEmpty()
        {
            if (items.Count > 0) return false;
            return true;
        }

        private void emptyCheck()
        {
            if (isEmpty())
            {
                this.items.Clear();
                parent?.NotifyEmpty(this);
            }
        }

        private void NotifyEmpty(QuadTreeNode<T> quadTreeNode)
        {
            if (this.children?.All(t => t == quadTreeNode || t.isEmpty()) == true && isSelfEmpty())
            {
                this.Clear();
                parent?.NotifyEmpty(this);
            }
        }

        public IEnumerable<T> Query(Rect bounds)
        {
            var list = new List<T>(items.Count);
            list.AddRange(items.Keys.Where(t => t.Bounds.Overlaps(bounds)));


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

            var subscription = Observable.FromEvent<BoundsChangedEvent>(h => item.BoundsChanged += h, h => item.BoundsChanged -= h).Subscribe(evt => itemMoved(evt, item));
            this.items.Add(item, subscription);
            notifyEvent(QuadTreeEvent<T>.Added(item));
            InvalidateBounds();
            return this;

        }

        private void itemMoved(BoundsChangedEvent evt, T item)
        {
            if (!Encapsulates(evt.Bounds))
            {
                var newNode = tree.UpdateInternal(item, this); // item moved to new node
            }
        }

        private void notifyEvent(QuadTreeEvent<T> evt)
        {
            localEvents.OnNext(evt);
            parent?.notifyEvent(evt);
        }

        internal bool Encapsulates(Rect bounds) => QuadTreeUtils.Encapsulates(maxBounds, bounds) && this.bounds.Contains(bounds.center);

        public IEnumerable<T> Items
        {
            get
            {
                var list = new List<T>(items.Count);
                list.AddRange(items.Keys);


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
            if (items != null)
                foreach (var item in items.Keys.ToArray())
                    Remove(item);

            if (children != null)
                foreach (var child in children)
                    child.Destroy();

            Dispose();
        }
        private bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                foreach (var child in children)
                    child.Dispose();

                localEvents.OnCompleted();

                children = null;
                parent = null;
                quadrant = -1;
            }
        }

        public IDisposable Subscribe(IObserver<QuadTreeEvent<T>> observer)
        {
            return localEvents.Subscribe(observer);
        }
    }
}
