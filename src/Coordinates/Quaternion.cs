using System;

namespace Rbx2Source.Coordinates
{
    // Quaternion acts as a utility for handling the interpolation of CFrame rotations.
    // it is not actually serialized into any model files, so it does not implement BaseCoordinates

    class Quaternion
    {
        private float x, y, z, w;

        public float X => x;
        public float Y => y;
        public float Z => z;
        public float W => w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaternion(Vector3 qv, float qw)
        {
            x = qv.x;
            y = qv.y;
            z = qv.z;
            w = qw;
        }

        public Quaternion(CFrame cf)
        {
            CFrame matrix = (cf - cf.p);
            float[] ac = cf.components();
            float mx  = ac[0], my  = ac[1],  mz  = ac[2], 
                  m11 = ac[3], m12 = ac[4],  m13 = ac[5], 
                  m21 = ac[6], m22 = ac[7],  m23 = ac[8],
                  m31 = ac[9], m32 = ac[10], m33 = ac[11];

            float trace = m11 + m22 + m33;

            if (trace > 0)
            {
                float s = (float)Math.Sqrt(1 + trace);
                float r = 0.5f / s;

                w = s * 0.5f;
                x = (m32 - m23) * r;
                y = (m13 - m31) * r;
                z = (m21 - m12) * r;
            }
            else
            {
                float big = Math.Max(Math.Max(m11, m22), m33);
                if (big == m11)
                {
                    float s = (float)Math.Sqrt(1 + m11 - m22 - m33);
                    float r = 0.5f / s;

                    w = (m32 - m23) * r;
                    x = 0.5f * s;
                    y = (m21 + m12) * r;
                    z = (m13 + m31) * r;
                }
                else if (big == m22)
                {
                    float s = (float)Math.Sqrt(1 - m11 + m22 - m33);
                    float r = 0.5f / s;

                    w = (m13 - m31) * r;
                    x = (m21 + m12) * r;
                    y = 0.5f * s;
                    z = (m32 + m23) * r;
                }
                else if (big == m33)
                {
                    float s = (float)Math.Sqrt(1 - m11 - m22 + m33);
                    float r = 0.5f / s;

                    w = (m21 - m12) * r;
                    x = (m13 + m31) * r;
                    y = (m32 + m23) * r;
                    z = 0.5f * s;
                }
            }
        }

        public float Dot(Quaternion other)
        {
            return (x * other.x) + (y * other.y) + (z * other.z) + (w * other.w);
        }

        public float Magnitude
        {
            get
            {
                float squared = Dot(this);
                return (float)Math.Sqrt(squared);
            }
        }

        public Quaternion Lerp(Quaternion other, float alpha)
        {
            Quaternion result = this * (1.0f - alpha) + other * alpha;
            return result / result.Magnitude;
        }

        public Quaternion Slerp(Quaternion other, float alpha)
        {
            float cosAng = Dot(other);

            if (cosAng < 0)
            {
                other = -other;
                cosAng = -cosAng;
            }

            float ang = (float)Math.Acos(cosAng);

            if (ang >= 0.05f)
            {
                float scale0 = (float)Math.Sin((1.0f - alpha) * ang);
                float scale1 = (float)Math.Sin(alpha * ang);
                float denom  = (float)Math.Sin(ang);

                return ((this * scale0) + (other * scale1)) / denom;
            }
            else
            {
                return Lerp(other, alpha);
            }
        }

        public CFrame ToCFrame()
        {
            float xc = x * 2f;
            float yc = y * 2f;
            float zc = z * 2f;

            float xx = x * xc;
            float xy = x * yc;
            float xz = x * zc;

            float wx = w * xc;
            float wy = w * yc;
            float wz = w * zc;

            float yy = y * yc;
            float yz = y * zc;
            float zz = z * zc;

            float[] components = new float[12]
            {
                0, 0, 0,

                1f - (yy + zz),
                xy - wz,
                xz + wy,

                xy + wz,
                1f - (xx + zz),
                yz - wx,

                xz - wy,
                yz + wx,
                1f - (xx + yy)
            };

            return CFrame.FromComponents(components);
        }

        public static Quaternion operator+(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static Quaternion operator-(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public static Quaternion operator *(Quaternion a, float f)
        {
            return new Quaternion(a.x * f, a.y * f, a.z * f, a.w * f);
        }

        public static Quaternion operator /(Quaternion a, float f)
        {
            return new Quaternion(a.x / f, a.y / f, a.z / f, a.w / f);
        }

        public static Quaternion operator-(Quaternion a)
        {
            return new Quaternion(-a.x, -a.y, -a.z, -a.w);
        }

        public static Quaternion operator*(Quaternion a, Quaternion b)
        {
            Vector3 v1 = new Vector3(a.x, a.y, a.z);
            float s1 = a.w;

            Vector3 v2 = new Vector3(b.x, b.y, b.z);
            float s2 = b.w;

            return new Quaternion(s1 * v2 + s2 * v1 + v1.Cross(v2), s1 * s2 - v1.Dot(v2));
        }
    }
}