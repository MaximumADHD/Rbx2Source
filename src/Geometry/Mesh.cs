using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Rbx2Source.Assembler;
using Rbx2Source.Coordinates;
using Rbx2Source.Properties;
using Rbx2Source.Reflection;
using Rbx2Source.Web;

namespace Rbx2Source.Geometry
{
    class Mesh
    {
        public Polygon[] Polygons;
        public uint PolyCount = 0;
        public bool Loaded = false;

        private static Dictionary<string, Asset> StandardLimbs = new Dictionary<string, Asset>
        {
            {"Head",        Asset.FromResource("Meshes/StandardLimbs/head.mesh")},
            {"Left Arm",    Asset.FromResource("Meshes/StandardLimbs/leftarm.mesh")},
            {"Right Arm",   Asset.FromResource("Meshes/StandardLimbs/rightarm.mesh")},
            {"Left Leg",    Asset.FromResource("Meshes/StandardLimbs/leftleg.mesh")},
            {"Right Leg",   Asset.FromResource("Meshes/StandardLimbs/rightleg.mesh")}, 
            {"Torso",       Asset.FromResource("Meshes/StandardLimbs/torso.mesh")}
        };

        private static void loadGeometryV1(StringReader reader, Mesh mesh)
        {
            string header = reader.ReadLine();
            if (!header.StartsWith("version 1"))
                throw new Exception("Expected version 1 header, got: " + header);

            string version = header.Substring(8);
            float vertScale = (version == "1.00" ? 0.5f : 1); // well, thats awkward.

            uint polyCount = 0;
            if (!uint.TryParse(reader.ReadLine(), out polyCount))
                throw new Exception("Expected 2nd line to be the polygon count.");

            string polyBuffer = reader.ReadLine();
            MatchCollection matches = Regex.Matches(polyBuffer, @"\[(.*?)\]");

            int polyIndex = 0;
            int index = 0;
            int state = 0;

            mesh.Polygons = new Polygon[polyCount];
            mesh.PolyCount = polyCount;

            Polygon currentPolygon = new Polygon();
            Vertex currentVertex = new Vertex();

            foreach (Match m in matches)
            {
                string vectorStr = m.Groups[1].ToString();
                float[] coords = Array.ConvertAll(vectorStr.Split(','), float.Parse);
                Vector3 vector = new Vector3(coords);

                if (state == 0)
                    currentVertex.Pos = vector * vertScale;
                else if (state == 1)
                    currentVertex.Norm = vector;
                else if (state == 2)
                    currentVertex.UV = new Vector3(vector.x, 1 - vector.y, 0);

                state = (state + 1) % 3;
                if (state == 0)
                {
                    currentPolygon.Verts[index] = currentVertex;
                    index = (index + 1) % 3;
                    currentVertex = new Vertex();
                    
                    if (index == 0)
                    {
                        mesh.Polygons[polyIndex++] = currentPolygon;
                        currentPolygon = new Polygon();
                    }
                }
            }

            mesh.Loaded = true;
        }
        
        private static void loadGeometryV2(BinaryReader reader, Mesh mesh)
        {
            Stream stream = reader.BaseStream;
            stream.Position = 13; // Move past the header.

            ushort cbSize = reader.ReadUInt16(); // internal sizeof mesh header
            byte cbVerticesStride = reader.ReadByte(); // internal sizeof Vertex
            byte cbFaceStride = reader.ReadByte(); // internal sizeof Polygon

            int vertBytesToSkip = 0;
            if (cbVerticesStride != 36) // Some new bytes we
                vertBytesToSkip = cbVerticesStride - 36;

            uint numVerts = reader.ReadUInt32();
            uint numFaces = reader.ReadUInt32();

            Vertex[] verts = new Vertex[numVerts];
            Polygon[] faces = new Polygon[numFaces];

            for (int i = 0; i < numVerts; i++)
            {
                Vertex vert = new Vertex();
                vert.Pos = new Vector3(reader);
                vert.Norm = new Vector3(reader);
                vert.UV = new Vector3(reader);
                verts[i] = vert;
                if (vertBytesToSkip > 0)
                    reader.ReadBytes(vertBytesToSkip);
            }

            mesh.PolyCount = numFaces;
            mesh.Polygons = new Polygon[mesh.PolyCount];

            for (int p = 0; p < mesh.PolyCount; p++)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < 3; i++)
                {
                    int setIndex = (int)reader.ReadUInt32();
                    Vertex vert = verts[setIndex];
                    poly.Verts[i] = vert;
                }
                mesh.Polygons[p] = poly;
            }

