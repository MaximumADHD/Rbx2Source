using System;
using System.IO;
using System.Linq;

namespace Rbx2Source.Reflection.BinaryFormat
{
    public enum BinaryPropertyFormat
    {
        Unknown,
        String,
        Bool,
        Int,
        Float,
        Double,
        UDim,
        UDim2,
        Ray,
        Faces,
        Axes,
        BrickColor,
        Color3,
        Vector2,
        Vector3,
        Vector2int16,
        CFrame,
        Quaternion,
        Enum,
        Ref,
        Vector3int16,
        NumberSequence,
        ColorSequence,
        NumberRange,
        Rect2D,
        PhysicalProperties,
        Color3uint8,
        Int64
    }

    [Flags]
    public enum Faces
    {
        None   = 0,
        Right  = 1,
        Top    = 2,
        Back   = 4,
        Left   = 8,
        Bottom = 16,
        Front  = 32,
        All    = 63
    }

    [Flags]
    public enum Axes
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
        All = 7
    }

    public struct UDim
    {
        public float Scale;
        public int Offset;

        public UDim(float scale, int offset)
        {
            Scale = scale;
            Offset = offset;
        }
    }

    public struct Ray
    {
        public float[] Origin;
        public float[] Direction;
    }

    public struct Keypoint
    {
        public float Time;
        public object Value;
        public float Envelope;
    }

    public class BinaryChunkPROP
    {
        public readonly int Index;
        public readonly string Name;
        public readonly BinaryPropertyFormat Format;
        public PropertyDescriptor[] Properties => props;

        private BinaryReader reader;
        private PropertyDescriptor[] props;

        public override string ToString()
        {
            return '[' + BinaryFile.GetEnumName(Format) + "] " + Name;
        }

        public BinaryChunkPROP(BinaryChunk chunk)
        {
            chunk.AssertChunkType(BinaryChunkType.PROP);
            reader = chunk.GetReader();

            Index = reader.ReadInt32();
            Name = BinaryFile.ReadString(reader);
            Format = (BinaryPropertyFormat)(reader.ReadByte());
        }

        public void ReadPropertyValues(BinaryChunkINST instChunk, ClassDescriptor[] instMap)
        {
            int[] ids = instChunk.InstanceIds;
            int instCount = ids.Length;

            props = new PropertyDescriptor[instCount];

            for (int i = 0; i < instCount; i++)
            {
                PropertyDescriptor prop = new PropertyDescriptor();
                prop.Name = Name;
                prop.Format = Format;

                props[i] = prop;
                instMap[ids[i]].Properties.Add(prop);
            }

            if (Format == BinaryPropertyFormat.String)
            {
                for (int i = 0; i < instCount; i++)
                    props[i].Value = BinaryFile.ReadString(reader);
            }
            else if (Format == BinaryPropertyFormat.Bool)
            {
                for (int i = 0; i < instCount; i++)
                    props[i].Value = reader.ReadBoolean();
            }
            else if (Format == BinaryPropertyFormat.Int)
            {
                int[] values = BinaryFile.ReadInts(reader, instCount);
                for (int i = 0; i < instCount; i++)
                    props[i].Value = values[i];
            }
            else if (Format == BinaryPropertyFormat.Float)
            {
                float[] values = BinaryFile.ReadFloats(reader, instCount);
                for (int i = 0; i < instCount; i++)
                    props[i].Value = values[i];
            }
            else if (Format == BinaryPropertyFormat.Double)
            {
                for (int i = 0; i < instCount; i++)
                    props[i].Value = reader.ReadDouble();
            }
            else if (Format == BinaryPropertyFormat.UDim)
            {
                float[] scales = BinaryFile.ReadFloats(reader, instCount);
                int[] offsets = BinaryFile.ReadInts(reader, instCount);

                for (int i = 0; i < instCount; i++)
                    props[i].Value = new UDim(scales[i], offsets[i]);
            }
            else if (Format == BinaryPropertyFormat.UDim2)
            {
                float[] sx = BinaryFile.ReadFloats(reader, instCount);
                float[] sy = BinaryFile.ReadFloats(reader, instCount);

                int[] ox = BinaryFile.ReadInts(reader, instCount);
                int[] oy = BinaryFile.ReadInts(reader, instCount);

                for (int i = 0; i < instCount; i++)
                {
                    UDim x = new UDim(sx[i], ox[i]);
                    UDim y = new UDim(sy[i], oy[i]);
                    props[i].Value = new UDim[2] { x, y };
                }
            }
            else if (Format == BinaryPropertyFormat.Ray)
            {
                for (int i = 0; i < instCount; i++)
                {
                    Ray ray = new Ray();
                    ray.Origin = BinaryFile.ReadFloats(reader, 3);
                    ray.Direction = BinaryFile.ReadFloats(reader, 3);
                    props[i].Value = ray;
                }
            }
            else if (Format == BinaryPropertyFormat.Faces)
            {
                for (int i = 0; i < instCount; i++)
                    props[i].Value = (Faces)reader.ReadByte();
            }
            else if (Format == BinaryPropertyFormat.Axes)
            {
                for (int i = 0; i < instCount; i++)
                    props[i].Value = (Axes)reader.ReadByte();
            }
            else if (Format == BinaryPropertyFormat.BrickColor)
            {
                int[] values = BinaryFile.ReadInts(reader, instCount);
                for (int i = 0; i < instCount; i++)
                    props[i].Value = values[i];
            }
            else if (Format == BinaryPropertyFormat.Color3)
            {
                float[] r = BinaryFile.ReadFloats(reader, instCount);
                float[] g = BinaryFile.ReadFloats(reader, instCount);
                float[] b = BinaryFile.ReadFloats(reader, instCount);

                for (int i = 0; i < instCount; i++)
                    props[i].Value = new float[3] { r[i], g[i], b[i] };
            }
            else if (Format == BinaryPropertyFormat.Vector2)
            {
                float[] x = BinaryFile.ReadFloats(reader, instCount);
                float[] y = BinaryFile.ReadFloats(reader, instCount);

                for (int i = 0; i < instCount; i++)
                    props[i].Value = new float[2] { x[i], y[i] };
            }
            else if (Format == BinaryPropertyFormat.Vector3)
            {
                float[] x = BinaryFile.ReadFloats(reader, instCount);
                float[] y = BinaryFile.ReadFloats(reader, instCount);
                float[] z = BinaryFile.ReadFloats(reader, instCount);

                for (int i = 0; i < instCount; i++)
                    props[i].Value = new float[3] { x[i], y[i], z[i] };
            }
            else if (Format == BinaryPropertyFormat.CFrame || Format == BinaryPropertyFormat.Quaternion)
            {
                float[][] matrices = new float[instCount][];
                for (int i = 0; i < instCount; i++)
                {
                    byte orientId = reader.ReadByte();
                    if (orientId > 0)
                    {
                        float[] R0 = BinaryFile.GetNormalFromId((orientId-1) / 6);
                        float[] R1 = BinaryFile.GetNormalFromId((orientId-1) % 6);

                        // Compute R2 using the cross product of R0 and R1
                        float[] R2 =
                        {
                            R0[1] * R1[2] - R1[1] * R0[2],
                            R0[2] * R1[0] - R1[2] * R0[0],
                            R0[0] * R1[1] - R1[0] * R0[1]
                        };

                        matrices[i] = R0.Concat(R1).Concat(R2).ToArray();
                    }
                    else if (Format == BinaryPropertyFormat.Quaternion)
                    {
                        float qx = reader.ReadSingle();
                        float qy = reader.ReadSingle();
                        float qz = reader.ReadSingle();
                        float qw = reader.ReadSingle();

                        float xc = qx * 2.0f;
                        float yc = qy * 2.0f;
                        float zc = qz * 2.0f;

                        float xx = qx * xc;
                        float xy = qx * yc;
                        float xz = qx * zc;

                        float wx = qw * xc;
                        float wy = qw * yc;
                        float wz = qw * zc;

                        float yy = qy * yc;
                        float yz = qy * zc;
                        float zz = qz * zc;

                        matrices[i] = new float[9]
                        {
                            1.0f - (yy + zz),
                            xy - wz,
                            xz + wy,
                            xy + wz,
                            1.0f - (xx + zz),
                            yz - wx,
                            xz - wy,
                            yz + wx,
                            1.0f - (xx + yy)
                        };
                    }
                    else
                    {
                        matrices[i] = BinaryFile.ReadFloats(reader, 9);
                    }
                }

                float[] x = BinaryFile.ReadFloats(reader, instCount);
                float[] y = BinaryFile.ReadFloats(reader, instCount);
                float[] z = BinaryFile.ReadFloats(reader, instCount);

                for (int i = 0; i < instCount; i++)
                {
                    float[] transform = new float[3] { x[i], y[i], z[i] };
                    float[] matrix = matrices[i];
                    props[i].Value = transform.Concat(matrix).ToArray();
                }
            }
            else if (Format == BinaryPropertyFormat.Enum)
            {
                uint[] enums = BinaryFile.ReadUInts(reader, instCount);
                for (int i = 0; i < instCount; i++)
                    props[i].Value = enums[i];
            }
            else if (Format == BinaryPropertyFormat.Ref)
            {
                int[] refs = BinaryFile.ReadInts(reader, instCount);
                for (int i = 0; i < instCount; i++)
                    if (refs[i] >= 0)
                        props[i].Value = instMap[refs[i]];
            }
            else if (Format == BinaryPropertyFormat.Vector3int16)
            {
                for (int i = 0; i < instCount; i++)
                {
                    short x = reader.ReadInt16();
                    short y = reader.ReadInt16();
                    short z = reader.ReadInt16();
                    props[i].Value = new short[3] { x, y, z };
                }
            }
            else if (Format == BinaryPropertyFormat.NumberSequence)
            {
                for (int i = 0; i < instCount; i++)
                {
                    int numKeys = reader.ReadInt32();
                    Keypoint[] sequence = new Keypoint[numKeys];

                    for (int k = 0; k < numKeys; k++)
                    {
                        Keypoint kp = new Keypoint();
                        kp.Time = reader.ReadSingle();
                        kp.Value = reader.ReadSingle();
                        kp.Envelope = reader.ReadSingle();
                        sequence[k] = kp;
                    }

                    props[i].Value = sequence;
                }
            }
            else if (Format == BinaryPropertyFormat.ColorSequence)
            {
                for (int i = 0; i < instCount; i++)
                {
                    int numKeys = reader.ReadInt32();
                    Keypoint[] sequence = new Keypoint[numKeys];

                    for (int k = 0; k < numKeys; k++)
                    {
                        Keypoint kp = new Keypoint();
                        kp.Time = reader.ReadSingle();

                        float r = reader.ReadSingle();
                        float g = reader.ReadSingle();
                        float b = reader.ReadSingle();
                        kp.Value = new float[3] { r, g, b };

                        kp.Envelope = reader.ReadSingle();
                        sequence[k] = kp;
                    }

                    props[i].Value = sequence;
                }
            }
            else if (Format == BinaryPropertyFormat.NumberRange)
            {
                for (int i = 0; i < instCount; i++)
                {
                    float min = reader.ReadSingle();
                    float max = reader.ReadSingle();
                    props[i].Value = new float[2] { min, max };
                }
            }
            else if (Format == BinaryPropertyFormat.Rect2D)
            {
                float[] x0 = BinaryFile.ReadFloats(reader, instCount);
                float[] y0 = BinaryFile.ReadFloats(reader, instCount);
                float[] x1 = BinaryFile.ReadFloats(reader, instCount);
                float[] y1 = BinaryFile.ReadFloats(reader, instCount);

                for (int i = 0; i < instCount; i++)
                    props[i].Value = new float[4] { x0[i], y0[i], x1[i], y1[i] };
            }
            else if (Format == BinaryPropertyFormat.PhysicalProperties)
            {
                for (int i = 0; i < instCount; i++)
                {
                    bool custom = reader.ReadBoolean();
                    if (custom)
                    {
                        float density = reader.ReadSingle();
                        float friction = reader.ReadSingle();
                        float elasticity = reader.ReadSingle();
                        float frictionWeight = reader.ReadSingle();
                        float elasticityWeight = reader.ReadSingle();
                        props[i].Value = new float[5] { density, friction, elasticity, frictionWeight, elasticityWeight };
                    }
                }
            }
            else if (Format == BinaryPropertyFormat.Color3uint8)
            {
                byte[] r = reader.ReadBytes(instCount);
                byte[] g = reader.ReadBytes(instCount);
                byte[] b = reader.ReadBytes(instCount);

                for (int i = 0; i < instCount; i++)
                    props[i].Value = new byte[3] { r[i], g[i], b[i] };
            }
            else if (Format == BinaryPropertyFormat.Int64)
            {
                for (int i = 0; i < instCount; i++)
                {
                    byte[] buffer = reader.ReadBytes(8);

                    long v0 = buffer[i];
                    long v1 = buffer[instCount + i];
                    long v2 = buffer[instCount * 2 + i];
                    long v3 = buffer[instCount * 3 + i];
                    long v4 = buffer[instCount * 4 + i];
                    long v5 = buffer[instCount * 5 + i];
                    long v6 = buffer[instCount * 6 + i];
                    long v7 = buffer[instCount * 7 + i];

                    long result = (v0 << 56) | (v1 << 48) | (v2 << 40) | (v3 << 32) | (v4 << 24) | (v5 << 16) | (v6 << 8) | v7;
                    props[i].Value = (long)((ulong)result >> 1) ^ (-(result & 1));
                }
            }
            else
            {
                Console.WriteLine("Unhandled Format {0}!", Format);
            }
        }
    }
}
