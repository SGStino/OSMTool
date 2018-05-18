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
    //OutRec: contains a path in the clipping solution. Edges in the AEL will
    //carry a pointer to an OutRec when they are part of the clipping solution.
    public class OutRec
    {
        internal int IDx;
        internal OutRec Owner;
        internal Active StartE;
        internal Active EndE;
        internal OutPt Pts;
        internal PolyPath PolyPath;
        internal OutrecFlag Flag;
    };
    //------------------------------------------------------------------------------

} //namespace