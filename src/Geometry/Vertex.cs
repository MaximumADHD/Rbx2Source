using System.Drawing;
using Rbx2Source.Coordinates;

namespace Rbx2Source.Geometry
{
    struct Vertex
    {
        public Vector3 Pos;
        public Vector3 Norm;
        public Vector3 UV;

        public Point ToPoint()
        {
            int x = (int)Pos.x;
            int y = (int)Pos.y;

            return new Point(x, y);
        }

        public Point ToPoint(Rectangle canvas, Point offset)
        {
            int x = (int)Pos.x;
            int y = (int)Pos.y;

            Point result = new Point(offset.X + x, (offset.Y - y) + canvas.Height);
            return result;
        }
    }
}
