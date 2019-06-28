using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using Rbx2Source.Animating;
using Rbx2Source.DataTypes;
using Rbx2Source.Reflection;
using Rbx2Source.Resources;
using Rbx2Source.StudioMdl;
using Rbx2Source.Textures;
using Rbx2Source.Web;

namespace Rbx2Source.Assembler
{
    public class R15CharacterAssembler : CharacterAssembler, ICharacterAssembler
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
        
        // Scale R15 -> Rthro Normal
        private static AvatarScaleRules R15_TO_RTHRO_NORMAL = new AvatarScaleRules()
        {
            Head          = new Vector3(0.942f, 0.942f, 0.942f),
            UpperTorso    = new Vector3(1.033f, 1.310f, 1.140f),
            LowerTorso    = new Vector3(1.033f, 1.309f, 1.140f),

            LeftUpperArm  = new Vector3(1.129f, 1.342f, 1.132f),
            LeftLowerArm  = new Vector3(1.129f, 1.342f, 1.132f),
            LeftHand      = new Vector3(1.066f, 1.174f, 1.231f),

            RightUpperArm = new Vector3(1.129f, 1.342f, 1.132f),
            RightLowerArm = new Vector3(1.129f, 1.342f, 1.132f),
            RightHand     = new Vector3(1.066f, 1.174f, 1.231f),
            
            LeftUpperLeg  = new Vector3(1.023f, 1.506f, 1.023f),
            LeftLowerLeg  = new Vector3(1.023f, 1.506f, 1.023f),
            LeftFoot      = new Vector3(1.079f, 1.267f, 1.129f),

            RightUpperLeg = new Vector3(1.023f, 1.506f, 1.023f),
            RightLowerLeg = new Vector3(1.023f, 1.506f, 1.023f),
            RightFoot     = new Vector3(1.079f, 1.267f, 1.129f),
        };

        // Scale R15 -> Rthro Slender
        private static AvatarScaleRules R15_TO_RTHRO_SLENDER = new AvatarScaleRules()
        {
            Head          = new Vector3(0.896f, 0.942f, 0.896f),
            UpperTorso    = new Vector3(0.905f, 1.204f, 1.013f),
            LowerTorso    = new Vector3(0.986f, 1.004f, 1.013f),

            LeftUpperArm  = new Vector3(1.004f, 1.207f, 1.006f),
            LeftLowerArm  = new Vector3(1.004f, 1.207f, 1.006f),
            LeftHand      = new Vector3(0.948f, 1.174f, 1.094f),
            
            RightUpperArm = new Vector3(1.004f, 1.208f, 1.006f),
            RightLowerArm = new Vector3(1.004f, 1.208f, 1.006f),
            RightHand     = new Vector3(0.948f, 1.174f, 1.094f),
            
            LeftUpperLeg  = new Vector3(0.976f, 1.401f, 0.909f),
            LeftLowerLeg  = new Vector3(0.976f, 1.301f, 0.909f),
            LeftFoot      = new Vector3(1.030f, 1.133f, 1.004f),
            
            RightUpperLeg = new Vector3(0.976f, 1.401f, 0.909f),
            RightLowerLeg = new Vector3(0.976f, 1.301f, 0.909f),
            RightFoot     = new Vector3(1.030f, 1.133f, 1.004f),
        };

