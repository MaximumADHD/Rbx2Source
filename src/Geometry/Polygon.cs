using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rbx2Source.Coordinates;

namespace Rbx2Source.Geometry
{
    struct Vertex
    {
        public Vector3 Pos;
        public Vector3 Norm;
        public Vector3 UV;
    }

    class Polygon
    {
        public Vertex[] Verts;

        public Polygon()
        {
            Verts = new Vertex[3];
        }
    }
}
