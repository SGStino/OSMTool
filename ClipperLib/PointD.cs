
/*******************************************************************************
* Author    :  Angus Johnson                                                   *
* Version   :  10.0 (beta)                                                     *
* Date      :  8 Noveber 2017                                                  *
* Website   :  http://www.angusj.com                                           *
* Copyright :  Angus Johnson 2010-2017                                         *
* Purpose   :  Offset paths                                                    *
* License   :  http://www.boost.org/LICENSE_1_0.txt                            *
*******************************************************************************/

using System.Collections.Generic;

namespace ClipperLib
{
    public struct PointD
    {
        public double X;
        public double Y;

        public PointD(double x = 0, double y = 0)
        {
            this.X = x; this.Y = y;
        }
        public PointD(PointD dp)
        {
            this.X = dp.X; this.Y = dp.Y;
        }
        public PointD(Point64 ip)
        {
            this.X = ip.X; this.Y = ip.Y;
        }
    } //PointD

} //namespace
