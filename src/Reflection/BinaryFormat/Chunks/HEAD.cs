using System;
using System.IO;
using System.Text;

namespace Rbx2Source.Reflection.BinaryFormat
{
    public class BinaryChunkHEAD
    {
        private static string EXPECTED_HEADER = "<roblox!" + Encoding.ASCII.GetString(new byte[] { 0x89, 0xff, 0x0d, 0x0a, 0x1a, 0x0a });

        public ushort Version;
        public uint NumTypes;
        public uint NumInstances;
        public long Reserved;

        public BinaryChunkHEAD(BinaryReader reader)
        {
            byte[] binHeader = reader.ReadBytes(14);
            string header = Encoding.ASCII.GetString(binHeader);

            if (EXPECTED_HEADER == header)
            {
                Version = reader.ReadUInt16();
                NumTypes = reader.ReadUInt32();
                NumInstances = reader.ReadUInt32();
                Reserved = reader.ReadInt64();
            }
            else
            {
                throw new Exception("Unrecognized Header!");
            }
        }
    }
}
