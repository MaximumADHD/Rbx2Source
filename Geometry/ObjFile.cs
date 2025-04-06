using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using RobloxFiles.DataTypes;

namespace Rbx2Source.Geometry
{
    public struct ObjVert
    {
        public int Vert;

        public int? UV;
        public int? Group;
        public int? Normal;
    }

    public class ObjFile
    {
        public List<Vector2> UVs = new List<Vector2>();
        public List<Vector3> Verts = new List<Vector3>();
        public List<Vector3> Normals = new List<Vector3>();

        public List<string> Groups = new List<string>();
        public List<ObjVert[]> Faces = new List<ObjVert[]>();

        public ObjFile(string content)
        {
            int? groupIndex = null;

            using (var reader = new StringReader(content))
            {
                while (true)
                {
                    string line = reader.ReadLine();

                    if (line == null)
                        break;

                    var parts = line
                        .Split(' ')
                        .Where(part => part != "")
                        .ToList();

                    if (parts.Count == 0)
                        continue;

                    string cmd = parts.First();
                    parts.RemoveAt(0);

                    if (cmd == "g")
                    {
                        if (groupIndex == null)
                            groupIndex = 0;
                        else
                            groupIndex++;

                        var group = parts.First();
                        Groups.Add(group);
                    }
                    else if (cmd == "v" || cmd == "vn" || cmd == "vt")
                    {
                        float[] values = parts
                            .Select(value => float.Parse(value))
                            .ToArray();

                        if (cmd == "vt")
                        {
                            float x = values[0];
                            float y = 1 - values[1];

                            var uv = new Vector2(x, y);
                            UVs.Add(uv);
                        }
                        else
                        {
                            var list = cmd == "vn" ? Normals : Verts;
                            list.Add(new Vector3(values));
                        }
                    }
                    else if (cmd == "f")
                    {
                        var face = new List<ObjVert>();

                        foreach (var faceData in parts)
                        {
                            var indices = faceData.Split('/');

                            var objVert = new ObjVert();
                            objVert.Group = groupIndex;

                            if (indices.Length > 0)
                                int.TryParse(indices[0], out objVert.Vert);

                            if (indices.Length > 1)
                                if (int.TryParse(indices[1], out int norm))
                                    objVert.Normal = norm - 1;

                            if (indices.Length > 2)
                                if (int.TryParse(indices[2], out int uv))
                                    objVert.UV = uv - 1;

                            objVert.Vert -= 1;
                            face.Add(objVert);
                        }

                        var rawFace = face.ToArray();
                        Faces.Add(rawFace);
                    }
                }
            }
        }
    }
}
