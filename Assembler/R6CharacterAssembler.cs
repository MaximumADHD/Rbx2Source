using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Rbx2Source.Animating;
using Rbx2Source.Resources;
using Rbx2Source.StudioMdl;
using Rbx2Source.Textures;
using Rbx2Source.Web;

using RobloxFiles;
using RobloxFiles.Enums;
using RobloxFiles.DataTypes;
using System.Diagnostics.Contracts;

namespace Rbx2Source.Assembler
{
    public class R6CharacterAssembler : CharacterAssembler, ICharacterAssembler
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        // TEXTURE COMPOSITION CONSTANTS
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const string COMPOSIT_TORSO     = "CompositTorso";
        private const string COMPOSIT_LEFT_ARM  = "CompositLeftArm";
        private const string COMPOSIT_LEFT_LEG  = "CompositLeftLeg";
        private const string COMPOSIT_RIGHT_ARM = "CompositRightArm";
        private const string COMPOSIT_RIGHT_LEG = "CompositRightLeg";

        private const string COMPOSIT_SHIRT     = "CompositShirtTemplate";
        private const string COMPOSIT_PANTS     = "CompositPantsTemplate";

        private static readonly Rectangle RECT_FULL   = new Rectangle(  0,   0, 1024, 768);
        private static readonly Rectangle RECT_HEAD   = new Rectangle(400,   0,  200, 200);
        private static readonly Rectangle RECT_BODY   = new Rectangle(  0, 256, 1024, 512);
        private static readonly Rectangle RECT_ITEM   = new Rectangle(  0,   0,  512, 512);
        private static readonly Rectangle RECT_TSHIRT = new Rectangle( 32, 321,  128, 128);

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        public byte[] CollisionModelScript => ResourceUtility.GetResource("AvatarData/R6/CollisionJoints.qc");
        private static readonly Asset R6AssemblyAsset = Asset.FromResource("AvatarData/R6/CharacterBase.rbxm");

        private static readonly IReadOnlyDictionary<BodyPart, string> LimbMatcher = new Dictionary<BodyPart, string>()
        {
            {BodyPart.Head,     "Head"},
            {BodyPart.LeftArm,  "Left Arm"},
            {BodyPart.RightArm, "Right Arm"},
            {BodyPart.LeftLeg,  "Left Leg"},
            {BodyPart.RightLeg, "Right Leg"},
            {BodyPart.Torso,    "Torso"}
        };

        private static readonly IReadOnlyDictionary<string, long> R6_ANIMATION_IDS = new Dictionary<string, long>()
        {
            { "Climb", 180436334 },
            { "Fall",  180436148 },
            { "Idle",  180435571 },
            { "Jump",  125750702 },
            { "Look",  180435792 },
            { "Run",   180426354 },
            { "Sit",   178130996 },
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
                AnimationId animId = new AnimationId()
                {
                    AssetId = R6_ANIMATION_IDS[animName],
                    AnimationType = AnimationType.KeyframeSequence
                };

                animIds.Add(animName, animId);
            }

