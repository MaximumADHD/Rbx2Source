using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

using Rbx2Source.Geometry;

namespace Rbx2Source.StudioMdl
{
    public class Triangle : IStudioMdlEntity<Triangle>
    {
        public string GroupName => "triangles";

        public string Material;
        public int FaceIndex;

        public Node Node;
        public Mesh Mesh;
        
        public void WriteStudioMdl(StringWriter buffer, List<Triangle> triangles)
        {
            Contract.Requires(buffer != null && triangles != null);

            var verts = Mesh.Verts;
            int bone = Node.Index;

            int[] face = Mesh.Faces[FaceIndex];
            buffer.WriteLine(Material);

            for (int i = 0; i < 3; i++)
            {
                Vertex vert = verts[face[i]];
                string coords = vert.WriteStudioMdl();

                string line = string.Join(" ", bone, coords, 1, bone, 1);
                buffer.WriteLine(line);
            }
        }
    }
}
