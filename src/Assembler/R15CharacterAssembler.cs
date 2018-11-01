using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Rbx2Source.Coordinates;
using Rbx2Source.Reflection;
using Rbx2Source.Resources;
using Rbx2Source.StudioMdl;
using Rbx2Source.Textures;
using Rbx2Source.Web;

namespace Rbx2Source.Assembler
{
    class R15CharacterAssembler : CharacterAssembler, ICharacterAssembler
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        // TEXTURE COMPOSITION CONSTANTS
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static string     COMPOSIT_TORSO      = "R15CompositTorsoBase";
        private static string     COMPOSIT_LEFT_LIMB  = "R15CompositLeftArmBase";
        private static string     COMPOSIT_RIGHT_LIMB = "R15CompositRightArmBase";

        private static Rectangle  RECT_HEAD           =  new Rectangle ( 240, 272, 256, 296 );
        private static Rectangle  RECT_TORSO          =  new Rectangle (   0,   0, 388, 272 );
        private static Rectangle  RECT_LEFT_ARM       =  new Rectangle ( 496,   0, 264, 284 );
        private static Rectangle  RECT_LEFT_LEG       =  new Rectangle ( 496, 284, 264, 284 );
        private static Rectangle  RECT_RIGHT_ARM      =  new Rectangle ( 760,   0, 264, 284 );
        private static Rectangle  RECT_RIGHT_LEG      =  new Rectangle ( 760, 284, 264, 284 );
        private static Rectangle  RECT_TSHIRT         =  new Rectangle (   2,  74, 128, 128 );

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        // RTHRO CONSTANTS 
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        // Controls how much each limb should be scaled up based on the BodyTypeScale value
        private static Dictionary<string, Vector3> BODY_TYPE_SCALES = new Dictionary<string, Vector3>()
        {
            { "LeftHand",      new Vector3(1.066f, 1.151f, 1.231f) },
            { "LeftLowerArm",  new Vector3(1.129f, 1.315f, 1.132f) },
            { "LeftUpperArm",  new Vector3(1.129f, 1.315f, 1.132f) },
            { "RightHand",     new Vector3(1.066f, 1.151f, 1.231f) },
            { "RightLowerArm", new Vector3(1.129f, 1.315f, 1.132f) },
            { "RightUpperArm", new Vector3(1.129f, 1.315f, 1.132f) },
            { "UpperTorso",    new Vector3(1.033f, 1.283f, 1.140f) },
            { "LeftFoot",      new Vector3(1.079f, 1.242f, 1.129f) },
            { "LeftLowerLeg",  new Vector3(1.023f, 1.476f, 1.023f) },
            { "LeftUpperLeg",  new Vector3(1.023f, 1.476f, 1.023f) },
            { "RightFoot",     new Vector3(1.079f, 1.242f, 1.129f) },
            { "RightLowerLeg", new Vector3(1.023f, 1.476f, 1.023f) },
            { "RightUpperLeg", new Vector3(1.023f, 1.476f, 1.023f) },
            { "LowerTorso",    new Vector3(1.033f, 1.283f, 1.140f) },
            { "Head",          new Vector3(0.942f, 0.942f, 0.942f) },
        };

