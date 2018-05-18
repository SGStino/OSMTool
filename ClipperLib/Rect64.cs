using System;
using System.Collections.Generic;
using System.Text;

namespace ClipperLib
{
    public struct Rect64
    {
        public Int64 left;
        public Int64 top;
        public Int64 right;
        public Int64 bottom;

        public Rect64(Int64 l, Int64 t, Int64 r, Int64 b)
        {
            this.left = l; this.top = t;
            this.right = r; this.bottom = b;
        }
        public Rect64(Rect64 r)
        {
            this.left = r.left; this.top = r.top;
            this.right = r.right; this.bottom = r.bottom;
        }
    } //Rect64


}
