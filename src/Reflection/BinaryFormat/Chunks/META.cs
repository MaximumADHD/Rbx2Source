using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rbx2Source.Reflection.BinaryFormat
{
    public class BinaryChunkMETA
    {
        public int Length;
        public Dictionary<string, string> Values;

        public BinaryChunkMETA(BinaryChunk chunk)
        {
            chunk.AssertChunkType(BinaryChunkType.META);
            using (BinaryReader reader = chunk.GetReader())
            {
                Length = reader.ReadInt32();
                Values = new Dictionary<string, string>(Length);

                for (int i = 0; i < Length; i++)
                {
                    string key = BinaryFile.ReadString(reader);
                    string value = BinaryFile.ReadString(reader);
                    Values.Add(key, value);
                }
            }
        }
    }
}
