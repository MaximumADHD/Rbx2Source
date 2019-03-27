using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using Rbx2Source.Animating;
using Rbx2Source.Coordinates;
using Rbx2Source.Geometry;
using Rbx2Source.Reflection;
using Rbx2Source.QC;
using Rbx2Source.StudioMdl;
using Rbx2Source.Textures;
using Rbx2Source.Web;

namespace Rbx2Source.Assembler
{
    public enum Limb { Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg, Unknown }

    public struct BoneAssemblePrep
    {
        public List<Attachment> NonRigs;
        public List<Attachment> Completed;

        public List<Bone> Bones;
        public List<Node> Nodes;

        public bool AllowNonRigs;
    }

    public class CharacterAssembler : IAssembler
    {
        private static bool DEBUG_RAPID_ASSEMBLY = false;

        public static Limb GetLimb(BasePart part)
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
                        BasePart part0 = (BasePart)a0.Parent;
                        BasePart part1 = (BasePart)a1.Parent;

                        bool isRigAttachment = a0.Name.EndsWith("RigAttachment");

                        if (isRigAttachment || prep.AllowNonRigs)
                        {
                            Bone bone = new Bone(part1.Name, part0, part1);
                            bone.C0 = a0.CFrame;
                            bone.C1 = a1.CFrame;
                            bone.IsAvatarBone = !prep.AllowNonRigs;
                            prep.Bones.Add(bone);

                            // .________________.
                            // I honestly have no clue whats going on here.

                            if (part0.Name == "UpperTorso" && part1.Name != "Head")
                            {
                                CFrame cf = a0.CFrame;
                                float x = cf.x;
                                float w = part0.Size.X / 2f;

                                if (Math.Abs(x) > w)
                                {
                                    int sign = Math.Sign(x);
                                    bone.C0 = new CFrame(w * sign, cf.y, cf.z) * (cf - cf.p);
                                }
                            }

                            Node node = bone.Node;
                            node.NodeIndex = prep.Bones.IndexOf(bone);
                            prep.Nodes.Add(node);

                            if (!prep.Completed.Contains(a0))
                                prep.Completed.Add(a0);

                            prep.Completed.Add(a1);

                            if (!prep.AllowNonRigs)
                            {
                                GenerateBones(prep, part1.GetChildrenOfClass<Attachment>());
                            }
                        }
                        else // We'll deal with Accessory attachments afterwards.
                        {
                            prep.NonRigs.Add(a0);
                        }
                    }
                }
            }
        }

        public static void ApplyBoneCFrames(BasePart part)
        {
            foreach (Bone bone in part.GetChildrenOfClass<Bone>())
            {
                BasePart part0 = bone.Part0;
                BasePart part1 = bone.Part1;

                part1.CFrame = part0.CFrame * bone.C0 * bone.C1.inverse();

                if (part0 != part1)
                    ApplyBoneCFrames(part1);

            }
        }

        public static BoneKeyframe AssembleBones(StudioMdlWriter meshBuilder, BasePart rootPart)
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

            // Assemble the base rig.
            GenerateBones(prep, rootPart.GetChildrenOfClass<Attachment>());

            // Assemble the accessories.
            prep.AllowNonRigs = true;
            GenerateBones(prep, prep.NonRigs.ToArray());

            // Apply the rig cframe data.
            ApplyBoneCFrames(rootPart);
            meshBuilder.Skeleton.Add(kf);

            return kf;
        }

        public static void PrepareAccessory(Instance asset, Folder assembly)
        {
            if (DEBUG_RAPID_ASSEMBLY)
            {
                asset.Destroy();
                return;
            }

            BasePart handle = asset.FindFirstChild<BasePart>("Handle");

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

        public static void OverwriteHead(Instance asset, BasePart head)
        {
            DataModelMesh currentMesh = head.FindFirstChild<DataModelMesh>("Mesh");
            if (currentMesh != null) currentMesh.Destroy();

            DataModelMesh mesh = (DataModelMesh)asset;
            mesh.Name = "Mesh";
            mesh.Parent = head;

            // Apply Rthro adjustments
            foreach (Vector3Value attachmentOverride in mesh.GetChildrenOfClass<Vector3Value>())
            {
                Attachment attachment = head.FindFirstChild<Attachment>(attachmentOverride.Name);
                if (attachment != null)
                {
                    CFrame cf = attachment.CFrame;
                    attachment.CFrame = new CFrame(attachmentOverride.Value) * (cf - cf.p);
                }
            }

            // Copy any extra instances into the Head.
            foreach (Instance metadata in mesh.GetChildren())
            {
                if (!metadata.IsA("Vector3Value"))
                {
                    metadata.Parent = head;
                }
            }
        }

        public static void BuildAvatarGeometry(StudioMdlWriter meshBuilder, Bone bone)
        {
            string task = "BuildGeometry_" + bone.Node.Name;
            Rbx2Source.ScheduleTasks(task);

            Node node = bone.Node;
            BasePart part = bone.Part1;

            bool isAvatarLimb = bone.IsAvatarBone;
            string materialName;

            if (isAvatarLimb)
            {
                Limb limb = GetLimb(part);
                materialName = Rbx2Source.GetEnumName(limb);
            }
            else
            {
                materialName = part.Name;
            }

            Material material = new Material();
            material.UseAvatarMap = isAvatarLimb;

            Rbx2Source.Print("Building Geometry for {0}",part.Name);
            Rbx2Source.IncrementStack();

            Mesh geometry = Mesh.BakePart(part, material);
            meshBuilder.Materials[materialName] = material;
            
            for (int i = 0; i < geometry.FaceCount; i++)
            {
                Triangle tri = new Triangle();
                tri.Node = node;
                tri.FaceIndex = i;
                tri.Mesh = geometry;
                tri.Material = materialName;

                meshBuilder.Triangles.Add(tri);
            }

            Rbx2Source.DecrementStack();
            Rbx2Source.MarkTaskCompleted(task);
        }

        public static Folder AppendCharacterAssets(UserAvatar avatar, string avatarType, string context = "CHARACTER")
        {
            Rbx2Source.PrintHeader("GATHERING " + context + " ASSETS");

            Folder characterAssets = new Folder();
            List<long> assetIds = avatar.AccessoryVersionIds;

            foreach (long id in assetIds)
            {
                Asset asset = Asset.Get(id, "/asset/?assetversionid=");

                Folder import = RBXM.LoadFromAsset(asset);
                Folder typeSpecific = import.FindFirstChild<Folder>(avatarType);

                if (typeSpecific != null)
                    import = typeSpecific;

                foreach (Instance obj in import.GetChildren())
                    obj.Parent = characterAssets;
            }

            return characterAssets;
        }

        public static Folder AppendCollisionAssets(UserAvatar avatar, string avatarType)
        {
            Folder collisionAssets;

            if (DEBUG_RAPID_ASSEMBLY)
                collisionAssets = new Folder();
            else
                collisionAssets = AppendCharacterAssets(avatar, avatarType, "COLLISION");

            // Replace the head mesh with a low-poly head
            DataModelMesh oldHeadMesh = collisionAssets.FindFirstChildOfClass<DataModelMesh>();

            if (oldHeadMesh != null)
            {
                oldHeadMesh.Destroy();
                oldHeadMesh = null;
            }

            SpecialMesh lowPolyHead = new SpecialMesh();
            lowPolyHead.MeshId = "rbxassetid://582002794";
            lowPolyHead.MeshType = MeshType.FileMesh;
            lowPolyHead.Scale = new Vector3(1, 1, 1);
            lowPolyHead.Offset = new Vector3();
            lowPolyHead.Parent = collisionAssets;

            return collisionAssets;
        }

        public static Asset GetAvatarFace(Folder characterAssets)
        {
            // Check if this avatar is using an Rthro head with a texture overlay.
            Folder assembly = characterAssets.FindFirstChild<Folder>("ASSEMBLY");

            if (assembly != null)
            {
                BasePart head = assembly.FindFirstChild<BasePart>("Head");

                if (head != null)
                {
                    SpecialMesh headMesh = head.FindFirstChildOfClass<SpecialMesh>();

                    if (headMesh != null && headMesh.TextureId != null && headMesh.TextureId.Length > 0)
                    {
                        // One last check to make sure this is *probably* an Rthro head.
                        // The reason this check is necessary is due to the iBot Head, which has a texture and allows a face to be drawn on it.
                        // I suspect Roblox will expand this behavior later, so I need to keep an eye on it.
                        StringValue scaleType = head.FindFirstChild<StringValue>("AvatarPartScaleType");

                        if (scaleType != null && scaleType.Value != "Classic")
                        {
                            return Asset.GetByAssetId(headMesh.TextureId);
                        }
                    }
                }
            }

            // Fall back to normal behavior.
            Decal face = characterAssets.FindFirstChild<Decal>("face");
            Asset result;

            if (face != null && face.Texture != "rbxasset://textures/face.png")
                result = Asset.GetByAssetId(face.Texture);
            else
                result = Asset.FromResource("Images/face.png");

            return result;
        }

        public static float ComputeFloorLevel(Folder assembly)
        {
            BasePart lowest = null;
            float lowestY = float.MaxValue;

            foreach (BasePart part in assembly.GetChildrenOfClass<BasePart>())
            {
                float y = part.Position.Y;
                if (y < lowestY)
                {
                    lowest = part;
                    lowestY = y;
                }
            }

            return (lowestY - (lowest.Size.Y / 2f)) * Rbx2Source.MODEL_SCALE;
        }

        public AssemblerData Assemble(object metadata)
        {
            UserAvatar avatar = metadata as UserAvatar;
            if (avatar == null)
                throw new InvalidDataException("bad cast");

            UserInfo userInfo = avatar.UserInfo;
            string userName = FileUtility.MakeNameWindowsSafe(userInfo.Username);

            string appData = Environment.GetEnvironmentVariable("AppData");
            string rbx2Src = Path.Combine(appData, "Rbx2Source");
            string avatars = Path.Combine(rbx2Src, "Avatars");
            string userBin = Path.Combine(avatars, userName);

            string modelDir = Path.Combine(userBin, "Model");
            string anim8Dir = Path.Combine(modelDir, "Animations");
            string texturesDir = Path.Combine(userBin, "Textures");
            string materialsDir = Path.Combine(userBin, "Materials");

            FileUtility.InitiateEmptyDirectories(modelDir, anim8Dir, texturesDir, materialsDir);

            AvatarType avatarType = avatar.ResolvedAvatarType;
            ICharacterAssembler assembler;

            if (avatarType == AvatarType.R15)
                assembler = new R15CharacterAssembler();
            else
                assembler = new R6CharacterAssembler();

            string avatarTypeName = Rbx2Source.GetEnumName(avatar.ResolvedAvatarType);
            Folder characterAssets = AppendCharacterAssets(avatar, avatarTypeName);
            string compileDirectory = "roblox_avatars/" + userName;

            Rbx2Source.ScheduleTasks("BuildCharacter", "BuildCollisionModel", "BuildAnimations", "BuildTextures", "BuildMaterials", "BuildCompilerScript");

            Rbx2Source.PrintHeader("BUILDING CHARACTER MODEL");
            #region Build Character Model
            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            StudioMdlWriter writer = assembler.AssembleModel(characterAssets, avatar.Scales, DEBUG_RAPID_ASSEMBLY);

            string studioMdl = writer.BuildFile();
            string modelPath = Path.Combine(modelDir, "CharacterModel.smd");
            FileUtility.WriteFile(modelPath, studioMdl);

            string staticPose = writer.BuildFile(false);
            string refPath = Path.Combine(modelDir, "ReferencePos.smd");
            FileUtility.WriteFile(refPath, staticPose);

            Rbx2Source.MarkTaskCompleted("BuildCharacter");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Rbx2Source.PrintHeader("BUILDING COLLISION MODEL");
            #region Build Character Collisions
            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            Folder collisionAssets = AppendCollisionAssets(avatar, avatarTypeName);
            StudioMdlWriter collisionWriter = assembler.AssembleModel(collisionAssets, avatar.Scales, true);

            string collisionModel = collisionWriter.BuildFile();
            string cmodelPath = Path.Combine(modelDir, "CollisionModel.smd");
            FileUtility.WriteFile(cmodelPath, collisionModel); 

            byte[] collisionJoints = assembler.CollisionModelScript;
            string cjointsPath = Path.Combine(modelDir, "CollisionJoints.qc");

            FileUtility.WriteFile(cjointsPath, collisionJoints);
            Rbx2Source.MarkTaskCompleted("BuildCollisionModel");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Rbx2Source.PrintHeader("BUILDING CHARACTER ANIMATIONS");
            #region Build Character Animations
            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            var animIds = assembler.CollectAnimationIds(avatar);
            var compileAnims = new Dictionary<string, Asset>();
                
            if (animIds.Count > 0)
            {
                Rbx2Source.Print("Collecting Animations...");
                Rbx2Source.IncrementStack();

                Action<string, Asset> collectAnimation = (animName, animAsset) =>
                {
                    if (!compileAnims.ContainsKey(animName))
                    {
                        Rbx2Source.Print("Collected animation {0} with id {1}", animName, animAsset.Id);
                        compileAnims.Add(animName, animAsset);
                    }
                };

                foreach (string animName in animIds.Keys)
                {
                    AnimationId animId = animIds[animName];
                    Asset animAsset = animId.GetAsset();
                    Folder import = RBXM.LoadFromAsset(animAsset);

                    if (animId.AnimationType == AnimationType.R15AnimFolder)
                    {
                        Folder r15Anim = import.FindFirstChild<Folder>("R15Anim");
                        if (r15Anim != null)
                        {
                            foreach (Instance animDef in r15Anim.GetChildren())
                            {
                                if (animDef.Name == "idle")
                                {
                                    Animation[] anims = animDef.GetChildrenOfClass<Animation>();
                                    if (anims.Length == 2)
                                    {
                                        Animation lookAnim = anims.OrderBy(anim => anim.Weight).First();
                                        lookAnim.Destroy();

                                        Asset lookAsset = Asset.GetByAssetId(lookAnim.AnimationId);
                                        collectAnimation("Idle2", lookAsset);
                                    }
                                }

                                Animation compileAnim = animDef.FindFirstChildOfClass<Animation>();

                                if (compileAnim != null)
                                {
                                    Asset compileAsset = Asset.GetByAssetId(compileAnim.AnimationId);
                                    collectAnimation(animName, compileAsset);
                                }
                            }
                        }
                    }
                    else
                    {
                        collectAnimation(animName, animAsset);
                    }
                }

                Rbx2Source.DecrementStack();
            }
            else
            {
                Rbx2Source.Print("No animations found :(");
            }

            if (compileAnims.Count > 0)
            {
                Rbx2Source.Print("Assembling Animations...");
                Rbx2Source.IncrementStack();

                foreach (string animName in compileAnims.Keys)
                {
                    Rbx2Source.Print("Building Animation {0}...", animName);

                    Asset animAsset = compileAnims[animName];
                    Folder import = RBXM.LoadFromAsset(animAsset);

                    KeyframeSequence sequence = import.FindFirstChildOfClass<KeyframeSequence>();
                    sequence.AvatarType = avatar.ResolvedAvatarType;
                    sequence.Name = animName;

                    string animation = AnimationAssembler.Assemble(sequence, writer.Skeleton[0].Bones);
                    string animPath = Path.Combine(anim8Dir, animName + ".smd");

                    FileUtility.WriteFile(animPath, animation);
                }
            }

            Rbx2Source.MarkTaskCompleted("BuildAnimations");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Rbx2Source.PrintHeader("BUILDING CHARACTER TEXTURES");
            #region Build Character Textures
            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            Dictionary<string, Material> materials = writer.Materials;

            TextureAssembly texAssembly;

            if (DEBUG_RAPID_ASSEMBLY)
            {
                texAssembly = new TextureAssembly();
                materials.Clear();
            }
            else
            {
                TextureCompositor texCompositor = assembler.ComposeTextureMap(characterAssets, avatar.BodyColors);
                texAssembly = assembler.AssembleTextures(texCompositor, materials);
            }

            var images = texAssembly.Images;
            texAssembly.MaterialDirectory = compileDirectory;

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

            CompositData.FreeAllocatedTextures();
            Rbx2Source.MarkTaskCompleted("BuildTextures");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Rbx2Source.PrintHeader("WRITING MATERIAL FILES");
            #region Write Material Files
            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            var matLinks = texAssembly.MatLinks;

            foreach (string mtlName in matLinks.Keys)
            {
                Rbx2Source.Print("Building VMT {0}.vmt", mtlName);

                string targetVtf = matLinks[mtlName];
                string vmtPath = Path.Combine(materialsDir, mtlName + ".vmt");

                Material mtl = materials[mtlName];
                mtl.SetVmtField("basetexture", "models/" + compileDirectory + "/" + targetVtf);
                mtl.WriteVmtFile(vmtPath);
            }

            Rbx2Source.MarkTaskCompleted("BuildMaterials");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Rbx2Source.PrintHeader("WRITING COMPILER SCRIPT");
            #region Write Compiler Script
            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            QCWriter qc = new QCWriter();

            QCommand model = new QCommand("body", userName, "CharacterModel.smd");
            qc.AddCommand(model);

            string modelNameStr = compileDirectory + ".mdl";
            qc.WriteBasicCmd("modelname", modelNameStr);
            qc.WriteBasicCmd("upaxis", "y");

            // Compute the floor level of the avatar.
            Folder assembly = characterAssets.FindFirstChild<Folder>("ASSEMBLY");

            if (assembly != null)
            {
                float floor = ComputeFloorLevel(assembly);
                string originStr = "0 " + floor.ToString(Rbx2Source.NormalParse) + " 0";
                qc.WriteBasicCmd("origin", originStr, false);
            }

            qc.WriteBasicCmd("cdmaterials", "models/" + compileDirectory);
            qc.WriteBasicCmd("surfaceprop", "flesh");
            qc.WriteBasicCmd("include", "CollisionJoints.qc");

            QCommand reference = new QCommand("sequence", "reference", "ReferencePos.smd");
            reference.AddParameter("fps", "1");
            reference.AddParameter("loop");

            qc.AddCommand(reference);

            foreach (string animName in compileAnims.Keys)
            {
                QCommand sequence = new QCommand("sequence", animName.ToLower(), "Animations/" + animName + ".smd");
                sequence.AddParameter("fps", AnimationAssembler.FrameRate);
                sequence.AddParameter("loop");

                if (avatarType == AvatarType.R6)
                    sequence.AddParameter("delta");

                qc.AddCommand(sequence);
            }

            string qcFile = qc.BuildFile();
            string qcPath = Path.Combine(modelDir, "Compile.qc");

            FileUtility.WriteFile(qcPath, qcFile);
            Rbx2Source.MarkTaskCompleted("BuildCompilerScript");

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            AssemblerData data = new AssemblerData()
            {
                ModelData = writer,
                TextureData = texAssembly,
                CompilerScript = qcPath,
                RootDirectory = userBin,
                MaterialDirectory = materialsDir,
                TextureDirectory = compileDirectory,
                ModelName = modelNameStr
            };

            return data;
        }
    }
}
