using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rbx2Source.Coordinates;

namespace Rbx2Source.Geometry
{
    class Polygon
    {
        public Vector3[] Verts;
        public Vector3[] Norms;
        public Vector3[] TexCoords;

        public Polygon()
        {
            Verts = new Vector3[3];
            Norms = new Vector3[3];
            TexCoords = new Vector3[3];
        }
    }
}
