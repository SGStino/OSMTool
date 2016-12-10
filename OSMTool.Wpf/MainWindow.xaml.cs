using OsmSharp;
using OsmSharp.Streams;
using OsmSharp.Tags;
using OSMTool.Wpf.Traffic;
using Simulation.Traffic;
using Simulation.Traffic.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
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
using UnityEngine;

namespace OSMTool.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //LoadAsync(@"C:\Users\stijn\Downloads\luxembourg-latest.osm.pbf");
            //LoadAsync(@"C:\Users\stijn\Downloads\knokke-heist_01_01.pbf");
            LoadAsync(@"C:\Users\stijn\Downloads\bruges.osm.pbf");
        }
        
        private CanvasRoadManager manager;

        private class coordinateSet
        {
            public float minLat = float.PositiveInfinity, minLon = float.PositiveInfinity;
            public float maxLat = float.NegativeInfinity, maxLon = float.NegativeInfinity;

            public void fit(float lat, float lon)
            {
                minLat = Math.Min(minLat, lat);
                minLon = Math.Min(minLon, lon);
                maxLat = Math.Max(maxLat, lat);
                maxLon = Math.Max(maxLon, lon);

            }


            public float height => (maxLat - minLat);
            public float width => (maxLon - minLon);
        }

        public async void LoadAsync(string file)
        {
            var bounds = new coordinateSet();
            var bounds2 = new coordinateSet();
            int failCount = 0;
            var nodeDictionary = new Dictionary<long, Simulation.Traffic.Node>();
            using (var stream = File.OpenRead(file))
            {
                using (var source = new PBFOsmStreamSource(stream))
                {



                    var nodes = source.AsParallel().OfType<OsmSharp.Node>().ToDictionary(k => k.Id);
                    var ways = source.AsParallel().Where(s => s.Type == OsmSharp.OsmGeoType.Way).OfType<Way>();



                    var highWays = ways.Where(w => w.Tags.ContainsKey("highway") || w.Tags.ContainsKey("railway")).ToArray();

                    //foreach (var way in highWays)

                    highWays.AsParallel().ForAll(way =>
                    {
                        foreach (var n in way.Nodes)
                        {
                            OsmSharp.Node node;
                            if (nodes.TryGetValue(n, out node))
                            {
                                var lat = (node.Latitude ?? 0);
                                var lon = (node.Longitude ?? 0);
                                lock (bounds)
                                bounds.fit(lat, lon);
                            }
                        }
                    });

                    manager = new CanvasRoadManager(canvas, toX(bounds, bounds.maxLon, bounds.maxLat), toY(bounds, bounds.maxLon, bounds.maxLat));
                    foreach (var way in highWays)
                    {
                        string tagValue;
                        if(way.Tags.TryGetValue("area", out tagValue))
                        {
                            if (tagValue == "yes")
                                continue;
                        }

                        Simulation.Traffic.Node lastTrafficNode = null;
                        SegmentDescription description = getDescription(way);
                        foreach (var n in way.Nodes)
                        {
                            OsmSharp.Node node;
                            if (nodes.TryGetValue(n, out node))
                            {
                                Simulation.Traffic.Node trafficNode = null;
                                if (!nodeDictionary.TryGetValue(n, out trafficNode))
                                {
                                    var lat = node.Latitude ?? 0;
                                    var lon = node.Longitude ?? 0;
                                    nodeDictionary.Add(n, trafficNode = manager.CreateNodeAt(toX(bounds, lon, lat), toY(bounds, lon, lat)));
                                    (trafficNode as TrafficNode).OSMNode = node;
                                }

                                if (lastTrafficNode != null)
                                    manager.CreateSegment(lastTrafficNode, trafficNode, description);
                                lastTrafficNode = trafficNode;

                            }
                            else
                                failCount++;
                        }

                    }

                    int nodeCountBefore = this.manager.Nodes.Count();
                    mergeCloseNodes();

                    cleanupStraights();

                    cleanupCorners();
                    int nodeCountAfter = this.manager.Nodes.Count();

                    System.Diagnostics.Debug.WriteLine($"Before:  {nodeCountBefore}, after: {nodeCountAfter}");

                    manager.Update();
                }
            }
        }

        private void cleanupCorners()
        {
            var nodes = manager.Nodes.OfType<TrafficNode>()
                .Where(n => n.IsDeletionPossible && n.Segments.Count() == 2)
                .OrderBy(n =>
                {
                    var a = n.Segments.First().Segment;
                    var b = n.Segments.Last().Segment;
                    return Math.Max(getLength(a), getLength(b));
                })
                .ToArray();

            cleanupCorners(nodes, true);
            //cleanupCorners(nodes, true);
        }

        private static void cleanupCorners(TrafficNode[] nodes, bool remove)
        {
            foreach (var node in nodes)
            {
                if (node.Segments.Count() == 2)
                {
                    var a = node.Segments.First();
                    var b = node.Segments.Last();

                    var dot = Vector3.Dot(a.Tangent, b.Tangent);
                    if (dot > -0.5) continue;

                    var otherA = a.Segment.Start == a ? a.Segment.End : a.Segment.Start;
                    var otherB = b.Segment.Start == b ? b.Segment.End : b.Segment.Start;

                    Arc arc2;
                    Arc arc1;
                    BiarcInterpolation.Biarc(otherA, otherB, out arc1, out arc2);

                    Arc arc1A;
                    Arc arc2A;
                    Arc arc1B;
                    Arc arc2B;

                    BiarcInterpolation.Biarc(otherA, a, out arc1A, out arc2A);
                    BiarcInterpolation.Biarc(otherB, b, out arc1B, out arc2B);

                    float maxDistanceSqr = 0;
                    calcDistance(arc1A, arc1, ref maxDistanceSqr);
                    calcDistance(arc1A, arc2, ref maxDistanceSqr);
                    calcDistance(arc2A, arc1, ref maxDistanceSqr);
                    calcDistance(arc2A, arc2, ref maxDistanceSqr);
                    calcDistance(arc1B, arc1, ref maxDistanceSqr);
                    calcDistance(arc1B, arc2, ref maxDistanceSqr);
                    calcDistance(arc2B, arc1, ref maxDistanceSqr);
                    calcDistance(arc2B, arc2, ref maxDistanceSqr);

                    if (maxDistanceSqr < 1)
                    {
                        node.Manager.MergeSegments(node);
                    }
                }
            }
        }

        private static void calcDistance(Arc arc1A, Arc arc1, ref float maxDistance)
        {
            float count = 1 + (int)arc1A.arcLength;
            for (int i = 1; i <= count; i++)
            {
                var p = i / count;
                var point = arc1A.Interpolate(p);
                float snap;
                if (arc1.GetClosestPoint(point, true, 0, out snap))
                {
                    var point2 = arc1.Interpolate(snap);
                    maxDistance = Math.Max(maxDistance, (point2 - point).sqrMagnitude);
                }
            }
        }

        private void cleanupStraights()
        {
            var processed = new HashSet<SegmentNodeConnection>();
            var nodeSet = new HashSet<TrafficNode>(manager.Nodes.Where(n => n.Segments.Count() != 2).OfType<TrafficNode>());


            while (nodeSet.Any())
            {
                var node = nodeSet.FirstOrDefault();
                nodeSet.Remove(node);
                foreach (var con in node.Segments)
                {
                    if (processed.Add(con))
                        walkToNextNode(con, processed);
                }
                node.VisitedAsIntersection = true;
            }



            var nodes = processed.Select(n => n.Node as TrafficNode).Distinct().ToArray();
            int cleanupCount = 0;
            foreach (var node in nodes)
            {
                var roundaboutPaths = node.Segments.Where(s => (s.Segment.Description as TrafficSegmentDescription).IsRoundabout).ToArray();
                if (roundaboutPaths.Length == 2)
                    fixTangents(roundaboutPaths[0], roundaboutPaths[1]);

                var types = node.Segments.Select(t => (t.Segment.Description as TrafficSegmentDescription).Type).Distinct();
                foreach (var type in types)
                {
                    var typePaths = node.Segments.Where(s => (s.Segment.Description as TrafficSegmentDescription).Type == type).ToArray();
                    if (typePaths.Length == 2)
                        fixTangents(typePaths[0], typePaths[1]);
                }
            }
            System.Diagnostics.Debug.WriteLine($"Cleaned up {cleanupCount} straight nodes");
        }

        private double getLength(Segment segment)
        {
            return (segment.Start.Node.Position - segment.End.Node.Position).magnitude;
        }

        private void walkToNextNode(SegmentNodeConnection con, HashSet<SegmentNodeConnection> processed)
        {
            var segment = con.Segment;
            SegmentNodeConnection target;
            if (segment.Start == con)
                target = segment.End;
            else
                target = segment.Start;
            if (processed.Add(target))
                walkToNextSegment(target, processed);
        }

        private void walkToNextSegment(SegmentNodeConnection target, HashSet<SegmentNodeConnection> processed)
        {
            var node = target.Node;
            var trafficNode = node as TrafficNode;
            trafficNode.VisitedAsLine = true;

            if (node.Segments.Count() != 2) return;// stop walking

            var other = node.Segments.FirstOrDefault(s => s != target);
            walkToNextNode(other, processed);


            var incomingSegment = target.Segment.Description as TrafficSegmentDescription;
            var outgoingSegment = other.Segment.Description as TrafficSegmentDescription;


            if (TrafficSegmentDescription.CanMerge(incomingSegment, outgoingSegment))
            {
                trafficNode.IsDeletionPossible = true;
            }

            var dot = fixTangents(target, other);

            if (dot < Math.Cos(Math.PI / 180 * 175))
                trafficNode.IsAlmostStraight = true;
        }

        private double fixTangents(SegmentNodeConnection a, SegmentNodeConnection b)
        {
            var targetLen = (a.Segment.Start.Node.Position - a.Segment.End.Node.Position).sqrMagnitude;
            var otherLen = (b.Segment.Start.Node.Position - b.Segment.End.Node.Position).sqrMagnitude;

            var dot = Vector3.Dot(a.Tangent, b.Tangent);

            if (dot < -Math.Cos(Math.PI / 4))
            {

                var t = (a.Tangent * targetLen - b.Tangent * otherLen).normalized;

                a.Tangent = t;
                b.Tangent = -t;
            }
            return dot;
        }

        private void mergeCloseNodes()
        {
            var mergedNodes = new ConcurrentBag<Tuple<Simulation.Traffic.Node, Simulation.Traffic.Node>>();
            bool any = false;
            var allNodes = manager.Nodes.ToArray();
            Parallel.For(0, allNodes.Length, x =>
            //for (int x = 0; x < allNodes.Length; x++)
            {
                for (int y = x + 1; y < allNodes.Length; y++)
                {
                    var a = allNodes[x];
                    var b = allNodes[y];
                    if ((a.Position - b.Position).sqrMagnitude < 2)
                    {
                        mergedNodes.Add(new Tuple<Simulation.Traffic.Node, Simulation.Traffic.Node>(a, b));
                        //manager.MergeNodes(a, b); 
                    }
                }
            });
            foreach (var merge in mergedNodes)
                manager.MergeNodes(merge.Item1, merge.Item2);
        }

        private SegmentDescription getDescription(Way way)
        {
            var desc = new TrafficSegmentDescription();


            string value;

            if (way.Tags.TryGetValue("highway", out value))
                desc.Type = value;
            else if (way.Tags.TryGetValue("railway", out value))
                desc.Type = value;


            if (way.Tags.TryGetValue("junction", out value))
            {
                if (value == "roundabout")
                {
                    desc.IsOneWay = true;
                    desc.IsRoundabout = true;
                }
                else if (value == "circular")
                {
                    desc.IsOneWay = true;
                    desc.IsRoundabout = false;
                }
                else
                {

                }
            }

            if (way.Tags.TryGetValue("name", out value))
                desc.Name = value;

            if (!way.Tags.TryGetValue("lanes", out value))
                value = "1";
            desc.Lanes = int.Parse(value);

            if (way.Tags.TryGetValue("oneway", out value))
            {
                desc.IsOneWay = value == "yes" || value == "-1";
                if (!desc.IsOneWay && value != "no")
                {

                }
            }

            desc.OsmWay = way;


            return desc;
        }

        private float toY(coordinateSet bounds, float lon, float lat)
        {
            if (lat < bounds.minLat) throw new InvalidOperationException();
            if (lat > bounds.maxLat) throw new InvalidOperationException();
            if (lon < bounds.minLon) throw new InvalidOperationException();
            if (lon > bounds.maxLon) throw new InvalidOperationException();

            var a = new GeoCoordinate(lat, lon);
            var b = new GeoCoordinate(bounds.minLat, lon);

            var dst = (float)a.GetDistanceTo(b);

            return dst;
            //return (float)distance(bounds.minLat, lon, lat, lon);
        }

        private float toX(coordinateSet bounds, float lon, float lat)
        {
            if (lat < bounds.minLat) throw new InvalidOperationException();
            if (lat > bounds.maxLat) throw new InvalidOperationException();
            if (lon < bounds.minLon) throw new InvalidOperationException();
            if (lon > bounds.maxLon) throw new InvalidOperationException();
            var a = new GeoCoordinate(lat, lon);
            var b = new GeoCoordinate(lat, bounds.minLon);

            var dst = (float)a.GetDistanceTo(b);

            return dst;
            //return (float)(distance(lat, bounds.minLon, lat, lon) * Math.PI);

        }

    }
}
