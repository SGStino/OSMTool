using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Simulation.Traffic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Simulation.Traffic.IO
{
    public class RoadsWriter : IDisposable
    {
        private enum writeStage
        {
            None,
            Nodes,
            Descriptions,
            Segments
        };

        private readonly Stream stream;
        private readonly bool keepOpen;
        private readonly Dictionary<Node, int> nodes;
        private readonly Dictionary<SegmentDescription, int> descriptions;
        private readonly JsonSerializer serializer;
        private readonly JsonTextWriter writer;
        private writeStage stage;
        private int nodeCount = 0;
        private int descriptionCount;
        private int segmentCount;
        private bool disposed;

        public RoadsWriter(Stream stream, bool keepOpen = false)
        {
            this.stream = stream;
            this.keepOpen = keepOpen;
            this.nodes = new Dictionary<Node, int>();
            this.descriptions = new Dictionary<SegmentDescription, int>();

            writer = new JsonTextWriter(new StreamWriter(stream, Encoding.UTF8));
            //var writer = new BsonWriter(stream);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartObject();
            serializer = new JsonSerializer();
            serializer.Converters.Add(new StringEnumConverter());
            serializer.DefaultValueHandling = DefaultValueHandling.Ignore;
            stage = writeStage.None;
            disposed = false;
        }

        #region Ensures
        private void EnsureNotDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RoadsWriter));
        }
        private void EnsureBegin(writeStage newStage)
        {
            EnsureNotDisposed();
            if (stage != writeStage.None)
                if (newStage != stage)
                    throw new InvalidOperationException("Can't begin writing " + newStage + ", call EndWrite" + stage + "() first");
                else
                    throw new InvalidOperationException("Already writing " + newStage);
            stage = newStage;
        }

        private void EnsureEnd(writeStage requiredStage)
        {
            EnsureNotDisposed();
            if (requiredStage != stage) throw new InvalidOperationException("Can't end " + requiredStage + ", call BeginWrite" + requiredStage + "() instead");
            stage = writeStage.None;
        }

        private void EnsureWrite(writeStage requiredStage)
        {
            EnsureNotDisposed();
            if (requiredStage != stage) throw new InvalidOperationException("Can't write " + requiredStage + ", call BeginWrite" + requiredStage + "() First");
        }
        #endregion

        #region Nodes
        public void BeginWriteNodes()
        {
            EnsureBegin(writeStage.Nodes);
            writer.WritePropertyName(Constants.TAG_NODES);
            writer.WriteStartArray();
            stage = writeStage.Nodes;
        }

        public void EndWriteNodes()
        {
            EnsureEnd(writeStage.Nodes);
            writer.WriteEndArray();
            stage = writeStage.None;
        }

        public int Write(Node node)
        {
            EnsureWrite(writeStage.Nodes);
            int i = nodeCount++;
            nodes[node] = i;
            serializer.Serialize(writer, (Vector3D)node.Position);
            return i;
        }
        #endregion

        #region Descriptions
        public void BeginWriteDescriptions()
        {
            EnsureBegin(writeStage.Descriptions);
            writer.WritePropertyName(Constants.TAG_SEGMENT_DESCRIPTIONS);
            writer.WriteStartArray();
        }
        public void EndWriteDescriptions()
        {
            EnsureEnd(writeStage.Descriptions);
            writer.WriteEndArray();
        }

        public int Write(SegmentDescription description)
        {
            EnsureWrite(writeStage.Descriptions);
            int i = descriptionCount++;
            descriptions[description] = i;

            writer.WriteStartObject();
            writer.WritePropertyName(Constants.TAG_SEGMENT_DESCRIPTION_LANES);
            writer.WriteStartArray();
            foreach (var lane in description.Lanes)
                serializer.Serialize(writer, lane);
            writer.WriteEndArray();
            writer.WriteEndObject();
            return i;
        }
        #endregion

        #region  Segments
        public void BeginWriteSegments()
        {
            EnsureBegin(writeStage.Segments);
            writer.WritePropertyName(Constants.TAG_SEGMENTS);
            writer.WriteStartArray();
        }
        public void EndWriteSegments()
        {
            writer.WriteEndArray();
        }
        public int Write(Segment segment)
        {
            int i = segmentCount++;
            writer.WriteStartObject();
            {
                writeConnection(serializer, nodes, writer, segment.Start, Constants.TAG_SEGMENT_START);
                writeConnection(serializer, nodes, writer, segment.End, Constants.TAG_SEGMENT_END);
            }
            writer.WritePropertyName(Constants.TAG_SEGMENT_DESCRIPTION);
            writer.WriteValue(descriptions[segment.Description]);
            writer.WriteEndObject();
            return i;
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
        #endregion 


        public void WriteAll(RoadManager roads)
        {
            var nodes = roads.Nodes.ToArray();
            var segments = roads.Segments.ToArray();

            BeginWriteNodes();
            foreach (var node in nodes.OrderBy(n => n.Position.sqrMagnitude))
                Write(node);
            EndWriteNodes();
            BeginWriteDescriptions();
            foreach (var description in segments.Select(s => s.Description).Distinct())
                Write(description);
            EndWriteDescriptions();
            BeginWriteSegments();
            foreach (var segment in segments.OrderBy(s => s.Start.Node.Position.sqrMagnitude))
                Write(segment);
            EndWriteSegments();
            writer.Flush();
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
            {
                writer.Close();
                stream.Close();
            }
        }
        ~RoadsWriter()
        {
            Dispose(false);
        }
        #endregion
    }
}
