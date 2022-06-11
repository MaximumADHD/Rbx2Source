using System.Diagnostics.Contracts;
using System.Drawing;

namespace Rbx2Source.Geometry
{
    // 2D Texture components of the Vertex class
    // The 3D Geometry components are defined in Geometry/Vertex3D.cs
    
    public partial class Vertex
    {
        public Point ToPoint()
        {
            int x = (int)(Position.X + 0.5f),
                y = (int)(Position.Y + 0.5f);

            return new Point(x, y);
        }

        public Point ToPoint(Rectangle canvas, Point offset)
        {
            int x = (int)(Position.X + 0.5f),
                y = (int)(Position.Y + 0.5f);

            return new Point(offset.X + x, (offset.Y - y) + canvas.Height);
        }

        public Point ToUV(Bitmap target)
        {
            Contract.Requires(target != null);

            float x = UV.X * target.Width,
                  y = UV.Y * target.Height;

            int ix = (int)(x + 0.5f),
                iy = (int)(y + 0.5f);

            return new Point(ix, iy);
        }
    }
}
