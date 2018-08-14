using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rbx2Source.Reflection.BinaryFormat
{
    public class BinaryChunkPRNT
    {
        public int LinkCount;
        public int[] ObjectIds;
        public int[] ParentIds;

        public BinaryChunkPRNT(BinaryChunk chunk)
        {
            using (BinaryReader reader = chunk.GetReader())
            {
                byte format = reader.ReadByte();

                LinkCount = reader.ReadInt32();
                ObjectIds = BinaryFile.ReadIds(reader, LinkCount);
                ParentIds = BinaryFile.ReadIds(reader, LinkCount);
            }
        }
    }
}
