using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Rbx2Source.Coordinates
{
    class CFrame : BaseCoordinates
    {
        private float m11 = 1, m12 = 0, m13 = 0, m14 = 0;
        private float m21 = 0, m22 = 1, m23 = 0, m24 = 0;
        private float m31 = 0, m32 = 0, m33 = 1, m34 = 0;
        private const float m41 = 0, m42 = 0, m43 = 0, m44 = 1;

        public readonly float x = 0, y = 0, z = 0;
        public readonly Vector3 p = new Vector3(0, 0, 0);
        public readonly Vector3 lookVector = new Vector3(0, 0, -1);
        public readonly Vector3 rightVector = new Vector3(1, 0, 0);
        public readonly Vector3 upVector = new Vector3(0, 1, 0);

        private static Vector3 RIGHT = new Vector3(1, 0, 0);
        private static Vector3 UP = new Vector3(0, 1, 0);
        private static Vector3 BACK = new Vector3(0, 0, 1);

        // constructors

        public CFrame(Vector3 pos)
        {
            m14 = pos.x;
            m24 = pos.y;
            m34 = pos.z;
            x = m14; y = m24; z = m34;
            p = new Vector3(m14, m24, m34);
            lookVector = new Vector3(-m13, -m23, -m33);
            rightVector = new Vector3(m11, m21, m31);
            upVector = new Vector3(m12, m22, m32);
        }

        public CFrame(Vector3 eye, Vector3 look)
        {
            Vector3 zAxis = (eye - look).unit;
            Vector3 xAxis = Vector3.Cross(UP, zAxis);
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis);
            if (xAxis.magnitude == 0)
            {
                if (zAxis.y < 0)
                {
                    xAxis = new Vector3(0, 0, -1);
                    yAxis = new Vector3(1, 0, 0);
                    zAxis = new Vector3(0, -1, 0);
                }
                else
                {
                    xAxis = new Vector3(0, 0, 1);
                    yAxis = new Vector3(1, 0, 0);
                    zAxis = new Vector3(0, 1, 0);
                }
            }
            m11 = xAxis.x; m12 = yAxis.x; m13 = zAxis.x; m14 = eye.x;
            m21 = xAxis.y; m22 = yAxis.y; m23 = zAxis.y; m24 = eye.y;
            m31 = xAxis.z; m32 = yAxis.z; m33 = zAxis.z; m34 = eye.z;
            x = m14; y = m24; z = m34;
            p = new Vector3(m14, m24, m34);
            lookVector = new Vector3(-m13, -m23, -m33);
            rightVector = new Vector3(m11, m21, m31);
            upVector = new Vector3(m12, m22, m32);
        }

        public CFrame(float nx = 0, float ny = 0, float nz = 0)
        {
            m14 = nx;
            m24 = ny;
            m34 = nz;
            x = m14; y = m24; z = m34;
            p = new Vector3(m14, m24, m34);
            lookVector = new Vector3(-m13, -m23, -m33);
            rightVector = new Vector3(m11, m21, m31);
            upVector = new Vector3(m12, m22, m32);
        }

        public CFrame(float nx, float ny, float nz, float i, float j, float k, float w)
        {
            m14 = nx;
            m24 = ny;
            m34 = nz;
            m11 = 1 - 2 * (float)Math.Pow(j, 2) - 2 * (float)Math.Pow(k, 2);
            m12 = 2 * (i * j - k * w);
            m13 = 2 * (i * k + j * w);
            m21 = 2 * (i * j + k * w);
            m22 = 1 - 2 * (float)Math.Pow(i, 2) - 2 * (float)Math.Pow(k, 2);
            m23 = 2 * (j * k - i * w);
            m31 = 2 * (i * k - j * w);
            m32 = 2 * (j * k + i * w);
            m33 = 1 - 2 * (float)Math.Pow(i, 2) - 2 * (float)Math.Pow(j, 2);
            x = m14; y = m24; z = m34;
            p = new Vector3(m14, m24, m34);
            lookVector = new Vector3(-m13, -m23, -m33);
            rightVector = new Vector3(m11, m21, m31);
            upVector = new Vector3(m12, m22, m32);
        }

        public CFrame(float n14, float n24, float n34, float n11, float n12, float n13, float n21, float n22, float n23, float n31, float n32, float n33)
        {
            m14 = n14; m24 = n24; m34 = n34;
            m11 = n11; m12 = n12; m13 = n13;
            m21 = n21; m22 = n22; m23 = n23;
            m31 = n31; m32 = n32; m33 = n33;
            x = m14; y = m24; z = m34;
            p = new Vector3(m14, m24, m34);
            lookVector = new Vector3(-m13, -m23, -m33);
            rightVector = new Vector3(m11, m21, m31);
            upVector = new Vector3(m12, m22, m32);
        }

        public static CFrame FromXml(XmlNode cfData)
        {
            float[] p = new float[12];
            for (int i = 0; i < 12; i++)
                p[i] = float.Parse(cfData.ChildNodes[i].InnerText);

            return new CFrame(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10], p[11]);
        }

        public static CFrame Scale(CFrame cf, Vector3 scaleBy)
        {
            CFrame rot = cf - cf.p;
            return new CFrame(cf.p * scaleBy) * rot;
        }

        // opperator overloads

        public static CFrame operator +(CFrame a, Vector3 b)
        {
            float[] ac = a.components();
            float x = ac[0], y = ac[1], z = ac[2], m11 = ac[3], m12 = ac[4], m13 = ac[5], m21 = ac[6], m22 = ac[7], m23 = ac[8], m31 = ac[9], m32 = ac[10], m33 = ac[11];
            return new CFrame(x + b.x, y + b.y, z + b.z, m11, m12, m13, m21, m22, m23, m31, m32, m33);
        }

        public static CFrame operator -(CFrame a, Vector3 b)
        {
            float[] ac = a.components();
            float x = ac[0], y = ac[1], z = ac[2], m11 = ac[3], m12 = ac[4], m13 = ac[5], m21 = ac[6], m22 = ac[7], m23 = ac[8], m31 = ac[9], m32 = ac[10], m33 = ac[11];
            return new CFrame(x - b.x, y - b.y, z - b.z, m11, m12, m13, m21, m22, m23, m31, m32, m33);
        }

        public static Vector3 operator *(CFrame a, Vector3 b)
        {
            float[] ac = a.components();
            float x = ac[0], y = ac[1], z = ac[2], m11 = ac[3], m12 = ac[4], m13 = ac[5], m21 = ac[6], m22 = ac[7], m23 = ac[8], m31 = ac[9], m32 = ac[10], m33 = ac[11];
            Vector3 right = new Vector3(m11, m21, m31);
            Vector3 up = new Vector3(m12, m22, m32);
            Vector3 back = new Vector3(m13, m23, m33);
            return a.p + b.x * right + b.y * up + b.z * back;
        }

        public static CFrame operator *(CFrame a, CFrame b)
        {
            float[] ac = a.components();
            float[] bc = b.components();
            float a14 = ac[0], a24 = ac[1], a34 = ac[2], a11 = ac[3], a12 = ac[4], a13 = ac[5], a21 = ac[6], a22 = ac[7], a23 = ac[8], a31 = ac[9], a32 = ac[10], a33 = ac[11];
            float b14 = bc[0], b24 = bc[1], b34 = bc[2], b11 = bc[3], b12 = bc[4], b13 = bc[5], b21 = bc[6], b22 = bc[7], b23 = bc[8], b31 = bc[9], b32 = bc[10], b33 = bc[11];
            float n11 = a11 * b11 + a12 * b21 + a13 * b31 + a14 * m41;
            float n12 = a11 * b12 + a12 * b22 + a13 * b32 + a14 * m42;
            float n13 = a11 * b13 + a12 * b23 + a13 * b33 + a14 * m43;
            float n14 = a11 * b14 + a12 * b24 + a13 * b34 + a14 * m44;
            float n21 = a21 * b11 + a22 * b21 + a23 * b31 + a24 * m41;
            float n22 = a21 * b12 + a22 * b22 + a23 * b32 + a24 * m42;
            float n23 = a21 * b13 + a22 * b23 + a23 * b33 + a24 * m43;
            float n24 = a21 * b14 + a22 * b24 + a23 * b34 + a24 * m44;
            float n31 = a31 * b11 + a32 * b21 + a33 * b31 + a34 * m41;
            float n32 = a31 * b12 + a32 * b22 + a33 * b32 + a34 * m42;
            float n33 = a31 * b13 + a32 * b23 + a33 * b33 + a34 * m43;
            float n34 = a31 * b14 + a32 * b24 + a33 * b34 + a34 * m44;
            float n41 = m41 * b11 + m42 * b21 + m43 * b31 + m44 * m41;
            float n42 = m41 * b12 + m42 * b22 + m43 * b32 + m44 * m42;
            float n43 = m41 * b13 + m42 * b23 + m43 * b33 + m44 * m43;
            float n44 = m41 * b14 + m42 * b24 + m43 * b34 + m44 * m44;
            return new CFrame(n14, n24, n34, n11, n12, n13, n21, n22, n23, n31, n32, n33);
        }

        public override string ToString()
        {
            return String.Join(", ", components());
        }

        // private static functions

        private static Vector3 vectorAxisAngle(Vector3 n, Vector3 v, float t)
        {
            n = n.unit;
            return v * (float)Math.Cos(t) + Vector3.Dot(v, n) * n * (1 - (float)Math.Cos(t)) + Vector3.Cross(n, v) * (float)Math.Sin(t);
        }

        private static float getDeterminant(CFrame a)
        {
            float[] ac = a.components();
            float a14 = ac[0], a24 = ac[1], a34 = ac[2], a11 = ac[3], a12 = ac[4], a13 = ac[5], a21 = ac[6], a22 = ac[7], a23 = ac[8], a31 = ac[9], a32 = ac[10], a33 = ac[11];
            float det = (a11 * a22 * a33 * m44 + a11 * a23 * a34 * m42 + a11 * a24 * a32 * m43
                    + a12 * a21 * a34 * m43 + a12 * a23 * a31 * m44 + a12 * a24 * a33 * m41
                    + a13 * a21 * a32 * m44 + a13 * a22 * a34 * m41 + a13 * a24 * a31 * m42
                    + a14 * a21 * a33 * m42 + a14 * a22 * a31 * m43 + a14 * a23 * a32 * m41
                    - a11 * a22 * a34 * m43 - a11 * a23 * a32 * m44 - a11 * a24 * a33 * m42
                    - a12 * a21 * a33 * m44 - a12 * a23 * a34 * m41 - a12 * a24 * a31 * m43
                    - a13 * a21 * a34 * m42 - a13 * a22 * a31 * m44 - a13 * a24 * a32 * m41
                    - a14 * a21 * a32 * m43 - a14 * a22 * a33 * m41 - a14 * a23 * a31 * m42);
            return det;
        }

        private static CFrame invert4x4(CFrame a)
        {
            float[] ac = a.components();
            float a14 = ac[0], a24 = ac[1], a34 = ac[2], a11 = ac[3], a12 = ac[4], a13 = ac[5], a21 = ac[6], a22 = ac[7], a23 = ac[8], a31 = ac[9], a32 = ac[10], a33 = ac[11];
            float det = getDeterminant(a);
            if (det == 0) { return a; }
            float b11 = (a22 * a33 * m44 + a23 * a34 * m42 + a24 * a32 * m43 - a22 * a34 * m43 - a23 * a32 * m44 - a24 * a33 * m42) / det;
            float b12 = (a12 * a34 * m43 + a13 * a32 * m44 + a14 * a33 * m42 - a12 * a33 * m44 - a13 * a34 * m42 - a14 * a32 * m43) / det;
            float b13 = (a12 * a23 * m44 + a13 * a24 * m42 + a14 * a22 * m43 - a12 * a24 * m43 - a13 * a22 * m44 - a14 * a23 * m42) / det;
            float b14 = (a12 * a24 * a33 + a13 * a22 * a34 + a14 * a23 * a32 - a12 * a23 * a34 - a13 * a24 * a32 - a14 * a22 * a33) / det;
            float b21 = (a21 * a34 * m43 + a23 * a31 * m44 + a24 * a33 * m41 - a21 * a33 * m44 - a23 * a34 * m41 - a24 * a31 * m43) / det;
            float b22 = (a11 * a33 * m44 + a13 * a34 * m41 + a14 * a31 * m43 - a11 * a34 * m43 - a13 * a31 * m44 - a14 * a33 * m41) / det;
            float b23 = (a11 * a24 * m43 + a13 * a21 * m44 + a14 * a23 * m41 - a11 * a23 * m44 - a13 * a24 * m41 - a14 * a21 * m43) / det;
            float b24 = (a11 * a23 * a34 + a13 * a24 * a31 + a14 * a21 * a33 - a11 * a24 * a33 - a13 * a21 * a34 - a14 * a23 * a31) / det;
            float b31 = (a21 * a32 * m44 + a22 * a34 * m41 + a24 * a31 * m42 - a21 * a34 * m42 - a22 * a31 * m44 - a24 * a32 * m41) / det;
            float b32 = (a11 * a34 * m42 + a12 * a31 * m44 + a14 * a32 * m41 - a11 * a32 * m44 - a12 * a34 * m41 - a14 * a31 * m42) / det;
            float b33 = (a11 * a22 * m44 + a12 * a24 * m41 + a14 * a21 * m42 - a11 * a24 * m42 - a12 * a21 * m44 - a14 * a22 * m41) / det;
            float b34 = (a11 * a24 * a32 + a12 * a21 * a34 + a14 * a22 * a31 - a11 * a22 * a34 - a12 * a24 * a31 - a14 * a21 * a32) / det;
            float b41 = (a21 * a33 * m42 + a22 * a31 * m43 + a23 * a32 * m41 - a21 * a32 * m43 - a22 * a33 * m41 - a23 * a31 * m42) / det;
            float b42 = (a11 * a32 * m43 + a12 * a33 * m41 + a13 * a31 * m42 - a11 * a33 * m42 - a12 * a31 * m43 - a13 * a32 * m41) / det;
            float b43 = (a11 * a23 * m42 + a12 * a21 * m43 + a13 * a22 * m41 - a11 * a22 * m43 - a12 * a23 * m41 - a13 * a21 * m42) / det;
            float b44 = (a11 * a22 * a33 + a12 * a23 * a31 + a13 * a21 * a32 - a11 * a23 * a32 - a12 * a21 * a33 - a13 * a22 * a31) / det;
            return new CFrame(b14, b24, b34, b11, b12, b13, b21, b22, b23, b31, b32, b33);
        }

        private static float[] quaternionFromCFrame(CFrame a)
        {
            float[] ac = a.components();
            float mx = ac[0], my = ac[1], mz = ac[2], m11 = ac[3], m12 = ac[4], m13 = ac[5], m21 = ac[6], m22 = ac[7], m23 = ac[8], m31 = ac[9], m32 = ac[10], m33 = ac[11];
            float trace = m11 + m22 + m33;
            float w = 1, i = 0, j = 0, k = 0;
            if (trace > 0)
            {
                float s = (float)Math.Sqrt(1 + trace);
                float r = 0.5f / s;
                w = s * 0.5f; i = (m32 - m23) * r; j = (m13 - m31) * r; k = (m21 - m12) * r;
            }
            else
            {
                float big = Math.Max(Math.Max(m11, m22), m33);
                if (big == m11)
                {
                    float s = (float)Math.Sqrt(1 + m11 - m22 - m33);
                    float r = 0.5f / s;
                    w = (m32 - m23) * r; i = 0.5f * s; j = (m21 + m12) * r; k = (m13 + m31) * r;
                }
                else if (big == m22)
                {
                    float s = (float)Math.Sqrt(1 - m11 + m22 - m33);
                    float r = 0.5f / s;
                    w = (m13 - m31) * r; i = (m21 + m12) * r; j = 0.5f * s; k = (m32 + m23) * r;
                }
                else if (big == m33)
                {
                    float s = (float)Math.Sqrt(1 - m11 - m22 + m33);
                    float r = 0.5f / s;
                    w = (m21 - m12) * r; i = (m13 + m31) * r; j = (m32 + m23) * r; k = 0.5f * s;
                }
            }
            return new float[] { w, i, j, k };
        }

        private static CFrame lerpinternal(CFrame a, CFrame b, float t)
        {
            CFrame cf = a.inverse() * b;
            float[] q = quaternionFromCFrame(cf);
            float w = q[0], i = q[1], j = q[2], k = q[3];
            float theta = (float)Math.Acos(w) * 2;
            Vector3 v = new Vector3(i, j, k);
            Vector3 p = a.p.Lerp(b.p, t);
            if (theta != 0)
            {
                CFrame r = a * fromAxisAngle(v, theta * t);
                return (r - r.p) + p;
            }
            else
            {
                return (a - a.p) + p;
            }
        }

        // public static functions

        public static CFrame fromAxisAngle(Vector3 axis, float theta)
        {
            Vector3 r = vectorAxisAngle(axis, RIGHT, theta);
            Vector3 u = vectorAxisAngle(axis, UP, theta);
            Vector3 b = vectorAxisAngle(axis, BACK, theta);
            return new CFrame(0, 0, 0, r.x, u.x, b.x, r.y, u.y, b.y, r.z, u.z, b.z);
        }

        public static CFrame Angles(float x, float y, float z)
        {
            CFrame cfx = fromAxisAngle(RIGHT, x);
            CFrame cfy = fromAxisAngle(UP, y);
            CFrame cfz = fromAxisAngle(BACK, z);
            return cfx * cfy * cfz;
        }

        public static CFrame fromEulerAnglesXYZ(float x, float y, float z)
        {
            return Angles(x, y, z);
        }

        // methods

        public CFrame inverse()
        {
            return invert4x4(this);
        }

        public CFrame lerp(CFrame cf2, float t)
        {
            return lerpinternal(this, cf2, t);
        }

        public CFrame toWorldSpace(CFrame cf2)
        {
            return this * cf2;
        }

        public CFrame toObjectSpace(CFrame cf2)
        {
            return this.inverse() * cf2;
        }

        public Vector3 pointToWorldSpace(Vector3 v)
        {
            return this * v;
        }

        public Vector3 pointToObjectSpace(Vector3 v)
        {
            return this.inverse() * v;
        }

        public Vector3 vectorToWorldSpace(Vector3 v)
        {
            return (this - this.p) * v;
        }

        public Vector3 vectorToObjectSpace(Vector3 v)
        {
            return (this - this.p).inverse() * v;
        }

        public float[] components()
        {
            return new float[] { m14, m24, m34, m11, m12, m13, m21, m22, m23, m31, m32, m33 };
        }

        public float[] toEulerAnglesXYZ()
        {
            float x = (float)Math.Atan2(-m23, m33);
            float y = (float)Math.Asin(m13);
            float z = (float)Math.Atan2(-m12, m11);
            return new float[] { x, y, z };
        }

        protected override string ToStudioMdlString_Impl(bool excludeZ = false)
        {
            string pos = p.ToStudioMdlString() + " ";
            float[] ang = toEulerAnglesXYZ();
            string rotation = string.Join(" ", truncate(ang));
            return pos + rotation;
        }
    }
}
