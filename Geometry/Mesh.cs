using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Rbx2Source.Assembler;
using Rbx2Source.Web;


using RobloxFiles;
using RobloxFiles.Enums;
using RobloxFiles.DataTypes;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Rbx2Source.Geometry
{
    public class Mesh
    {
        public int Version;
        public ushort NumMeshes;

        public int NumVerts;
        public List<Vertex> Verts;

        public int NumFaces;
        public List<int[]> Faces;

        public ushort NumLODs;
        public List<int> LODs;

        public int NumBones;
        public List<MeshBone> Bones;

        public ushort NumSkinData;
        public List<SkinData> SkinData;

        public int NameTableSize;
        public byte[] NameTable;

        // TODO: What is this?
        public ushort Stub = 0;

        public bool HasLODs => (Version >= 3);
        public bool HasSkinning => (Version >= 4);
        public bool HasVertexColors { get; private set; }


        private static readonly IReadOnlyDictionary<string, Asset> StandardLimbs = new Dictionary<string, Asset>
        {
            {"Left Arm",    Asset.FromResource("Meshes/StandardLimbs/leftarm.mesh")},
            {"Right Arm",   Asset.FromResource("Meshes/StandardLimbs/rightarm.mesh")},
            {"Left Leg",    Asset.FromResource("Meshes/StandardLimbs/leftleg.mesh")},
            {"Right Leg",   Asset.FromResource("Meshes/StandardLimbs/rightleg.mesh")},
            {"Torso",       Asset.FromResource("Meshes/StandardLimbs/torso.mesh")}
        };

        public override string ToString()
        {
            return $"Mesh (v{Version}) [{NumFaces} Faces, {NumVerts} Verts, {NumLODs} LODs]";
        }

        private static Vector3 ReadVector3(BinaryReader reader)
        {
            float x = reader.ReadSingle(),
                  y = reader.ReadSingle(),
                  z = reader.ReadSingle();

            return new Vector3(x, y, z);
        }

        private static void LoadGeometry_Ascii(StringReader reader, Mesh mesh)
        {
            string header = reader.ReadLine();
            mesh.NumMeshes = 1;

            if (!header.StartsWith("version 1", StringComparison.InvariantCulture))
                throw new Exception("Expected version 1 header, got: " + header);

            string version = header.Substring(8);
            float vertScale = (version == "1.00" ? 0.5f : 1);

            if (int.TryParse(reader.ReadLine(), out mesh.NumFaces))
                mesh.NumVerts = mesh.NumFaces * 3;
            else
                throw new Exception("Expected 2nd line to be the polygon count.");

            mesh.Faces = new List<int[]>();
            mesh.Verts = new List<Vertex>();

            string polyBuffer = reader.ReadLine();
            MatchCollection matches = Regex.Matches(polyBuffer, @"\[(.*?)\]");

            int face = 0;
            int target = 0;

            var vertex = new Vertex();

            foreach (Match m in matches)
            {
                string vectorStr = m.Groups[1].ToString();

                float[] coords = vectorStr.Split(',')
                    .Select(coord => Format.ParseFloat(coord))
                    .ToArray();

                if (target == 0)
                    vertex.Position = new Vector3(coords) * vertScale;
                else if (target == 1)
                    vertex.Normal = new Vector3(coords);
                else if (target == 2)
                    vertex.UV = new Vector3(coords[0], 1 - coords[1], 0);

                target = (target + 1) % 3;

                if (target == 0)
                {
                    mesh.Verts.Add(vertex);
                    vertex = new Vertex();

                    int v = (face++) * 3;
                    int[] faceDef = new int[3] { v, v + 1, v + 2 };
                    mesh.Faces.Add(faceDef);
                }
            }
        }

        private static void LoadGeometry_Binary(BinaryReader reader, Mesh mesh)
        {
            _ = reader.ReadBytes(13); // version x.xx\n
            _ = reader.ReadUInt16();

            if (mesh.HasSkinning)
            {
                mesh.HasVertexColors = true;
                mesh.NumMeshes = reader.ReadUInt16();

                mesh.NumVerts = reader.ReadInt32();
                mesh.NumFaces = reader.ReadInt32();

                mesh.NumLODs = reader.ReadUInt16();
                mesh.NumBones = reader.ReadUInt16();

                mesh.NameTableSize = reader.ReadInt32();
                mesh.NumSkinData = reader.ReadUInt16();

                mesh.Stub = reader.ReadUInt16();
            }
            else
            {
                var sizeof_Vertex = reader.ReadByte();
                mesh.HasVertexColors = (sizeof_Vertex > 36);

                _ = reader.ReadByte();

                if (mesh.HasLODs)
                {
                    _ = reader.ReadUInt16();
                    mesh.NumLODs = reader.ReadUInt16();
                }

                if (mesh.NumLODs > 0)
                    mesh.NumMeshes = (ushort)(mesh.NumLODs - 1);
                else
                    mesh.NumMeshes = 1;

                mesh.NumVerts = reader.ReadInt32();
                mesh.NumFaces = reader.ReadInt32();

                mesh.NameTable = Array.Empty<byte>();
            }

            mesh.LODs = new List<int>();
            mesh.Faces = new List<int[]>();
            mesh.Verts = new List<Vertex>();
            mesh.Bones = new List<MeshBone>();
            mesh.SkinData = new List<SkinData>();

            // Read Vertices
            for (int i = 0; i < mesh.NumVerts; i++)
            {
                var vert = new Vertex()
                {
                    Position = ReadVector3(reader),
                    Normal = ReadVector3(reader),
                    UV = ReadVector3(reader)
                };

                Color? color = null;

                if (mesh.HasVertexColors)
                {
                    int rgba = reader.ReadInt32();
                    color = Color.FromArgb(rgba << 24 | rgba >> 8);
                }

                vert.Color = color;
                mesh.Verts.Add(vert);
            }

            if (mesh.HasSkinning && mesh.NumBones > 0)
            {
                // Read Bone Weights?
                for (int i = 0; i < mesh.NumVerts; i++)
                {
                    var vert = mesh.Verts[i];

                    var weights = new BoneWeights()
                    {
                        Bones = reader.ReadBytes(4),
                        Weights = reader.ReadBytes(4)
                    };

                    vert.Weights = weights;
                }
            }

            // Read Faces
            for (int i = 0; i < mesh.NumFaces; i++)
            {
                int[] face = new int[3];

                for (int f = 0; f < 3; f++)
                    face[f] = reader.ReadInt32();

                mesh.Faces.Add(face);
            }

            if (mesh.HasLODs && mesh.NumLODs > 0)
            {
                // Read LOD ranges
                for (int i = 0; i < mesh.NumLODs; i++)
                {
                    int lod = reader.ReadInt32();
                    mesh.LODs.Add(lod);
                }
            }

            if (mesh.HasSkinning)
            {
                // Read Bones
                for (int i = 0; i < mesh.NumBones; i++)
                {
                    float[] cf = new float[12];

                    var bone = new MeshBone()
                    {
                        NameIndex = reader.ReadInt32(),
                        Id = reader.ReadInt16(),

                        ParentId = reader.ReadInt16(),
                        Unknown = reader.ReadSingle()
                    };

                    for (int m = 0; m < 12; m++)
                    {
                        int index = (m + 3) % 12;
                        cf[index] = reader.ReadSingle();
                    }

                    bone.CFrame = new CFrame(cf);
                    mesh.Bones.Add(bone);
                }

                // Read Bone Names & Parents
                var nameTable = reader.ReadBytes(mesh.NameTableSize);
                mesh.NameTable = nameTable;

                foreach (MeshBone bone in mesh.Bones)
                {
                    int index = bone.NameIndex;
                    int parentId = bone.ParentId;

                    var buffer = new List<byte>();
                    while (true)
                    {
                        if (nameTable.Length <= 0) break;
                        System.Console.WriteLine(index);
                        byte next = nameTable[index];

                        if (next > 0)
                            index++;
                        else
                            break;

                        buffer.Add(next);
                    }

                    var result = buffer.ToArray();
                    bone.Name = Encoding.UTF8.GetString(result);

                    if (parentId >= 0)
                    {
                        var parent = mesh.Bones[parentId];
                        bone.Parent = parent;
                    }
                }

                // Read Skin Data
                for (int p = 0; p < mesh.NumSkinData; p++)
                {
                    var skinData = new SkinData()
                    {
                        FacesBegin = reader.ReadInt32(),
                        FacesLength = reader.ReadInt32(),

                        VertsBegin = reader.ReadInt32(),
                        VertsLength = reader.ReadInt32(),

                        NumBones = reader.ReadInt32(),
                        BoneIndexTree = new short[26]
                    };

                    for (int i = 0; i < 26; i++)
                        skinData.BoneIndexTree[i] = reader.ReadInt16();

                    mesh.SkinData.Add(skinData);
                }
            }
        }

        public static Mesh FromBuffer(byte[] data)
        {
            string file = Encoding.UTF8.GetString(data);

            if (!file.StartsWith("version ", StringComparison.InvariantCulture))
                throw new Exception("Invalid .mesh header!");

            string versionStr = file.Substring(8, 4);
            double version = Format.ParseDouble(versionStr);

            Mesh mesh = new Mesh() { Version = (int)version };
            
            if (mesh.Version == 1)
            {
                StringReader buffer = new StringReader(file);
                LoadGeometry_Ascii(buffer, mesh);
                buffer.Dispose();
            }
            else
            {
                MemoryStream stream = new MemoryStream(data);

                using (BinaryReader reader = new BinaryReader(stream))
                    LoadGeometry_Binary(reader, mesh);

                stream.Dispose();
            }

            return mesh;
        }

        public static Mesh FromStream(Stream stream)
        {
            Contract.Requires(stream != null);
            byte[] data;

            using (MemoryStream buffer = new MemoryStream())
            {
                stream.CopyTo(buffer);
                data = buffer.ToArray();
            }

            return FromBuffer(data);
        }

        public static Mesh FromFile(string path)
        {
            Mesh result;

            using (FileStream meshStream = File.OpenRead(path))
                result = FromStream(meshStream);

            return result;
        }

        public static Mesh FromAsset(Asset asset)
        {
            Contract.Requires(asset != null);
            byte[] content = asset.GetContent();

            return FromBuffer(content);
        }

        public void BakeGeometry(Vector3 scale, CFrame offset)
        {
            for (int i = 0; i < NumVerts; i++)
            {
                var vert = Verts[i];
                vert.Position = (offset * new CFrame(vert.Position * scale)).Position;
            }
        }

        public static Mesh BakePart(BasePart part, ValveMaterial material = null)
        {
            Contract.Requires(part != null);
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
                if (part is MeshPart meshPart)
                {
                    string meshId = meshPart.MeshId;
                    
                    if (meshId != null && meshId.Length > 0)
                    {
                        meshAsset = Asset.GetByAssetId(meshId);
                    }
                    else
                    {
                        string partName = meshPart.Name;
                        StandardLimbs.TryGetValue(partName, out meshAsset);
                    }

                    if (meshPart.TextureID != null)
                        textureAsset = Asset.GetByAssetId(meshPart.TextureID);

                    scale = meshPart.Size / meshPart.InitialSize;
                    offset = part.CFrame;
                }
                else
                {
                    SpecialMesh specialMesh = part.FindFirstChildOfClass<SpecialMesh>();
                    offset = part.CFrame;
                    
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