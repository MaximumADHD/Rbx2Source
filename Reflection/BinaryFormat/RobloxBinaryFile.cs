using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rbx2Source.Reflection.BinaryFormat
{
    public class RobloxBinaryFile
    {
        // Header Specific
        public const string MagicHeader = "<roblox!\x89\xff\x0d\x0a\x1a\x0a";

        public ushort Version;
        public uint   NumTypes;
        public uint   NumInstances;
        public byte[] Reserved;
        
        // Runtime Specific
        public FileInstance Contents = new FileInstance("Folder", "RobloxBinaryFile");
        public List<RobloxBinaryChunk> Chunks = new List<RobloxBinaryChunk>();
        public override string ToString() => GetType().Name;

        public INST[] Types;
        public META Metadata;
        public FileInstance[] Instances;
        
        public RobloxBinaryFile(byte[] contents)
        {
            using (MemoryStream file = new MemoryStream(contents))
            using (RobloxBinaryReader reader = new RobloxBinaryReader(file))
            {
                // Verify the signature of the file.
                byte[] binSignature = reader.ReadBytes(14);
                string signature = Encoding.UTF7.GetString(binSignature);

                if (signature != MagicHeader)
                    throw new InvalidDataException("Provided file's signature does not match RobloxBinaryFile.MagicHeader!");

                // Read header data.
                Version = reader.ReadUInt16();
                NumTypes = reader.ReadUInt32();
                NumInstances = reader.ReadUInt32();
                Reserved = reader.ReadBytes(8);

                // Begin reading the file chunks.
                bool reading = true;

                Types = new INST[NumTypes];
                Instances = new FileInstance[NumInstances];

                while (reading)
                {
                    try
                    {
                        RobloxBinaryChunk chunk = new RobloxBinaryChunk(reader);
                        Chunks.Add(chunk);

                        switch (chunk.ChunkType)
                        {
                            case "INST":
                                INST type = new INST(chunk);
                                type.Allocate(this);
                                break;
                            case "PROP":
                                PROP prop = new PROP(chunk);
                                prop.ReadProperties(this);
                                break;
                            case "PRNT":
                                PRNT hierarchy = new PRNT(chunk);
                                hierarchy.Assemble(this);
                                break;
                            case "META":
                                Metadata = new META(chunk);
                                break;
                            case "END\0":
                                reading = false;
                                break;
                            default:
                                Chunks.Remove(chunk);
                                break;
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        throw new Exception("Unexpected end of file!");
                    }
                }
            }
        }
    }
}