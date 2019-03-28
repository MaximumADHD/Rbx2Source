using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Rbx2Source.Coordinates;
using Rbx2Source.Geometry;
using Rbx2Source.QuakeC;
using Rbx2Source.Reflection;
using Rbx2Source.Resources;
using Rbx2Source.StudioMdl;
using Rbx2Source.Textures;
using Rbx2Source.Web;

namespace Rbx2Source.Assembler
{
    public class CatalogItemAssembler : IAssembler
    {
        // TODO: When I wrote this function about 2 years ago, I think I had the intention of
        //       filtering redundant material files based on the material configuration. I should
        //       look into integrating this. Right now it remains unused.

        private static string serializeBrickColorMtl(Material mat)
        {
            long refl = (byte)(mat.Reflectance * 255);
            long trsp = (byte)(mat.Transparency * 255);

            long R = (byte)(mat.VertexColor.X * 255);
            long G = (byte)(mat.VertexColor.Y * 255);
            long B = (byte)(mat.VertexColor.Z * 255);

            long hash = (refl << 32) | (trsp << 24) | (R << 16) | (G << 8) | B;
            string key = hash.ToString("X2");

            return "PartStyle-" + key.ToString();
        }

        private static void AddParts(List<BasePart> parts, Instance scan)
        {
            foreach (BasePart part in scan.GetChildrenOfClass<BasePart>())
            {
                if (part.Transparency < 1)
                {
                    parts.Add(part);
                    Rbx2Source.Print("Found Part {0}", part.Name);
                }
            }

            scan.ForEachChild(inst => AddParts(parts, inst));
        }

        public static StudioMdlWriter AssembleModel(Asset asset)
        {
            Folder content = RBXM.LoadFromAsset(asset);

            Rbx2Source.ScheduleTasks("GatherParts", "BuildMesh");

            List<BasePart> parts = new List<BasePart>();
            AddParts(parts, content);

            if (parts.Count == 0)
                throw new Exception("No parts were found inside of this asset!");

            BasePart primaryPart = null;

            foreach (BasePart part in parts)
            {
                if (part.IsA("MeshPart") || part.Name == "Handle")
                {
                    primaryPart = part;
                    break;
                }
            }

            if (primaryPart == null) // k lol
                primaryPart = parts[0];

            primaryPart.Name = asset.ProductInfo.Name;

            // Mark the primaryPart's location as the center.
            CFrame rootCoord = primaryPart.CFrame;

            foreach (BasePart part in parts)
                part.CFrame = rootCoord.toObjectSpace(part.CFrame);

            Rbx2Source.MarkTaskCompleted("GatherParts");
            Rbx2Source.PrintHeader("BUILDING MESH");

            StudioMdlWriter writer = new StudioMdlWriter();

            BoneKeyframe skeleton = new BoneKeyframe();
            writer.Skeleton.Add(skeleton);

            List<Bone> bones = skeleton.Bones;
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
                    name += count.ToString();
                    part.Name = name;
                }
                else
                {
                    nameCounts[name] = 0;
                }
                
                // Assemble the part.
                Material material = new Material();
                Mesh geometry = Mesh.BakePart(part, material);

                if (geometry != null && geometry.FaceCount > 0)
                {
                    string task = "BuildGeometry_" + name;
                    Rbx2Source.ScheduleTasks(task);
                    Rbx2Source.Print("Building Geometry for {0}", name);

                    Bone bone = new Bone(name, primaryPart, part);
                    bone.C0 = part.CFrame;
                    bones.Add(bone);

                    Node node = bone.Node;
                    nodes.Add(node);

                    materials.Add(name, material);

                    for (int i = 0; i < geometry.FaceCount; i++)
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

        public static TextureAssembly AssembleTextures(Dictionary<string,Material> materials)
        {
            TextureAssembly assembly = new TextureAssembly();
            var images = assembly.Images;
            var matLinks = assembly.MatLinks;

            foreach (string mtlName in materials.Keys)
            {
                Material material = materials[mtlName];
                Asset textureAsset = material.TextureAsset;

                if (textureAsset == null || textureAsset.Id == 9854798)
                {
                    string bcName = "Institutional white";
                    int brickColor = material.LinkedTo.BrickColor;

                    if (BrickColors.NumericalSearch.ContainsKey(brickColor))
                    {
                        BrickColor color = BrickColors.NumericalSearch[brickColor];

                        float r = color.R / 255.0f;
                        float g = color.G / 255.0f;
                        float b = color.B / 255.0f;

                        material.VertexColor = new Vector3(r, g, b);
                        bcName = color.Name;
                    }
                    else
                    {
                        material.VertexColor = new Vector3(1, 1, 1);
                    }

                    if (!images.ContainsKey("BrickColor"))
                    {
                        byte[] rawImg = ResourceUtility.GetResource("Images/BlankWhite.png");

                        using (MemoryStream imgStream = new MemoryStream(rawImg))
                        {
                            Image image = Image.FromStream(imgStream);
                            images.Add("BrickColor", image);
                        }
                    }

                    material.UseReflectance = true;
                    matLinks.Add(mtlName, "BrickColor");
                }
                else
                {
                    byte[] rawImg = textureAsset.GetContent();

                    using (MemoryStream imgStream = new MemoryStream(rawImg))
                    {
                        Image image = Image.FromStream(imgStream);
                        images.Add(mtlName, image);
                        matLinks.Add(mtlName, mtlName);
                    }
                }
            }

            return assembly;
        }

        public AssemblerData Assemble(object metadata)
        {
            long assetId = (long)metadata;

            Asset asset = Asset.Get(assetId);
            string assetName = asset.ProductInfo.WindowsSafeName;

            string appData = Environment.GetEnvironmentVariable("AppData");
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
            var texAssembly = AssembleTextures(materials);

            var images = texAssembly.Images;
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

            texAssembly.MaterialDirectory = compileDir;
            Rbx2Source.MarkTaskCompleted("BuildTextures");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Rbx2Source.PrintHeader("WRITING MATERIAL FILES");
            #region Write Materials
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            string mtlDir = "models/" + compileDir;

            var matLinks = texAssembly.MatLinks;
            var matLookup = new Dictionary<string, Material>();

            foreach (string matName in matLinks.Keys)
            {
                string vtfTarget = matLinks[matName];
                string vmtPath = Path.Combine(materialsDir, matName + ".vmt");

                Material mat = materials[matName];
                mat.SetVmtField("basetexture", mtlDir + "/" + vtfTarget);
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
                CompilerScript = qcPath,
                ModelName = modelName,
                TextureData = texAssembly,

                RootDirectory = rootDir,
                CompileDirectory = compileDir,
                TextureDirectory = texturesDir,
                MaterialDirectory = materialsDir,
            };

            return data;
        }
    }
}
