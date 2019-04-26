using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Rbx2Source.Assembler;
using Rbx2Source.DataTypes;
using Rbx2Source.Reflection;
using Rbx2Source.Web;

namespace Rbx2Source.Geometry
{
    public class Mesh
    {
        public int Version;

        public Vertex[] Verts;
        public int[][] Faces;
        public Mesh[] LODs;

        public int NumVerts = 0;
        public int NumFaces = 0;

        public bool Loaded = false;

        private static Dictionary<string, Asset> StandardLimbs = new Dictionary<string, Asset>
        {
            {"Left Arm",    Asset.FromResource("Meshes/StandardLimbs/leftarm.mesh")},
            {"Right Arm",   Asset.FromResource("Meshes/StandardLimbs/rightarm.mesh")},
            {"Left Leg",    Asset.FromResource("Meshes/StandardLimbs/leftleg.mesh")},
            {"Right Leg",   Asset.FromResource("Meshes/StandardLimbs/rightleg.mesh")},
            {"Torso",       Asset.FromResource("Meshes/StandardLimbs/torso.mesh")}
        };

        private static void LoadGeometry_Ascii(StringReader reader, Mesh mesh)
        {
            string header = reader.ReadLine();

            if (!header.StartsWith("version 1"))
                throw new Exception("Expected version 1 header, got: " + header);

            string version = header.Substring(8);
            float vertScale = (version == "1.00" ? 0.5f : 1);

            if (int.TryParse(reader.ReadLine(), out mesh.NumFaces))
                mesh.NumVerts = mesh.NumFaces * 3;
            else
                throw new Exception("Expected 2nd line to be the polygon count.");

            mesh.Faces = new int[mesh.NumFaces][];
            mesh.Verts = new Vertex[mesh.NumVerts];

            string polyBuffer = reader.ReadLine();
            MatchCollection matches = Regex.Matches(polyBuffer, @"\[(.*?)\]");

            int face = 0;
            int index = 0;
            int target = 0;

            Vertex currentVertex = new Vertex();

            foreach (Match m in matches)
            {
                string vectorStr = m.Groups[1].ToString();

                float[] coords = vectorStr.Split(',')
                    .Select(coord => Format.ParseFloat(coord))
                    .ToArray();

                if (target == 0)
                    currentVertex.Position = new Vector3(coords) * vertScale;
                else if (target == 1)
                    currentVertex.Normal = new Vector3(coords);
                else if (target == 2)
                    currentVertex.UV = new Vector3(coords[0], 1 - coords[1], 0);

                target = (target + 1) % 3;

                if (target == 0)
                {
                    mesh.Verts[index++] = currentVertex;
                    currentVertex = new Vertex();

                    if (index % 3 == 0)
                    {
                        int v = face * 3;
                        mesh.Faces[face++] = new int[3] { v, v + 1, v + 2 };
                    }
                }
            }

            mesh.Loaded = true;
        }

        private static void LoadGeometry_Binary(BinaryReader reader, Mesh rootMesh)
        {
            byte[] binVersion = reader.ReadBytes(13);
            var headerSize = reader.ReadUInt16();

            var vertSize = reader.ReadByte();
            var faceSize = reader.ReadByte();
            
            if (rootMesh.Version >= 3)
            {
                var lodRangeSize = reader.ReadUInt16();
                var numLodRanges = reader.ReadUInt16();
                rootMesh.LODs = new Mesh[numLodRanges - 2];
            }

            int numVerts = reader.ReadInt32();
            int numFaces = reader.ReadInt32();

            var verts = new Vertex[numVerts];
            var faces = new int[numFaces][];

            rootMesh.NumVerts = numVerts;
            rootMesh.Verts = new Vertex[numVerts];

            for (int i = 0; i < numVerts; i++)
            {
                Vertex vert = new Vertex()
                {
                    Position = new Vector3(reader),
                    Normal = new Vector3(reader),
                    UV = new Vector3(reader)
                };

                if (vertSize > 36)
                {
                    byte r = reader.ReadByte(),
                         g = reader.ReadByte(),
                         b = reader.ReadByte(),
                         a = reader.ReadByte();

                    int argb = (a << 24 | r << 16 | g << 8 | b);
                    vert.Color = Color.FromArgb(argb);
                    vert.HasColor = true;
                }

                verts[i] = vert;
            }

            for (int f = 0; f < numFaces; f++)
            {
                int[] face = new int[3];

                for (int i = 0; i < 3; i++)
                    face[i] = reader.ReadInt32();

                faces[f] = face;
            }

            if (rootMesh.Version >= 3)
            {
                int rBegin = reader.ReadInt32();

                int numLods = rootMesh.LODs.Length + 1;
                int numLodRanges = numLods + 1;

                for (int i = 0; i < numLodRanges; i++)
                {
                    int rEnd = reader.ReadInt32();
                    int range = (rEnd - rBegin);

                    if (i == 0)
                    {
                        rootMesh.NumFaces = range;
                        rootMesh.Faces = new int[range][];
                    }
                }
            }

            rootMesh.Loaded = true;
        }

