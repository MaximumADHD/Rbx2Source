using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Rbx2Source.Assembler;
using Rbx2Source.Coordinates;
using Rbx2Source.Reflection;
using Rbx2Source.Web;

namespace Rbx2Source.Geometry
{
    class Mesh
    {
        public Vertex[] Verts;
        public int[][] Faces;
        public int Version;

        public uint FaceCount = 0;
        public uint VertCount = 0;

        public bool Loaded = false;

        private static Dictionary<string, Asset> StandardLimbs = new Dictionary<string, Asset>
        {
            {"Left Arm",    Asset.FromResource("Meshes/StandardLimbs/leftarm.mesh")},
            {"Right Arm",   Asset.FromResource("Meshes/StandardLimbs/rightarm.mesh")},
            {"Left Leg",    Asset.FromResource("Meshes/StandardLimbs/leftleg.mesh")},
            {"Right Leg",   Asset.FromResource("Meshes/StandardLimbs/rightleg.mesh")},
            {"Torso",       Asset.FromResource("Meshes/StandardLimbs/torso.mesh")}
        };

        private static float parseFloat(string f)
        {
            return float.Parse(f, Rbx2Source.NormalParse);
        }

        private static Converter<string, float> toFloat = new Converter<string, float>(parseFloat);

        private static void loadGeometryV1(StringReader reader, Mesh mesh)
        {
            string header = reader.ReadLine();
            if (!header.StartsWith("version 1"))
                throw new Exception("Expected version 1 header, got: " + header);

            string version = header.Substring(8);
            float vertScale = (version == "1.00" ? 0.5f : 1); // well, thats awkward.

            if (!uint.TryParse(reader.ReadLine(), out mesh.FaceCount))
                throw new Exception("Expected 2nd line to be the polygon count.");

            mesh.VertCount = mesh.FaceCount * 3;
            mesh.Faces = new int[mesh.FaceCount][];
            mesh.Verts = new Vertex[mesh.VertCount];

            string polyBuffer = reader.ReadLine();
            MatchCollection matches = Regex.Matches(polyBuffer, @"\[(.*?)\]");

            int face = 0;
            int index = 0;
            int state = 0;

            Vertex currentVertex = new Vertex();

            foreach (Match m in matches)
            {
                string vectorStr = m.Groups[1].ToString();
                float[] coords = Array.ConvertAll(vectorStr.Split(','), toFloat);
                Vector3 vector = new Vector3(coords);

                if (state == 0)
                    currentVertex.Pos = new Vector3(coords) * vertScale;
                else if (state == 1)
                    currentVertex.Norm = new Vector3(coords);
                else if (state == 2)
                    currentVertex.UV = new Vector3(coords[0], 1 - coords[1], 0);

                state = (state + 1) % 3;
                if (state == 0)
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

        private static void loadGeometryV2(BinaryReader reader, Mesh mesh)
        {
            Stream stream = reader.BaseStream;
            stream.Position = 13; // Move past the header.

            ushort cbSize = reader.ReadUInt16();
            byte cbVerticesStride = reader.ReadByte();
            byte cbFaceStride = reader.ReadByte();

            int vertBytesToSkip = 0;
            if (cbVerticesStride != 36) // Some new bytes were added to new meshes regarding vertex color data. Skip over them if we can.
                vertBytesToSkip = cbVerticesStride - 36;

            mesh.VertCount = reader.ReadUInt32();
            mesh.FaceCount = reader.ReadUInt32();
            mesh.Verts = new Vertex[mesh.VertCount];
            mesh.Faces = new int[mesh.FaceCount][];

            for (int i = 0; i < mesh.VertCount; i++)
            {
                Vertex vert = new Vertex();
                vert.Pos = new Vector3(reader);
                vert.Norm = new Vector3(reader);
                vert.UV = new Vector3(reader);
                mesh.Verts[i] = vert;
                if (vertBytesToSkip > 0)
                    reader.ReadBytes(vertBytesToSkip);
            }

            for (int p = 0; p < mesh.FaceCount; p++)
            {
                int[] face = new int[3];
                for (int i = 0; i < 3; i++)
                    face[i] = reader.ReadInt32();

                mesh.Faces[p] = face;
            }

            mesh.Loaded = true;
        }

        private static Mesh load(byte[] data)
        {
            string file = Encoding.ASCII.GetString(data);
            if (!file.StartsWith("version "))
                throw new Exception("Invalid .mesh header!");

            Mesh mesh = new Mesh();
            double version = double.Parse(file.Substring(8, 4), Rbx2Source.NormalParse);

            mesh.Version = (int)version;

            if (mesh.Version == 1)
            {
                StringReader buffer = new StringReader(file);
                loadGeometryV1(buffer, mesh);
            }
            else if (mesh.Version == 2)
            {
                MemoryStream stream = new MemoryStream(data);
                BinaryReader reader = new BinaryReader(stream);
                loadGeometryV2(reader, mesh);
            }
            else
            {
                throw new Exception("Unknown .mesh file version: " + version);
            }

            return mesh;
        }

        public void BakeGeometry(Vector3 scale, CFrame offset)
        {
            for (int i = 0; i < VertCount; i++)
                Verts[i].Pos = (offset * new CFrame(Verts[i].Pos * scale)).p;
        }

        public static Mesh FromFile(string path)
        {
            FileStream meshStream = File.OpenRead(path);

            long length = meshStream.Length;

            byte[] data = new byte[length];
            meshStream.Read(data, 0, (int)length);
            meshStream.Close();

            return load(data);
        }

        public static Mesh FromAsset(Asset asset)
        {
            byte[] content = asset.GetContent();
            return load(content);
        }

        public static Mesh BakePart(Part part, Material material = null)
        {
            Mesh result = null;

            Asset meshAsset = null;
            Asset textureAsset = null;

            Vector3 scale = null;
            CFrame offset = null;

            if (material != null)
            {
                material.LinkedTo = part;
                material.Transparency = part.Transparency;
                material.Reflectance = part.Reflectance;
            }

            if (part.Transparency < 1)
            {
                if (part.IsA("MeshPart"))
                {
                    MeshPart meshPart = (MeshPart)part;
                    if (meshPart.MeshID == null)
                    {
                        string partName = meshPart.Name;
                        if (StandardLimbs.ContainsKey(partName))
                            meshAsset = StandardLimbs[partName];
                    }
                    else
                    {
                        meshAsset = Asset.GetByAssetId(meshPart.MeshID);
                    }

                    if (meshPart.TextureID != null)
                        textureAsset = Asset.GetByAssetId(meshPart.TextureID);

                    scale = meshPart.Size / meshPart.InitialSize;
                    offset = part.CFrame;
                }
                else
                {
                    offset = part.CFrame;

                    SpecialMesh specialMesh = (SpecialMesh)part.FindFirstChildOfClass("SpecialMesh");
                    if (specialMesh != null && specialMesh.MeshType == MeshType.FileMesh)
                    {
                        meshAsset = Asset.GetByAssetId(specialMesh.MeshId);
                        scale = specialMesh.Scale;
                        offset *= new CFrame(specialMesh.Offset);
                        if (material != null)
                            textureAsset = Asset.GetByAssetId(specialMesh.TextureId);
                    }
                    else
                    {
                        DataModelMesh legacy = (DataModelMesh)part.FindFirstChildOfClass("DataModelMesh");
                        if (legacy != null)
                        {
                            meshAsset = Head.ResolveHeadMeshAsset(legacy);
                            scale = legacy.Scale;
                            offset *= new CFrame(legacy.Offset);
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