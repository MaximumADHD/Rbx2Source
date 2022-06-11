using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using RobloxFiles;
using RobloxFiles.DataTypes;

using Rbx2Source.Geometry;
using Rbx2Source.QuakeC;
using Rbx2Source.Resources;
using Rbx2Source.StudioMdl;
using Rbx2Source.Web;
using System.Diagnostics.Contracts;

namespace Rbx2Source.Assembler
{
    public class CatalogItemAssembler : IAssembler<long>
    {
        private static void AddParts(List<BasePart> parts, Instance scan)
        {
            foreach (BasePart part in scan.GetChildrenOfType<BasePart>())
            {
                if (part.Transparency < 1)
                {
                    parts.Add(part);
                    Rbx2Source.Print("Found Part {0}", part.Name);
                }
            }

            var children = scan
                .GetChildren()
                .ToList();

            children.ForEach(inst => AddParts(parts, inst));
        }

        public static StudioMdlWriter AssembleModel(Asset asset)
        {
            Contract.Requires(asset != null);

            var content = asset.OpenAsModel();
            Rbx2Source.ScheduleTasks("GatherParts", "BuildMesh");

            List<BasePart> parts = new List<BasePart>();
            AddParts(parts, content);

            if (parts.Count == 0)
                throw new Exception("No parts were found inside of this asset!");

            BasePart primaryPart = null;

            foreach (BasePart part in parts)
            {
                if (part is MeshPart || part.Name == "Handle")
                {
                    primaryPart = part;
                    break;
                }
            }

            if (primaryPart == null) // k lol
                primaryPart = parts[0];

            primaryPart.Name = asset.ProductInfo.WindowsSafeName.Trim();

            // Mark the primaryPart's location as the center.
            CFrame rootCoord = primaryPart.CFrame;

            foreach (BasePart part in parts)
                part.CFrame = rootCoord.ToObjectSpace(part.CFrame);

            Rbx2Source.MarkTaskCompleted("GatherParts");
            Rbx2Source.PrintHeader("BUILDING MESH");

            StudioMdlWriter writer = new StudioMdlWriter();

            BoneKeyframe skeleton = new BoneKeyframe();
            writer.Skeleton.Add(skeleton);

            List<StudioBone> bones = skeleton.Bones;
            List<Node> nodes = writer.Nodes;

            List<Triangle> triangles = writer.Triangles;
            int numAssembledParts = 0;

            var materials = writer.Materials;
            var nameCounts = new Dictionary<string, int>();

            foreach (BasePart part in parts)
            {
                // Make sure this part has a unique name.
                string name = part.Name;

                if (nameCounts.ContainsKey(name))
                {
                    int count = ++nameCounts[name];
                    name += count.ToInvariantString();
                    part.Name = name;
                }
                else
                {
                    nameCounts[name] = 0;
                }
                
                // Assemble the part.
                var material = new ValveMaterial();
                Mesh geometry = Mesh.BakePart(part, material);

                if (geometry != null && geometry.NumFaces > 0)
                {
                    string task = "BuildGeometry_" + name;
                    Rbx2Source.ScheduleTasks(task);
                    Rbx2Source.Print("Building Geometry for {0}", name);

                    var bone = new StudioBone(name, primaryPart, part) { C0 = part.CFrame };
                    bones.Add(bone);

                    Node node = bone.Node;
                    nodes.Add(node);

                    int faceStride;
                    materials.Add(name, material);

                    if (geometry.HasLODs)
                        faceStride = geometry.LODs[1];
                    else
                        faceStride = geometry.NumFaces;

                    for (int i = 0; i < faceStride; i++)
                    {
                        Triangle tri = new Triangle()
                        {
                            Node = node,
                            Mesh = geometry,
                            FaceIndex = i,
                            Material = name,
                        };

                        triangles.Add(tri);
                    }

                    Rbx2Source.MarkTaskCompleted(task);
                    numAssembledParts++;
                }
            }
                

            Rbx2Source.MarkTaskCompleted("BuildMesh");
            return writer;
        }

        public static TextureBindings BindTextures(Dictionary<string, ValveMaterial> materials)
        {
            Contract.Requires(materials != null);

            var textures = new TextureBindings();
            var images = textures.Images;

            foreach (string mtlName in materials.Keys)
            {
                ValveMaterial material = materials[mtlName];
                Asset textureAsset = material.TextureAsset;

                if (textureAsset == null || textureAsset.Id == 9854798)
                {
                    var linkedTo = material.LinkedTo;
                    Color3 color;

                    if (linkedTo.BrickColor != null)
                    {
                        BrickColor bc = linkedTo.BrickColor;
                        color = bc.Color;
                    }
                    else if (linkedTo.Color3uint8 != null)
                    {
                        color = linkedTo.Color3uint8;
                    }
                    else
                    {
                        BrickColor def = BrickColor.FromNumber(-1);
                        color = def.Color;
                    }

                    float r = color.R,
                          g = color.G,
                          b = color.B;

                    if (!images.ContainsKey("BrickColor"))
                    {
                        byte[] rawImg = ResourceUtility.GetResource("Images/BlankWhite.png");

                        using (MemoryStream imgStream = new MemoryStream(rawImg))
                        {
                            Image image = Image.FromStream(imgStream);
                            textures.BindTexture("BrickColor", image, false);
                        }
                    }

                    material.UseEnvMap = true;
                    material.VertexColor = new Vector3(r, g, b);

                    textures.BindTextureAlias(mtlName, "BrickColor");
                }
                else
                {
                    byte[] rawImg = textureAsset.GetContent();

                    using (MemoryStream imgStream = new MemoryStream(rawImg))
                    {
                        Image image = Image.FromStream(imgStream);
                        textures.BindTexture(mtlName, image);
                    }
                }
            }

            return textures;
        }

