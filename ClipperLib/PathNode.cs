
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

    using Path = List<Point64>;

    internal class PathNode
    {
        internal Path path;
        internal JoinType joinType;
        internal EndType endType;
        internal int lowestIdx;

        public PathNode(Path p, JoinType jt, EndType et)
        {
            joinType = jt;
            endType = et;

            int lenP = p.Count;
            if (et == EndType.Polygon || et == EndType.OpenJoined)
                while (lenP > 1 && p[lenP - 1] == p[0]) lenP--;
            else if (lenP == 2 && p[1] == p[0])
                lenP = 1;
            if (lenP == 0) return;

            if (lenP < 3 && (et == EndType.Polygon || et == EndType.OpenJoined))
            {
                if (jt == JoinType.Round) endType = EndType.OpenRound;
                else endType = EndType.OpenSquare;
            }

            path = new Path(lenP);
            path.Add(p[0]);

            Point64 lastIp = p[0];
            lowestIdx = 0;
            for (int i = 1; i < lenP; i++)
            {
                if (lastIp == p[i]) continue;
                path.Add(p[i]);
                lastIp = p[i];
                if (et != EndType.Polygon) continue;
                if (p[i].Y >= path[lowestIdx].Y &&
                  (p[i].Y > path[lowestIdx].Y || p[i].X < path[lowestIdx].X))
                    lowestIdx = i;
            }
            if (endType == EndType.Polygon && path.Count < 3) path = null;
        }
    } //PathNode

} //namespace