        // Scale Rthro Normal -> R15
        private static AvatarScaleRules RTHRO_NORMAL_TO_R15 = new AvatarScaleRules()
        {
            Head          = new Vector3(1.600f, 1.600f, 1.600f),
            UpperTorso    = new Vector3(1.014f, 0.814f, 0.924f),
            LowerTorso    = new Vector3(1.014f, 0.814f, 0.924f),

            LeftUpperArm  = new Vector3(1.121f, 0.681f, 0.968f),
            LeftLowerArm  = new Vector3(1.121f, 0.681f, 0.968f),
            LeftHand      = new Vector3(1.390f, 0.967f, 1.201f),

            RightUpperArm = new Vector3(1.121f, 0.681f, 0.968f),
            RightLowerArm = new Vector3(1.121f, 0.681f, 0.968f),
            RightHand     = new Vector3(1.390f, 0.967f, 1.201f),
            
            LeftUpperLeg  = new Vector3(0.978f, 0.814f, 1.056f),
            LeftLowerLeg  = new Vector3(0.978f, 0.814f, 1.056f),
            LeftFoot      = new Vector3(1.404f, 0.953f, 0.931f),

            RightUpperLeg = new Vector3(0.978f, 0.814f, 1.056f),
            RightLowerLeg = new Vector3(0.978f, 0.814f, 1.056f),
            RightFoot     = new Vector3(1.404f, 0.953f, 0.931f),
        };

        // Scale Rthro Slender -> R15
        private static AvatarScaleRules RTHRO_SLENDER_TO_R15 = new AvatarScaleRules()
        {
            Head          = new Vector3(1.600f, 1.600f, 1.600f),
            UpperTorso    = new Vector3(1.156f, 0.885f, 1.039f),
            LowerTorso    = new Vector3(1.063f, 1.061f, 1.040f),

            LeftUpperArm  = new Vector3(1.261f, 0.756f, 1.089f),
            LeftLowerArm  = new Vector3(1.261f, 0.756f, 1.089f),
            LeftHand      = new Vector3(1.564f, 0.967f, 1.351f),

            RightUpperArm = new Vector3(1.261f, 0.756f, 1.089f),
            RightLowerArm = new Vector3(1.261f, 0.756f, 1.089f),
            RightHand     = new Vector3(1.564f, 0.967f, 1.351f),

            LeftUpperLeg  = new Vector3(1.025f, 0.875f, 1.188f),
            LeftLowerLeg  = new Vector3(1.025f, 0.943f, 1.188f),
            LeftFoot      = new Vector3(1.471f, 1.065f, 1.047f),
            
            RightUpperLeg = new Vector3(1.025f, 0.875f, 1.188f),
            RightLowerLeg = new Vector3(1.025f, 0.943f, 1.188f),
            RightFoot     = new Vector3(1.471f, 1.065f, 1.047f),
        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Asset R15AssemblyAsset = Asset.FromResource("AvatarData/R15/CharacterBase.rbxm");
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
            { "Jump",  507765000  },
            { "Fall",  507767968  },
            { "Idle",  507766388  },
            { "Idle2", 507766666  },
            { "Laugh", 507770818  },
            { "Pose",  532421348  },
            { "Run",   913376220  },
            { "Walk",  913402848  },
            { "Point", 507770453  },
            { "Sit",   2506281703 },
        };

        public static string GetAvatarPartScaleType(BasePart part)
        {
            StringValue scaleType = part.FindFirstChild<StringValue>("AvatarPartScaleType");
            string result = "Classic";

            if (scaleType != null)
                result = scaleType.Value;

            return result.Replace("Proportions", "");
        }

        public static void SampleScales(BasePart limb, string scaleType, out Vector3 scaleR15, out Vector3 scaleNormal, out Vector3 scaleSlender)
        {
            string limbName = limb.Name;
            Vector3 sampleNoChange = new Vector3(1, 1, 1);

            // Sample the scaling tables
            Vector3 sampleR15ToNormal = R15_TO_RTHRO_NORMAL[limbName];
            Vector3 sampleNormalToR15 = RTHRO_NORMAL_TO_R15[limbName];

            Vector3 sampleR15ToSlender = R15_TO_RTHRO_SLENDER[limbName];
            Vector3 sampleSlenderToR15 = RTHRO_SLENDER_TO_R15[limbName];

            Vector3 sampleNormalToSlender = (sampleR15ToNormal / sampleR15ToSlender);
            Vector3 sampleSlenderToNormal = (sampleR15ToSlender / sampleR15ToNormal);

            if (scaleType == "Normal")
            {
                scaleR15 = sampleNormalToR15;
                scaleNormal = sampleNoChange;
                scaleSlender = sampleNormalToSlender;
            }
            else if (scaleType == "Slender")
            {
                scaleR15 = sampleSlenderToR15;
                scaleNormal = sampleSlenderToNormal;
                scaleSlender = sampleNoChange;
            }
            else
            {
                scaleR15 = sampleNoChange;
                scaleNormal = sampleR15ToNormal;
                scaleSlender = sampleR15ToSlender;
            }
        }