        // Alternative proportions for rthro, blended between BODY_TYPE_SCALES using the BodyProportionScale value
        private static Dictionary<string, Vector3> BODY_PROPORTION_SCALES = new Dictionary<string, Vector3>()
        {
            { "LeftHand",      new Vector3(0.948f, 1.151f, 1.094f) },
            { "LeftLowerArm",  new Vector3(1.004f, 1.184f, 1.006f) },
            { "LeftUpperArm",  new Vector3(1.004f, 1.184f, 1.006f) },
            { "RightHand",     new Vector3(0.948f, 1.151f, 1.094f) },
            { "RightLowerArm", new Vector3(1.004f, 1.184f, 1.006f) },
            { "RightUpperArm", new Vector3(1.004f, 1.184f, 1.006f) },
            { "UpperTorso",    new Vector3(0.905f, 1.180f, 1.013f) },
            { "LeftFoot",      new Vector3(1.030f, 1.111f, 1.004f) },
            { "LeftLowerLeg",  new Vector3(0.976f, 1.275f, 0.909f) },
            { "LeftUpperLeg",  new Vector3(0.976f, 1.373f, 0.909f) },
            { "RightFoot",     new Vector3(1.030f, 1.111f, 1.004f) },
            { "RightLowerLeg", new Vector3(0.976f, 1.275f, 0.909f) },
            { "RightUpperLeg", new Vector3(0.976f, 1.373f, 0.909f) },
            { "LowerTorso",    new Vector3(0.986f, 0.985f, 1.013f) },
            { "Head",          new Vector3(0.896f, 0.942f, 0.896f) },
        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Asset R15AssemblyAsset = Asset.FromResource("AvatarData/R15/ASSEMBLY.rbxmx");
        public byte[] CollisionModelScript => ResourceUtility.GetResource("AvatarData/R15/CollisionJoints.qc");

        private static Dictionary<Limb, Rectangle> UVCrops = new Dictionary<Limb, Rectangle>()
        {
            { Limb.Head,     RECT_HEAD      },
            { Limb.Torso,    RECT_TORSO     },
            { Limb.LeftArm,  RECT_LEFT_ARM  },
            { Limb.LeftLeg,  RECT_LEFT_LEG  },
            { Limb.RightArm, RECT_RIGHT_ARM },
            { Limb.RightLeg, RECT_RIGHT_LEG },
        };

        private static Dictionary<string, long> R15_ANIMATION_IDS = new Dictionary<string, long>()
        {
            { "Climb", 507765644  },
            { "Fall",  507767968  },
            { "Idle",  507766388  },
            { "Jump",  507765000  },
            { "Idle2", 507766666  },
            { "Run",   913376220  },
            { "Walk",  913402848  },
            { "Sit",   2506281703 },
            { "Wave",  507770239  },
            { "Point", 507770453  },
            { "Cheer", 507770677  },
            { "Swim",  913384386  },
            { "Float", 913389285  },
        };

        public static Vector3 ComputeLimbScale(AvatarScale avatarScale, BasePart part)
        {
            string limbName = part.Name;

            // Foundational scale
            Vector3 scale = new Vector3(avatarScale.Width, avatarScale.Height, avatarScale.Depth);
            
            // Sample the target body scale from the BODY_TYPE_SCALES lookup table.
            Vector3 bodyScale = new Vector3(1, 1, 1);
            if (BODY_TYPE_SCALES.ContainsKey(limbName))
                bodyScale = BODY_TYPE_SCALES[limbName];

            // Sample the target proportions scale from the BODY_PROPORTION_SCALES lookup table.
            Vector3 propScale = new Vector3(1, 1, 1);
            if (BODY_PROPORTION_SCALES.ContainsKey(limbName))
                propScale = BODY_PROPORTION_SCALES[limbName];

            // The target proportions are determined by blending between bodyScale and propScale
            // using the value of the Humanoid's BodyProportionScale value.
            Vector3 rthroTarget = bodyScale.Lerp(propScale, avatarScale.Proportion);

            // Now we compute how much the rthroTarget value will be applied to the limb based on the
            // value of the humanoid's BodyTypeScale value.
            Vector3 rthroScale = new Vector3(1, 1, 1);
            rthroScale = rthroScale.Lerp(rthroTarget, avatarScale.BodyType);

            // If there is a StringValue named AvatarPartScaleType, then this limb is being scaled 
            // relative to one of the scale lookup tables. In other words, its original size was 
            // already scaled to an Rthro configuration, so we need to tune the scale accordingly.
            StringValue avatarScaleType = part.FindFirstChild<StringValue>("AvatarPartScaleType");

            if (avatarScaleType != null)
            {
                Vector3 rthroBase = new Vector3(1, 1, 1);

                if (avatarScaleType.Value == "ProportionsNormal")
                    rthroBase = bodyScale;
                else if (avatarScaleType.Value == "ProportionsSlender")
                    rthroBase = propScale;

                rthroScale /= rthroBase;
            }

            return scale * rthroScale;
        }

        public Dictionary<string, AnimationId> CollectAnimationIds(UserAvatar avatar)
        {
            var animIds = new Dictionary<string, AnimationId>();
            var userAnims = avatar.Animations;

            foreach (string animName in R15_ANIMATION_IDS.Keys)
            {
                AnimationId animId = new AnimationId();

                if (userAnims.ContainsKey(animName))
                {
                    animId.AnimationType = AnimationType.R15AnimFolder;
                    animId.AssetId = userAnims[animName];
                }
                else
                {
                    animId.AnimationType = AnimationType.KeyframeSequence;
                    animId.AssetId = R15_ANIMATION_IDS[animName];
                }

                animIds.Add(animName, animId);
            }

            // Some user animations are bundled together, so they will be handled differently if they are in the user's animation table.
            if (userAnims.ContainsKey("Swim") && animIds.ContainsKey("Float"))
                animIds.Remove("Float"); // Remove default swimidle

            if (userAnims.ContainsKey("Idle") && animIds.ContainsKey("Idle2"))
                animIds.Remove("Idle2"); // Remove default lookaround

            return animIds;
        }

        public StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale, bool collisionModel = false)
        {
            StudioMdlWriter meshBuilder = new StudioMdlWriter();

            // Build Character
            Folder import = RBXM.LoadFromAsset(R15AssemblyAsset);
            Folder assembly = import.FindFirstChild<Folder>("ASSEMBLY");
            assembly.Parent = characterAssets;

            BasePart head = assembly.FindFirstChild<BasePart>("Head");

            foreach (Instance asset in characterAssets.GetChildren())
            {
                if (asset.IsA("BasePart"))
                {
                    BasePart existing = assembly.FindFirstChild<BasePart>(asset.Name);
                    if (existing != null)
                        existing.Destroy();

                    asset.Parent = assembly;
                }
                else if (asset.IsA("Folder") && asset.Name == "R15ArtistIntent")
                {
                    foreach (BasePart child in asset.GetChildrenOfClass<BasePart>())
                    {
                        BasePart existing = assembly.FindFirstChild<BasePart>(child.Name);
                        if (existing != null)
                            existing.Destroy();

                        child.Parent = assembly;
                    }
                }
                else if (asset.IsA("Accoutrement") && !collisionModel)
                {
                    PrepareAccessory(asset, assembly);
                } 
                else if (asset.IsA("DataModelMesh"))
                {
                    OverwriteHead(asset, head);
                }
            }

            // Apply limb scaling
            foreach (BasePart part in assembly.GetChildrenOfClass<BasePart>())
            {
                Limb limb = GetLimb(part);

                if (limb != Limb.Unknown)
                {
                    Vector3 limbScale = ComputeLimbScale(scale, part);
                    part.Size *= limbScale;

                    foreach (Attachment attachment in part.GetChildrenOfClass<Attachment>())
                        attachment.CFrame = CFrame.Scale(attachment.CFrame, limbScale);
                }
            }

            BasePart torso = assembly.FindFirstChild<BasePart>("LowerTorso");
            torso.CFrame = new CFrame();

            BoneKeyframe keyframe = AssembleBones(meshBuilder, torso);
            List<Bone> bones = keyframe.Bones;

            // Build File Data.
            Rbx2Source.Print("Building Geometry...");
            Rbx2Source.IncrementStack();

            foreach (Bone bone in bones)
                BuildAvatarGeometry(meshBuilder, bone);

            Rbx2Source.DecrementStack();
            return meshBuilder;
        }

