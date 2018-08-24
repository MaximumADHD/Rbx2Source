using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Rbx2Source.Animation;
using Rbx2Source.Coordinates;
using Rbx2Source.Geometry;
using Rbx2Source.Reflection;
using Rbx2Source.Resources;
using Rbx2Source.QC;
using Rbx2Source.StudioMdl;
using Rbx2Source.Web;

namespace Rbx2Source.Assembler
{
    struct BoneAssemblePrep
    {
        public List<Attachment> NonRigs;
        public List<Attachment> Completed;
        public List<Bone> Bones;
        public List<Node> Nodes;
        public bool AllowNonRigs;
    }

    enum Limb {Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg, Unknown}

    class CharacterAssembler : IAssembler
    {
        public static Limb GetLimb(Part part)
        {
            string name = part.Name;

            if (name == "Head")
                return Limb.Head;
            else if (name.EndsWith("Torso"))
                return Limb.Torso;

            string limbName;

            if (name.StartsWith("Left"))
                limbName = "Left";
            else if (name.StartsWith("Right"))
                limbName = "Right";
            else
                return Limb.Unknown;

            if (name.EndsWith("Arm") || name.EndsWith("Hand"))
                limbName += "Arm";
            else if (name.EndsWith("Leg") || name.EndsWith("Foot"))
                limbName += "Leg";

            Limb result = Limb.Unknown;
            Enum.TryParse(limbName, out result);

            return result;
        }

        public static List<Attachment> FindOtherAttachments(Attachment a, Instance bin)
        {
            List<Attachment> result = new List<Attachment>();
            foreach (Instance child in bin.GetChildren())
            {
                Attachment b = child.FindFirstChild<Attachment>(a.Name);
                if (b != null && a != b)
                    result.Add(b);
            }
            return result;
        }

        private static void GenerateBones(BoneAssemblePrep prep, Attachment[] queue)
        {
            if (queue.Length == 0) return;

            Instance bin = queue[0].Parent.Parent;

            foreach (Attachment a0 in queue)
            {
                List<Attachment> a1s = FindOtherAttachments(a0, bin);
                foreach (Attachment a1 in a1s)
                {
                    if (a1 != null && !prep.Completed.Contains(a1))
                    {
                        Part part0 = (Part)a0.Parent;
                        Part part1 = (Part)a1.Parent;
                        bool isRigAttachment = a0.Name.EndsWith("RigAttachment");
                        if (isRigAttachment || prep.AllowNonRigs)
                        {
                            Bone bone = new Bone(part1.Name, part0, part1);
                            bone.C0 = a0.CFrame;
                            bone.C1 = a1.CFrame;
                            bone.IsAvatarBone = !prep.AllowNonRigs;
                            prep.Bones.Add(bone);

                            Node node = bone.Node;
                            node.NodeIndex = prep.Bones.IndexOf(bone);
                            prep.Nodes.Add(node);

                            if (!prep.Completed.Contains(a0))
                                prep.Completed.Add(a0);

                            prep.Completed.Add(a1);

                            if (!prep.AllowNonRigs)
                                GenerateBones(prep, part1.GetChildrenOfClass<Attachment>());
                        }
                        else // We'll deal with Accessory attachments afterwards.
                            prep.NonRigs.Add(a0);
                    }
                }
            }
        }

        public static void ApplyBoneCFrames(Part part)
        {
            foreach (Bone bone in part.GetChildrenOfClass<Bone>())
            {
                Part part0 = bone.Part0;
                Part part1 = bone.Part1;

                part1.CFrame = part0.CFrame * bone.C0 * bone.C1.inverse();

                if (part0 != part1)
                    ApplyBoneCFrames(part1);

            }
        }

        public static BoneKeyframe AssembleBones(StudioMdlWriter meshBuilder, Part rootPart)
        {
            Rbx2Source.Print("Building Skeleton...");

            BoneKeyframe kf = new BoneKeyframe();
            List<Bone> bones = kf.Bones;
            List<Node> nodes = meshBuilder.Nodes;

            Bone rootBone = new Bone(rootPart.Name, rootPart);
            rootBone.C0 = new CFrame();
            rootBone.IsAvatarBone = true;
            bones.Add(rootBone);

            Node rootNode = rootBone.Node;
            rootNode.NodeIndex = 0;
            nodes.Add(rootNode);

            BoneAssemblePrep prep = new BoneAssemblePrep();
            prep.Bones = bones;
            prep.Nodes = nodes;
            prep.NonRigs = new List<Attachment>();
            prep.Completed = new List<Attachment>();

            // Assemble the base rig
            GenerateBones(prep, rootPart.GetChildrenOfClass<Attachment>());

            // Assemble the accessories.
            prep.AllowNonRigs = true;
            GenerateBones(prep, prep.NonRigs.ToArray());

            ApplyBoneCFrames(rootPart);
            meshBuilder.Skeleton.Add(kf);

            return kf;
        }

