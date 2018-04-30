using Simulation.Traffic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.Unity
{
    public class SimulationManager
    {
        public SimulationManager()
        {
        }


        public (IEnumerable<Node> nodes, IEnumerable<Segment> segments) GenerateTestScene()
        {
            var highway = new LaneDescription
            {
                LaneType = LaneType.Highway,
                MaxSpeed = 130,
                Reverse = false,
                Turn = Turn.None,
                VehicleTypes = VehicleTypes.Vehicle,
                Width = 4
            };
            var highwayReverse = new LaneDescription
            {
                LaneType = LaneType.Highway,
                MaxSpeed = 130,
                Reverse = true,
                Turn = Turn.None,
                VehicleTypes = VehicleTypes.Vehicle,
                Width = 4
            };
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

            int n = 4;
            float s = 100;
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    var node = nodes[x, y] = Node.CreateAt(x * s, y * s); 
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
            return (nodes.OfType<Node>(), segments);
        }
    }


}
