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
    struct PolySet
    {
        public Vector3 Vert;
        public Vector3 Norm;
        public Vector3 Tex;
    }

    class Mesh
    {
        public Polygon[] Polygons;
        public int PolyCount = 0;
        public bool Loaded = false;

        private Vector3 cachedBounds = null;
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

            int polyCount = -1;
            if (!int.TryParse(reader.ReadLine(), out polyCount))
                throw new Exception("Expected 2nd line to be the polygon count.");

            string polyBuffer = reader.ReadLine();
            MatchCollection matches = Regex.Matches(polyBuffer, @"\[(.*?)\]");

            int polyIndex = 0;
            int index = 0;
            int state = 0;

            mesh.Polygons = new Polygon[polyCount];
            mesh.PolyCount = polyCount;

            Polygon currentPolygon = new Polygon();

            foreach (Match m in matches)
            {
                string vectorStr = m.Groups[1].ToString();
                float[] coords = Array.ConvertAll(vectorStr.Split(','), float.Parse);
                Vector3 vector = new Vector3(coords);

                if (state == 0)
                    currentPolygon.Verts[index] = vector * vertScale;
                else if (state == 1)
                    currentPolygon.Norms[index] = vector;
                else if (state == 2)
                    currentPolygon.TexCoords[index] = new Vector3(vector.x,1f-vector.y,0);

                state = (state + 1) % 3;
                if (state == 0)
                {
                    index = (index + 1) % 3;
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
            stream.Position = 17; // Move past the header.

            int setCount = reader.ReadInt32();
            int polyCount = reader.ReadInt32();

            PolySet[] sets = new PolySet[setCount];

            for (int i = 0; i < setCount; i++)
            {
                PolySet set = new PolySet();
                set.Vert = new Vector3(reader);
                set.Norm = new Vector3(reader);
                set.Tex = new Vector3(reader); 
                sets[i] = set;
            }

            mesh.PolyCount = polyCount;
            mesh.Polygons = new Polygon[mesh.PolyCount];

            for (int p = 0; p < mesh.PolyCount; p++)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < 3; i++)
                {
                    int setIndex = reader.ReadInt32();
                    PolySet set = sets[setIndex];
                    poly.Verts[i] = set.Vert;
                    poly.Norms[i] = set.Norm;
                    poly.TexCoords[i] = set.Tex;
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

        public Vector3 Bounds
        {
            get
            {
                if (cachedBounds == null)
                {
                    float[] min = new float[3];
                    float[] max = new float[3];
                    foreach (Polygon p in Polygons)
                    {
                        foreach (Vector3 v in p.Verts)
                        {
                            if (v.x < min[0]) min[0] = v.x;
                            if (v.y < min[1]) min[1] = v.y;
                            if (v.z < min[2]) min[2] = v.z;

                            if (v.x > max[0]) max[0] = v.x;
                            if (v.y > max[1]) max[1] = v.y;
                            if (v.z > max[2]) max[2] = v.z;
                        }
                    }
                    Vector3 minBounds = new Vector3(min);
                    Vector3 maxBounds = new Vector3(max);
                    cachedBounds = (maxBounds - minBounds);
                }
                return cachedBounds;
            }
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
                newPolygon.Norms = src.Norms.Clone() as Vector3[];
                newPolygon.TexCoords = src.TexCoords.Clone() as Vector3[];
                for (int v = 0; v < 3; v++)
                {
                    Vector3 vert = src.Verts[v];
                    Vector3 newVert = (offset * new CFrame(vert * scale)).p;
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
