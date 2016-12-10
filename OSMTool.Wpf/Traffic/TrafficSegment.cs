using Simulation.Traffic;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System;
using Simulation.Traffic.Utilities;
using UnityEngine;
using System.Linq;
using System.Globalization;

namespace OSMTool.Wpf.Traffic
{
    internal class TrafficSegment : Segment
    {
        private static Dictionary<string, double> laneWidths = new Dictionary<string, double>()
        {
            { "cycleway", 1.5 },
            { "pedestrian", 2.5 },
            { "footway", 1.5 },
            { "path", 2.2 },
            { "platform", 1.5 },
            { "yard", 1.5 },
            { "rail", 1.5 },
            { "tram", 1 }
        };

        private static Dictionary<string, Brush> laneColors = new Dictionary<string, Brush>()
        {
            { "cycleway", Brushes.Orange},
            { "pedestrian", Brushes.Gray },
            { "service", Brushes.WhiteSmoke },
            { "footway", Brushes.SandyBrown },
            { "path", Brushes.Brown },
            { "platform", Brushes.Gray },
            { "residential", Brushes.LightGray },
            { "rail", Brushes.Blue },
            { "yard", Brushes.DarkBlue },
            { "turntable", Brushes.DarkBlue },
            { "tram", Brushes.Green }
        };

        public TrafficSegment(SegmentDescription description, RoadManager manager) : base(description, manager)
        {
        }
        protected override void OnCreated()
        {
            base.OnCreated();
        }

        public void Update()
        {
            var dsc = (Description as TrafficSegmentDescription);
            var mgr = (Manager as CanvasRoadManager);
            var scale = mgr.Scale;
            var height = mgr.Drawing.Height;

            double laneWidth;

            if (!laneWidths.TryGetValue(dsc.Type, out laneWidth))
                laneWidth = 3.7;

            Brush laneColor;

            if (!laneColors.TryGetValue(dsc.Type, out laneColor))
                laneColor = Brushes.White;

            var start = new Point(this.Start.Node.Position.x * scale, height - this.Start.Node.Position.y * scale);
            var end = new Point(this.End.Node.Position.x * scale, height - this.End.Node.Position.y * scale);

            var layer1 = mgr.Drawing.GetLayer(DrawingLayer.Ways);
            var layer2 = mgr.Drawing.GetLayer(DrawingLayer.Outlines);


            string gauge;
            if (dsc.OsmWay.Tags.TryGetValue("gauge", out gauge))
                laneWidth = double.Parse(gauge, CultureInfo.InvariantCulture) * 0.001;

            bool isDouble = !dsc.IsOneWay && dsc.Type != "rail" && dsc.Type != "tram";
            if (isDouble)
            {
                laneWidth *= 2; // both ways
            }
            laneWidth *= dsc.Lanes;




            //var tangentStart = new Vector(this.Start.Tangent.x, -this.Start.Tangent.y);
            //var tangentEnd = new Vector(this.End.Tangent.x, -this.End.Tangent.y);


            //var center = getIntersection(start, tangentStart, end, tangentEnd);


            //var len = 0.40 * (start - end).Length;


            //var controlStart = start + tangentStart * len;
            //var controlEnd = end + tangentEnd * len;


            var pen = new Pen(laneColor, laneWidth * scale)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };

            //var p1 = new Vector2((float)start.X, (float)start.Y);
            //var p2 = new Vector2((float)end.X, (float)end.Y);

            //var t1 = new Vector2((float)tangentStart.X, (float)tangentStart.Y);
            //var t2 = -new Vector2((float)tangentEnd.X, (float)tangentEnd.Y);

            PathFigure figure;

            var elements = new List<PathSegment>();

            Arc a1;
            Arc a2;
            //BiarcInterpolation.Biarc(p1, t1, p2, t2, out a1, out a2);

            BiarcInterpolation.Biarc(Start, End, out a1, out a2);


            var cp = a1.Interpolate(1);
            var center = new Point(cp.x * scale, height - cp.y * scale);

            var size1 = new Size(a1.radius * scale, a1.radius * scale);
            var size2 = new Size(a2.radius * scale, a2.radius * scale);

            var sweep1 = a1.IsClockwise() ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
            var sweep2 = a2.IsClockwise() ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;



            if ((start - center).LengthSquared > 0.01)
            {
                if (a1.radius < 0.1)
                {

                }

                if (Math.Abs(a1.angle) < 0.1)
                    elements.Add(new LineSegment(center, true));
                else
                    elements.Add(new ArcSegment(center, size1, 0, false, sweep1, true));
            }


            if ((center - end).LengthSquared > 0.01)
            {
                if (a2.radius < 0.1)
                {
                    elements.Add(new LineSegment(end, true));
                }
                else {

                    if (Math.Abs(a2.angle) < 0.1)
                        elements.Add(new LineSegment(end, true));
                    else {
                        elements.Add(new ArcSegment(end, size2, 0, false, sweep2, true));
                    }
                }
            }


