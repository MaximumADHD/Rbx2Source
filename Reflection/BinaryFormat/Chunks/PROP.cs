using System;
using System.Linq;

using Rbx2Source.Coordinates;

namespace Rbx2Source.Reflection.BinaryFormat
{
    public class PROP
    {
        public readonly string Name;
        public readonly int TypeIndex;
        public readonly PropertyType Type;

        private RobloxBinaryReader Reader;

        public PROP(RobloxBinaryChunk chunk)
        {
            Reader = chunk.GetReader("PROP");

            TypeIndex = Reader.ReadInt32();
            Name = Reader.ReadString();

            try
            {
                byte propType = Reader.ReadByte();
                Type = (PropertyType)propType;
            }
            catch
            {
                Type = PropertyType.Unknown;
            }
        }

        public void ReadProperties(RobloxBinaryFile file)
        {
            INST type = file.Types[TypeIndex];
            FileProperty[] props = new FileProperty[type.NumInstances];

            int[] ids = type.InstanceIds;
            int instCount = type.NumInstances;

            for (int i = 0; i < instCount; i++)
            {
                int id = ids[i];
                FileInstance instance = file.Instances[id];

                FileProperty prop = new FileProperty();
                prop.Instance = instance;
                prop.Name = Name;
                prop.Type = Type;
                
                props[i] = prop;
                instance.AddProperty(ref prop);
            }

            // Setup some short-hand functions for actions frequently used during the read procedure.
            var readInts = new Func<int[]>(() => Reader.ReadInts(instCount));
            var readFloats = new Func<float[]>(() => Reader.ReadFloats(instCount));

            var loadProperties = new Action<Func<int, object>>(read =>
            {
                for (int i = 0; i < instCount; i++)
                {
                    object result = read(i);
                    props[i].Value = result;
                }
            });

            // Read the property data based on the property type.
            // NOTE: This only reads the property types needed for Rbx2Source.
            //       You can visit the following link for a full implementation:
            //       https://github.com/CloneTrooper1019/Roblox-File-Format/blob/master/BinaryFormat/ChunkTypes/PROP.cs

            switch (Type)
            {
                case PropertyType.String:
                    loadProperties(i => Reader.ReadString());
                    break;
                case PropertyType.Bool:
                    loadProperties(i => Reader.ReadBoolean());
                    break;
                case PropertyType.Int:
                case PropertyType.BrickColor:
                    int[] ints = readInts();
                    loadProperties(i => ints[i]);
                    break;
                case PropertyType.Float:
                    float[] floats = readFloats();
                    loadProperties(i => floats[i]);
                    break;
                case PropertyType.Double:
                    loadProperties(i => Reader.ReadDouble());
                    break;
                case PropertyType.Vector3:
                    float[] Vector3_X = readFloats(),
                            Vector3_Y = readFloats(),
                            Vector3_Z = readFloats();

                    loadProperties(i =>
                    {
                        float x = Vector3_X[i],
                              y = Vector3_Y[i],
                              z = Vector3_Z[i];

                        return new Vector3(x, y, z);
                    });

                    break;
                case PropertyType.CFrame:
                case PropertyType.Quaternion:
                    // Temporarily load the rotation matrices into their properties.
                    // We'll update them to CFrames once we iterate over the position data.

                    loadProperties(i =>
                    {
                        int normXY = Reader.ReadByte();

                        if (normXY > 0)
                        {
                            // Make sure this value is in a safe range.
                            normXY = (normXY - 1) % 36;

                            NormalId normX = (NormalId)(normXY / 6);
                            Vector3 R0 = Vector3.FromNormalId(normX);

                            NormalId normY = (NormalId)(normXY % 6);
                            Vector3 R1 = Vector3.FromNormalId(normY);

                            // Compute R2 using the cross product of R0 and R1.
                            Vector3 R2 = R0.Cross(R1);

                            // Generate the rotation matrix and return it.
                            return new float[9]
                            {
                                R0.X, R0.Y, R0.Z,
                                R1.X, R1.Y, R1.Z,
                                R2.X, R2.Y, R2.Z,
                            };
                        }
                        else if (Type == PropertyType.Quaternion)
                        {
                            float qx = Reader.ReadFloat(), qy = Reader.ReadFloat(),
                                  qz = Reader.ReadFloat(), qw = Reader.ReadFloat();

                            Quaternion quaternion = new Quaternion(qx, qy, qz, qw);
                            var rotation = quaternion.ToCFrame();

                            return rotation.GetComponents();
                        }
                        else
                        {
                            float[] matrix = new float[9];

                            for (int m = 0; m < 9; m++)
                            {
                                float value = Reader.ReadFloat();
                                matrix[m] = value;
                            }

                            return matrix;
                        }
                    });

                    float[] CFrame_X = readFloats(),
                            CFrame_Y = readFloats(),
                            CFrame_Z = readFloats();

                    loadProperties(i =>
                    {
                        float[] matrix = props[i].Value as float[];

                        float x = CFrame_X[i],
                              y = CFrame_Y[i],
                              z = CFrame_Z[i];

                        float[] position = new float[3] { x, y, z };
                        float[] components = position.Concat(matrix).ToArray();

                        return new CFrame(components);
                    });

                    break;
                case PropertyType.Enum:
                    uint[] enums = Reader.ReadInterleaved(instCount, BitConverter.ToUInt32);
                    loadProperties(i => enums[i]);

                    break;
                case PropertyType.Ref:
                    int[] instIds = Reader.ReadInstanceIds(instCount);

                    loadProperties(i =>
                    {
                        int instId = instIds[i];
                        return instId >= 0 ? file.Instances[instId] : null;
                    });

                    break;
                case PropertyType.Int64:
                    long[] int64s = Reader.ReadInterleaved(instCount, (buffer, start) =>
                    {
                        long result = BitConverter.ToInt64(buffer, start);
                        return (long)((ulong)result >> 1) ^ (-(result & 1));
                    });

                    loadProperties(i => int64s[i]);
                    break;
            }

            Reader.Dispose();
        }
    }
}
