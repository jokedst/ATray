using System;
using System.IO.Filesystem.Ntfs;
using Newtonsoft.Json;

namespace DiskUsage
{
    internal class CompactNodeConverter : JsonConverter
    {
        public int MaxDepth { get; }

        public CompactNodeConverter(int maxDepth = 2147483647) => this.MaxDepth = maxDepth;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is TreeNode treeNode1))
                throw new ArgumentException("Not an instance of TreeNode", nameof(value));
            writer.WriteStartArray();
            serializer.Serialize(writer, (object)treeNode1.NodeIndex);
            serializer.Serialize(writer, (object)treeNode1.Attributes);
            serializer.Serialize(writer, (object)treeNode1.Name);
            serializer.Serialize(writer, (object)(ulong)(treeNode1.SizeWithChildren != 0UL ? (long)treeNode1.SizeWithChildren : (long)treeNode1.Size));
            if (treeNode1.Children != null && treeNode1.Children.Count > 0)
            {
                int num = 1;
                for (TreeNode treeNode2 = treeNode1; treeNode2.Parent != null; treeNode2 = treeNode2.Parent)
                    ++num;
                if (num < this.MaxDepth)
                    serializer.Serialize(writer, (object)treeNode1.Children);
            }
            writer.WriteEndArray();
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