using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Simulation.Traffic;
using Simulation.Traffic.Lofts;
using Simulation.Traffic.Utilities;
namespace Simulation.WpfTest
{
    /// <summary>
    /// Interaction logic for NodeView.xaml
    /// </summary>
    public partial class NodeView : UserControl
    {
        private bool dragging;
        private Vector3 startDragPosition;
        private Point startDragMousePosition;
        private Node node; 
        public NodeView()
         {
            InitializeComponent();
            this.Background = Brushes.Red;
        }

        internal void Bind(Node node)
        {
            this.node = node;
            node.Bounds.ObserveOn(Dispatcher).Subscribe(b =>
            {
                Canvas.SetLeft(this, b.Left);
                Canvas.SetTop(this, b.Top);
                Width = b.Size.X;
                Height = b.Size.Y;
            });

            node.Connections.ObserveOn(Dispatcher).Subscribe(s =>
            {
                var startHeres = s.Where(n => n.Segment.Start == n); 
                grid.Children.Clear(); 
                foreach (var n in startHeres.Select(n => n.Segment))
                {
                    var line = new SegmentView();
                    line.Bind(n);
                    grid.Children.Add(line); 
                }
            });
        }

        private void updateLine(ILoftPath path, Polyline line)
        {
            line.Points.Clear();
            for(int i = 0; i < path.Length; i+=10)
            {
                var t = path.GetTransform(i).GetTranslate(); 
                line.Points.Add(new Point(t.X, t.Z)); 
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            this.dragging = true;
            this.startDragPosition = node.Position.Value;
            this.startDragMousePosition = e.GetPosition(this.Parent as FrameworkElement);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.dragging)
            {
                var deltaMouse = e.GetPosition(this.Parent as FrameworkElement) - startDragMousePosition;

                var vector = new Vector3((float)deltaMouse.X, 0, (float)deltaMouse.Y);

                node.Move(startDragPosition + vector);
            }

        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            dragging = false;
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            dragging = false;
        }
    }
}