        public AssemblerData Assemble(long assetId)
        {
            Asset asset = Asset.Get(assetId);
            string assetName = asset.ProductInfo.WindowsSafeName.Trim();

            string appData = Environment.GetEnvironmentVariable("LocalAppData");
            string rbx2Source = Path.Combine(appData, "Rbx2Source");
            string items = Path.Combine(rbx2Source, "Items");
            string rootDir = Path.Combine(items, assetName);

            string modelDir = Path.Combine(rootDir, "Model");
            string texturesDir = Path.Combine(rootDir, "Textures");
            string materialsDir = Path.Combine(rootDir, "Materials");

            FileUtility.InitiateEmptyDirectories(modelDir, texturesDir, materialsDir);
            Rbx2Source.ScheduleTasks("BuildModel", "BuildTextures", "BuildMaterials", "BuildCompilerScript");

            Rbx2Source.PrintHeader("BUILDING MODEL");
            #region Build Model
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            StudioMdlWriter writer = AssembleModel(asset);

            string studioMdl = writer.BuildFile();
            string modelPath = Path.Combine(modelDir, "Asset.smd");
            FileUtility.WriteFile(modelPath, studioMdl);

            string reference = writer.BuildFile(false);
            string refPath = Path.Combine(modelDir, "Reference.smd");
            FileUtility.WriteFile(refPath, reference);
                
            Rbx2Source.MarkTaskCompleted("BuildModel");
            
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Rbx2Source.PrintHeader("BUILDING TEXTURES");
            #region Build Textures
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            var materials = writer.Materials;
            var textures = BindTextures(materials);

            var images = textures.Images;
            var compileDir = "roblox_assets/" + assetName;

            foreach (string imageName in images.Keys)
            {
                Rbx2Source.Print("Writing Image {0}", imageName);

                Image image = images[imageName];
                string imagePath = Path.Combine(texturesDir, imageName + ".png");

                try
                {
                    image.Save(imagePath, ImageFormat.Png);
                }
                catch
                {
                    Rbx2Source.Print("IMAGE {0}.png FAILED TO SAVE!", imageName);
                }

                FileUtility.LockFile(imagePath);
            }

            textures.MaterialDirectory = compileDir;
            Rbx2Source.MarkTaskCompleted("BuildTextures");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Rbx2Source.PrintHeader("WRITING MATERIAL FILES");
            #region Write Materials
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            string mtlDir = "models/" + compileDir;
            var matLinks = textures.MatLinks;
            
            foreach (string matName in matLinks.Keys)
            {
                string vtfTarget = matLinks[matName];
                string vmtPath = Path.Combine(materialsDir, matName + ".vmt");

                ValveMaterial mat = materials[matName];
                mat.SetVmtField("basetexture", mtlDir + '/' + vtfTarget);
                mat.WriteVmtFile(vmtPath);
            }

            Rbx2Source.MarkTaskCompleted("BuildMaterials");
            
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Rbx2Source.PrintHeader("WRITING COMPILER SCRIPT");
            #region Write Compiler Script
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            string modelName = compileDir + ".mdl";
            QuakeCWriter qc = new QuakeCWriter();

            qc.Add("body", assetName, "Asset.smd");
            qc.Add("modelname", modelName);
            qc.Add("upaxis", "y");
            qc.Add("cdmaterials", mtlDir);

            QuakeCItem phys = qc.Add("collisionjoints", "Asset.smd");
            phys.AddSubItem("$mass", 115.0);
            phys.AddSubItem("$inertia", 2.00);
            phys.AddSubItem("$damping", 0.01);
            phys.AddSubItem("$rotdamping", 0.40);

            QuakeCItem refAnim = qc.Add("sequence", "reference", "Reference.smd");
            refAnim.AddSubItem("fps", 1);
            refAnim.AddSubItem("loop");

            string qcFile = qc.ToString();
            string qcPath = Path.Combine(modelDir, "Compile.qc");

            FileUtility.WriteFile(qcPath, qcFile);
            Rbx2Source.MarkTaskCompleted("BuildCompilerScript");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            AssemblerData data = new AssemblerData()
            {
                ModelData = writer,
                ModelName = modelName,
                TextureData = textures,
                CompilerScript = qcPath,
                
                RootDirectory = rootDir,
                CompileDirectory = compileDir,
                TextureDirectory = texturesDir,
                MaterialDirectory = materialsDir
            };

            return data;
        }
    }
}
