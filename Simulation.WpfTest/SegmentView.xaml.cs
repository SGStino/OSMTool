using System;
using System.Collections.Generic;
using System.Linq;
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
using Simulation.Traffic;
using System.Reactive.Linq;
using Simulation.Traffic.Lofts;
using System.Numerics;

namespace Simulation.WpfTest
{
    /// <summary>
    /// Interaction logic for SegmentView.xaml
    /// </summary>
    public partial class SegmentView : UserControl
    {
        private CompositeDisposable disposable = new CompositeDisposable();
        private ISegment segment;

        public SegmentView()
        {
            InitializeComponent();
            this.Unloaded += (s, e) => disposable.Dispose();
        }


        internal void Bind(ISegment n)
        {
            segment = n;
            disposable.Add(n.Start.Node.Position.CombineLatest(n.LoftPath, (pos, path) => (pos: pos, path: path)).ObserveOn(Dispatcher).Subscribe(p => update(p.pos, p.path)));

            n.Start.Node.Position.CombineLatest(n.End.Node.Position, n.Start.Offset, n.End.Offset, (startPos, endPos, start, end) => (startPos: startPos, endPos: endPos, start: start, end: end)).ObserveOn(Dispatcher).Subscribe(p => update(p.startPos, p.endPos, p.start, p.end));

            App.Frame.Where(_ => changes > 0).Subscribe(_ => { System.Diagnostics.Debug.WriteLine("changes:" + changes); changes = 0; });
        }

        private void update(Vector3 startPos, Vector3 endPos, ConnectionOffset start, ConnectionOffset end)
        {
            var p1 = start.Tangent * 10;

            var p3 = endPos - startPos;
            var p2 = p3 + end.Tangent * 10;

            this.startTangent.X1 = p1.X;
            this.startTangent.Y1 = p1.Z;
            this.startTangent.X2 = 0;
            this.startTangent.Y2 = 0;

            this.endTangent.X1 = p2.X;
            this.endTangent.Y1 = p2.Z;
            this.endTangent.X2 = p3.X;
            this.endTangent.Y2 = p3.Z;


            this.directLine.X1 = 0;
            this.directLine.X2 = p3.X;
            this.directLine.Y1 = 0;
            this.directLine.Y2 = p3.Z;
        }

        int changes = 0;
        private Random rnd = new Random();
        private void update(Vector3 pos, ILoftPath path)
        {
            changes++;
            curve.Stroke = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255)));
            curve.Points.Clear();

            int points = MathF.RoundToInt(Math.Min(200, path.Length)) / 10;
            var delta = path.Length / points;

            for (float i = 0; i <= path.Length; i += delta)
            {
                var t = path.GetTransformedPoint(i, Vector3.Zero) - pos;
                curve.Points.Add(new Point(t.X, t.Z));
            }
            this.ToolTip = $"{path.Length}: {(path as BiArcLoftPath).Arc1.Length} : {(path as BiArcLoftPath).Arc2.Length}";


        }
    }
}
