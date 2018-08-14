using System.Collections.Generic;
using System.IO;

namespace Rbx2Source.Reflection.BinaryFormat
{
    public class BinaryChunkINST
    {
        public readonly int TypeIndex;
        public readonly string TypeName;
        public readonly bool IsService;
        public readonly int NumInstances;
        public readonly int[] InstanceIds;

        public Dictionary<string, BinaryChunkPROP> Properties;

        public override string ToString()
        {
            return TypeName;
        }

        public BinaryChunkINST(BinaryChunk chunk)
        {
            chunk.AssertChunkType(BinaryChunkType.INST);

            using (BinaryReader reader = chunk.GetReader())
            {
                TypeIndex = reader.ReadInt32();
                TypeName = BinaryFile.ReadString(reader);
                IsService = reader.ReadBoolean();
                NumInstances = reader.ReadInt32();
                InstanceIds = BinaryFile.ReadIds(reader, NumInstances);
            }

            Properties = new Dictionary<string, BinaryChunkPROP>();
        }
    }
}
