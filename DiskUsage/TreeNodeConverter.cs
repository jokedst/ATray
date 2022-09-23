using System;
using System.IO.Filesystem.Ntfs;
using Newtonsoft.Json;

namespace DiskUsage
{
    internal class TreeNodeConverter : JsonConverter
    {
        public int MaxDepth { get; }

        public bool Compact { get; }

        public TreeNodeConverter(int maxDepth = 2147483647, bool compact = false)
        {
            this.MaxDepth = maxDepth;
            this.Compact = compact;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is TreeNode treeNode1))
                throw new ArgumentException("Not an instance of TreeNode", nameof(value));
            bool flag = writer.WriteState == WriteState.Start;
            writer.WriteStartObject();
            writer.WritePropertyName(this.Compact ? "i" : "index");
            serializer.Serialize(writer, (object)treeNode1.NodeIndex);
            writer.WritePropertyName(this.Compact ? "a" : "attributes");
            serializer.Serialize(writer, (object)treeNode1.Attributes);
            writer.WritePropertyName(this.Compact ? "n" : "name");
            serializer.Serialize(writer, (object)treeNode1.Name);
            writer.WritePropertyName(this.Compact ? "s" : "size");
            serializer.Serialize(writer, (object)(ulong)(treeNode1.SizeWithChildren != 0UL ? (long)treeNode1.SizeWithChildren : (long)treeNode1.Size));
            if (treeNode1.Children != null && treeNode1.Children.Count > 0)
            {
                int num = 1;
                for (TreeNode treeNode2 = treeNode1; treeNode2.Parent != null; treeNode2 = treeNode2.Parent)
                    ++num;
                if (num < this.MaxDepth)
                {
                    writer.WritePropertyName(this.Compact ? "c" : "children");
                    serializer.Serialize(writer, (object)treeNode1.Children);
                }
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) => typeof(TreeNode).IsAssignableFrom(objectType);

        public override bool CanRead => false;
    }
}