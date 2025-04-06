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
using System.Runtime.InteropServices;

using Openize;
using Openize.Drako;
using System.Collections.ObjectModel;

namespace Rbx2Source.Geometry
{
    public class Mesh
    {
        public int Version;
        public List<Vertex> Verts;
        public List<int[]> Faces;

        public List<int> LodOffsets;
        public List<MeshBone> Bones;

        private static readonly IReadOnlyDictionary<string, Asset> StandardLimbs = new Dictionary<string, Asset>
        {
            {"Left Arm",    Asset.FromResource("Meshes/StandardLimbs/leftarm.mesh")},
            {"Right Arm",   Asset.FromResource("Meshes/StandardLimbs/rightarm.mesh")},
            {"Left Leg",    Asset.FromResource("Meshes/StandardLimbs/leftleg.mesh")},
            {"Right Leg",   Asset.FromResource("Meshes/StandardLimbs/rightleg.mesh")},
            {"Torso",       Asset.FromResource("Meshes/StandardLimbs/torso.mesh")}
        };

        private static readonly Dictionary<long, ObjFile> MorphObjs = new Dictionary<long, ObjFile>();
        private static readonly Dictionary<long, int> MorphGroups = new Dictionary<long, int>();

        public Mesh()
        {
            Verts = new List<Vertex>();
            Faces = new List<int[]>();
            Bones = new List<MeshBone>();
            LodOffsets = new List<int>() { 0, 0 };
        }

        public override string ToString()
        {
            return $"Mesh (v{Version}) [{Faces.Count} Faces, {Verts.Count} Verts, {LodOffsets.Count} LOD Offsets]";
        }

        private static Vector3 ReadVector3(BinaryReader reader)
        {
            float x = reader.ReadSingle(),
                  y = reader.ReadSingle(),
                  z = reader.ReadSingle();

            return new Vector3(x, y, z);
        }

        private static Vector2 ReadVector2(BinaryReader reader)
        {
            float x = reader.ReadSingle(),
                  y = reader.ReadSingle();

            return new Vector2(x, y);
        }

        private static void LoadGeometry_Ascii(StringReader reader, Mesh mesh)
        {
            string header = reader.ReadLine();

            if (!header.StartsWith("version 1", StringComparison.InvariantCulture))
                throw new Exception("Expected version 1 header, got: " + header);

            string version = header.Substring(8);
            float vertScale = (version == "1.00" ? 0.5f : 1);
            int numVerts;

            if (int.TryParse(reader.ReadLine(), out int numFaces))
                numVerts = numFaces * 3;
            else
                throw new Exception("Expected 2nd line to be the polygon count.");

            mesh.Faces = new List<int[]>();
            mesh.Verts = new List<Vertex>();
            mesh.LodOffsets = new List<int>() { 0, numFaces };

            string polyBuffer = reader.ReadLine();
            MatchCollection matches = Regex.Matches(polyBuffer, @"\[(.*?)\]");

            int face = 0;
            int index = 0;
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
                    vertex.UV = new Vector2(coords[0], 1 - coords[1]);

                target = (target + 1) % 3;

                if (target == 0)
                {
                    mesh.Verts.Add(vertex);
                    vertex = new Vertex();

                    if (index % 3 == 0)
                    {
                        int v = (face++) * 3;
                        int[] faceDef = new int[3] { v, v + 1, v + 2 };
                        mesh.Faces.Add(faceDef);
                    }
                }
            }
        }