            return animIds;
        }

        public StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale, bool collisionModel = false)
        {
            Contract.Requires(characterAssets != null && scale != null);
            StudioMdlWriter meshBuilder = new StudioMdlWriter();

            // Build Character
            var import = R6AssemblyAsset.OpenAsModel();
            Folder assembly = import.FindFirstChild<Folder>("ASSEMBLY");

            BasePart head = assembly.FindFirstChild<BasePart>("Head");
            assembly.Parent = characterAssets;

            foreach (Instance asset in characterAssets.GetChildren())
            {
                if (asset is CharacterMesh && !collisionModel)
                {
                    var characterMesh = asset as CharacterMesh;
                    string limbName = LimbMatcher[characterMesh.BodyPart];

                    var limb = assembly.FindFirstChild<MeshPart>(limbName);
                    limb.MeshId = "rbxassetid://" + characterMesh.MeshId;
                }
                else if (asset is Accoutrement && !collisionModel)
                {
                    PrepareAccessory(asset, assembly);
                }
                else if (asset is DataModelMesh)
                {
                    OverwriteHead(asset as DataModelMesh, head);
                }
            }

            BoneKeyframe skeleton = AssembleBones(meshBuilder, assembly);

            foreach (StudioBone bone in skeleton.Bones)
                BuildAvatarGeometry(meshBuilder, bone);
            
            return meshBuilder;
        }

        public TextureCompositor ComposeTextureMap(Folder characterAssets, WebBodyColors bodyColors)
        {
            Contract.Requires(characterAssets != null && bodyColors != null);
            var compositor = new TextureCompositor(RECT_FULL, HumanoidRigType.R6, characterAssets);
            
            // Append BodyColors
            compositor.AppendColor(bodyColors.TorsoColorId,    COMPOSIT_TORSO,     RECT_FULL);
            compositor.AppendColor(bodyColors.LeftArmColorId,  COMPOSIT_LEFT_ARM,  RECT_FULL);
            compositor.AppendColor(bodyColors.LeftLegColorId,  COMPOSIT_LEFT_LEG,  RECT_FULL);
            compositor.AppendColor(bodyColors.RightArmColorId, COMPOSIT_RIGHT_ARM, RECT_FULL);
            compositor.AppendColor(bodyColors.RightLegColorId, COMPOSIT_RIGHT_LEG, RECT_FULL);

            // Append Head & Face
            Asset faceAsset = GetAvatarFace(characterAssets);
            compositor.AppendTexture(faceAsset, RECT_HEAD, 1);
            compositor.AppendColor(bodyColors.HeadColorId, RECT_HEAD);

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

        public TextureBindings BindTextures(TextureCompositor compositor, Dictionary<string, ValveMaterial> materials)
        {
            Contract.Requires(compositor != null && materials != null);
            TextureBindings textures = new TextureBindings();

            Bitmap core = compositor.BakeTextureMap();
            Main.SetDebugImage(core);

            Bitmap head = TextureCompositor.CropBitmap(core, RECT_HEAD);
            textures.BindTexture("Head", head);

            Bitmap body = TextureCompositor.CropBitmap(core, RECT_BODY);
            Folder characterAssets = compositor.CharacterAssets;

            Main.Print("Processing Package Textures...");
            Main.IncrementStack();

            // Collect CharacterMeshes
            var packagedLimbs = characterAssets
                .GetChildrenOfType<CharacterMesh>()
                .ToDictionary(mesh => mesh.BodyPart);
            
            // Compose the textures that will be used
            var limbOverlays = new Dictionary<BodyPart, long>();
            var limbBitmaps = new Dictionary<long, Bitmap>() { {0, body} };

            foreach (BodyPart limb in LimbMatcher.Keys)
            {
                // Head is already textured, ignore it.
                if (limb == BodyPart.Head)
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
                            Bitmap overlayTex;

                            using (var overlayCompositor = new TextureCompositor(RECT_FULL))
                            {
                                overlayCompositor.SetContext("Overlay Texture " + overlayId);
                                overlayCompositor.AppendTexture(overlayAsset, RECT_BODY, 1);
                                overlayCompositor.AppendTexture(body, RECT_BODY);

                                overlayTex = overlayCompositor.BakeTextureMap(RECT_BODY);
                            }
                            
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
                            Bitmap baseTex;

                            using (var baseCompositor = new TextureCompositor(RECT_FULL))
                            {
                                baseCompositor.SetContext("Base Texture " + baseId);
                                baseCompositor.AppendTexture(baseAsset, RECT_BODY);
                                baseTex = baseCompositor.BakeTextureMap(RECT_BODY);
                            }
                            
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
            foreach (BodyPart limb in limbOverlays.Keys)
            {
                long id = limbOverlays[limb];
                string matName = GetBodyMatName(id);

                string limbName = Main.GetEnumName(limb);
                textures.BindTextureAlias(limbName, matName);
            }

            // Handle the rest of the materials
            foreach (string matName in materials.Keys)
            {
                if (!textures.MatLinks.ContainsKey(matName))
                {
                    ValveMaterial material = materials[matName];
                    Asset texture = material.TextureAsset;

                    TextureCompositor matComp = new TextureCompositor(RECT_ITEM, HumanoidRigType.R6);
                    matComp.SetContext("Accessory Texture " + matName);
                    matComp.AppendTexture(texture, RECT_ITEM);
                    
                    Bitmap bitmap = matComp.BakeTextureMap();
                    textures.BindTexture(matName, bitmap);
                }
            }

            Main.DecrementStack();
            return textures;
        }
    }
}
