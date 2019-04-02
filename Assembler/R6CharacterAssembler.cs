using System.Collections.Generic;
using System.Drawing;
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
    public class R6CharacterAssembler : CharacterAssembler, ICharacterAssembler
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        // TEXTURE COMPOSITION CONSTANTS
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static string COMPOSIT_TORSO     = "CompositTorso";
        private static string COMPOSIT_LEFT_ARM  = "CompositLeftArm";
        private static string COMPOSIT_LEFT_LEG  = "CompositLeftLeg";
        private static string COMPOSIT_RIGHT_ARM = "CompositRightArm";
        private static string COMPOSIT_RIGHT_LEG = "CompositRightLeg";

        private static string COMPOSIT_SHIRT     = "CompositShirtTemplate";
        private static string COMPOSIT_PANTS     = "CompositPantsTemplate";

        private static Rectangle RECT_FULL     = new Rectangle(  0,   0, 1024, 768);
        private static Rectangle RECT_HEAD     = new Rectangle(400,   0,  200, 200);
        private static Rectangle RECT_BODY     = new Rectangle(  0, 256, 1024, 512);
        private static Rectangle RECT_ITEM     = new Rectangle(  0,   0,  512, 512);

        private static Rectangle RECT_TSHIRT   = new Rectangle( 32, 321,  128, 128);

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        public byte[] CollisionModelScript => ResourceUtility.GetResource("AvatarData/R6/CollisionJoints.qc");
        private static Asset R6AssemblyAsset = Asset.FromResource("AvatarData/R6/ASSEMBLY.rbxmx");

        private static Dictionary<Limb, string> LimbMatcher = new Dictionary<Limb, string>()
        {
            {Limb.Head,     "Head"},
            {Limb.LeftArm,  "Left Arm"},
            {Limb.RightArm, "Right Arm"},
            {Limb.LeftLeg,  "Left Leg"},
            {Limb.RightLeg, "Right Leg"},
            {Limb.Torso,    "Torso"}
        };

        private static Dictionary<string, long> R6_ANIMATION_IDS = new Dictionary<string, long>()
        {
            {"Climb", 180436334},
            {"Fall",  180436148},
            {"Idle",  180435571},
            {"Jump",  125750702},
            {"Look",  180435792},
            {"Run",   180426354},
            {"Sit",   178130996},
            {"Wave",  128777973},
            {"Point", 128853357},
        };

        private static string GetBodyMatName(long id)
        {
            return id == 0 ? "Body" : "PackageOverlay" + id;
        }

        public Dictionary<string, AnimationId> CollectAnimationIds(UserAvatar avatar)
        {
            var animIds = new Dictionary<string, AnimationId>();

            foreach (string animName in R6_ANIMATION_IDS.Keys)
            {
                AnimationId animId = new AnimationId();
                animId.AssetId = R6_ANIMATION_IDS[animName];
                animId.AnimationType = AnimationType.KeyframeSequence;
                animIds.Add(animName, animId);
            }

            return animIds;
        }

        public StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale, bool collisionModel = false)
        {
            StudioMdlWriter meshBuilder = new StudioMdlWriter();

            // Build Character
            Folder import = RBXM.LoadFromAsset(R6AssemblyAsset);
            Folder assembly = import.FindFirstChild<Folder>("ASSEMBLY");
            assembly.Parent = characterAssets;

            BasePart head = assembly.FindFirstChild<BasePart>("Head");
            BasePart torso = assembly.FindFirstChild<BasePart>("Torso");
            torso.CFrame = new CFrame();

            foreach (Instance asset in characterAssets.GetChildren())
            {
                if (asset.IsA("CharacterMesh") && !collisionModel)
                {
                    CharacterMesh characterMesh = (CharacterMesh)asset;
                    string limbName = LimbMatcher[characterMesh.BodyPart];

                    MeshPart limb = assembly.FindFirstChild<MeshPart>(limbName);
                    if (limb != null)
                    {
                        limb.MeshId = "rbxassetid://" + characterMesh.MeshId;
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

            BoneKeyframe keyframe = AssembleBones(meshBuilder, torso);

            foreach (Bone bone in keyframe.Bones)
                BuildAvatarGeometry(meshBuilder, bone);
            
            return meshBuilder;
        }

        public TextureCompositor ComposeTextureMap(Folder characterAssets, BodyColors bodyColors)
        {
            TextureCompositor compositor = new TextureCompositor(AvatarType.R6, RECT_FULL);
            compositor.CharacterAssets = characterAssets;

            // Append BodyColors
            compositor.AppendColor(bodyColors.TorsoColor,    COMPOSIT_TORSO,     RECT_FULL);
            compositor.AppendColor(bodyColors.LeftArmColor,  COMPOSIT_LEFT_ARM,  RECT_FULL);
            compositor.AppendColor(bodyColors.LeftLegColor,  COMPOSIT_LEFT_LEG,  RECT_FULL);
            compositor.AppendColor(bodyColors.RightArmColor, COMPOSIT_RIGHT_ARM, RECT_FULL);
            compositor.AppendColor(bodyColors.RightLegColor, COMPOSIT_RIGHT_LEG, RECT_FULL);

            // Append Head & Face
            Asset faceAsset = GetAvatarFace(characterAssets);
            compositor.AppendColor(bodyColors.HeadColor, RECT_HEAD);
            compositor.AppendTexture(faceAsset, RECT_HEAD, 1);

            // Append Shirt
            Shirt shirt = characterAssets.FindFirstChildOfClass<Shirt>();
            if (shirt != null)
            {
                Asset shirtAsset = Asset.GetByAssetId(shirt.ShirtTemplate);
                compositor.AppendTexture(shirtAsset, COMPOSIT_SHIRT, RECT_FULL, 2);
            }

            // Append Pants
            Pants pants = characterAssets.FindFirstChildOfClass<Pants>();
            if (pants != null)
            {
                Asset pantsAsset = Asset.GetByAssetId(pants.PantsTemplate);
                compositor.AppendTexture(pantsAsset, COMPOSIT_PANTS, RECT_FULL, 1);
            }

            // Append T-Shirt
            ShirtGraphic tshirt = characterAssets.FindFirstChildOfClass<ShirtGraphic>();
            if (tshirt != null)
            {
                Asset tshirtAsset = Asset.GetByAssetId(tshirt.Graphic);
                compositor.AppendTexture(tshirtAsset, RECT_TSHIRT, 3, RotateFlipType.Rotate90FlipNone);
            }

            return compositor;
        }

        public TextureBindings BindTextures(TextureCompositor compositor, Dictionary<string, Material> materials)
        {
            TextureBindings textures = new TextureBindings();

            Bitmap core = compositor.BakeTextureMap();
            Rbx2Source.SetDebugImage(core);

            Bitmap head = TextureCompositor.CropBitmap(core, RECT_HEAD);
            textures.BindTexture("Head", head);

            Bitmap body = TextureCompositor.CropBitmap(core, RECT_BODY);
            Folder characterAssets = compositor.CharacterAssets;

            Rbx2Source.Print("Processing Package Textures...");
            Rbx2Source.IncrementStack();

            // Collect CharacterMeshes
            var packagedLimbs = characterAssets
                .GetChildrenOfClass<CharacterMesh>()
                .ToDictionary(mesh => mesh.BodyPart);
            
            // Compose the textures that will be used
            var limbOverlays = new Dictionary<Limb, long>();
            var limbBitmaps = new Dictionary<long, Bitmap>() { {0, body} };

            foreach (Limb limb in LimbMatcher.Keys)
            {
                // Head is already textured, ignore it.
                if (limb == Limb.Head)
                    continue;

                // Is there a CharacterMesh for this limb?
                if (packagedLimbs.ContainsKey(limb))
                {
                    // Check the CharacterMesh textures.
                    CharacterMesh mesh = packagedLimbs[limb];

                    if (mesh.OverlayTextureId > 0)
                    {
                        // Use the overlay texture for this limb.
                        long overlayId = mesh.OverlayTextureId;
                        limbOverlays.Add(limb, overlayId);

                        // Compose this overlay texture with the body texture if it doesn't exist yet.
                        if (!limbBitmaps.ContainsKey(overlayId))
                        {
                            Asset overlayAsset = Asset.Get(overlayId);

                            TextureCompositor overlayCompositor = new TextureCompositor(AvatarType.R6, RECT_FULL);
                            overlayCompositor.SetContext("Overlay Texture " + overlayId);
                            overlayCompositor.AppendTexture(overlayAsset, RECT_BODY, 1);
                            overlayCompositor.AppendTexture(body, RECT_BODY);
                            
                            Bitmap overlayTex = overlayCompositor.BakeTextureMap(RECT_BODY);
                            limbBitmaps.Add(overlayId, overlayTex);
                        }

                        continue;
                    }
                    else if (mesh.BaseTextureId > 0)
                    {
                        // Use the base texture for this limb.
                        long baseId = mesh.BaseTextureId;
                        limbOverlays.Add(limb, baseId);

                        // Compose the base texture if it doesn't exist yet.
                        if (!limbBitmaps.ContainsKey(baseId))
                        {
                            Asset baseAsset = Asset.Get(baseId);

                            TextureCompositor baseCompositor = new TextureCompositor(AvatarType.R6, RECT_FULL);
                            baseCompositor.SetContext("Base Texture " + baseId);
                            baseCompositor.AppendTexture(baseAsset, RECT_BODY);
                            
                            Bitmap baseTex = baseCompositor.BakeTextureMap(RECT_BODY);
                            limbBitmaps.Add(baseId, baseTex);
                        }

                        continue;
                    }
                }

                // If no continue statement is reached, fallback to using the body texture.
                // This occurs if the limb has no package, or the package limb has no textures.
                limbOverlays.Add(limb, 0);
            }

            // Add the images into the texture assembly.
            foreach (long id in limbBitmaps.Keys)
            {
                Bitmap bitmap = limbBitmaps[id];
                string matName = GetBodyMatName(id);
                textures.BindTexture(matName, bitmap, false);
            }

            // Link the limbs to their textures.
            foreach (Limb limb in limbOverlays.Keys)
            {
                long id = limbOverlays[limb];
                string matName = GetBodyMatName(id);

                string limbName = Rbx2Source.GetEnumName(limb);
                textures.BindTextureAlias(limbName, matName);
            }

            // Handle the rest of the materials
            foreach (string matName in materials.Keys)
            {
                if (!textures.MatLinks.ContainsKey(matName))
                {
                    Material material = materials[matName];
                    Asset texture = material.TextureAsset;

                    TextureCompositor matComp = new TextureCompositor(AvatarType.R6, RECT_ITEM);
                    matComp.SetContext("Accessory Texture " + matName);
                    matComp.AppendTexture(texture, RECT_ITEM);
                    
                    Bitmap bitmap = matComp.BakeTextureMap();
                    textures.BindTexture(matName, bitmap);
                }
            }

            Rbx2Source.DecrementStack();
            return textures;
        }
    }
}