        private static void LoadGeometry_Chunks(BinaryReader reader, Mesh mesh)
        {
            var stream = reader.BaseStream;

            while (stream.Position < stream.Length)
            {
                byte[] rawChunkId = reader.ReadBytes(8);
                string chunkId = Encoding.ASCII.GetString(rawChunkId).TrimEnd('\0');

                int version = reader.ReadInt32();
                int size = reader.ReadInt32();

                switch (chunkId)
                {
                    case "COREMESH":
                    {
                        if (version == 1)
                        {
                            int numVerts = reader.ReadInt32();

                            for (int i = 0; i < numVerts; i++)
                            {
                                var vert = new Vertex()
                                {
                                    Position = ReadVector3(reader),
                                    Normal = ReadVector3(reader),
                                    UV = ReadVector2(reader)
                                };

                                uint xyzs = reader.ReadUInt32();
                                int rgba = reader.ReadInt32();

                                vert.Color = Color.FromArgb(rgba << 24 | rgba >> 8);
                                mesh.Verts.Add(vert);
                            }

                            int numFaces = reader.ReadInt32();

                            for (int i = 0; i < numFaces; i++)
                            {
                                var face = new int[3];

                                for (int f = 0; f < 3; f++)
                                    face[f] = reader.ReadInt32();

                                mesh.Faces.Add(face);
                            }
                        }
                        else if (version == 2)
                        {
                            var packed = reader.ReadInt32();
                            var rawBuffer = reader.ReadBytes(packed);
                            var dracoMesh = Draco.Decode(rawBuffer) as DracoMesh;

                            var attrUVs = dracoMesh.GetNamedAttribute(Openize.Drako.AttributeType.TexCoord);
                            var uvSpan = MemoryMarshal.Cast<byte, float>(attrUVs.Buffer.AsSpan());

                            var attrVerts = dracoMesh.GetNamedAttribute(Openize.Drako.AttributeType.Position);
                            var vertSpan = MemoryMarshal.Cast<byte, float>(attrVerts.Buffer.AsSpan());

                            var attrNorms = dracoMesh.GetNamedAttribute(Openize.Drako.AttributeType.Generic);
                            var normSpan = MemoryMarshal.Cast<byte, float>(attrNorms.Buffer.AsSpan());

                            var attrColors = dracoMesh.GetNamedAttribute(Openize.Drako.AttributeType.Color);
                            var colorSpan = attrColors.Buffer.AsSpan();

                            var verts = mesh.Verts;
                            var faces = mesh.Faces;

                            for (int i = 0; i < dracoMesh.NumPoints; i++)
                            {
                                int uvId = i * 2;
                                int posId = i * 3;

                                var posX = vertSpan[posId];
                                var posY = vertSpan[posId + 1];
                                var posZ = vertSpan[posId + 2];

                                var normX = normSpan[posId];
                                var normY = normSpan[posId + 1];
                                var normZ = normSpan[posId + 2];

                                var uvX = uvSpan[uvId];
                                var uvY = uvSpan[uvId + 1];
                                var rgba = colorSpan[i];

                                var vert = new Vertex()
                                {
                                    Color = Color.FromArgb(rgba << 24 | rgba >> 8),
                                    Normal = new Vector3(normX, normY, normZ),
                                    Position = new Vector3(posX, posY, posZ),
                                    UV = new Vector2(uvX, uvY),
                                };

                                verts.Add(vert);
                            }

                            for (int i = 0; i < dracoMesh.NumFaces; i++)
                            {
                                int[] dracoFace = new int[3];
                                dracoMesh.ReadFace(i, dracoFace);
                                faces.Add(dracoFace);
                            }
                        }
                        else
                        {
                            reader.ReadBytes(size);
                        }

                        mesh.LodOffsets = new List<int>() { 0, mesh.Faces.Count };
                        break;
                    }
                    case "LODS":
                    {
                        if (version == 1)
                        {
                            _ = reader.ReadUInt16(); //lodType;
                            _ = reader.ReadByte(); // numHighQualityLODs;

                            uint numLodOffsets = reader.ReadUInt32();
                            mesh.LodOffsets.Clear();

                            for (int i = 0; i < numLodOffsets; i++)
                            {
                                int lodOffset = reader.ReadInt32();
                                mesh.LodOffsets.Add(lodOffset);
                            }
                        }
                        else
                        {
                            mesh.LodOffsets = new List<int>() { 0, mesh.Faces.Count };
                            reader.ReadBytes(size);
                        }

                        break;
                    }
                    default:
                    {
                        reader.ReadBytes(size);
                        break;
                    }
                }
            }
        }

