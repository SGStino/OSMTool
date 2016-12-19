using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation.Traffic.IO
{
    public class Reader
    {
        public void Load(RoadManager roads, Stream data)
        {
            var nodes = new List<Node>();
            var reader = new JsonTextReader(new StreamReader(data));

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
                                    readNodes(reader, roads, serializer, nodes);
                                    break;
                                case Constants.TAG_SEGMENT_DESCRIPTIONS:
                                    readDescriptions(reader, serializer, descriptions);
                                    break;
                                case Constants.TAG_SEGMENTS:
                                    readSegments(reader, roads, serializer, nodes, descriptions);
                                    break;
                            }
                        }
                    }
                } while (reader.TokenType != JsonToken.EndObject);
            }
        }

        private void readDescriptions(JsonTextReader reader, JsonSerializer serializer, List<SegmentDescription> descriptions)
        {
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

        private void readSegments(JsonTextReader reader, RoadManager roads, JsonSerializer serializer, List<Node> nodes, List<SegmentDescription> descriptions)
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

        private void readNodes(JsonTextReader reader, RoadManager roads, JsonSerializer serializer, List<Node> nodes)
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
                                nodes.Add(roads.CreateNode(position));
                            }
                        }

                    } while (reader.TokenType != JsonToken.EndArray);
                }
                else throw new FormatException("Expected array start");
            }
        }

    }
}
