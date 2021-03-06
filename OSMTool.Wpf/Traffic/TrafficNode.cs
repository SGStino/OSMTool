﻿using Simulation.Traffic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OSMTool.Wpf.Traffic
{
    public class TrafficNode : AINode
    {

        public TrafficNode(UnityEngine.Vector3 position, CanvasRoadManager manager) : base(position, manager)
        {
        }
        protected override void OnCreated()
        {
            base.OnCreated();
        }


        public void Update()
        {

            var mgr = (Manager as CanvasRoadManager);
            var scale = mgr.Scale;
            var height = mgr.Drawing.Height;


            var layer = mgr.Drawing.GetLayer(DrawingLayer.Markers);

            var start = new Point(Position.x * scale, height - Position.z * scale);
            if (OSMNode?.Tags?.Count > 0)
                layer.DrawEllipse(null, new Pen(Brushes.Black, 1), start, 5, 5);


            foreach (TrafficSegmentNodeConnection connection in Segments)
                connection.Update();

        }

        public OsmSharp.Node OSMNode { get; set; }
        public bool IsDeletionPossible { get; internal set; } = false;
        public bool VisitedAsLine { get; internal set; } = false;
        public bool VisitedAsIntersection { get; internal set; } = false;
        public bool IsAlmostStraight { get; internal set; } = false;
        public bool IsMarkedForDeletion { get; internal set; } = false;
    }
}