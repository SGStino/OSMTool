using Simulation.Traffic;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System;
using UnityEngine;
using System.Linq;
using System.Globalization;

namespace OSMTool.Wpf.Traffic
{
    internal class TrafficSegment : AISegment
    {

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


        public void RenderAIPaths(System.Windows.Media.DrawingContext drawingContext)
        {
            if (Start == null || End == null) return;
            var dsc = (Description as TrafficSegmentDescription);
            var mgr = (Manager as CanvasRoadManager);
            var scale = mgr.Scale;
            var height = mgr.Drawing.Height;

            var laneColor = Brushes.Red;
            var pen = new Pen(laneColor, 0.5f * scale)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            pen.Freeze();


            float offset = 5 * (float)Math.Sin((DateTime.Now - DateTime.Today).TotalSeconds);


            //drawingContext.DrawLine(pen, start, end);

            foreach (var path in AIPaths)
            {
                DrawArc(drawingContext, scale, height, pen, path.PathOffsetStart);
            }
        }

        private void DrawArc(DrawingContext drawingContext, float scale, double height, Pen pen, float offset)
        {
            Arc a1;
            Arc a2;
            //BiarcInterpolation.Biarc(p1, t1, p2, t2, out a1, out a2);

            BiarcInterpolation.Biarc(Start, End, out a1, out a2);




            var cp = a1.Interpolate(1, offset);

            var sp = a1.Interpolate(0, offset);

            var ep = a2.Interpolate(1, offset);


            var start = new Point(sp.x * scale, height - sp.y * scale);
            var end = new Point(ep.x * scale, height - ep.y * scale);

            var center = new Point(cp.x * scale, height - cp.y * scale);

            var r1 = Math.Max(0, a1.radius + (a1.IsClockwise() ? offset : -offset));
            var r2 = Math.Max(0, a2.radius + (a2.IsClockwise() ? offset : -offset));

             
            var size1 = new Size(r1 * scale, r1 * scale);
            var size2 = new Size(r2 * scale, r2 * scale);

            var sweep1 = a1.IsClockwise() ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
            var sweep2 = a2.IsClockwise() ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;


            var geometry = new StreamGeometry();
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
                    else
                    {

                        if (Math.Abs(a2.angle) < 0.1)
                            context.LineTo(end, true, false);
                        else
                        {
                            context.ArcTo(end, size2, 0, a1.IsGreatArc(), sweep2, true, false);
                        }
                    }
                }

                context.LineTo(end, true, true);
            }

            geometry.Freeze();
            drawingContext.DrawGeometry(null, pen, geometry);
        }

        public void Update()
        {
            var dsc = (Description as TrafficSegmentDescription);
            var mgr = (Manager as CanvasRoadManager);
            var scale = mgr.Scale;
            var height = mgr.Drawing.Height;


            Brush laneColor;

            if (!laneColors.TryGetValue(dsc.Type, out laneColor))
                laneColor = Brushes.White;

            var start = new Point(this.Start.Node.Position.x * scale, height - this.Start.Node.Position.z * scale);
            var end = new Point(this.End.Node.Position.x * scale, height - this.End.Node.Position.z * scale);

            var layer1 = mgr.Drawing.GetLayer(DrawingLayer.Ways);
            var layer2 = mgr.Drawing.GetLayer(DrawingLayer.Outlines);


            double laneWidth = dsc.Lanes.Sum(w => w.Width);


            bool isDouble = !dsc.IsOneWay && dsc.Type != "rail" && dsc.Type != "tram";





            var pen = new Pen(laneColor, laneWidth * scale)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            pen.Freeze();


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
                        else
                        {

                            if (Math.Abs(a2.angle) < 0.1)
                                context.LineTo(end, true, false);
                            else
                            {
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
                    brush.Freeze();
                    pen = new Pen(brush, 1)
                    {
                        StartLineCap = PenLineCap.Round,
                        EndLineCap = PenLineCap.Round
                    };
                    pen.Freeze();
                    var layer3 = mgr.Drawing.GetLayer(DrawingLayer.Markers);
                    layer3.DrawGeometry(null, pen, geometry);
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