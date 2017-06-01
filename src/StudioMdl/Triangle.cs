using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rbx2Source.Coordinates;
using Rbx2Source.Geometry;

namespace Rbx2Source.StudioMdl
{
    class Triangle : IStudioMdlEntity
    {
        public string Material;
        public Polygon Polygon;
        public Node Node;

        public string GroupName
        {
            get { return "triangles"; }
        }

        public void Write(StringWriter buffer, IList rawTriList, object rawTri)
        {
            Triangle tri = rawTri as Triangle;
            buffer.WriteLine(Material);

            Polygon poly = tri.Polygon;
            Node node = tri.Node;
            int bone = node.NodeIndex;

            for (int i = 0; i < 3; i++)
            {
                Vertex vert = poly.Verts[i];
                string pos = vert.Pos.ToStudioMdlString();
                string norm = vert.Norm.ToStudioMdlString();
                string uv = vert.UV.ToStudioMdlString(true);

                string line = string.Join(" ", bone, pos, norm, uv, 1, bone, 1);
                buffer.WriteLine(line);
            }
        }
    }
}
