using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Simulation.Traffic.IO
{
    public class Writer
    {
        public void Save(RoadManager roads, Stream stream)
        {
            var nodes = new Dictionary<Node, int>();
            var descriptions = new Dictionary<SegmentDescription, int>();

            var writer = new JsonTextWriter(new StreamWriter(stream));
            //var writer = new BsonWriter(stream);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartObject();
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new StringEnumConverter());
            serializer.DefaultValueHandling = DefaultValueHandling.Ignore;

            writeNodes(roads, nodes, writer, serializer);
            writeDescriptions(roads, descriptions, writer, serializer);
            writeSegments(roads, nodes, descriptions, writer, serializer);
            writer.WriteEndObject();
            writer.Close();
        }

        private void writeDescriptions(RoadManager roads, Dictionary<SegmentDescription, int> descriptions, JsonTextWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName(Constants.TAG_SEGMENT_DESCRIPTIONS);
            writer.WriteStartArray();
            foreach (var segment in roads.Segments)
            {
                int index;
                if (!descriptions.TryGetValue(segment.Description, out index))
                {
                    index = descriptions.Count;
                    descriptions.Add(segment.Description, index);

                    writer.WriteStartObject();

                    writer.WritePropertyName(Constants.TAG_SEGMENT_DESCRIPTION_LANES);

                    writer.WriteStartArray();

                    foreach (var lane in segment.Description.Lanes)
                        serializer.Serialize(writer, lane);

                    writer.WriteEndArray();

                    writer.WriteEndObject();

                }
            }
            writer.WriteEndArray();
        }

        private static void writeNodes(RoadManager roads, Dictionary<Node, int> nodes, JsonTextWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName(Constants.TAG_NODES);
            writer.WriteStartArray();
            var i = 0;
            foreach (var node in roads.Nodes)
            {
                nodes[node] = i;
                serializer.Serialize(writer, (Vector3D)node.Position);
                i++;
            }
            writer.WriteEndArray();
        }

        private static void writeSegments(RoadManager roads, Dictionary<Node, int> nodes, Dictionary<SegmentDescription, int> descriptions, JsonTextWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName(Constants.TAG_SEGMENTS);
            writer.WriteStartArray();
            foreach (var segment in roads.Segments)
            {
                writer.WriteStartObject();
                {
                    writeConnection(serializer, nodes, writer, segment.Start, Constants.TAG_SEGMENT_START);
                    writeConnection(serializer, nodes, writer, segment.End, Constants.TAG_SEGMENT_END);
                }
                writer.WritePropertyName(Constants.TAG_SEGMENT_DESCRIPTION);
                writer.WriteValue(descriptions[segment.Description]);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        private static void writeConnection(JsonSerializer serializer, Dictionary<Node, int> nodes, JsonWriter writer, SegmentNodeConnection connection, string type)
        {
            writer.WritePropertyName(type);
            writer.WriteStartObject();
            {
                writer.WritePropertyName(Constants.TAG_SEGMENT_CONNECTION_NODE);
                writer.WriteValue(nodes[connection.Node]);
                writer.WritePropertyName(Constants.TAG_SEGMENT_CONNECTION_TANGENT);
                serializer.Serialize(writer, (Vector3D)connection.Tangent);
            }
            writer.WriteEndObject();
        }

    }
}
