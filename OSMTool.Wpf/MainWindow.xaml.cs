using OsmSharp;
using OsmSharp.Streams;
using OsmSharp.Tags;
using OSMTool.Wpf.Traffic;
using Simulation.Traffic;
using System;
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
            LoadAsync(@"C:\Users\stijn\Downloads\knokke-heist_01_01.pbf");
        }

        private static Dictionary<string, Action<Polyline>> styles = new Dictionary<string, Action<Polyline>>()
        {
            {
                "motorway",
                p => {
                    p.StrokeThickness = 5;
                    p.Stroke = Brushes.Black;
                }
            },
            {
                "primary",
                p => {
                    p.StrokeThickness = 3;
                    p.Stroke = Brushes.Green;
                }
            },
            {
                "secondary",
                p => {
                    p.StrokeThickness = 2;
                    p.Stroke = Brushes.Blue;
                }
            },
            {
                "tertiary",
                p => {
                    p.StrokeThickness = 1;
                    p.Stroke = Brushes.Yellow;
                }
            },
            {
                "service",
                p => {
                    p.StrokeThickness = 1;
                    p.Stroke = Brushes.Gray;
                }
            },
            {
                "cycleway",
                null
            },
            {
                "track",
                null
            },
            {
                "pedestrian",
                null
            },
            {
                "footway",
                null
            },
            {
                "yes",
                null
            },
            {
                "error",
                null
            },
            {
                "escape",
                null
            },
            {
                "count",
                null
            },
            {
                "doesnotexist!",
                null
            },
            {
                "steps!",
                null
            },
            {
                "elevator!",
                null
            },
            {
                "corridor!",
                null
            },
            {
                "bus_stop",
                null
            },
            {
                "bus_guideway",
                null
            },
            {
                "path",
                null
            }
        };
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
                    var ways = source.Where(s => s.Type == OsmSharp.OsmGeoType.Way).OfType<Way>();



                    var highWays = ways.Where(w => w.Tags.ContainsKey("highway") || w.Tags.ContainsKey("railway")).ToArray();

                    var otherways = ways.Where(w => !w.Tags.ContainsKey("highway")).ToArray();

                    var tagKeys = otherways.SelectMany(t => t.Tags).Select(t => t.Key).Distinct().ToArray();

                    var roadTagKeys = highWays.SelectMany(t => t.Tags).Select(t => t.Key).Distinct().ToArray();
                    foreach (var way in highWays)
                    {
                        foreach (var n in way.Nodes)
                        {
                            OsmSharp.Node node;
                            if (nodes.TryGetValue(n, out node))
                            {
                                var lat = (node.Latitude ?? 0);
                                var lon = (node.Longitude ?? 0);
                                bounds.fit(lat, lon);
                            }
                        }
                    }

                    var x1 = toX(bounds, bounds.maxLon, bounds.maxLat);
                    var x2 = toX(bounds, bounds.minLon, bounds.minLat);
                    var x3 = toX(bounds, bounds.maxLon, bounds.minLat);
                    var x4 = toX(bounds, bounds.minLon, bounds.maxLat);
                    var y1 = toY(bounds, bounds.maxLon, bounds.maxLat);
                    var y2 = toY(bounds, bounds.minLon, bounds.minLat);
                    var y3 = toY(bounds, bounds.maxLon, bounds.minLat);
                    var y4 = toY(bounds, bounds.minLon, bounds.maxLat);
                    manager = new CanvasRoadManager(canvas, toX(bounds, bounds.maxLon, bounds.maxLat), toY(bounds, bounds.maxLon, bounds.maxLat));
                    foreach (var way in highWays)
                    {

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
                                }

                                if (lastTrafficNode != null)
                                    manager.CreateSegment(lastTrafficNode, trafficNode, description);
                                lastTrafficNode = trafficNode;

                            }
                            else
                                failCount++;
                        }

                    }
                    manager.Update();
                }
            }
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
