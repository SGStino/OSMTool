using Simulation.Traffic;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Simulation.WpfTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static LaneDescription highway = new LaneDescription
        {
            LaneType = LaneType.Highway,
            MaxSpeed = 130,
            Reverse = false,
            Turn = Turn.None,
            VehicleTypes = VehicleTypes.Vehicle,
            Width = 4
        };
        private static LaneDescription highwayReverse = new LaneDescription
        {
            LaneType = LaneType.Highway,
            MaxSpeed = 130,
            Reverse = true,
            Turn = Turn.None,
            VehicleTypes = VehicleTypes.Vehicle,
            Width = 4
        };
        public MainWindow()
        {
            InitializeComponent();

            var roadDescription = new SegmentDescription
            {
                Lanes = new LaneDescription[]
                 {
                     highway,
                     highway,
                     highwayReverse,
                     highwayReverse
                 }
            };
            var nodes = new Node[10, 10];
            var segments = new List<Segment>();

            int n = 2;
            float s = 100;
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    var node = nodes[x, y] = Node.CreateAt(x * s, y * s);

                    var view = new NodeView();
                    view.Bind(node);
                    canvas.Children.Add(view);
                    if (x > 0)
                    {
                        var seg = Segment.Create(nodes[x - 1, y], nodes[x, y], roadDescription);
                        segments.Add(seg);
                    }
                    if (y > 0)
                    {
                        var seg = Segment.Create(nodes[x, y - 1], nodes[x, y], roadDescription);
                        segments.Add(seg);
                    }
                }
            }
        }

    }
}
