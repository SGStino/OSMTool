using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OSMTool.Wpf
{
    public class ZoomBox : Decorator
    {
        private Point? dragStart;
        private readonly MatrixTransform matrix;

        public ZoomBox()
        {
            var group = new TransformGroup();

            group.Children.Add(matrix = new MatrixTransform());



            this.RenderTransform = group;
        }


        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this);
            base.OnMouseWheel(e);
            var pos = e.GetPosition(parent as IInputElement);
            var m = matrix.Matrix;

            var x = pos.X;// / ActualWidth;
            var y = pos.Y;// / ActualHeight;

            var d = 1 + 0.25 * Math.Sign(e.Delta);

            m.ScaleAt(d, d, x, y);
            matrix.Matrix = m;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            this.dragStart = e.GetPosition(Application.Current.MainWindow);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            dragStart = null;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragStart.HasValue)
            {
                var pos = e.GetPosition(Application.Current.MainWindow);
                var offset = pos - dragStart.Value;
                var m = matrix.Matrix;

                m.Translate(offset.X, offset.Y);
                matrix.Matrix = m;

                dragStart = pos;
            }
        }
    }
}
