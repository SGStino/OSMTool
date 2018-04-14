using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Simulation.WpfTest
{
    /// <summary>
    /// Interaction logic for BiArcWindow.xaml
    /// </summary>
    public partial class BiArcWindow : Window
    {
        private class DragControl
        {
            public event Action moved;
            private Ellipse startPoint;
            private bool dragging;
            private Point dragStart;

            public DragControl(Ellipse startPoint)
            {
                this.startPoint = startPoint;
                startPoint.MouseDown += StartPoint_MouseDown;
                startPoint.MouseMove += StartPoint_MouseMove;
                startPoint.MouseUp += StartPoint_MouseUp;
                startPoint.MouseLeave += StartPoint_MouseLeave;


            }

            private void StartPoint_MouseLeave(object sender, MouseEventArgs e)
            {
                dragging = false;
            }

            private void StartPoint_MouseUp(object sender, MouseButtonEventArgs e)
            {
                dragging = false;
            }

            private void StartPoint_MouseMove(object sender, MouseEventArgs e)
            {
                if (dragging)
                {
                    var point = e.GetPosition(startPoint.Parent as FrameworkElement);
                    Canvas.SetLeft(startPoint, point.X);
                    Canvas.SetTop(startPoint, point.Y);
                    moved?.Invoke();
                }
            }

            private void StartPoint_MouseDown(object sender, MouseButtonEventArgs e)
            {
                dragging = true;

                dragStart = e.GetPosition(startPoint.Parent as FrameworkElement);
            }


        }

        public BiArcWindow()
        {
            InitializeComponent();
            new DragControl(startPoint).moved += BiArcWindow_moved;
            new DragControl(endPoint).moved += BiArcWindow_moved;
            new DragControl(startTangent).moved += BiArcWindow_moved;
            new DragControl(endTangent).moved += BiArcWindow_moved;
            BiArcWindow_moved();
        }

        private void BiArcWindow_moved()
        {
            var p1 = getPoint(startPoint);
            var p2 = getPoint(endPoint);
            var p3 = getPoint(startTangent);
            var p4 = getPoint(endTangent);
            line1.X1 = p1.X;
            line1.X2 = p2.X;
            line1.Y1 = p1.Y;
            line1.Y2 = p2.Y;

            var v1 = new Vector3((float)p1.X, 0, (float)p1.Y);
            var v2 = new Vector3((float)p2.X, 0, (float)p2.Y);
            var v3 = new Vector3((float)p3.X, 0, (float)p3.Y);
            var v4 = new Vector3((float)p4.X, 0, (float)p4.Y);



            var t1 = Vector3.Normalize(v3 - v1);
            var t2 = Vector3.Normalize(v4 - v2);

            var parameters = new BiArcParameters(v1 / 10, t1, v2 / 10, t2);

            var (arc1, arc2) = BiArcGenerator.Form1(parameters, 1);



            var center1 = new Point(arc1.Center.X * 10, arc1.Center.Z * 10);
            polyLine.Points.Clear();

            arc(arc1);
            arc(arc2);

        }

        private void arc(ArcDefinition arc1)
        {
            var sign = -MathF.Sign(arc1.Theta);
            var angle = MathF.Abs(arc1.Theta)*2;
            var delta = angle / 50;
            for (float i = 0; i < angle; i += delta)
            {
                var quat = Quaternion.CreateFromAxisAngle(arc1.Axis, i * sign);
                var p3d = arc1.Center + Vector3.Transform(arc1.EndDir, quat) * arc1.R;
                var p2d = new Point(p3d.X * 10, p3d.Z * 10);
                polyLine.Points.Add(p2d);
            }
        }

        private Point getPoint(Ellipse startPoint)
        {
            return new Point(Canvas.GetLeft(startPoint), Canvas.GetTop(startPoint));
        }
    }
}