            //elements.Add(new LineSegment(center, true));
            // elements.Add(new LineSegment(end, true));

            /*
            int count1 = (int)Math.Ceiling(a1.arcLength);
            int count2 = (int)Math.Ceiling(a2.arcLength);

            var range1 = Enumerable.Range(1, count1).Select(i => i / (float)count1);
            var range2 = Enumerable.Range(1, count2).Select(i => i / (float)count2);

            var points1 = range1.Select(p => a1.Interpolate(p)).Select(n => new Point(n.x * scale, height - n.y * scale));
            var points2 = range2.Select(p => a2.Interpolate(p)).Select(n => new Point(n.x * scale, height - n.y * scale));

            elements.AddRange(points1.Select(p => new LineSegment(p, true)));
            elements.AddRange(points2.Select(p => new LineSegment(p, true)));

            if(elements.Count == 0)
            {
                elements.Add(new LineSegment(end, true));
            }
            */


            /*
            // TODO: premature optimization, tangents can be equal, but that doesn't mean they are on the same line
            if (false && Vector3.Dot(Start.Tangent, End.Tangent) < -0.9) // al
            {
                elements.Add(new LineSegment(end, true));
            }
            else {
                Arc a1;
                Arc a2;
                //BiarcInterpolation.Biarc(p1, t1, p2, t2, out a1, out a2);

                BiarcInterpolation.Biarc(Start, End, out a1, out a2);

                var p3 = a1.Interpolate(1);
                var center = new Point(p3.x, height - p3.y * scale);

                var angle1Great = false;// a1.IsGreatArc();
                var angle2Great = false;// a2.IsGreatArc();


                var size1 = new Size(a1.radius * scale, a1.radius * scale) ;
                var size2 = new Size(a2.radius * scale, a2.radius * scale);

                var sweep1 = !a1.IsClockwise() ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
                var sweep2 = !a2.IsClockwise() ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;

                 


                if (a1.radius > 0.1)
                {
                    if (a1.radius < 100000)
                        elements.Add(new ArcSegment(center, size1, 0, angle1Great, sweep1, true));
                    else
                        elements.Add(new LineSegment(center, true));
                }
                else
                    elements.Add(new LineSegment(end, true));
                if (a2.radius > 0.1)
                {
                    if (a2.radius < 100000)
                        elements.Add(new ArcSegment(end, size2, 0, angle2Great, sweep2, true));
                    else
                        elements.Add(new LineSegment(end, true));
                }
                else
                    elements.Add(new LineSegment(end, true));
            }
            */
            figure = new PathFigure(start, elements, false);

            //if (a1.radius > 1 && a2.radius > 1)
            //{
            //    figure = new PathFigure(start, new [] {
            //    new ArcSegment(center, size1,0, false, sweep1, true),
            //    new ArcSegment(end, size2,0, false, sweep2, true)
            //    }, false);
            //}
            //else
            //    figure = new PathFigure(start, new [] { new LineSegment(end, true) }, false);



            // figure = new PathFigure(start, new [] {
            //    new BezierSegment(controlStart, controlEnd, end, true) }, false);

            // var interpolationPoints = Enumerable.Range(1, 10).Select(i => i / 10.0f);
            // var arc1Points = interpolationPoints.Select(n => a1.Interpolate(n)).Select(n => new LineSegment(new Point(n.x, n.y), true));
            // var arc2Points = interpolationPoints.Select(n => a2.Interpolate(n)).Select(n => new LineSegment(new Point(n.x, n.y), true));

            // figure = new PathFigure(start,  arc1Points.Concat(arc2Points), false);
             
            var geometry = new PathGeometry(new[]
            {
                figure
            });
            geometry.Freeze();
            layer1.DrawGeometry(null, pen, geometry);

            if (isDouble)
            {

                var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 0));
                var layer3 = mgr.Drawing.GetLayer(DrawingLayer.Markers);
                layer3.DrawGeometry(null, new Pen(brush, 1)
                {
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round
                }, geometry);
            }



            //layer1.DrawLine(new Pen(laneColor, laneWidth * scale)
            //{
            //    StartLineCap = PenLineCap.Round,
            //    EndLineCap = PenLineCap.Round
            //}
            //, start, end);

            //layer2.DrawLine(new Pen(Brushes.Black, laneWidth * scale + 1)
            //{
            //    StartLineCap = PenLineCap.Round,
            //    EndLineCap = PenLineCap.Round
            //}, start, end);



        }

        private Point getIntersection(Point a, Vector d, Point b, Vector e)
        {



            var v = (a.Y * d.X - a.X * d.Y - b.Y * d.X + b.X * d.Y) / (d.X * e.Y - d.Y * e.X);

            var x = b.X + e.X * v;
            var y = b.Y + e.Y * v;

            return new Point(x, y);
        }
    }
}