using Simulation.Traffic;
using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Simulation.Unity
{
    public class SimulationManager
    {
        public SimulationManager()
        {
        }


        public (IEnumerable<Node> nodes, IEnumerable<Segment> segments) GenerateTestScene()
        {
            var shape0 = ArrayLoftShape.CreateFlat(
                U1: 112 / 2048f,
                U2: 176 / 2048f,
                V: 1 / 8f,
                Width: 2,
                center: -9);
            var shape3 = ArrayLoftShape.CreateFlat(
                U1: 112 / 2048f,
                U2: 176 / 2048f,
                V: -1 / 8f,
                Width: 2,
                center: 9);

            var shape1 = ArrayLoftShape.CreateFlat(
                U1: 176 / 2048f,
                U2: 368 / 2048f,
                V: 1 / 8f,
                Width: 8,
                center: -4);
            var  shape2 = ArrayLoftShape.CreateFlat(
                U1: 176 / 2048f,
                U2: 368 / 2048f,
                V: -1 / 8f,
                Width: 8,
                center: 4);

            //var highwayTextureA = new LoftTextureInfo
            //{
            //    U1 = 176 / 2048f,
            //    U2 = 272 / 2048f,
            //    V = 96 / 256f
            //};
            //var highwayTextureB = new LoftTextureInfo
            //{
            //    U1 = 272 / 2048f,
            //    U2 = 368 / 2048f,
            //    V = 96 / 256f
            //};
            var highway = new LaneDescription
            {
                LaneType = LaneType.Highway,
                MaxSpeed = 130,
                Reverse = false,
                Turn = Turn.None,
                VehicleTypes = VehicleTypes.Vehicle,
                Width = 4,
            };
            var highwayReverse = new LaneDescription
            {
                LaneType = LaneType.Highway,
                MaxSpeed = 130,
                Reverse = true,
                Turn = Turn.None,
                VehicleTypes = VehicleTypes.Vehicle,
                Width = 4,
            };
            var roadDescription = new SegmentDescription
            {
                Lanes = new LaneDescription[]
                 {
                     highway,
                     highway,
                     highwayReverse,
                     highwayReverse
                 },
                MeshParts = new LaneMeshInfo[]
                {
                    new LaneMeshInfo(){ Shape = shape0 },
                    new LaneMeshInfo(){ Shape = shape1 },
                    new LaneMeshInfo(){ Shape = shape2 },
                    new LaneMeshInfo(){ Shape = shape3 }
                }
            };


            var offset = roadDescription.Lanes.Sum(l => l.Width) / 2 + 2;
            int n = 4;
            var nodes = new Node[n, n];
            var segments = new List<Segment>();
            var tangents = new Vector3[10];
            float s = 100;
            for (int θ = 0; θ < n; θ++)
            {
                for (int r = 0; r < n; r++)
                {
                    var radius = (r + 2) * s;
                    var x = MathF.Sin(θ * 30 * MathF.Deg2Rad) * radius;
                    var y = MathF.Cos(θ * 30 * MathF.Deg2Rad) * radius;


                    var dX = MathF.Cos(θ * 30 * MathF.Deg2Rad);
                    var dY = -MathF.Sin(θ * 30 * MathF.Deg2Rad);

                    tangents[θ] = new Vector3(dX, 0, dY);

                    var node = nodes[θ, r] = Node.CreateAt(x, y);
                    if (θ > 0)
                    {
                        var t1 = tangents[θ - 1];
                        var t2 = tangents[θ];
                        var seg = Segment.Create(nodes[θ - 1, r], nodes[θ, r], roadDescription);
                        (seg.Start as SegmentNodeConnection).Move(new Vector3(0, 0, offset), t1);
                        (seg.End as SegmentNodeConnection).Move(new Vector3(0, 0, offset), -t2);
                        segments.Add(seg);
                    }
                    if (r > 0)
                    {
                        var seg = Segment.Create(nodes[θ, r - 1], nodes[θ, r], roadDescription);
                        (seg.Start as SegmentNodeConnection).MoveOffset(new Vector3(0, 0, offset));
                        (seg.End as SegmentNodeConnection).MoveOffset(new Vector3(0, 0, offset));
                        segments.Add(seg);
                    }
                }
            }
            return (nodes.OfType<Node>(), segments);
        }
    }


}
