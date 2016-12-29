using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Traffic;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace OSMTool.Wpf.Traffic
{
    public class TrafficSegmentNodeConnection : SegmentNodeConnection
    {
        private Line tangentLine;

        public TrafficSegmentNodeConnection(Segment segment, Node start, CanvasRoadManager canvasRoadManager) : base(segment, start, canvasRoadManager)
        {

        }


        protected override void OnCreated()
        {
            base.OnCreated();
        }

        protected override void OnTangentChanged()
        {
            base.OnTangentChanged();
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
        }

        internal void Update()
        {
            var mgr = (Manager as CanvasRoadManager);
            var scale = mgr.Scale;
            var layer = mgr.Drawing.GetLayer(DrawingLayer.Markers);
            var height = mgr.Drawing.Height;



            var start = new Point(Node.Position.x * scale, height - Node.Position.z * scale);
            var end = start + new Vector(Tangent.x * scale, -Tangent.z * scale) * 2;

            if (end != start)
                layer.DrawLine(new Pen(Brushes.HotPink, 1.5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Triangle }, start, end);

        }
    }
}
