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
    internal class Vertex
    {
        internal Point64 Pt;
        internal Vertex Next;
        internal Vertex Prev;
        internal VertexFlags Flags;
        public Vertex(Point64 ip) { Pt.X = ip.X; Pt.Y = ip.Y; }
    }
    //------------------------------------------------------------------------------

} //namespace