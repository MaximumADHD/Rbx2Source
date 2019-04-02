using System;
using System.Drawing;

using Rbx2Source.DataTypes;
using Rbx2Source.Geometry;

namespace Rbx2Source.Textures
{
    public class BarycentricPoint
    {
        public double U, V, W;
    }

    static class CompositUtility
    {
        public static Point VertexToUV(Vertex vert, Bitmap target)
        {
            Vector3 uv = vert.UV;

            float x = uv.X * target.Width;
            float y = uv.Y * target.Height;

            int ix = (int)(x + 0.5f);
            int iy = (int)(y + 0.5f);

            return new Point(ix, iy);
        }

        public static Point VertexToPoint(Vertex vert)
        {
            int x = (int)(vert.Position.X + 0.5f);
            int y = (int)(vert.Position.Y + 0.5f);

            return new Point(x, y);
        }

        public static Point VertexToPoint(Vertex vert, Rectangle canvas, Point offset)
        {
            int x = (int)(vert.Position.X + 0.5f);
            int y = (int)(vert.Position.Y + 0.5f);

            Point result = new Point(offset.X + x, (offset.Y - y) + canvas.Height);
            return result;
        }

        public static Point Subtract(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static double Dot(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Rectangle GetBoundingBox(params Point[] points)
        {
            int min_X = int.MaxValue;
            int min_Y = int.MaxValue;

            int max_X = int.MinValue;
            int max_Y = int.MinValue;

            foreach (Point point in points)
            {
                int point_X = point.X;
                int point_Y = point.Y;

                min_X = Math.Min(min_X, point_X);
                min_Y = Math.Min(min_Y, point_Y);

                max_X = Math.Max(max_X, point_X);
                max_Y = Math.Max(max_Y, point_Y);
            }

            int width = max_X - min_X;
            int height = max_Y - min_Y;

            return new Rectangle(min_X, min_Y, width, height);
        }

        public static BarycentricPoint ToBarycentric(Point p, Point a, Point b, Point c)
        {
            Point v0 = Subtract(b, a);
            Point v1 = Subtract(c, a);
            Point v2 = Subtract(p, a);

            double d00 = Dot(v0, v0);
            double d01 = Dot(v0, v1);
            double d11 = Dot(v1, v1);
            double d20 = Dot(v2, v0);
            double d21 = Dot(v2, v1);

            double denom = d00 * d11 - d01 * d01;

            BarycentricPoint bp = new BarycentricPoint();
            bp.V = (d11 * d20 - d01 * d21) / denom;
            bp.W = (d00 * d21 - d01 * d20) / denom;
            bp.U = 1.0 - bp.V - bp.W;

            return bp;
        }

        public static Point ToCartesian(BarycentricPoint bp, Point a, Point b, Point c)
        {
            double x = (a.X * bp.U) + (b.X * bp.V) + (c.X * bp.W);
            double y = (a.Y * bp.U) + (b.Y * bp.V) + (c.Y * bp.W);

            int ix = (int)(x + 0.5f);
            int iy = (int)(y + 0.5f);

            return new Point(ix, iy);
        }

        public static bool InTriangle(BarycentricPoint bc)
        {
            // Use int approximation to avoid floating point errors.
            int u = (int)bc.U * 100000;
            int v = (int)bc.V * 100000;

            return u >= 0 && v >= 0 && u + v <= 100000;
        }
    }
}
