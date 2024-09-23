using System.Collections.Generic;
using Unity.Netcode;

namespace Utilities
{
    public static class ListStringSerializationExtensions
    {
        public static void WriteValueSafe(this FastBufferWriter writer, in List<string> value)
        {
            writer.WriteValueSafe(value.Count);
            foreach (var item in value)
            {
                writer.WriteValueSafe(item);
            }
        }

        public static void ReadValueSafe(this FastBufferReader reader, out List<string> value)
        {
            value = new List<string>();
            reader.ReadValueSafe(out int count);
            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out string item);
                value.Add(item);
            }
        }
    }
}