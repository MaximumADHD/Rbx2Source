using System.Collections.Generic;
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
        
        public void WriteStudioMdl(StringWriter buffer, Triangle triangle, List<Triangle> triangles)
        {
            Mesh mesh = triangle.Mesh;
            Vertex[] verts = mesh.Verts;

            Node node = triangle.Node;
            int bone = node.NodeIndex;

            int[] face = mesh.Faces[FaceIndex];
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