        public TextureCompositor ComposeTextureMap(Folder characterAssets, BodyColors bodyColors)
        {
            TextureCompositor compositor = new TextureCompositor(AvatarType.R15, 1024, 568);

            // Append BodyColors
            compositor.AppendColor(bodyColors.HeadColor,     RECT_HEAD);
            compositor.AppendColor(bodyColors.TorsoColor,    RECT_TORSO);
            compositor.AppendColor(bodyColors.LeftArmColor,  RECT_LEFT_ARM);
            compositor.AppendColor(bodyColors.LeftLegColor,  RECT_LEFT_LEG);
            compositor.AppendColor(bodyColors.RightArmColor, RECT_RIGHT_ARM);
            compositor.AppendColor(bodyColors.RightLegColor, RECT_RIGHT_LEG);

            // Append Face
            Asset face = GetAvatarFace(characterAssets);
            compositor.AppendTexture(face, RECT_HEAD, 1);

            // Append Shirt
            Shirt shirt = characterAssets.FindFirstChildOfClass<Shirt>();
            if (shirt != null)
            {
                Asset shirtTemplate = Asset.GetByAssetId(shirt.ShirtTemplate);
                compositor.AppendTexture(shirtTemplate, COMPOSIT_TORSO,      RECT_TORSO,     2);
                compositor.AppendTexture(shirtTemplate, COMPOSIT_LEFT_LIMB,  RECT_LEFT_ARM,  1);
                compositor.AppendTexture(shirtTemplate, COMPOSIT_RIGHT_LIMB, RECT_RIGHT_ARM, 1);
            }

            // Append Pants
            Pants pants = characterAssets.FindFirstChildOfClass<Pants>();
            if (pants != null)
            {
                Asset pantsTemplate = Asset.GetByAssetId(pants.PantsTemplate);
                compositor.AppendTexture(pantsTemplate, COMPOSIT_TORSO,      RECT_TORSO,     1);
                compositor.AppendTexture(pantsTemplate, COMPOSIT_LEFT_LIMB,  RECT_LEFT_LEG,  1);
                compositor.AppendTexture(pantsTemplate, COMPOSIT_RIGHT_LIMB, RECT_RIGHT_LEG, 1);
            }

            // Append T-Shirt
            ShirtGraphic tshirt = characterAssets.FindFirstChildOfClass<ShirtGraphic>();
            if (tshirt != null)
            {
                Asset graphic = Asset.GetByAssetId(tshirt.Graphic);
                compositor.AppendTexture(graphic, RECT_TSHIRT, 4);
            }

            // Append Package Overlays
            Folder avatarParts = characterAssets.FindFirstChild<Folder>("ASSEMBLY");
            List<Limb> overlainLimbs = new List<Limb>();

            foreach (MeshPart part in avatarParts.GetChildrenOfClass<MeshPart>())
            {
                Limb limb = GetLimb(part);
                string textureId = part.TextureId;

                if (textureId != null && textureId.Length > 0 && !overlainLimbs.Contains(limb))
                {
                    Asset overlay = Asset.GetByAssetId(textureId);
                    Rectangle crop = UVCrops[limb];
                    compositor.AppendTexture(overlay, crop, 3);
                    overlainLimbs.Add(limb);
                }
            }

            return compositor;
        }

