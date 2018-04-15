using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Disposables;
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
using Simulation.Data;
using Simulation.Traffic;

namespace Simulation.WpfTest
{
    /// <summary>
    /// Interaction logic for ConnectorView.xaml
    /// </summary>
    public partial class ConnectorView : UserControl
    {
        private readonly CompositeDisposable disposable;
        private Point startDragPos;
        private ConnectionOffset startOffset;
        private bool dragging;
        private ISegmentNodeConnection con;

        public ConnectorView()
        {
            disposable = new CompositeDisposable();
            InitializeComponent();

            Unloaded += (s, e) => disposable.Dispose();
        }

        internal void Bind(ISegmentNodeConnection c)
        {
            con = c;
            disposable.Add(c.Offset.Subscribe(o => move(o)));
        }

        private void move(ConnectionOffset o)
        {

            var p = o.GetPosition(Vector3.Zero);

            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Z);

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            this.startDragPos = e.GetPosition(this.Parent as Canvas);
            this.startOffset = con.Offset.Value;

            this.dragging = true;
            e.Handled = true;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.dragging)
            {
                var newOffset = e.GetPosition(this.Parent as Canvas);
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    var startPosition = startOffset.GetPosition(Vector3.Zero);

                    var absMovement2d = newOffset - startDragPos;

                    var move = new Vector3((float)absMovement2d.X, 0, (float)absMovement2d.Y);

                    var m = Matrix4x4.CreateWorld(startPosition, -startOffset.Tangent, new Vector3(0, -1, 0));

                    if (Matrix4x4.Invert(m, out var inverted))
                    {
                        var localOffsetMovement = Vector3.TransformNormal(move, inverted);
                        var offset = new ConnectionOffset(startOffset.Offset + localOffsetMovement, startOffset.Tangent);
                        (con as SegmentNodeConnection).Move(offset);
                    }

                }
                else
                {
                    var nodePos = con.Node.Position.Value;
                    var startDirection = startOffset.GetPosition(nodePos);

                    var absMovement2d = newOffset - startDragPos;

                    var move = new Vector3((float)absMovement2d.X, 0, (float)absMovement2d.Y);
                    var endDirection = startDirection + move / 2 - nodePos;
                    startDirection -= nodePos;

                    endDirection = Vector3.Normalize(endDirection);
                    startDirection = Vector3.Normalize(startDirection);

                    var angleAxis = Vector3.Cross(endDirection, startDirection);

                    if (angleAxis.Length() > 0.001f)
                    {
                        var w = Vector3.Dot(endDirection, startDirection);
                        Quaternion q = Quaternion.Inverse(Quaternion.Normalize(new Quaternion(angleAxis, w)));

                        var offset = new ConnectionOffset(startOffset.Offset, Vector3.Transform(startOffset.Tangent, q));

                        (con as SegmentNodeConnection).Move(offset);
                    }

                }
                e.Handled = true;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (this.dragging)
            {
                e.Handled = true;
                dragging = false;
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (this.dragging)
            {
                e.Handled = true;
                dragging = false;
            }
            base.OnMouseUp(e);
        }


    }
}
