using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Rbx2Source.Reflection.BinaryFormat
{
    public class PropertyDescriptor
    {
        public string Name;
        public BinaryPropertyFormat Format;
        public object Value;

        public override string ToString()
        {
            string typeName = BinaryFile.GetEnumName(Format);
            string valueLabel;

            if (Value != null)
                valueLabel = Value.ToString();
            else
                valueLabel = "?";

            return string.Join(" ", typeName, Name, '=', valueLabel);
        }
    }

    public class ClassDescriptor
    {
        private List<ClassDescriptor> _children = new List<ClassDescriptor>();
        private ClassDescriptor _parent;

        public string ClassName;
        public List<PropertyDescriptor> Properties = new List<PropertyDescriptor>();

        public bool IsAncestorOf(ClassDescriptor other)
        {
            while (other != null)
            {
                if (other == this)
                    return true;

                other = other._parent;
            }

            return false;
        }

        public bool IsDescendantOf(ClassDescriptor other)
        {
            return other.IsAncestorOf(this);
        }

        public ClassDescriptor Parent
        {
            get { return _parent; }
            set
            {
                if (IsAncestorOf(Parent))
                    throw new Exception("Parent would result in circular reference.");

                if (Parent == this)
                    throw new Exception("Attempt to set parent to self");

                if (_parent != null)
                    _parent._children.Remove(this);

                value._children.Add(this);
                _parent = value;
            }
        }

        public ReadOnlyCollection<ClassDescriptor> Children
        {
            get { return _children.AsReadOnly(); }
        }

        public override string ToString()
        {
            string result = '[' + ClassName + ']';

            PropertyDescriptor nameDescriptor = Properties.Where(prop => prop.Name == "Name").First();
            if (nameDescriptor != null)
                result += ' ' + nameDescriptor.Value.ToString();

            return result;
        }
    }

    public class BinaryFile
    {
        private List<BinaryChunk> Chunks = new List<BinaryChunk>();

        private BinaryChunkHEAD Headers;
        private BinaryChunkPRNT ParentLinks;

        private List<BinaryChunkINST> INSTs = new List<BinaryChunkINST>();
        private List<BinaryChunkPROP> PROPs = new List<BinaryChunkPROP>();

        public ClassDescriptor[] Instances;
        private BinaryChunkMETA Metadata;
        public List<ClassDescriptor> TreeRoot = new List<ClassDescriptor>();

        public static string GetEnumName<T>(T value)
        {
            return Enum.GetName(typeof(T), value);
        }

        internal static uint decodeUInt(int value)
        {
            return (uint)value;
        }

        internal static int decodeInt(int value)
        {
            return (value >> 1) ^ (-(value & 1));
        }

        internal static float decodeFloat(int value)
        {
            // Sign bit is moved to the end during encoding.
            uint u = (uint)value;
            uint i = (u >> 1) | (u << 31);

            byte[] b = BitConverter.GetBytes(i);
            return BitConverter.ToSingle(b, 0);
        }

        internal static T[] readIntEncodedArray<T>(BinaryReader reader, int count, Func<int,T> decode)
        {
            byte[] buffer = reader.ReadBytes(count * 4);
            T[] values = new T[count];

            for (int i = 0; i < count; i++)
            {
                byte v0 = buffer[i];
                byte v1 = buffer[count + i];
                byte v2 = buffer[count * 2 + i];
                byte v3 = buffer[count * 3 + i];

                int result = (v0 << 24) | (v1 << 16) | (v2 << 8) | v3;
                values[i] = decode(result);
            }

            return values;
        }

        internal static int[] ReadInts(BinaryReader reader, int count) => readIntEncodedArray(reader, count, decodeInt);
        internal static float[] ReadFloats(BinaryReader reader, int count) => readIntEncodedArray(reader, count, decodeFloat);
        internal static uint[] ReadUInts(BinaryReader reader, int count) => readIntEncodedArray(reader, count, decodeUInt);

        internal static int[] ReadIds(BinaryReader reader, int count)
        {
            int[] values = ReadInts(reader, count);
            int last = 0;

            for (int i = 0; i < count; ++i)
            {
                values[i] += last;
                last = values[i];
            }

            return values;
        }

        internal static float[] GetNormalFromId(int normalId)
        {
            switch (normalId)
            {
                case 0: // Right
                    return new float[3] {  1.0f,  0.0f,  0.0f };
                case 1: // Top
                    return new float[3] {  0.0f,  1.0f,  0.0f };
                case 2: // Back
                    return new float[3] {  0.0f,  0.0f,  1.0f };
                case 3: // Left
                    return new float[3] { -1.0f,  0.0f,  0.0f };
                case 4: // Bottom
                    return new float[3] {  0.0f, -1.0f,  0.0f };
                case 5: // Front
                    return new float[3] {  0.0f,  0.0f, -1.0f };
                default:
                    throw new Exception("Unknown NormalId");
            }
        }

        internal static string ReadString(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            byte[] buffer = reader.ReadBytes(length);
            return Encoding.Default.GetString(buffer);
        }

        public BinaryFile(byte[] contents)
        {
            using (MemoryStream file = new MemoryStream(contents))
            using (BinaryReader inStream = new BinaryReader(file))
            {
                Headers = new BinaryChunkHEAD(inStream);
                Instances = new ClassDescriptor[Headers.NumInstances];

                // Begin reading the file chunks.
                bool firstChunk = true;
                Chunks = new List<BinaryChunk>();

                while (true)
                {
                    try
                    {
                        BinaryChunk chunk = new BinaryChunk(inStream);
                        Chunks.Add(chunk);

                        if (chunk.ChunkType == BinaryChunkType.INST)
                        {
                            BinaryChunkINST inst = new BinaryChunkINST(chunk);
                            INSTs.Add(inst);
                        }
                        else if (chunk.ChunkType == BinaryChunkType.PROP)
                        {
                            BinaryChunkPROP prop = new BinaryChunkPROP(chunk);
                            PROPs.Add(prop);
                        }
                        else if (chunk.ChunkType == BinaryChunkType.PRNT)
                        {
                            ParentLinks = new BinaryChunkPRNT(chunk);
                        }
                        else if (chunk.ChunkType == BinaryChunkType.META)
                        {
                            if (firstChunk)
                                Metadata = new BinaryChunkMETA(chunk);
                            else
                                throw new Exception("Unexpected metadata chunk");
                        }
                        else if (chunk.ChunkType == BinaryChunkType.END)
                        {
                            break;
                        }

                        firstChunk = false;
                    }
                    catch (EndOfStreamException)
                    {
                        throw new Exception("Unexpected end of file!");
                    }
                }

                foreach (BinaryChunkINST inst in INSTs)
                {
                    foreach (int id in inst.InstanceIds)
                    {
                        ClassDescriptor bind = new ClassDescriptor();
                        bind.ClassName = inst.TypeName;
                        Instances[id] = bind;
                    }
                }

                foreach (BinaryChunkPROP prop in PROPs)
                {
                    BinaryChunkINST inst = INSTs[prop.Index];
                    prop.ReadPropertyValues(inst, Instances);
                }

                for (int i = 0; i < ParentLinks.LinkCount; i++)
                {
                    int objectId = ParentLinks.ObjectIds[i];
                    int parentId = ParentLinks.ParentIds[i];

                    if (parentId >= 0)
                        Instances[objectId].Parent = Instances[parentId];
                    else
                        TreeRoot.Add(Instances[objectId]);
                }
            }
        }
    }
}
