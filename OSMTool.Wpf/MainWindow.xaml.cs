﻿using Alpinechough.Srtm;
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
using System.Globalization;
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
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            InitializeComponent();
            startupAsync();
        }

        private async void startupAsync()
        {
            //await LoadAsync(@"C:\Users\stijn\Downloads\luxembourg-latest.osm.pbf");
            await LoadAsync(@"C:\Users\stijn\Downloads\knokke-heist_01_01.pbf");

            //await LoadAsync(@"C:\Users\stijn\Downloads\bruges.osm.pbf");
            //await LoadAsync(@"C:\Users\stijn\Downloads\ghent.osm.pbf");

            using (var stream = File.Create(@"C:\Users\stijn\Downloads\mapOutput.json"))
            {
                var writer = new Simulation.Traffic.IO.RoadsWriter(stream);
                writer.WriteAll(manager);
            }

            //var newSet = new RoadManager();
            //using (var stream = File.OpenRead(@"C:\Users\stijn\Downloads\mapOutput.json"))
            //{
            //    var reader = new Simulation.Traffic.IO.Reader();
            //    reader.Load(newSet, stream);
            //}

            //using (var stream = File.Create(@"C:\Users\stijn\Downloads\mapOutput_2.json"))
            //{
            //    var writer = new Simulation.Traffic.IO.Writer();
            //    writer.Save(newSet, stream);
            //}
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

        public async Task LoadAsync(string file)
        {
            var bounds = new coordinateSet();
            var bounds2 = new coordinateSet();
            int failCount = 0;
            var nodeDictionary = new Dictionary<long, Simulation.Traffic.Node>();
            using (var stream = File.OpenRead(file))
            {
                OsmStreamSource source;
                if (file.EndsWith(".pbf"))
                    source = new PBFOsmStreamSource(stream);
                else
                    source = new XmlOsmStreamSource(stream);

                using (source)
                {
                    var nodes = source.AsParallel().OfType<OsmSharp.Node>().ToDictionary(k => k.Id);
                    var ways = source.AsParallel().Where(s => s.Type == OsmSharp.OsmGeoType.Way).OfType<Way>();

                    var highWays = ways.Where(w => (w.Tags.ContainsKey("highway") || w.Tags.ContainsKey("railway")) && !w.Tags.ContainsKey("fixme")).ToArray();

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

                    var srtmFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SRTM");
                    System.IO.Directory.CreateDirectory(srtmFolder);

                    var b = new OsmSharp.API.Bounds
                    {
                        MinLatitude = bounds.minLat,
                        MaxLatitude = bounds.maxLat,
                        MinLongitude = bounds.minLon,
                        MaxLongitude = bounds.maxLon
                    };
                    await SRTM.SRTMDownloader.DownloadAsync(b, srtmFolder);
                    var data = new SrtmData(srtmFolder);
                    manager = new CanvasRoadManager(canvas, toX(bounds, bounds.maxLon, bounds.maxLat), toY(bounds, bounds.maxLon, bounds.maxLat));
                    foreach (var way in highWays)
                    {
                        string tagValue;
                        if (way.Tags.TryGetValue("area", out tagValue))
                        {
                            if (tagValue == "yes")
                                continue;
                        }

                        Simulation.Traffic.Node lastTrafficNode = null;
                        SegmentDescription description = getDescription(way);
                        if (description == null) continue;
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
                                    var ele = GetHeight(data, lat, lon);

                                    nodeDictionary.Add(n, trafficNode = manager.CreateNode(new Vector3(toX(bounds, lon, lat), ele, toY(bounds, lon, lat))));
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

            detailsGrid.DataContext = manager;
        }

        private float GetHeight(SrtmData data, float lat, float lon)
        {
            int pointsPerCell = 1201;

            var latInt = (int)lat;
            var lonInt = (int)lon;

            float minLat = (float)Math.Floor((lat - latInt) * pointsPerCell) / pointsPerCell + latInt;
            float minLon = (float)Math.Floor((lon - lonInt) * pointsPerCell) / pointsPerCell + lonInt;
            float maxLat = minLat + 1.0f / pointsPerCell;
            float maxLon = minLon + 1.0f / pointsPerCell;
            // int points

            float h00 = (short)data.GetHeight(new GeographicalCoordinates(minLat, minLon));
            float h01 = (short)data.GetHeight(new GeographicalCoordinates(minLat, maxLon));
            float h10 = (short)data.GetHeight(new GeographicalCoordinates(maxLat, minLon));
            float h11 = (short)data.GetHeight(new GeographicalCoordinates(maxLat, maxLon));

            var h0 = interpolate(h00, h01, minLon, maxLon, lon);
            var h1 = interpolate(h10, h11, minLon, maxLon, lon);
            var h = interpolate(h0, h1, minLat, maxLat, lat);

            return h;
        }

        private float interpolate(float h0, float h1, float minLon, float maxLon, float lon)
        {
            var d = maxLon - minLon;
            var o = lon - minLon;

            return h0 + (h1 - h0) * o / d;
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

            int laneCount = 1;
            int forwardLanes = 0;
            int backwardLanes = 0;
            if (way.Tags.TryGetValue("lanes", out value))
                laneCount = int.Parse(value);
            if (way.Tags.TryGetValue("lanes:forward", out value))
                forwardLanes = int.Parse(value);
            if (way.Tags.TryGetValue("lanes:backward", out value))
                backwardLanes = int.Parse(value);



            if (way.Tags.TryGetValue("oneway", out value))
            {
                desc.IsOneWay = value == "yes" || value == "-1";
                if (!desc.IsOneWay && value != "no")
                {

                }
            }



            if (backwardLanes > 0 || forwardLanes > 0 && laneCount == 0)
                desc.LaneCount = backwardLanes + forwardLanes;
            else if (backwardLanes == 0 && forwardLanes == 0)
            {
                if (desc.IsOneWay)
                {
                    forwardLanes = laneCount;
                    backwardLanes = 0;
                }
                else {
                    forwardLanes = backwardLanes = laneCount;
                    laneCount *= 2;
                }
            }
            else
                desc.LaneCount = laneCount;
            if (forwardLanes > 0 && (laneCount - forwardLanes) != backwardLanes)
                backwardLanes = laneCount - forwardLanes;
            if (backwardLanes > 0 && (laneCount - backwardLanes) != forwardLanes)
                forwardLanes = laneCount - backwardLanes;

            if (laneCount - backwardLanes != forwardLanes)
            {
                // issue
            }
            if (laneCount == 0)
            {
                // issue
            }

            desc.LaneCount = laneCount;

            desc.OsmWay = way;




            float laneWidth = 3.7f;
            float maxSpeed = 130;
            VehicleTypes vehicles = VehicleTypes.Vehicle;
            LaneType laneType = LaneType.Road;
            switch (desc.Type)
            {
                case "motorway":
                case "motorway_link":
                case "trunk":
                case "trunk_link":
                    laneType = LaneType.Highway;
                    break;
                case "primary":
                case "primary_link":
                    laneWidth = 3.25f;
                    maxSpeed = 90;
                    laneType = LaneType.Road;
                    break;
                case "secondary":
                case "secondary_link":
                    laneWidth = 3f;
                    maxSpeed = 90;
                    laneType = LaneType.Road;
                    break;
                case "tertiary":
                case "tertiary_link":
                case "unclassified":
                    laneWidth = 2.8f;
                    maxSpeed = 70;
                    laneType = LaneType.Road;
                    break;
                case "residential":
                    laneWidth = 2.8f;
                    maxSpeed = 50;
                    laneType = LaneType.Road;
                    break;
                case "service":
                    laneWidth = 2.5f;
                    maxSpeed = 30f;
                    laneType = LaneType.Road;
                    break;
                case "living_street":
                    laneWidth = 2.8f;
                    maxSpeed = 30;
                    laneType = LaneType.Road;
                    break;
                case "pedestrian":
                    laneWidth = 2.5f;
                    maxSpeed = 15;
                    vehicles = VehicleTypes.Pedestrian | VehicleTypes.Bicycle;
                    laneType = LaneType.Pedestrian;
                    break;
                case "track":
                    laneWidth = 3f;
                    maxSpeed = 20;
                    laneType = LaneType.DirtTrack;
                    break;
                case "bus_guideway":
                    laneWidth = 4f;
                    maxSpeed = 70;
                    vehicles = VehicleTypes.Bus;
                    laneType = LaneType.Buslane;
                    break;
                case "escape":
                //vehicles = VehicleTypes.Emergency;
                //laneType = LaneType.Emergency;
                //break;
                //return null; // don't need paths
                case "road":
                    maxSpeed = 50;
                    laneType = LaneType.Road;
                    break;
                case "footway":
                    //maxSpeed = 10;
                    //vehicles = VehicleTypes.Pedestrian;
                    //laneWidth = 1.25f;
                    //laneType = LaneType.Path;
                    //break;
                    return null; // don't need paths
                case "cycleway":
                    //laneWidth = 1.25f;
                    //maxSpeed = 15;
                    //vehicles = VehicleTypes.Bicycle;
                    //laneType = LaneType.Bicycle;
                    //break;
                    return null; // don't need paths
                case "path":
                case "bridleway":
                case "steps":
                    //laneWidth = 1.10f;
                    //maxSpeed = 5;
                    //vehicles = VehicleTypes.Pedestrian;
                    //laneType = LaneType.Path;
                    //break;
                    return null; // don't need paths
                case "tram":
                    maxSpeed = 50;
                    vehicles = VehicleTypes.Tram;
                    laneWidth = 1;
                    laneType = LaneType.Emergency;
                    break;
                case "rail":
                    maxSpeed = 200;
                    vehicles = VehicleTypes.Train;
                    laneWidth = 1.45f;
                    laneType = LaneType.Train;
                    break;
                case "platform":
                case "vehicle_depot":
                    return null;
                default:
                    break;
            }

            desc.Lanes = new LaneDescription[laneCount];
            for (int i = 0; i < desc.Lanes.Length; i++)
                desc.Lanes[i] = new LaneDescription
                {
                    Width = laneWidth,
                    MaxSpeed = maxSpeed,
                    Turn = Turn.None,
                    VehicleTypes = vehicles,
                    Reverse = i >= forwardLanes,
                    LaneType = laneType
                };




            processTag<float>("gauge", way.Tags, desc.Lanes, float.TryParse, (d, i, v) => d.Width = v * 0.001f); // gauge is in mm            
            processTag<float>("maxspeed", way.Tags, desc.Lanes, float.TryParse, (d, i, v) => d.MaxSpeed = v);
            processTag<turnValues>("turn", way.Tags, desc.Lanes, Enum.TryParse, (d, i, v) => d.Turn |= getTurn(v));
            processTag<float>("width", way.Tags, desc.Lanes, float.TryParse, (d, i, v) => d.Width = v);

            for (int i = 0; i < desc.Lanes.Length; i++)
            {
                var l = desc.Lanes[i];
                if (l.Turn == Turn.None)
                {
                    if (i == 0)
                        l.Turn = Turn.Left;

                    if (forwardLanes == 1 || (i > 0 && i < forwardLanes))
                        l.Turn |= Turn.Through;

                    if (i == forwardLanes - 1)
                        l.Turn |= Turn.Right;

                    if (backwardLanes > 0)
                    {
                        if (i == forwardLanes)
                            l.Turn |= Turn.Right;
                        if (backwardLanes == 1 || (i > forwardLanes && i < laneCount))
                            l.Turn |= Turn.Through;
                        if (i == laneCount - 1)
                            l.Turn |= Turn.Left;
                    }
                }
            }
            // todo: multiple designated values can be set: foot and bicycle for example, this should combine!


            var dicts = new bool[laneCount, 8, 3];

            processTag<accessValues>("hgv", way.Tags, desc.Lanes, Enum.TryParse, (d, i, v) => setVehicleType(d, i, v, VehicleTypes.Truck, dicts));
            processTag<accessValues>("hazmat", way.Tags, desc.Lanes, Enum.TryParse, (d, i, v) => setVehicleType(d, i, v, VehicleTypes.Hazmat, dicts));
            processTag<accessValues>("bus", way.Tags, desc.Lanes, Enum.TryParse, (d, i, v) => setVehicleType(d, i, v, VehicleTypes.Bus, dicts));
            processTag<accessValues>("psv", way.Tags, desc.Lanes, Enum.TryParse, (d, i, v) => setVehicleType(d, i, v, VehicleTypes.Bus, dicts));
            processTag<accessValues>("taxi", way.Tags, desc.Lanes, Enum.TryParse, (d, i, v) => setVehicleType(d, i, v, VehicleTypes.Taxi, dicts));
            processTag<accessValues>("emergency", way.Tags, desc.Lanes, Enum.TryParse, (d, i, v) => setVehicleType(d, i, v, VehicleTypes.Emergency, dicts));
            processTag<accessValues>("bicycle", way.Tags, desc.Lanes, Enum.TryParse, (d, i, v) => setVehicleType(d, i, v, VehicleTypes.Bicycle, dicts));
            processTag<accessValues>("foot", way.Tags, desc.Lanes, Enum.TryParse, (d, i, v) => setVehicleType(d, i, v, VehicleTypes.Pedestrian, dicts));
            processTag<accessValues>("vehicle", way.Tags, desc.Lanes, Enum.TryParse, (d, i, v) => setVehicleType(d, i, v, VehicleTypes.Vehicle, dicts));

            for (int i = 0; i < laneCount; i++)
            {
                var allowed = desc.Lanes[i].VehicleTypes; // get default value

                for (int t = 1; t < dicts.GetLength(1); t++)
                {
                    if (dicts[i, t, (int)accessValues.designated])
                        allowed = VehicleTypes.None;
                }
                for (int t = 1; t < dicts.GetLength(1); t++)
                {
                    var type = (VehicleTypes)(1 << t);
                    if (dicts[i, t, (int)accessValues.yes] || dicts[i, t, (int)accessValues.designated])
                        allowed |= type;
                    else if (dicts[i, t, (int)accessValues.no])
                        allowed &= ~type;
                }
                desc.Lanes[i].VehicleTypes = allowed;
            }

            return desc;
        }

        private void setVehicleType(LaneDescription lane, int index, accessValues v, VehicleTypes type, bool[,,] dicts)
        {
            for (int i = 0; (1 << i) <= (int)type; i++)
            {
                var n = (VehicleTypes)(1 << i);
                if (type.HasFlag(n))
                    dicts[index, i, (int)v] = true;

            }
        }

        delegate bool ProcessTagValue<T>(string input, out T output);
        private void processTag<T>(string v, TagsCollectionBase tags, LaneDescription[] descriptions, ProcessTagValue<T> process, Action<LaneDescription, int, T> setter)
        {
            T parsedValue;
            string value;

            if (tags.TryGetValue(v, out value))
                if (process(value, out parsedValue))
                    for (int i = 0; i < descriptions.Length; i++)
                        setter(descriptions[i], i, parsedValue);


            if (tags.TryGetValue(v + ":lanes", out value))
            {
                var split = value.Split('|');
                for (int i = 0; i < split.Length; i++)
                {
                    if (i < descriptions.Length)
                    {
                        foreach (var val in split[i].Split(';'))
                            if (process(val, out parsedValue))
                                setter(descriptions[i], i, parsedValue);
                    }
                    else
                    {
                        // something wrong here
                    }
                }
            }
        }

        private Turn getTurn(turnValues turn) => (Turn)(int)turn;

        private enum turnValues
        {
            sharp_left = (int)Turn.SharpLeft,
            left = (int)Turn.Left,
            slight_left = (int)Turn.SlightLeft,
            merge_to_left = (int)Turn.MergeLeft,
            through = (int)Turn.Through,
            none = 0,
            merge_to_right = (int)Turn.MergeRight,
            slight_right = (int)Turn.SlightRight,
            right = (int)Turn.Right,
            sharp_right = (int)Turn.SharpRight
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

    internal enum accessValues
    {
        yes = 1,
        no = 0,
        designated = 2
    }
}