        private static Mesh Load(byte[] data)
        {
            string file = Encoding.ASCII.GetString(data);

            if (!file.StartsWith("version "))
                throw new Exception("Invalid .mesh header!");
            
            string versionStr = file.Substring(8, 4);
            double version = Format.ParseDouble(versionStr);

            Mesh mesh = new Mesh();
            mesh.Version = (int)version;

            IDisposable toDispose;

            if (mesh.Version == 1)
            {
                StringReader buffer = new StringReader(file);
                LoadGeometry_Ascii(buffer, mesh);
                toDispose = buffer;
            }
            else if (mesh.Version == 2 || mesh.Version == 3)
            {
                MemoryStream stream = new MemoryStream(data);

                using (BinaryReader reader = new BinaryReader(stream))
                    LoadGeometry_Binary(reader, mesh);

                toDispose = stream;
            }
            else
            {
                throw new Exception($"Unknown .mesh file version: {version}");
            }

            toDispose.Dispose();
            toDispose = null;

            return mesh;
        }

        public void BakeGeometry(Vector3 scale, CFrame offset)
        {
            for (int i = 0; i < NumVerts; i++)
            {
                var vert = Verts[i];
                var cframe = (offset * new CFrame(vert.Position * scale));
                vert.Position = cframe.Position;
            }
        }

        public static Mesh FromFile(string path)
        {
            byte[] data;

            using (FileStream meshStream = File.OpenRead(path))
            {
                using (MemoryStream buffer = new MemoryStream())
                {
                    meshStream.CopyTo(buffer);
                    data = buffer.ToArray();
                }
            }

            return Load(data);
        }

        public static Mesh FromAsset(Asset asset)
        {
            byte[] content = asset.GetContent();
            return Load(content);
        }

        public static Mesh BakePart(BasePart part, Material material = null)
        {
            Mesh result = null;

            Asset meshAsset = null;
            Asset textureAsset = null;

            Vector3 scale = null;
            CFrame offset = null;

            if (material != null)
            {
                material.LinkedTo = part;
                material.Reflectance = part.Reflectance;
                material.Transparency = part.Transparency;
            }

            if (part.Transparency < 1)
            {
                if (part.IsA("MeshPart"))
                {
                    MeshPart meshPart = part as MeshPart;

                    if (meshPart.MeshId != null && meshPart.MeshId.Length > 0)
                    {
                        string meshId = meshPart.MeshId;
                        meshAsset = Asset.GetByAssetId(meshId);
                    }
                    else
                    {
                        string partName = meshPart.Name;
                        StandardLimbs.TryGetValue(partName, out meshAsset);
                    }

                    if (meshPart.TextureId != null)
                        textureAsset = Asset.GetByAssetId(meshPart.TextureId);

                    scale = meshPart.Size / meshPart.InitialSize;
                    offset = part.CFrame;
                }
                else
                {
                    offset = part.CFrame;

                    SpecialMesh specialMesh = part.FindFirstChildOfClass<SpecialMesh>();

                    if (specialMesh != null && specialMesh.MeshType == MeshType.FileMesh)
                    {
                        meshAsset = Asset.GetByAssetId(specialMesh.MeshId);
                        offset *= new CFrame(specialMesh.Offset);
                        scale = specialMesh.Scale;

                        if (material != null)
                        {
                            textureAsset = Asset.GetByAssetId(specialMesh.TextureId);
                            material.VertexColor = specialMesh.VertexColor;
                        }
                    }
                    else
                    {
                        DataModelMesh legacy = part.FindFirstChildOfClass<DataModelMesh>();

                        if (legacy != null)
                        {
                            meshAsset = Head.ResolveHeadMeshAsset(legacy);
                            offset *= new CFrame(legacy.Offset);
                            scale = legacy.Scale;
                        }
                    }
                }
            }
            else
            {
                // Just give it a blank mesh to eat for now.
                result = new Mesh();
            }

            if (meshAsset != null)
            {
                if (material != null)
                    material.TextureAsset = textureAsset;

                result = FromAsset(meshAsset);
                result.BakeGeometry(scale, offset);
            }

            return result;
        }
    }
}