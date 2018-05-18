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
    public class Active
    {
        internal Point64 Bot;
        internal Point64 Curr;       //current (updated for every new Scanline)
        internal Point64 Top;
        internal double Dx;
        internal int WindDx;     //wind direction (ascending: +1; descending: -1)
        internal int WindCnt;    //current wind count
        internal int WindCnt2;   //current wind count of opposite TPathType
        internal OutRec OutRec;
        internal Active NextInAEL;
        internal Active PrevInAEL;
        internal Active NextInSEL;
        internal Active PrevInSEL;
        internal Active MergeJump;
        internal Vertex VertTop;
        internal LocalMinima LocalMin;
    };
    //------------------------------------------------------------------------------

} //namespace