using System;
using System.IO;
using System.Text;
using LZ4;

namespace Rbx2Source.Reflection.BinaryFormat
{
    public enum BinaryChunkType
    {
        INST,
        PROP,
        PRNT,
        META,
        END,
    }

    public class BinaryChunk
    {
        public readonly BinaryChunkType ChunkType = BinaryChunkType.END;

        public readonly int CompressedSize;
        public readonly byte[] CompressedData;

        public readonly int Size;
        public readonly int Reserved;
        public readonly byte[] Data;

        public readonly bool HasCompressedData;

        public override string ToString()
        {
            return ChunkType + " Chunk [" + Size + ']';
        }

        public BinaryReader GetReader()
        {
            MemoryStream buffer = new MemoryStream(Data);
            return new BinaryReader(buffer);
        }

        internal void AssertChunkType(BinaryChunkType type)
        {
            if (ChunkType != type)
                throw new Exception("Expected " + Enum.GetName(typeof(BinaryChunkType), type) + " ChunkType from input BinaryChunk");
        }

        public BinaryChunk(BinaryReader reader)
        {
            byte[] bChunkType = reader.ReadBytes(4);
            string sChunkType = Encoding.Default.GetString(bChunkType).Replace('\0', ' ').Trim();
            if (!Enum.TryParse(sChunkType, out ChunkType))
                throw new Exception("Unknown Chunk Type: " + sChunkType);

            CompressedSize = reader.ReadInt32();
            Size = reader.ReadInt32();
            Reserved = reader.ReadInt32();

            if (CompressedSize == 0)
            {
                HasCompressedData = false;
                Data = reader.ReadBytes(Size);
            }
            else
            {
                HasCompressedData = true;
                CompressedData = reader.ReadBytes(CompressedSize);
                Data = LZ4Codec.Decode(CompressedData, 0, CompressedSize, Size);
            }
        }
    }
}
