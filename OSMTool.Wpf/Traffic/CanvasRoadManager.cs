using Simulation.Traffic;
using System.Windows.Controls;
using System;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Linq;
using System.Windows;
using UnityEngine;
using System.Windows.Documents;
using System.ComponentModel;

namespace OSMTool.Wpf.Traffic
{
    public class CanvasRoadManager : RoadManager, INotifyPropertyChanged
    {
        private float height;
        private float width;
        private float scale = 10; // wpf pixel = 1 / scale meters
        private PointerAdorner pointerAdorner;

        public event PropertyChangedEventHandler PropertyChanged;

        public float Height => height;
        public float Width => width;
        public CanvasRoadManager(DrawingVisualHost drawing, float width, float height)
        {
            this.width = width;
            this.height = height;
            this.Drawing = drawing;

            var parent = VisualTreeHelper.GetParent(drawing) as FrameworkElement;

            parent.MouseMove += Drawing_MouseMove;
            var adornerLayer = AdornerLayer.GetAdornerLayer(drawing);

            adornerLayer.Add(pointerAdorner = new PointerAdorner(this, drawing));

            pointerAdorner.PreviewMouseUp += PointerAdorner_PreviewMouseUp;
        }

        private void PointerAdorner_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var node = pointerAdorner.Node;

            if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {
                if (node != null && node.Segments.Count() == 2 && node.IsDeletionPossible)
                    MergeSegments(node);
            }
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {

                this.SelectedNode = node;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedNode)));
            }

            pointerAdorner.SetPosition(null, e.GetPosition(sender as IInputElement));
            Update();
        }

        private void Drawing_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var senderHost = Drawing;
            var pos = e.GetPosition(sender as IInputElement);
            var p = new Vector3((float)(pos.X / senderHost.ActualWidth) * width, 0, height - (float)(pos.Y / senderHost.ActualHeight) * height);


            var closenodes = QueryNodes(p, 10);

            var node = closenodes.OrderBy(n => (n.Position - p).sqrMagnitude).FirstOrDefault();

            pointerAdorner.SetPosition(node as TrafficNode, pos);

        }






        public void Update()
        {
            Drawing.Width = width * Scale;
            Drawing.Height = height * Scale;
            Drawing.OpenLayers();

            var nodes = Nodes.Where(n => n != null).ToArray();
            var segments = Segments.Where(n => n != null).ToArray();
            foreach (TrafficNode node in nodes)
                node.Update();
            foreach (TrafficSegment segment in segments.OrderBy(m => (m?.Description as TrafficSegmentDescription)?.Type))
                segment.Update();

            var layer = Drawing.GetLayer(DrawingLayer.Outlines);

            var rect = new System.Windows.Rect(0, 0, Drawing.Width, Drawing.Height);
            rect.Inflate(-10.0, -10.0);
            layer.DrawRectangle(null, new Pen(Brushes.HotPink, 1), rect);

            Drawing.CloseLayers();
            //Drawing.InvalidateVisual();
        }

        public DrawingVisualHost Drawing { get; private set; }
        public float Scale { get { return scale; } set { scale = value; Update(); } }

        public TrafficNode SelectedNode { get; private set; }

        protected override Node createNode(UnityEngine.Vector3 position)
        {
            return new TrafficNode(position, this);
        }
        protected override Segment createSegment(Node start, Node end, SegmentDescription description)
        {
            return new TrafficSegment(description, this);
        }

        protected override SegmentNodeConnection createConnection(Segment segment, Node start)
        {
            return new TrafficSegmentNodeConnection(segment, start, this);
        }
    }
}