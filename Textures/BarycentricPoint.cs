using System;
using System.Drawing;

namespace Rbx2Source.Textures
{
    public class BarycentricPoint
    {
        public readonly double U, V, W;

        public BarycentricPoint(Point p, params Point[] poly)
        {
            Point a = poly[0],
                  b = poly[1],
                  c = poly[2];
            
            Point v0 = b.Subtract(a),
                  v1 = c.Subtract(a),
                  v2 = p.Subtract(a);

            double d00 = v0.Dot(v0),
                   d01 = v0.Dot(v1),
                   d11 = v1.Dot(v1),
                   d20 = v2.Dot(v0),
                   d21 = v2.Dot(v1);

            double denom = d00 * d11 - d01 * d01;

            V = (d11 * d20 - d01 * d21) / denom;
            W = (d00 * d21 - d01 * d20) / denom;

            U = 1.0 - V - W;
        }

        public Point ToCartesian(params Point[] poly)
        {
            Point a = poly[0],
                  b = poly[1],
                  c = poly[2];
            
            double x = (a.X * U) + (b.X * V) + (c.X * W),
                   y = (a.Y * U) + (b.Y * V) + (c.Y * W);

            int ix = (int)(x + 0.5f),
                iy = (int)(y + 0.5f);

            return new Point(ix, iy);
        }

        public bool InBounds()
        {
            // Use int approximation to avoid floating point errors.
            int u = (int)U * 100000,
                v = (int)V * 100000;

            return u >= 0 && v >= 0 && u + v <= 100000;
        }
    }
}
