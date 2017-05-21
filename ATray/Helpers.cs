namespace ATray
{
    using System.Collections.Generic;
    using System.IO;

    internal static class Helpers
    {
        public static void Write(this BinaryWriter binaryWriter, IList<string> strings)
        {
            binaryWriter.Write(strings.Count);
            foreach (var str in strings)
            {
                binaryWriter.Write(str);
            }
        }

        public static IEnumerable<string> ReadStrings(this BinaryReader binaryReader)
        {
            var count = binaryReader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                yield return binaryReader.ReadString();
            }
        }
    }
}
