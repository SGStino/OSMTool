using Simulation.Traffic.Utilities;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using UnityEngine;

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

        public void SetPosition(TrafficNode node, Point mouse)
        {
            this.node = node;
            this.mouse = mouse;
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (node == null) return;
            var Position = node?.Position ?? new UnityEngine.Vector3(0, 0, 0);
            var scale = manager.Scale;
            var height = manager.Height;
            var nodePoint = new Point(Position.x * scale, (height - Position.y) * scale);


            base.OnRender(drawingContext);


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
                var nodePoint1 = new Point(point1.x * scale, (height - point1.y) * scale);
                var point2 = otherB.Node.Position;
                var nodePoint2 = new Point(point2.x * scale, (height - point2.y) * scale);

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

                    var center = (seg.Start.Node.Position + seg.End.Node.Position) / 2;
                    var centerPoint = new Point(center.x * scale, (height - center.y) * scale);


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