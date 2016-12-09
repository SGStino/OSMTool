using Simulation.Traffic;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
            { "platform", 1.5 }
        };

        private static Dictionary<string, Brush> laneColors = new Dictionary<string, Brush>()
        {
            { "cycleway", Brushes.Orange},
            { "pedestrian", Brushes.Gray },
            { "footway", Brushes.SandyBrown },
            { "path", Brushes.Brown },
            { "platform", Brushes.Gray },
            { "rail", Brushes.Blue },
            { "tram", Brushes.Green }
        };
        private Line line;
        private Line line2;

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

            if (!dsc.IsOneWay && dsc.Type != "rail" && dsc.Type != "tram")
            {
                laneWidth *= 2; // both ways


                var layer3 = mgr.Drawing.GetLayer(DrawingLayer.Markers);
                layer3.DrawLine(new Pen(Brushes.LightGray, 1)
                {
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round 
                }, start, end);

            }

            layer1.DrawLine(new Pen(laneColor, laneWidth * scale)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            }
            , start, end);

            //layer2.DrawLine(new Pen(Brushes.Black, laneWidth * scale + 1)
            //{
            //    StartLineCap = PenLineCap.Round,
            //    EndLineCap = PenLineCap.Round
            //}, start, end);



        }
    }
}