            mesh.Loaded = true;
        }

        private static Mesh buildMesh(byte[] data)
        {
            string file = Encoding.UTF8.GetString(data);
            if (!file.StartsWith("version "))
                throw new Exception("Invalid .mesh header!");

            Mesh mesh = new Mesh();
            double version = double.Parse(file.Substring(8,4));

            if ((int)version == 1)
            {
                StringReader buffer = new StringReader(file);
                loadGeometryV1(buffer, mesh);
            }
            else if ((int)version == 2)
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

        /// <summary>
        /// Creates a copy of the polygon array, then rescales and offsets the vertices based on the parameters.
        /// </summary>
        /// <param name="scale">The scale of the mesh</param>
        /// <param name="offset">The offset of the mesh, relative to its origin</param>
        /// <returns>The copied polygon array, with the geometry changes applied</returns>
        public Polygon[] BakeGeometry(Vector3 scale, CFrame offset)
        {
            Polygon[] bakedPolygons = new Polygon[PolyCount];

            for (int i = 0; i < PolyCount; i++)
            {
                Polygon src = Polygons[i];
                Polygon newPolygon = new Polygon();
                for (int v = 0; v < 3; v++)
                {
                    Vertex vert = src.Verts[v];
                    Vertex newVert = new Vertex();
                    newVert.Pos = (offset * new CFrame(vert.Pos * scale)).p;
                    newVert.Norm = vert.Norm;
                    newVert.UV = vert.UV;
                    newPolygon.Verts[v] = newVert;
                }
                bakedPolygons[i] = newPolygon;
            }

            return bakedPolygons;
        }

        public static Mesh FromFile(string path)
        {
            FileStream meshStream = File.OpenRead(path);

            long length = meshStream.Length;

            byte[] data = new byte[length];
            meshStream.Read(data, 0, (int)length);
            meshStream.Close();

            return buildMesh(data);
        }

        public static Mesh FromAsset(Asset asset)
        {
            byte[] content = asset.GetContent();
            return buildMesh(content);
        }

        public static Polygon[] BakePart(Part part, Material material = null)
        {
            Polygon[] result = null;

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

            if (part.IsA("MeshPart"))
            {
                MeshPart meshPart = (MeshPart)part;
                if (meshPart.MeshID == null)
                {
                    string partName = meshPart.Name;
                    if (StandardLimbs.ContainsKey(partName))
                        meshAsset = StandardLimbs[partName];
                }
                else meshAsset = Asset.GetByAssetId(meshPart.MeshID);

                if (meshPart.TextureID != null)
                    textureAsset = Asset.GetByAssetId(meshPart.TextureID); 
                
                scale = meshPart.Size / meshPart.InitialSize;
                offset = part.CFrame;
            }
            else
            {
                offset = part.CFrame;

                DataModelMesh legacy = (DataModelMesh)part.FindFirstChildOfClass("DataModelMesh");
                if (legacy != null)
                {
                    scale = legacy.Scale;
                    offset *= new CFrame(legacy.Offset);

                    if (material != null)
                        material.VertexColor = legacy.VertexColor;

                    if (legacy.IsA("SpecialMesh"))
                    {
                        SpecialMesh specialMesh = (SpecialMesh)legacy;
                        if (specialMesh.MeshType == MeshType.Head)
                            meshAsset = Asset.FromResource("Meshes/StandardLimbs/head.mesh");
                        else
                            meshAsset = Asset.GetByAssetId(specialMesh.MeshId);

                        if (specialMesh.TextureId != null)
                            textureAsset = Asset.GetByAssetId(specialMesh.TextureId);
                    }

                    else if (legacy.IsA("CylinderMesh"))
                    {
                        CylinderMesh cylinderMesh = (CylinderMesh)legacy;
                        if (cylinderMesh.UsingSuperAwkwardHeadProtocol)
                        {
                            meshAsset = Asset.FromResource("Meshes/Heads/" + cylinderMesh.HeadAssetName + ".mesh");
                            scale = new Vector3(1, 1, 1);
                        }
                    }
                }
            }

            if (meshAsset != null)
            {
                if (material != null)
                    material.TextureAsset = textureAsset;

                Mesh mesh = Mesh.FromAsset(meshAsset);
                result = mesh.BakeGeometry(scale, offset);
            }

            return result;
        }
    }
}
