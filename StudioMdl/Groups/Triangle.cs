using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
        
        public void WriteStudioMdl(StringWriter buffer, StudioMdlWriter writer, List<Triangle> triangles)
        {
            Contract.Requires(buffer != null && triangles != null);

            var verts = Mesh.Verts;
            int boneIndex = Node.NodeIndex;

            int[] face = Mesh.Faces[FaceIndex];
            buffer.WriteLine(Material);

            var studioBone = Node.StudioBone;
            var part1 = studioBone.Part1;
            
            for (int i = 0; i < 3; i++)
            {
                Vertex vert = verts[face[i]];
                string coords = vert.WriteStudioMdl(writer, part1, Mesh);

                string line = string.Join(" ", boneIndex, coords);
                buffer.WriteLine(line);
            }
        }
    }
}
