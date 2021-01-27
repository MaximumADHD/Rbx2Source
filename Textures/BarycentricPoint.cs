using System;
using System.Drawing;

namespace Rbx2Source.Textures
{
    public class BarycentricPoint
    {
        public readonly float U, V, W;

        public BarycentricPoint(PointF p, params PointF[] poly)
        {
            PointF a = poly[0],
                   b = poly[1],
                   c = poly[2];
            
            PointF v0 = b.Subtract(a),
                   v1 = c.Subtract(a),
                   v2 = p.Subtract(a);

            float d00 = v0.Dot(v0),
                  d01 = v0.Dot(v1),
                  d11 = v1.Dot(v1),
                  d20 = v2.Dot(v0),
                  d21 = v2.Dot(v1);

            float denom = d00 * d11 - d01 * d01;

            V = (d11 * d20 - d01 * d21) / denom;
            W = (d00 * d21 - d01 * d20) / denom;

            U = 1f - V - W;
        }

        public PointF ToCartesian(params PointF[] poly)
        {
            PointF a = poly[0],
                   b = poly[1],
                   c = poly[2];
            
            float x = (a.X * U) + (b.X * V) + (c.X * W),
                  y = (a.Y * U) + (b.Y * V) + (c.Y * W);

            return new PointF(x, y);
        }

        public bool InBounds
        {
            get
            {
                // Use int approximation to avoid floating point errors.
                int u = (int)U * 100000,
                    v = (int)V * 100000;

                return u >= 0 && v >= 0 && u + v <= 100000;
            }
        }
    }
}