        public static void PrepareAccessory(Instance asset, Folder assembly)
        {
            Part handle = asset.FindFirstChild<Part>("Handle");
            if (handle != null)
            {
                handle.Name = FileUtility.MakeNameWindowsSafe(asset.Name);
                handle.Parent = assembly;
                handle.CFrame = new CFrame();
                if (asset.IsA("Hat")) // Treat it as an Accessory that is using the HatAttachment
                {
                    Hat hat = (Hat)asset;
                    Attachment hatAttachment = new Attachment();
                    hatAttachment.Name = "HatAttachment";
                    hatAttachment.CFrame = new CFrame();
                }
                else if (asset.IsA("Accessory")) // Make sure the attachment is in the Handle
                {
                    Attachment accAtt = asset.FindFirstChildOfClass<Attachment>();
                    if (accAtt != null)
                        accAtt.Parent = handle;
                }
            }
        }

        public static void OverwriteHead(Instance asset, Part head)
        {
            DataModelMesh mesh = (DataModelMesh)asset;
            DataModelMesh currentMesh = head.FindFirstChild<DataModelMesh>("Mesh");
            if (currentMesh != null) currentMesh.Destroy();
            mesh.Name = "Mesh";
            mesh.Parent = head;
        }

        public static void BuildAvatarGeometry(StudioMdlWriter meshBuilder, Bone bone)
        {
            string task = "BuildGeometry_" + bone.Node.Name;
            Rbx2Source.ScheduleTasks(task);
            Node node = bone.Node;
            Part part = bone.Part1;
            bool IsAvatarLimb = bone.IsAvatarBone;

            string materialName;

            if (IsAvatarLimb)
            {
                Limb limb = GetLimb(part);
                materialName = Rbx2Source.GetEnumName(limb);
            }
            else
            {
                materialName = part.Name;
            }

            Material material = new Material();
            material.UseAvatarMap = IsAvatarLimb;

            Rbx2Source.Print("Building Geometry for {0}",part.Name);
            Rbx2Source.IncrementStack();

            Mesh geometry = Mesh.BakePart(part, material);
            if (!meshBuilder.Materials.ContainsKey(materialName))
                meshBuilder.Materials.Add(materialName, material);

            for (int i = 0; i < geometry.FaceCount; i++)
            {
                Triangle tri = new Triangle();
                tri.Node = node;
                tri.Mesh = geometry;
                tri.FaceIndex = i;
                tri.Material = materialName;

                meshBuilder.Triangles.Add(tri);
            }

            Rbx2Source.DecrementStack();
            Rbx2Source.MarkTaskCompleted(task);
        }

        public static Folder AppendCharacterAssets(UserAvatar avatar, string avatarType)
        {
            Rbx2Source.PrintHeader("GATHERING CHARACTER ASSETS");

            Folder characterAssets = new Folder();
            List<long> assetIds = avatar.AccessoryVersionIds;

            foreach (long id in assetIds)
            {
                Asset asset = Asset.Get(id,"/asset/?assetversionid=");
                Folder import = RBXM.LoadFromAsset(asset);
                Folder typeSpecific = import.FindFirstChild<Folder>(avatarType);
                if (typeSpecific != null)
                    import = typeSpecific;

                foreach (Instance obj in import.GetChildren())
                    obj.Parent = characterAssets;
            }

            return characterAssets;
        }

        public static Dictionary<string,string> GatherAnimations(AvatarType avatarType)
        {
            Dictionary<string, string> animations = new Dictionary<string, string>();

            string avatarTypeName = Enum.GetName(typeof(AvatarType), avatarType);
            string animFilesDir = "AvatarData/" + avatarTypeName + "/Animations";

            List<string> animFilePaths = ResourceUtility.GetFiles(animFilesDir);

            foreach (string animFilePath in animFilePaths)
            {
                string animName = animFilePath.Replace(animFilesDir + "/", "").Replace(".rbxmx","");
                animations[animName] = animFilePath;
            }

            return animations;
        }

