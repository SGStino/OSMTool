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


            var geometry = new StreamGeometry();

            //if (dsc.OsmWay.Id == 198204219 && (End.Node as TrafficNode).OSMNode.Id == 2083678228) {
            { 
                using (var context = geometry.Open())
                {
                    context.BeginFigure(start, false, false);


                    if ((start - center).LengthSquared > 0.01)
                    {
                        if (Math.Abs(a1.angle) < 0.1)
                            context.LineTo(center, true, false);
                        else
                            context.ArcTo(center, size1, 0, a1.IsGreatArc(), sweep1, true, false);
                    }


                    if ((center - end).LengthSquared > 0.01)
                    {
                        if (a2.radius < 0.1)
                        {
                            context.LineTo(end, true, false);
                        }
                        else {

                            if (Math.Abs(a2.angle) < 0.1)
                                context.LineTo(end, true, false);
                            else {
                                context.ArcTo(end, size2, 0, a1.IsGreatArc(), sweep2, true, false);
                            }
                        }
                    }

                    context.LineTo(end, true, true);
                }

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