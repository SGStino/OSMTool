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
using Simulation.Traffic.AI.Navigation;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Simulation.Traffic.AI;

namespace OSMTool.Wpf.Traffic
{
    public class CanvasRoadManager : AIRoadManager, INotifyPropertyChanged
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


            var closenodes = QueryNodes(p, 1000);

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
        public Segment FromSegment { get => fromSegment; set { fromSegment = value; this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FromSegment")); } }
        public Segment ToSegment { get => toSegment; set { toSegment = value; this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("toSegment")); } }

        private Segment toSegment;
        private Segment fromSegment;

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
            return new TrafficSegmentNodeConnection(segment as TrafficSegment, start as TrafficNode, this);
        }

        internal async void Navigate()
        {
            var fromAI = fromSegment as AISegment;
            var toAI = toSegment as AISegment;
            if (fromAI != null && toAI != null)
            {
                var solver = new RouteSolver(fromAI.AIRoutes, toAI.AIRoutes);

                while (solver.Iterate())
                {
                    await Task.Yield();
                }
                if (solver.IsSuccess)
                {
                    var solution = solver.Solution.ToArray();
                }
                else
                {
                }
                    var idGen = new ObjectIDGenerator();
                    var graph = solver.GetGraph(node => getUniqueName(node, idGen));
            }
        }

        private string getUniqueName(IAIRoute route, ObjectIDGenerator idGen)
        {
            var seg = (route as SegmentAIRoute)?.Segment as TrafficSegment;
            if (seg != null)
            {

                var name = (seg.Description as TrafficSegmentDescription).Name;
                var id = (seg.Description as TrafficSegmentDescription).OsmWay.Id;

                var from = (route as SegmentAIRoute).GetStart();
                var to = (route as SegmentAIRoute).GetEnd();
                return $"{name} ({id}: ({(from.Node as TrafficNode).OSMNode.Id} -> {(to.Node as TrafficNode).OSMNode.Id})) [{idGen.GetId(route, out var isNew1)}]";
            }

            var node = ((route as NodeAIRoute)?.Node as TrafficNode).OSMNode;
            return $"node {node.Id} [{idGen.GetId(route, out var isNew2)}]" ;
        }
    }
}