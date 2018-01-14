using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.Utilities
{
    public class GraphViz
    {
        private Dictionary<Tuple<string, string>, IDictionary<string, string>> pairs = new Dictionary<Tuple<string, string>, IDictionary<string, string>>();
        private Dictionary<string, IDictionary<string, string>> nodes = new Dictionary<string, IDictionary<string, string>>();
        private Dictionary<string, IDictionary<string, string>> groups = new Dictionary<string, IDictionary<string, string>>();

        private Dictionary<string, string> groupMembers = new Dictionary<string, string>();


        public void SetGroup(string group, string item)
            => groupMembers[item] = group;

        public void Add(string name, IDictionary<string, string> attributes = null)
        {
            if (nodes.TryGetValue(name, out var dict))
            {
                if (attributes != null)
                    foreach (var attrib in attributes)
                        dict[attrib.Key] = attrib.Value;
            }
            else
            {
                nodes[name] = new Dictionary<string, string>(attributes ?? new Dictionary<string, string>());
            }
        }

        public void Connect(string a, string b, IDictionary<string, string> attributes = null, bool overwrite = true)
        {
            Add(a);
            Add(b);
            var key = new Tuple<string, string>(a, b);
            if (pairs.TryGetValue(key, out var dict))
            {
                if (attributes != null)
                    foreach (var attrib in attributes)
                    {
                        if (overwrite || !dict.ContainsKey(attrib.Key))
                            dict[attrib.Key] = attrib.Value;
                    }
            }
            else
            {
                pairs[key] = new Dictionary<string, string>(attributes ?? new Dictionary<string, string>());
            }
        }

        public override string ToString()
        {

            var sb = new StringBuilder();
            sb.AppendLine("digraph G {");

            var nodesByGroup = nodes.GroupBy(t => groupMembers.TryGetValue(t.Key, out string group) ? group : null);

            foreach (var group in nodesByGroup.OrderBy(t => t.Key))
            {
                if (group.Key != null)
                {
                    sb.Append("subgraph cluster");
                    sb.Append(group.Key);
                    sb.AppendLine("{");

                    if (groups.TryGetValue(group.Key, out var groupDetails))
                    {
                        foreach (var item in groupDetails)
                        {
                            sb.AppendLine(getKVstring(item));
                        }

                    }
                }
                foreach (var node in group.OrderBy(t => t.Key))
                {
                    sb.Append("\"");
                    sb.Append(node.Key.Replace("\"", "\"\""));
                    sb.Append("\"");
                    if (node.Value?.Any() ?? false)
                    {
                        sb.Append(" [");
                        sb.Append(string.Join(", ", node.Value.Select(getKVstring)));
                        sb.Append("]");
                    }
                    sb.AppendLine();
                }

                if (group.Key != null)
                    sb.AppendLine("}");
            }

            foreach (var pairs in pairs)
            {
                sb.Append("\"");
                sb.Append(pairs.Key.Item1.Replace("\"", "\"\""));
                sb.Append("\" -> \"");
                sb.Append(pairs.Key.Item2.Replace("\"", "\"\""));
                sb.Append("\"");
                if (pairs.Value?.Any() ?? false)
                {
                    sb.Append(" [");
                    sb.Append(string.Join(", ", pairs.Value.Select(getKVstring)));
                    sb.Append("]");
                }
                sb.AppendLine();
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string getKVstring(KeyValuePair<string, string> arg)
        {
            var sb = new StringBuilder();

            sb.Append("\"");
            sb.Append(arg.Key.Replace("\"", "\"\""));
            sb.Append("\"=\"");
            sb.Append(arg.Value.Replace("\"", "\"\""));
            sb.Append("\"");

            return sb.ToString();
        }

        public void GroupDetails(string name, IDictionary<string, string> attributes = null)
        {
            if (groups.TryGetValue(name, out var dict))
            {
                if (attributes != null)
                    foreach (var attrib in attributes)
                        dict[attrib.Key] = attrib.Value;
            }
            else
            {
                groups[name] = new Dictionary<string, string>(attributes ?? new Dictionary<string, string>());
            }
        }
    }

}
