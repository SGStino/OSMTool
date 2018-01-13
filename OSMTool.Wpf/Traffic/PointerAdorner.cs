using Simulation.Traffic;
using Simulation.Traffic.AI;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using Simulation.Traffic.Lofts;
using System.Collections.Generic;

namespace OSMTool.Wpf.Traffic
{
    internal class PointerAdorner : Adorner
    {
        private Point mouse;
        private TrafficNode node;
        private CanvasRoadManager manager;

        public TrafficNode Node => node;


        public PointerAdorner(CanvasRoadManager manager, UIElement element) : base(element)
        {
            this.manager = manager;
        }
        private CancellationTokenSource cancel;
        public async void SetPosition(TrafficNode node, Point mouse)
        {
            cancel?.Cancel();
            this.node = node;
            this.mouse = mouse;
            this.InvalidateVisual();
        }

        public void RenderAIPaths(System.Windows.Media.DrawingContext drawingContext, IEnumerable<IAIPath> paths, Brush color, Pen connector)
        {
            var mgr = manager;
            var scale = mgr.Scale;
            var height = mgr.Drawing.Height;

            var laneColor = color;
            var pen = new Pen(laneColor, 0.125f * scale)
            {
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat
            };
            pen.Freeze();
            var pen2 = new Pen(laneColor, 0.125f * scale * 0.5f)
            {
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat
            };
            pen2.Freeze();


            float offset = 5 * (float)Math.Sin((DateTime.Now - DateTime.Today).TotalSeconds);


            //drawingContext.DrawLine(pen, start, end);

            float time = DateTime.Now.Millisecond / 1000.0f;

            foreach (var path in paths)
            {
                if (path.Path == null) continue;
                //DrawArc(drawingContext, scale, height, pen, path.PathOffsetStart);


                var len = path.Path.Length - path.PathOffsetStart - path.PathOffsetEnd;

                var t = path.Reverse ? 1 - time : time;
                //for (float i = path.PathOffsetStart; i < len; i++)
                //{
                //    if (i + t < len)
                //    {
                //        Vector3 pos = new Vector3(Mathf.Lerp(path.SideOffsetStart, path.SideOffsetEnd, (i - path.PathOffsetStart) / len), 0, 0);
                //        var p = path.Path.GetTransformedPoint(t + i, pos);

                //        drawDot(drawingContext, p, scale, height, color, null);
                //    }
                //}


                int count = 1 + (int)(len * 4);

                count = Math.Min(100, count);
                var points = Enumerable.Range(0, count + 1)
                    .Select(n => n / (float)count)
                    .Select(n => path.Path.GetTransformedPoint(path.GetOffsetPercentual(n), new Vector3(Mathf.Lerp(path.SideOffsetStart, path.SideOffsetEnd, n), 0, 0)))
                    .Select(p => new Point(p.x * scale, height - p.z * scale))
                    .ToArray();


                var first = points.FirstOrDefault();
                bool draw = true;
                int pI = 0;
                foreach (var point in points.Skip(1))
                {

                    var s = ((double)pI++ / points.Length);

                    var w = draw ? 0.125 : 0.25;
                    var p = new Pen(color, 15 * w * s);


                    drawingContext.DrawLine(p, first, point);


                    draw = !draw;
                    first = point;
                }

                if (connector != null)
                {

                    Vector3 pos = new Vector3(path.Reverse ? path.SideOffsetStart : path.SideOffsetEnd, 0, 0);
                    var p = path.Path.GetTransformedPoint(path.GetEndOffset(), pos);

                    //if (path.Reverse)
                    //    drawDot(drawingContext, p, scale, height, Brushes.Cyan, connector);
                    //else
                    //    drawDot(drawingContext, p, scale, height, Brushes.Magenta, connector);


                    var segPath = path as SegmentAIPath;
                    if (segPath != null)
                    {

                        var point = new Point(p.x * scale, height - p.z * scale);
                        drawingContext.DrawText(new FormattedText(segPath.Lane.Turn.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 5, Brushes.Red), point);
                    }

                }
            }
        }

        private void drawDot(DrawingContext drawingContext, Vector3 pos, float scale, double height, Brush color, Pen pen)
        {
            var center = new Point(pos.x * scale, height - pos.z * scale);

            drawingContext.DrawEllipse(color, pen, center, 1, 1);
        }