        public static Asset GetAvatarFace(Folder characterAssets)
        {
            Decal face = characterAssets.FindFirstChild<Decal>("face");
            if (face != null && face.Texture != "rbxasset://textures/face.png")
                return Asset.GetByAssetId(face.Texture);
            else
                return Asset.FromResource("Images/face.png");
        }

        public AssemblerData Assemble(object metadata)
        {
            UserAvatar avatar = metadata as UserAvatar;
            if (avatar == null)
                throw new Exception("bad cast");

            UserInfo userInfo = avatar.UserInfo;
            string userName = FileUtility.MakeNameWindowsSafe(userInfo.Username);

            string appData = Environment.GetEnvironmentVariable("AppData");
            string rbx2Source = Path.Combine(appData, "Rbx2Source");
            string avatars = Path.Combine(rbx2Source, "Avatars");
            string userBin = Path.Combine(avatars, userName);

            string modelDir = Path.Combine(userBin, "Model");
            string animDir = Path.Combine(modelDir, "Animations");
            string texturesDir = Path.Combine(userBin, "Textures");
            string materialsDir = Path.Combine(userBin, "Materials");

            FileUtility.InitiateEmptyDirectories(modelDir, animDir, texturesDir, materialsDir);

            AvatarType avatarType = avatar.ResolvedAvatarType;
            ICharacterAssembler assembler;

            if (avatarType == AvatarType.R15)
                assembler = new R15CharacterAssembler();
            else
                assembler = new R6CharacterAssembler();

            string avatarTypeName = Rbx2Source.GetEnumName(avatar.ResolvedAvatarType);
            Folder characterAssets = AppendCharacterAssets(avatar, avatarTypeName);

            Rbx2Source.ScheduleTasks("BuildCharacter", "BuildCollisionModel", "BuildAnimations", "BuildTextures", "BuildMaterials", "BuildCompilerScript");
            Rbx2Source.PrintHeader("BUILDING CHARACTER MODEL");

            StudioMdlWriter writer = assembler.AssembleModel(characterAssets, avatar.Scales);

            string studioMdl = writer.BuildFile();
            string modelPath = Path.Combine(modelDir,"CharacterModel.smd");
            FileUtility.WriteFile(modelPath, studioMdl);

            // Clear the triangles so we can build a reference pose .smd file.
            writer.Triangles.Clear();
            string staticPose = writer.BuildFile();
            string refPath = Path.Combine(modelDir, "ReferencePos.smd");
            FileUtility.WriteFile(refPath, staticPose);
            Rbx2Source.MarkTaskCompleted("BuildCharacter");

            Rbx2Source.PrintHeader("BUILDING COLLISION MODEL");

            Folder lowPoly = new Folder();

            SpecialMesh lowPolyHead = new SpecialMesh();
            lowPolyHead.MeshId = "rbxassetid://582002794";
            lowPolyHead.MeshType = MeshType.FileMesh;
            lowPolyHead.Scale = new Vector3(1, 1, 1);
            lowPolyHead.Offset = new Vector3();
            lowPolyHead.Parent = lowPoly;

            StudioMdlWriter collisionWriter = assembler.AssembleModel(lowPoly, avatar.Scales);

            string collisionModel = collisionWriter.BuildFile();
            string cmodelPath = Path.Combine(modelDir, "CollisionModel.smd");
            FileUtility.WriteFile(cmodelPath, collisionModel);

            byte[] collisionJoints = assembler.CollisionModelScript;
            string cjointsPath = Path.Combine(modelDir,"CollisionJoints.qc");

            FileUtility.WriteFile(cjointsPath, collisionJoints);
            Rbx2Source.MarkTaskCompleted("BuildCollisionModel");

            Rbx2Source.PrintHeader("BUILDING CHARACTER ANIMATIONS");
            Dictionary<string, string> animations = GatherAnimations(avatarType);

            if (animations.Count > 0)
            {
                foreach (string animName in animations.Keys)
                {
                    Rbx2Source.Print("Building Animation {0}", animName);
                    string localAnimPath = animations[animName];
                    Asset animAsset = Asset.FromResource(localAnimPath);
                    Folder import = RBXM.LoadFromAsset(animAsset);
                    KeyframeSequence sequence = import.FindFirstChildOfClass<KeyframeSequence>();
                    sequence.Name = animName;
                    sequence.AvatarType = avatarType;
                    string animation = AnimationAssembler.Assemble(sequence, writer.Skeleton[0].Bones);
                    string animPath = Path.Combine(animDir, animName + ".smd");
                    FileUtility.WriteFile(animPath, animation);
                }
            }
            else
            {
                Rbx2Source.Print("No animations found :(");
            }

            Rbx2Source.MarkTaskCompleted("BuildAnimations");
            Dictionary<string, Material> materials = writer.Materials;
            Rbx2Source.PrintHeader("BUILDING CHARACTER TEXTURES");

            string compileDirectory = "roblox_avatars/" + userName;

            TextureCompositor texCompositor = assembler.ComposeTextureMap(characterAssets, avatar.BodyColors);
            Rbx2Source.SetDebugImage(texCompositor.BakeTextureMap());

            TextureAssembly texAssembly = assembler.AssembleTextures(texCompositor, materials);
            texAssembly.MaterialDirectory = compileDirectory;

            Dictionary<string, Image> images = texAssembly.Images;

            foreach (string imageName in images.Keys)
            {
                Rbx2Source.Print("Writing Image {0}.png", imageName);
                Image image = images[imageName];
                string imagePath = Path.Combine(texturesDir,imageName + ".png");
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
            
            Rbx2Source.MarkTaskCompleted("BuildTextures");
            Rbx2Source.PrintHeader("BUILDING MATERIAL FILES");

            Dictionary<string, string> matLinks = texAssembly.MatLinks;

            foreach (string mtlName in matLinks.Keys)
            {
                Rbx2Source.Print("Building VMT {0}.vmt", mtlName);
                string targetVtf = matLinks[mtlName];
                Material mtl = materials[mtlName];
                ValveMaterial vmt = new ValveMaterial(mtl);
                vmt.SetField("basetexture", "models/" + compileDirectory + "/" + targetVtf);
                string vmtPath = Path.Combine(materialsDir, mtlName + ".vmt");
                string vmtContent = vmt.ToString();
                FileUtility.WriteFile(vmtPath, vmtContent);
            }

            Rbx2Source.MarkTaskCompleted("BuildMaterials");
            Rbx2Source.PrintHeader("WRITING COMPILER SCRIPT");

            QCWriter qc = new QCWriter();

            QCommand model = new QCommand("body", userName, "CharacterModel.smd");
            qc.AddCommand(model);

            string modelNameStr = compileDirectory + ".mdl";
            qc.WriteBasicCmd("modelname", modelNameStr);
            qc.WriteBasicCmd("upaxis", "y");

            string originStr = "";
            if (avatarType == AvatarType.R6)
                originStr = "0 -30 0";
            else
                originStr = "0 " + (-23.5 * avatar.Scales.Height).ToString(Rbx2Source.NormalParse) + " 0";

            qc.WriteBasicCmd("origin", originStr, false);
            qc.WriteBasicCmd("cdmaterials", "models/" + compileDirectory);
            qc.WriteBasicCmd("surfaceprop", "flesh");
            qc.WriteBasicCmd("include", "CollisionJoints.qc");

            QCommand reference = new QCommand("sequence", "reference", "ReferencePos.smd");
            reference.AddParameter("fps", "1");
            reference.AddParameter("loop");
            qc.AddCommand(reference);

            foreach (string animName in animations.Keys)
            {
                QCommand sequence = new QCommand("sequence", animName.ToLower(), "Animations/" + animName + ".smd");
                sequence.AddParameter("fps", AnimationAssembler.FrameRate.ToString());
                sequence.AddParameter("loop");
                if (avatarType == AvatarType.R6) // TODO: Find a work around so I can get rid of this.
                    sequence.AddParameter("delta");

                qc.AddCommand(sequence);
            }

            string qcFile = qc.BuildFile();
            string qcPath = Path.Combine(modelDir, "Compile.qc");
            FileUtility.WriteFile(qcPath, qcFile);
            Rbx2Source.MarkTaskCompleted("BuildCompilerScript");

            AssemblerData data = new AssemblerData();
            data.ModelData = writer;
            data.TextureData = texAssembly;
            data.CompilerScript = qcPath;
            data.RootDirectory = userBin;
            data.MaterialDirectory = materialsDir;
            data.TextureDirectory = texturesDir;
            data.CompileDirectory = compileDirectory;
            data.ModelName = modelNameStr;

            return data;
        }
    }
}