        public TextureAssembly AssembleTextures(TextureCompositor compositor, Dictionary<string, Material> materials)
        {
            TextureAssembly assembly = new TextureAssembly();
            assembly.Images = new Dictionary<string, Image>();
            assembly.MatLinks = new Dictionary<string, string>();

            Bitmap uvMap = compositor.BakeTextureMap();
            Rbx2Source.SetDebugImage(uvMap);

            foreach (string materialName in materials.Keys)
            {
                Rbx2Source.Print("Building Material {0}", materialName);
                Material material = materials[materialName];
                Image image = null;

                if (material.UseAvatarMap)
                {
                    Limb limb;
                    if (Enum.TryParse(materialName, out limb))
                    {
                        Rectangle cropRegion = UVCrops[limb];
                        image = TextureCompositor.CropBitmap(uvMap, cropRegion);
                    }
                }
                else
                {
                    Asset texture = material.TextureAsset;
                    if (texture != null)
                    {
                        byte[] textureData = texture.GetContent();
                        MemoryStream textureStream = new MemoryStream(textureData);
                        image = Image.FromStream(textureStream);
                    }
                }

                if (image != null)
                    assembly.LinkDirectly(materialName, image);
                else
                    Rbx2Source.Print("Missing Image for Material {0}?", materialName);

            }

            return assembly;
        }
    }
}