        protected override void OnRender(DrawingContext drawingContext)
        {
            if (node == null) return;









            foreach (TrafficSegmentNodeConnection seg in node.Segments)
            {
                RenderAIPaths(drawingContext, seg.Segment.AIRoutes.SelectMany(t => t.Paths), Brushes.Red, new Pen(Brushes.Lime, 1));
                RenderAIPaths(drawingContext, seg.AIRoutes.SelectMany(t => t.Paths), Brushes.Blue, null);
            }

            this.InvalidateVisual();


            var Position = node?.Position ?? new UnityEngine.Vector3(0, 0, 0);
            var scale = manager.Scale;
            var height = manager.Height;
            var nodePoint = new Point(Position.x * scale, (height - Position.z) * scale);


            base.OnRender(drawingContext);

            int i = 0;
            foreach (var con in node.Segments) {
                i++;
                var r = i % 3 == 0 ? (byte)0xff : (byte)0x00;
                var g = i % 3 == 1 ? (byte)0xff : (byte)0x00;
                var b = i % 3 == 2 ? (byte)0xff : (byte)0x00;
                var w = con.Segment.GetWidth();
                var sPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x22, r, g, b)), w * scale);

                var point = (con.GetPosition()) ;

                var p1 = new Point(point.x * scale, (height - point.z) * scale);

                drawingContext.DrawLine(sPen, nodePoint, p1);
            }


            Pen pen = null;

            if (node.VisitedAsLine)
                pen = new Pen(Brushes.Yellow, 1);
            if (node.VisitedAsIntersection)
                pen = new Pen(Brushes.Magenta, 1);
            if (node.IsDeletionPossible)
                pen = new Pen(Brushes.Red, 2);

            drawingContext.DrawEllipse(node?.IsMarkedForDeletion ?? false ? Brushes.Red : Brushes.Lime, pen, nodePoint, 10, 10);




            drawingContext.DrawLine(new Pen(Brushes.Blue, 1), nodePoint, mouse);



            if (node.Segments.Count() == 2)
            {
                var a = node.Segments.First();
                var b = node.Segments.Last();
                var otherA = a.Segment.Start == a ? a.Segment.End : a.Segment.Start;
                var otherB = b.Segment.Start == b ? b.Segment.End : b.Segment.Start;

                var point1 = otherA.Node.Position;
                var nodePoint1 = new Point(point1.x * scale, (height - point1.z) * scale);
                var point2 = otherB.Node.Position;
                var nodePoint2 = new Point(point2.x * scale, (height - point2.z) * scale);

                Arc arc2;
                Arc arc1;
                BiarcInterpolation.Biarc(otherA, otherB, out arc1, out arc2);

                float snap;


                if (arc1.GetClosestPoint(node.Position, true, 0, out snap))
                {
                    var closestPoint = arc1.Interpolate(snap);
                    var nodeCp = new Point(closestPoint.x * scale, (height - closestPoint.y) * scale);

                    drawingContext.DrawLine(new Pen(Brushes.Lime, 2), nodeCp, nodePoint);
                }
                if (arc2.GetClosestPoint(node.Position, true, 0, out snap))
                {
                    var closestPoint = arc2.Interpolate(snap);
                    var nodeCp = new Point(closestPoint.x * scale, (height - closestPoint.y) * scale);

                    drawingContext.DrawLine(new Pen(Brushes.Magenta, 2), nodeCp, nodePoint);
                }


                drawingContext.DrawLine(new Pen(Brushes.HotPink, 2), nodePoint1, nodePoint2);


            }


            if (node != null)
                foreach (var con in node.Segments)
                {
                    var seg = con.Segment as TrafficSegment;
                    var dsc = seg.Description as TrafficSegmentDescription;

                    if (seg.End == null || seg.Start == null) continue;

                    var center = (seg.Start.Node.Position + seg.End.Node.Position) / 2;
                    var centerPoint = new Point(center.x * scale, (height - center.z) * scale);


                    var sb = new StringBuilder();

                    sb.Append(dsc.Type);
                    sb.Append(" ");
                    sb.Append(dsc.LaneCount);
                    if (dsc.IsOneWay)
                        sb.Append(" OneWay");

                    sb.AppendLine();

                    sb.Append(dsc.Name);
                    sb.AppendLine();

                    sb.Append(dsc.OsmWay?.Id?.ToString() ?? "<custom>");
                    sb.AppendLine();

                    var text = new FormattedText(sb.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 10, Brushes.Black);
                    drawingContext.DrawText(text, centerPoint);

                    sb = new StringBuilder();
                    foreach (var tag in dsc.OsmWay.Tags)
                    {
                        sb.Append(tag.Key);
                        sb.Append(" = ");
                        sb.Append(tag.Value);
                        sb.AppendLine();
                    }

                    centerPoint.Y += text.Height;

                    drawingContext.DrawText(new FormattedText(sb.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 7.5, Brushes.Black), centerPoint);

                    drawingContext.DrawText(new FormattedText(node.OSMNode?.Id?.ToString() ?? "<custom>", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 7.5, Brushes.Black), nodePoint);
                }
        }
    }
}