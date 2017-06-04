using System;
using System.IO;
using System.Xml;

namespace Rbx2Source.Coordinates
{

    class Vector3 : BaseCoordinates
    {
        public readonly float x, y, z;

        public Vector3 unit { get { return normalize(this); } }
        public float magnitude { get { return calcMagnitude(this); } }

        public Vector3(float x = 0, float y = 0, float z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Constructs a Vector3 by using the first 3 floats specified in the float[] parameter.
        /// </summary>
        /// <param name="coords">The XYZ coordinates packaged into a float array</param>
        public Vector3(float[] coords)
        {
            this.x = coords[0];
            this.y = coords[1];
            this.z = coords[2];
        }

        /// <summary>
        /// Constructs a Vector3 by reading the next 3 floats from the specified BinaryReader.
        /// </summary>
        /// <param name="reader">The BinaryReader to read from</param>
        public Vector3 (BinaryReader reader, bool flipX = false, bool flipY = false)
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();

            if (flipX) x = 1 - x;
            if (flipY) y = 1 - y;
        }

        public Vector3(XmlNode vecData)
        {
            float[] p = new float[3];
            for (int i = 0; i < 3; i++)
                p[i] = float.Parse(vecData.ChildNodes[i].InnerText, Rbx2Source.NormalParse);

            this.x = p[0];
            this.y = p[1];
            this.z = p[2];
        }

        public static Vector3 FromNormalId(int normalId)
        {
            Vector3 result = null;
            switch(normalId)
            {
                case 0:
                    result = new Vector3(1, 0, 0);
                    break;
                case 1:
                    result = new Vector3(0, 1, 0);
                    break;
                case 2:
                    result = new Vector3(0, 0, 1);
                    break;
                case 3:
                    result = new Vector3(-1, 0, 0);
                    break;
                case 4:
                    result = new Vector3(0, -1, 0);
                    break;
                case 5:
                    result = new Vector3(0, 0, -1);
                    break;
            }
            return result;
        }

        // operator overloads

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.x, -v.y, -v.z);
        }

        public static Vector3 operator *(float k, Vector3 a)
        {
            return new Vector3(a.x * k, a.y * k, a.z * k);
        }

        public static Vector3 operator *(Vector3 a, float k)
        {
            return new Vector3(a.x * k, a.y * k, a.z * k);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 operator /(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public override string ToString()
        {
            return x + "," + y + "," + z;
        }

        protected override string ToStudioMdlString_Impl(bool excludeZ = false)
        {
            float[] values;
            float scale = 10; // TODO: Make this a setting.
            if (excludeZ)
                values = new float[2] {x, 1-y};
            else
                values = new float[3] { x * scale, y * scale, z * scale };

            return string.Join(" ", truncate(values));
        }

        // statics

        private static float calcMagnitude(Vector3 v)
        {
            return (float)Math.Sqrt(Dot(v, v));
        }

        private static Vector3 normalize(Vector3 v)
        {
            float m = calcMagnitude(v);
            float nx = v.x / m, ny = v.y / m, nz = v.z / m;
            return new Vector3(nx, ny, nz);
        }

        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.y * b.z - b.y * a.z,
                a.z * b.x - b.z * a.x,
                a.x * b.y - b.x * a.y
            );
        }

        // methods

        public Vector3 Lerp(Vector3 b, float t)
        {
            return (1 - t) * this + t * b;
        }

    }
}
