using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

using Rbx2Source.Coordinates;
using Rbx2Source.Reflection;
using Rbx2Source.Resources;
using Rbx2Source.StudioMdl;
using Rbx2Source.Textures;
using Rbx2Source.Web;

namespace Rbx2Source.Assembler
{
    class R6CharacterAssembler : CharacterAssembler, ICharacterAssembler
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

        private static Rectangle CANVAS_RECT     = new Rectangle(  0,   0, 1024, 768);
        private static Rectangle CANVAS_HEAD     = new Rectangle(400,   0,  200, 200);
        private static Rectangle CANVAS_BODY     = new Rectangle(  0, 256, 1024, 512);
        private static Rectangle CANVAS_ITEM     = new Rectangle(  0,   0,  512, 512);

        private static Rectangle CANVAS_TSHIRT   = new Rectangle( 32, 321,  128, 128);

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

        private static string GetBodyMaterialName(long id)
        {
            return id == 0 ? "Body" : "PackageOverlay" + id;
        }

        public StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale)
        {
            StudioMdlWriter meshBuilder = new StudioMdlWriter();

            // Build Character
            Folder import = RBXM.LoadFromAsset(R6AssemblyAsset);
            Folder assembly = import.FindFirstChild<Folder>("ASSEMBLY");

            BasePart head = assembly.FindFirstChild<BasePart>("Head");
            BasePart torso = assembly.FindFirstChild<BasePart>("Torso");
            torso.CFrame = new CFrame();

            foreach (Instance asset in characterAssets.GetChildren())
            {
                if (asset.IsA("CharacterMesh"))
                {
                    CharacterMesh characterMesh = (CharacterMesh)asset;
                    string limbName = LimbMatcher[characterMesh.BodyPart];

                    MeshPart limb = assembly.FindFirstChild<MeshPart>(limbName);
                    if (limb != null)
                        limb.MeshID = "rbxassetid://" + characterMesh.MeshId;

                }
                else if (asset.IsA("Accoutrement"))
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
            TextureCompositor compositor = new TextureCompositor(AvatarType.R6, CANVAS_RECT);
            compositor.CharacterAssets = characterAssets;

            // Append BodyColors
            compositor.AppendColor(bodyColors.TorsoColor,    COMPOSIT_TORSO,     CANVAS_RECT);
            compositor.AppendColor(bodyColors.LeftArmColor,  COMPOSIT_LEFT_ARM,  CANVAS_RECT);
            compositor.AppendColor(bodyColors.LeftLegColor,  COMPOSIT_LEFT_LEG,  CANVAS_RECT);
            compositor.AppendColor(bodyColors.RightArmColor, COMPOSIT_RIGHT_ARM, CANVAS_RECT);
            compositor.AppendColor(bodyColors.RightLegColor, COMPOSIT_RIGHT_LEG, CANVAS_RECT);

            // Append Head & Face
            Asset faceAsset = GetAvatarFace(characterAssets);
            compositor.AppendColor(bodyColors.HeadColor, CANVAS_HEAD);
            compositor.AppendTexture(faceAsset, CANVAS_HEAD, 1);

            // Append Shirt
            Shirt shirt = characterAssets.FindFirstChildOfClass<Shirt>();
            if (shirt != null)
            {
                Asset shirtAsset = Asset.GetByAssetId(shirt.ShirtTemplate);
                compositor.AppendTexture(shirtAsset,  COMPOSIT_SHIRT,  CANVAS_RECT, 2);
            }

            // Append Pants
            Pants pants = characterAssets.FindFirstChildOfClass<Pants>();
            if (pants != null)
            {
                Asset pantsAsset = Asset.GetByAssetId(pants.PantsTemplate);
                compositor.AppendTexture(pantsAsset,  COMPOSIT_PANTS,  CANVAS_RECT, 1);
            }

            // Append T-Shirt
            ShirtGraphic tshirt = characterAssets.FindFirstChildOfClass<ShirtGraphic>();
            if (tshirt != null)
            {
                Asset tshirtAsset = Asset.GetByAssetId(tshirt.Graphic);
                compositor.AppendTexture(tshirtAsset, CANVAS_TSHIRT, 3, RotateFlipType.Rotate90FlipNone);
            }

            return compositor;
        }

        public TextureAssembly AssembleTextures(TextureCompositor compositor, Dictionary<string, Material> materials)
        {
            TextureAssembly assembly = new TextureAssembly();
            assembly.Images = new Dictionary<string, Image>();
            assembly.MatLinks = new Dictionary<string, string>();

            Bitmap core = compositor.BakeTextureMap();
            Rbx2Source.SetDebugImage(core);

            Bitmap head = TextureCompositor.CropBitmap(core, CANVAS_HEAD);
            assembly.LinkDirectly("Head", head);

            Bitmap body = TextureCompositor.CropBitmap(core, CANVAS_BODY);
            Folder characterAssets = compositor.CharacterAssets;

            Rbx2Source.Print("Processing Package Textures...");
            Rbx2Source.IncrementStack();

            // Collect CharacterMeshes
            var packagedLimbs = new Dictionary<Limb, CharacterMesh>();
            foreach (CharacterMesh mesh in characterAssets.GetChildrenOfClass<CharacterMesh>())
                packagedLimbs.Add(mesh.BodyPart, mesh);

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

                    long baseId = mesh.BaseTextureId;
                    long overlayId = mesh.OverlayTextureId;

                    if (overlayId > 0)
                    {
                        // Use the overlay texture for this limb.
                        limbOverlays.Add(limb, overlayId);

                        // Compose this overlay texture with the body texture if it doesn't exist yet.
                        if (!limbBitmaps.ContainsKey(overlayId))
                        {
                            Asset overlayAsset = Asset.Get(overlayId);

                            TextureCompositor overlayCompositor = new TextureCompositor(AvatarType.R6, CANVAS_RECT);
                            overlayCompositor.AppendTexture(body, CANVAS_BODY);
                            overlayCompositor.AppendTexture(overlayAsset, CANVAS_BODY, 1);
                            overlayCompositor.SetContext("Overlay Texture " + overlayId);

                            Bitmap overlayTex = overlayCompositor.BakeTextureMap(CANVAS_BODY);
                            limbBitmaps.Add(overlayId, overlayTex);
                        }

                        continue;
                    }
                    else if (baseId > 0)
                    {
                        // Use the base texture for this limb.
                        limbOverlays.Add(limb, baseId);

                        // Compose the base texture if it doesn't exist yet.
                        if (!limbBitmaps.ContainsKey(baseId))
                        {
                            Asset baseAsset = Asset.Get(baseId);

                            TextureCompositor baseCompositor = new TextureCompositor(AvatarType.R6, CANVAS_RECT);
                            baseCompositor.AppendTexture(baseAsset, CANVAS_BODY);
                            baseCompositor.SetContext("Base Texture " + baseId);

                            Bitmap baseTex = baseCompositor.BakeTextureMap(CANVAS_BODY);
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
                string materialName = GetBodyMaterialName(id);
                Bitmap bitmap = limbBitmaps[id];
                assembly.Images.Add(materialName, bitmap);
            }

            // Link the limbs to their textures.
            foreach (Limb limb in limbOverlays.Keys)
            {
                long id = limbOverlays[limb];
                string materialName = GetBodyMaterialName(id);

                string limbName = Rbx2Source.GetEnumName(limb);
                assembly.MatLinks.Add(limbName, materialName);
            }

            // Handle the rest of the materials
            foreach (string materialName in materials.Keys)
            {
                if (!assembly.MatLinks.ContainsKey(materialName))
                {
                    Material material = materials[materialName];
                    Asset texture = material.TextureAsset;

                    TextureCompositor matComp = new TextureCompositor(AvatarType.R6, CANVAS_ITEM);
                    matComp.AppendTexture(texture, CANVAS_ITEM);
                    matComp.SetContext("Accessory Texture " + materialName);

                    Bitmap bitmap = matComp.BakeTextureMap();
                    assembly.LinkDirectly(materialName, bitmap);
                }
            }

            Rbx2Source.DecrementStack();
            return assembly;
        }
    }
}