        public static Vector3 ComputeLimbScale(AvatarScale avatarScale, BasePart part)
        {
            Vector3 sampleNoChange = new Vector3(1, 1, 1);

            string limbName = part.Name;
            Limb limb = GetLimb(part);

            if (limb == Limb.Unknown)
                return sampleNoChange;

            // Compute the base scale
            Vector3 baseScale = new Vector3(avatarScale.Width, avatarScale.Height, avatarScale.Depth);

            // Determine the AvatarPartScaleType for this part.
            string scaleType = GetAvatarPartScaleType(part);

            // Select the scales we will interpolate.
            Vector3 scaleR15, scaleNormal, scaleSlender;

            SampleScales
            (
                part,
                scaleType,

                out scaleR15, 
                out scaleNormal, 
                out scaleSlender
            );

            // Compute the Rthro scaling based on the current proportions and body-type.
            Vector3 scaleProportions = scaleNormal.Lerp(scaleSlender, avatarScale.Proportion);
            Vector3 scaleBodyType = scaleR15.Lerp(scaleProportions, avatarScale.BodyType);

            Vector3 result = scaleBodyType;

            if (limbName == "Head")
                result *= avatarScale.Head;
            else
                result *= baseScale;

            return result;
        }
        
        public static Vector3 ComputeAccessoryScale(AvatarScale scale, BasePart limb, BasePart handle)
        {
            string limbScaleType = GetAvatarPartScaleType(limb);
            string handleScaleType = GetAvatarPartScaleType(handle);

            Vector3 limbScale = ComputeLimbScale(scale, limb);
            if (limbScaleType == handleScaleType)
                return limbScale;

            // Okay so... the goal here is to figure out what the scale of this accessory would be when
            // the limb is at its original size, and then multiply it by the current scale of the limb.

            Vector3 limbR15, limbNormal, limbSlender,
                    handleR15, handleNormal, handleSlender;

            SampleScales
            (
                limb,
                limbScaleType,

                out limbR15,
                out limbNormal,
                out limbSlender
            );

            SampleScales
            (
                limb,
                handleScaleType,

                out handleR15,
                out handleNormal,
                out handleSlender
            );

            Vector3 originScale;

            if (handleScaleType == "Normal")
                originScale = handleNormal / limbNormal;
            else if (handleScaleType == "Slender")
                originScale = handleSlender / limbSlender;
            else
                originScale = handleR15 / limbR15;

            return originScale * limbScale;
        }


        public static void ScalePart(BasePart part, Vector3 scale)
        {
            foreach (Attachment att in part.GetChildrenOfClass<Attachment>())
            {
                CFrame cf = att.CFrame;
                Vector3 pos = cf.Position;
                att.CFrame = new CFrame(pos * scale) * (cf - pos);
            }

            foreach (SpecialMesh mesh in part.GetChildrenOfClass<SpecialMesh>())
                mesh.Scale *= scale;
            
            part.Size *= scale;
        }

