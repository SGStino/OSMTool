/*******************************************************************************
* Author    :  Angus Johnson                                                   *
* Version   :  10.0 (beta)                                                     *
* Date      :  12 Noveber 2017                                                 *
* Website   :  http://www.angusj.com                                           *
* Copyright :  Angus Johnson 2010-2017                                         *
* Purpose   :  Base clipping module                                            *
* License   :  http://www.boost.org/LICENSE_1_0.txt                            *
*******************************************************************************/

using System;
using System.Collections.Generic;

namespace ClipperLib
{
    [Flags]
    internal enum VertexFlags { OpenStart = 1, OpenEnd = 2, LocMax = 4, LocMin = 8 };
    //------------------------------------------------------------------------------

} //namespace