        private static void LoadGeometry_Binary(BinaryReader reader, Mesh mesh)
        {
            _ = reader.ReadBytes(13); // version x.xx\n

            if (mesh.Version >= 6)
            {
                LoadGeometry_Chunks(reader, mesh);
                return;
            }

            int numVerts;
            int numFaces;
            int numBones = 0;
            int numSubsets = 0;
            int numLodOffsets = 0;

            //int facsDataFormat;
            //int facsDataSize;

            bool hasVertexColors = true;
            int boneNameTblSize = 0;

            byte[] boneNameTbl;
            var skinningData = new List<MeshSkinning>();

            mesh.Faces = new List<int[]>();
            mesh.Verts = new List<Vertex>();
            mesh.Bones = new List<MeshBone>();
            mesh.LodOffsets = new List<int>();

            if (mesh.Version >= 4)
            {
                // sizeof_FileMeshHeader
                _ = reader.ReadUInt16();

                // lodType
                _ = reader.ReadUInt16();

                numVerts = reader.ReadInt32();
                numFaces = reader.ReadInt32();

                numLodOffsets = reader.ReadUInt16();
                numBones = reader.ReadUInt16();

                boneNameTblSize = reader.ReadInt32();
                numSubsets = reader.ReadUInt16();

                // numHighQualityLODs
                _ = reader.ReadByte();

                // unused
                _ = reader.ReadByte(); 

                if (mesh.Version >= 5)
                {
                    //facsDataFormat = reader.ReadInt32();
                    //facsDataSize = reader.ReadInt32();
                }
            }
            else
            {
                // sizeof_FileMeshHeader
                _ = reader.ReadUInt16(); 

                var sizeof_Vertex = reader.ReadByte();
                hasVertexColors = (sizeof_Vertex == 40);

                // sizeof_MeshFace
                _ = reader.ReadByte(); 

                if (mesh.Version >= 3)
                {
                    _ = reader.ReadUInt16(); // sizeof_LodOffset
                    numLodOffsets = reader.ReadUInt16();
                }

                numVerts = reader.ReadInt32();
                numFaces = reader.ReadInt32();
            }

            // Read Vertices
            for (int i = 0; i < numVerts; i++)
            {
                var vert = new Vertex()
                {
                    Position = ReadVector3(reader),
                    Normal = ReadVector3(reader),
                    UV = ReadVector2(reader)
                };

                var xyzs = reader.ReadUInt32();
                Color? color = null;

                if (hasVertexColors)
                {
                    int rgba = reader.ReadInt32();
                    color = Color.FromArgb(rgba << 24 | rgba >> 8);
                }

                vert.Color = color;
                mesh.Verts.Add(vert);
            }

            if (mesh.Version >= 4 && numBones > 0)
            {
                // Read Skinning
                for (int i = 0; i < numVerts; i++)
                {
                    var skinning = new MeshSkinning()
                    {
                        SubsetIndices = reader.ReadBytes(4),
                        BoneWeights = reader.ReadBytes(4)
                    };

                    skinningData.Add(skinning);
                }
            }

            // Read Faces
            for (int i = 0; i < numFaces; i++)
            {
                int[] face = new int[3];

                for (int f = 0; f < 3; f++)
                    face[f] = reader.ReadInt32();

                mesh.Faces.Add(face);
            }

            if (mesh.Version >= 3 && numLodOffsets > 0)
            {
                // Read LOD ranges
                for (int i = 0; i < numLodOffsets; i++)
                {
                    int lod = reader.ReadInt32();
                    mesh.LodOffsets.Add(lod);
                }
            }
            else
            {
                mesh.LodOffsets = new List<int>() { 0, numFaces };
            }

            if (mesh.Version >= 4)
            {
                // Read Bones
                for (int i = 0; i < numBones; i++)
                {
                    float[] cf = new float[12];

                    var bone = new MeshBone()
                    {
                        NameIndex = reader.ReadInt32(),
                        ParentIndex = reader.ReadInt16(),

                        LodParentIndex = reader.ReadInt16(),
                        Culling = reader.ReadSingle()
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
                boneNameTbl = reader.ReadBytes(boneNameTblSize);

                foreach (MeshBone bone in mesh.Bones)
                {
                    int index = bone.NameIndex;
                    var buffer = new List<byte>();
                    int parentIndex = bone.ParentIndex;

                    while (true)
                    {
                        byte next = boneNameTbl[index];

                        if (next > 0)
                            index++;
                        else
                            break;

                        buffer.Add(next);
                    }

                    var result = buffer.ToArray();
                    bone.Name = Encoding.UTF8.GetString(result);

                    if (parentIndex >= 0)
                    {
                        var parent = mesh.Bones[parentIndex];
                        bone.Parent = parent;
                    }
                }

                // Read Subsets, then map bone weights
                for (int p = 0; p < numSubsets; p++)
                {
                    _ = reader.ReadInt32(); // FacesBegin
                    _ = reader.ReadInt32(); // FacesLength

                    var vertsBegin = reader.ReadInt32();
                    var vertsEnd = vertsBegin + reader.ReadInt32();

                    _ = reader.ReadInt32(); // NumBoneIndices
                    var boneIndices = new short[26];

                    for (int i = 0; i < 26; i++)
                        boneIndices[i] = reader.ReadInt16();

                    for (int i = vertsBegin; i < vertsEnd; i++)
                    {
                        Vertex vert = mesh.Verts[i];
                        var skinning = skinningData[i];

                        for (int j = 0; j < 3; j++)
                        {
                            byte subsetIndex = skinning.SubsetIndices[j];
                            byte boneWeight = skinning.BoneWeights[j];

                            if (boneWeight > 0)
                            {
                                var boneIndex = boneIndices[subsetIndex];
                                vert.Weights[boneIndex] = boneWeight / 255f;
                            }
                        }
                    }
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

        public void Recenter()
        {
            Vector3 center = Vector3.zero;

            foreach (var vert in Verts)
                center += vert.Position;

            center /= Verts.Count;
            Verts.ForEach(vert => vert.Position -= center);
        }

        public static void RegisterTemporaryBakeMorph(long assetId, ObjFile file, int group)
        {
            MorphGroups[assetId] = group;
            MorphObjs[assetId] = file;
        }

        public void BakeGeometry(Vector3 scale, CFrame offset)
        {
            foreach (var vert in Verts)
            {
                var pos = vert.Position;
                var norm = vert.Normal;

                if (scale != null && scale != Vector3.one)
                    pos *= scale;

                if (offset != null && offset != CFrame.identity)
                {
                    pos = offset.PointToWorldSpace(pos);
                    norm = offset.VectorToWorldSpace(norm);
                }

                vert.Position = pos;
                vert.Normal = norm;
            }
        }

        private static long HashUV(Vector2 uv)
        {
            long x = (long)Math.Round(uv.X * 1000);
            long y = (long)Math.Round(uv.Y * 1000);
            return x * 73856093 + y * 19351301;
        }

        public static Mesh BuildMorph(long assetId, Mesh source)
        {
            var obj = MorphObjs[assetId];
            var group = MorphGroups[assetId];
            var indices = new List<int>();

            var faces = obj.Faces
                .Where(face => face[0].Group == group)
                .ToArray();

            var morphTree = new Dictionary<long, HashSet<Vector3>>();

            foreach (var face in faces)
            {
                for (int i = 0; i < 3; i++)
                {
                    var index = face[i];
                    int? uvId = index.UV;

                    if (uvId.HasValue)
                    {
                        var uv = obj.UVs[uvId.Value];
                        var hash = HashUV(uv);

                        if (!morphTree.TryGetValue(hash, out var list))
                        {
                            list = new HashSet<Vector3>();
                            morphTree.Add(hash, list);
                        }

                        var vert = obj.Verts[index.Vert];
                        list.Add(vert);
                    }
                }
            }

            Rbx2Source.Print($"Mapping bones and weights...");
            var faceStride = source.LodOffsets[1];
            var facesToRemove = new List<int[]>();

            for (int i = 0; i < faceStride; i++)
            {
                var face = source.Faces[i];
                var matched = true;

                for (int j = 0; j < 3; j++)
                {
                    var vertId = face[j];
                    var vert = source.Verts[vertId];

                    var uv = vert.UV;
                    var hash = HashUV(uv);

                    if (morphTree.TryGetValue(hash, out var list))
                    {
                        var pos = list.First();
                        vert.Position = pos;
                    }
                    else
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                    continue;

                facesToRemove.Add(face);
            }

            source.Faces = source.Faces
                .Except(facesToRemove)
                .ToList();

            for (int i = 1; i < source.LodOffsets.Count; i++)
                source.LodOffsets[i] -= facesToRemove.Count;

            return source;
        }

        public static Mesh BakePart(BasePart part, ValveMaterial material = null)
        {
            Mesh result = null;
            Asset meshAsset = null;

            Asset albedoAsset = null;
            Asset normalAsset = null;

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
                        meshAsset = Asset.GetByAssetId(meshId);
                    else
                        StandardLimbs.TryGetValue(meshPart.Name, out meshAsset);

                    var surface = meshPart.FindFirstChildOfClass<SurfaceAppearance>();

                    if (surface != null)
                    {
                        if (material != null)
                            material.UseAvatarMap = false;

                        var colorMap = surface.ColorMap;
                        albedoAsset = Asset.GetByAssetId(colorMap);

                        var normalMap = surface.NormalMap;
                        normalAsset = Asset.GetByAssetId(normalMap);
                    }
                    else if (meshPart.TextureID != null)
                    {
                        albedoAsset = Asset.GetByAssetId(meshPart.TextureID);
                    }

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
                            albedoAsset = Asset.GetByAssetId(specialMesh.TextureId);
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
                {
                    material.AddTextureAsset("basetexture", albedoAsset);
                    material.AddTextureAsset("bumpmap", normalAsset);
                }

                result = FromAsset(meshAsset);

                if (MorphObjs.ContainsKey(meshAsset.Id))
                    result = BuildMorph(meshAsset.Id, result);

                result.BakeGeometry(scale, offset);
            }

            return result;
        }
    }
}