        public Dictionary<string, AnimationId> CollectAnimationIds(UserAvatar avatar)
        {
            var userAnims = avatar.Assets
                .Where(asset => AssetGroups.IsTypeInGroup(asset.Type, AssetGroup.Animations))
                .ToDictionary(asset => Rbx2Source.GetEnumName(asset.Type).Replace("Animation", ""));

            var animIds = new Dictionary<string, AnimationId>();

            foreach (string animName in R15_ANIMATION_IDS.Keys)
            {
                AnimationId animId = new AnimationId();

                if (userAnims.ContainsKey(animName))
                {
                    animId.AnimationType = AnimationType.R15AnimFolder;
                    animId.AssetId = userAnims[animName].Id;
                }
                else
                {
                    animId.AnimationType = AnimationType.KeyframeSequence;
                    animId.AssetId = R15_ANIMATION_IDS[animName];
                }

                animIds.Add(animName, animId);
            }

            if (userAnims.ContainsKey("Idle"))
            {
                // Remove default lookaround
                if (animIds.ContainsKey("Idle2"))
                    animIds.Remove("Idle2");

                // If this isn't rthro idle...
                long animId = userAnims["Idle"].Id;

                if (animId != 2510235063)
                {
                    // Remove default pose
                    if (animIds.ContainsKey("Pose"))
                        animIds.Remove("Pose");

                    // Append the pose animation
                    AnimationId pose = new AnimationId()
                    {
                        AnimationType = AnimationType.R15AnimFolder,
                        AssetId = animId
                    };

                    animIds.Add("Pose", pose);
                }
            }

            return animIds;
        }

        public StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale, bool collisionModel = false)
        {
            StudioMdlWriter meshBuilder = new StudioMdlWriter();

            // Build Character
            Folder import = RBXM.LoadFromAsset(R15AssemblyAsset);
            Folder assembly = import.FindFirstChild<Folder>("ASSEMBLY");

            BasePart head = assembly.FindFirstChild<BasePart>("Head");
            assembly.Parent = characterAssets;
            
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
            var parts = assembly.GetChildrenOfClass<BasePart>();
            var attachMap = new Dictionary<string, Attachment>();

            var avatarParts = parts.Where(part => GetLimb(part) != Limb.Unknown);
            var accessoryParts = parts.Except(avatarParts);
            
            foreach (BasePart avatarPart in avatarParts)
            {
                Vector3 limbScale = ComputeLimbScale(scale, avatarPart);

                foreach (Attachment att in avatarPart.GetChildrenOfClass<Attachment>())
                    attachMap[att.Name] = att;

                ScalePart(avatarPart, limbScale);
            }

            // Apply accessory scaling
            foreach (BasePart handle in accessoryParts)
            {
                Attachment handleAtt = handle.FindFirstChildOfClass<Attachment>();
                
                if (handleAtt != null)
                {
                    string attName = handleAtt.Name;
                    
                    if (attachMap.ContainsKey(attName))
                    {
                        Attachment avatarAtt = attachMap[attName];
                        BasePart avatarPart = avatarAtt.Parent as BasePart;

                        Vector3 accessoryScale = ComputeAccessoryScale(scale, avatarPart, handle);
                        ScalePart(handle, accessoryScale);
                    }
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
            compositor.AppendColor(bodyColors.HeadColorId,     RECT_HEAD);
            compositor.AppendColor(bodyColors.TorsoColorId,    RECT_TORSO);
            compositor.AppendColor(bodyColors.LeftArmColorId,  RECT_LEFT_ARM);
            compositor.AppendColor(bodyColors.LeftLegColorId,  RECT_LEFT_LEG);
            compositor.AppendColor(bodyColors.RightArmColorId, RECT_RIGHT_ARM);
            compositor.AppendColor(bodyColors.RightLegColorId, RECT_RIGHT_LEG);

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

        public TextureBindings BindTextures(TextureCompositor compositor, Dictionary<string, Material> materials)
        {
            TextureBindings textureBinds = new TextureBindings();
            Bitmap uvMap = compositor.BakeTextureMap();

            Rbx2Source.SetDebugImage(uvMap);

            foreach (string matName in materials.Keys)
            {
                Rbx2Source.Print("Building Material {0}", matName);
                Material material = materials[matName];
                Image image = null;

                if (material.UseAvatarMap)
                {
                    Limb limb;

                    if (Enum.TryParse(matName, out limb))
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
                        textureStream.Dispose();
                    }
                }

                textureBinds.BindTexture(matName, image);
            }

            return textureBinds;
        }
    }
}
