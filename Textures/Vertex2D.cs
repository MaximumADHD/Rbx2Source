using System.Diagnostics.Contracts;
using System.Drawing;

namespace Rbx2Source.Geometry
{
    // 2D Texture components of the Vertex class
    // The 3D Geometry components are defined in Geometry/Vertex3D.cs
    
    public partial class Vertex
    {
        public PointF ToPoint()
        {
            float x = Position.X,
                  y = Position.Y;

            return new PointF(x, y);
        }

        public PointF ToPoint(Rectangle canvas, Point offset)
        {
            float x = Position.X,
                  y = Position.Y;

            return new PointF(offset.X + x, offset.Y - y + canvas.Height);
        }

        public PointF ToUV(Bitmap target)
        {
            Contract.Requires(target != null);

            float x = UV.X * target.Width,
                  y = UV.Y * target.Height;

            return new PointF(x, y);
        }
    }
}
