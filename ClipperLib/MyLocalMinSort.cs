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
    internal class MyLocalMinSort : IComparer<LocalMinima>
    {
        public int Compare(LocalMinima lm1, LocalMinima lm2)
        {
            return lm2.Vertex.Pt.Y.CompareTo(lm1.Vertex.Pt.Y); //descending soft
        }
    }
    //------------------------------------------------------------------------------

} //namespace