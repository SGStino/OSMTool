using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Simulation.Traffic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation.Traffic.IO
{
    public enum RoadElementType
    {
        Node,
        Description,
        Segment
    }
    public class RoadsReader : IDisposable
    {
        private struct token
        {
            public token(RoadElementType type, object value)
            {
                Type = type;
                Value = value;
            }
            public readonly RoadElementType Type;
            public readonly object Value;
        }


        private readonly RoadManager roads;
        private readonly bool keepOpen;
        private readonly Stream stream;
        private IEnumerator<token> enumerator;
        private bool disposed;

        public RoadsReader(RoadManager roads, Stream stream, bool keepOpen = false)
        {
            this.roads = roads;
            this.stream = stream;
            this.keepOpen = keepOpen;


            var reading = getData(stream);
            this.enumerator = reading.GetEnumerator();
        }

        public void ReadAll()
        {
            while (Read()) ;
        }

        private IEnumerable<token> getData(Stream stream)
        {
            var nodes = new List<Node>();
            var reader = new JsonTextReader(new StreamReader(stream));

            var descriptions = new List<SegmentDescription>();
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new StringEnumConverter());

            if (reader.Read())
            {
                // inside root
                do
                {
                    if (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            // inside property
                            var name = (string)reader.Value;

                            switch (name)
                            {
                                case Constants.TAG_NODES:
                                    foreach (var node in readNodes(reader, roads, serializer, nodes))
                                        yield return new token(RoadElementType.Node, node);
                                    break;
                                case Constants.TAG_SEGMENT_DESCRIPTIONS:
                                    foreach (var description in readDescriptions(reader, serializer, descriptions))
                                        yield return new token(RoadElementType.Description, description);
                                    break;
                                case Constants.TAG_SEGMENTS:
                                    foreach (var segment in readSegments(reader, roads, serializer, nodes, descriptions))
                                        yield return new token(RoadElementType.Segment, segment);
                                    break;
                            }
                        }
                    }
                } while (reader.TokenType != JsonToken.EndObject);
            }
        }

        public bool Read()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RoadsReader));
            return enumerator.MoveNext();
        }

        public object Value => enumerator.Current.Value;
        public RoadElementType Type => enumerator.Current.Type;

         

        private IEnumerable<SegmentDescription> readDescriptions(JsonTextReader reader, JsonSerializer serializer, List<SegmentDescription> descriptions)
        {
            // use single buffer for the lanes, they are usually the same size and clear doesn't reset the capacity?
            var lanes = new List<LaneDescription>();
            if (reader.Read())
            {
                // start array
                if (reader.TokenType == JsonToken.StartArray)
                {
                    do
                    {
                        if (reader.Read()) // inside node
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                var segmentDescription = new SegmentDescription();
                                int depth = reader.Depth;
                                do
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.TokenType == JsonToken.PropertyName)
                                        {
                                            switch ((string)reader.Value)
                                            {
                                                case Constants.TAG_SEGMENT_DESCRIPTION_LANES:
                                                    segmentDescription.Lanes = readLanes(reader, serializer, lanes);
                                                    break;
                                            }
                                        }
                                    }
                                } while (reader.Depth > depth);
                                descriptions.Add(segmentDescription);
                                yield return segmentDescription;
                            }
                        }
                    } while (reader.TokenType != JsonToken.EndArray);
                }
            }
        }

        private LaneDescription[] readLanes(JsonTextReader reader, JsonSerializer serializer, List<LaneDescription> lanes)
        {
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    int depth = reader.Depth;
                    do
                    {
                        if (reader.Read())
                        {
                            // start array
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                lanes.Add(serializer.Deserialize<LaneDescription>(reader));
                            }
                        }
                    } while (reader.TokenType != JsonToken.EndArray);
                }
            }
            var result = lanes.ToArray();
            lanes.Clear();
            return result;
        }

        private IEnumerable<Segment> readSegments(JsonTextReader reader, RoadManager roads, JsonSerializer serializer, List<Node> nodes, List<SegmentDescription> descriptions)
        {
            if (reader.Read())
            {
                // start array
                if (reader.TokenType == JsonToken.StartArray)
                {
                    do
                    {
                        if (reader.Read()) // inside node
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                Node start = null, end = null;
                                Vector3 startTangent = Vector3.zero, endTangent = Vector3.zero;

                                SegmentDescription description = null;
                                int depth = reader.Depth;
                                do
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.TokenType == JsonToken.PropertyName)
                                        {
                                            switch ((string)reader.Value)
                                            {
                                                case Constants.TAG_SEGMENT_START:
                                                    readConnection(reader, serializer, roads, nodes, out start, out startTangent);
                                                    break;
                                                case Constants.TAG_SEGMENT_END:
                                                    readConnection(reader, serializer, roads, nodes, out end, out endTangent);
                                                    break;
                                                case Constants.TAG_SEGMENT_DESCRIPTION:
                                                    description = descriptions[reader.ReadAsInt32() ?? -1];
                                                    break;
                                            }
                                        }
                                    }
                                } while (reader.Depth > depth);

                                if (start != null && end != null)
                                {
                                    var seg = roads.CreateSegment(start, end, description);
                                    seg.Start.Tangent = startTangent;
                                    seg.End.Tangent = endTangent;
                                    yield return seg;
                                }
                                else
                                    throw new FormatException("expected start AND end connections for segment");
                            }
                        }
                    } while (reader.TokenType != JsonToken.EndArray);
                }
            }
        }



        private void readConnection(JsonTextReader reader, JsonSerializer serializer, RoadManager roads, List<Node> nodes, out Node start, out Vector3 startTangent)
        {
            start = null;
            startTangent = new Vector3(float.NaN, float.NaN, float.NaN);
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    int depth = reader.Depth;
                    do
                    {
                        if (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                switch ((string)reader.Value)
                                {
                                    case Constants.TAG_SEGMENT_CONNECTION_NODE:
                                        {
                                            int nodeID = reader.ReadAsInt32() ?? -1;
                                            start = nodes[nodeID];
                                        }
                                        break;
                                    case Constants.TAG_SEGMENT_CONNECTION_TANGENT:
                                        {
                                            if (reader.Read())
                                                startTangent = serializer.Deserialize<Vector3D>(reader);
                                        }
                                        break;
                                }
                            }
                        }
                    } while (reader.Depth > depth);
                }
            }
        }

        private IEnumerable<Node> readNodes(JsonTextReader reader, RoadManager roads, JsonSerializer serializer, List<Node> nodes)
        {
            if (reader.Read())
            {
                // start array
                if (reader.TokenType == JsonToken.StartArray)
                {
                    do
                    {
                        if (reader.Read()) // inside node
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                var position = serializer.Deserialize<Vector3D>(reader);
                                var node = roads.CreateNode(position);
                                nodes.Add(node);
                                yield return node;
                            }
                        }

                    } while (reader.TokenType != JsonToken.EndArray);
                }
                else throw new FormatException("Expected array start");
            }
        }

        #region Dispose
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool v)
        {
            disposed = true;
            if (v && !keepOpen)
                stream.Close();
        }
        ~RoadsReader()
        {
            Dispose(false);
        }
        #endregion
    }
}
