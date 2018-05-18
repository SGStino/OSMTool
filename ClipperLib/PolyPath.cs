/*******************************************************************************
* Author    :  Angus Johnson                                                   *
* Version   :  10.0 (beta)                                                     *
* Date      :  12 Noveber 2017                                                 *
* Website   :  http://www.angusj.com                                           *
* Copyright :  Angus Johnson 2010-2017                                         *
* Purpose   :  Base clipping module                                            *
* License   :  http://www.boost.org/LICENSE_1_0.txt                            *
*******************************************************************************/

using System.Collections.Generic;

namespace ClipperLib
{
    using Path = List<Point64>;
    using Paths = List<List<Point64>>;

    //------------------------------------------------------------------------------
    // PolyTree & PolyNode classes
    //------------------------------------------------------------------------------

    public class PolyPath
    {
        internal PolyPath parent;
        internal List<PolyPath> childs = new List<PolyPath>();
        internal Path path = new Path();

        //-----------------------------------------------------
        private bool IsHoleNode()
        {
            bool result = true;
            PolyPath node = parent;
            while (node != null)
            {
                result = !result;
                node = node.parent;
            }
            return result;
        }
        //-----------------------------------------------------

        internal PolyPath AddChild(Path p)
        {
            PolyPath child = new PolyPath();
            child.parent = this;
            child.path = p;
            Childs.Add(child);
            return child;
        }
        //-----------------------------------------------------

        public void Clear() { Childs.Clear(); }

        //the following two methods are really only for debugging ...

        private static void AddPolyNodeToPaths(PolyPath pp, Paths paths)
        {
            int cnt = pp.path.Count;
            if (cnt > 0)
            {
                Path p = new Path(cnt);
                foreach (Point64 ip in pp.path) p.Add(ip);
                paths.Add(p);
            }
            foreach (PolyPath polyp in pp.childs)
                AddPolyNodeToPaths(polyp, paths);
        }
        //-----------------------------------------------------

        public Paths PolyTreeToPaths()
        {
            Paths paths = new Paths();
            AddPolyNodeToPaths(this, paths);
            return paths;
        }
        //-----------------------------------------------------

        public Path Path { get { return path; } }

        public int ChildCount { get { return childs.Count; } }

        public List<PolyPath> Childs { get { return childs; } }

        public PolyPath Parent { get { return parent; } }

        public bool IsHole { get { return IsHoleNode(); } }
    }
    //------------------------------------------------------------------------------

} //namespace