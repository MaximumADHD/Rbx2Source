using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rbx2Source.Coordinates;
using Rbx2Source.Geometry;
using Rbx2Source.Reflection;
using Rbx2Source.Resources;
using Rbx2Source.StudioMdl;
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
        private static Rectangle  RECT_TSHIRT         =  new Rectangle (   2, 822, 128, 128 );

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Asset R15AssemblyAsset = Asset.FromResource("AvatarData/R15/ASSEMBLY.rbxmx");

        private static Dictionary<Limb, Rectangle> UVCrops = new Dictionary<Limb, Rectangle>()
        {
            { Limb.Head,     RECT_HEAD      },
            { Limb.Torso,    RECT_TORSO     },
            { Limb.LeftArm,  RECT_LEFT_ARM  },
            { Limb.LeftLeg,  RECT_LEFT_LEG  },
            { Limb.RightArm, RECT_RIGHT_ARM },
            { Limb.RightLeg, RECT_RIGHT_LEG },
        };

        public byte[] CollisionModelScript
        {
            get { return ResourceUtility.GetResource("AvatarData/R15/CollisionJoints.qc"); }
        }

        public static Vector3 GetAvatarScale(AvatarScale scale)
        {
            return new Vector3(scale.Width, scale.Height, 1);
        }

        public StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale)
        {
            StudioMdlWriter meshBuilder = new StudioMdlWriter();

            // Build Character
            Folder import = RBXM.LoadFromAsset(R15AssemblyAsset);
            Folder assembly = import.FindFirstChild<Folder>("ASSEMBLY");
            assembly.Parent = characterAssets;

            Part head = assembly.FindFirstChild<Part>("Head");
            Vector3 avatarScale = GetAvatarScale(scale);

            foreach (Instance asset in characterAssets.GetChildren())
            {
                if (asset.IsA("Part"))
                {
                    Part existing = assembly.FindFirstChild<Part>(asset.Name);
                    if (existing != null)
                        existing.Destroy();

                    asset.Parent = assembly;
                }
                else if (asset.IsA("Accoutrement"))
                    PrepareAccessory(asset, assembly);
                else if (asset.IsA("DataModelMesh"))
                    OverwriteHead(asset, head);
            }

            // Avatar Scaling

            foreach (Part part in assembly.GetChildrenOfClass<Part>())
            {
                Limb limb = GetLimb(part);
                if (limb != Limb.Unknown)
                {
                    part.Size *= avatarScale;

                    foreach (Attachment attachment in part.GetChildrenOfClass<Attachment>())
                        attachment.CFrame = CFrame.Scale(attachment.CFrame, avatarScale);
                }
            }

            Part torso = assembly.FindFirstChild<Part>("LowerTorso");
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
            TextureCompositor compositor = new TextureCompositor(AvatarType.R15, 1024, 768);

            // Append BodyColors
            compositor.AppendColor(bodyColors.TorsoColor,    COMPOSIT_TORSO,      RECT_TORSO);
            compositor.AppendColor(bodyColors.LeftArmColor,  COMPOSIT_LEFT_LIMB,  RECT_LEFT_ARM);
            compositor.AppendColor(bodyColors.LeftLegColor,  COMPOSIT_LEFT_LIMB,  RECT_LEFT_LEG);
            compositor.AppendColor(bodyColors.RightArmColor, COMPOSIT_RIGHT_LIMB, RECT_RIGHT_ARM);
            compositor.AppendColor(bodyColors.RightLegColor, COMPOSIT_RIGHT_LIMB, RECT_RIGHT_LEG);

            // Append Head & Face
            Asset face = GetAvatarFace(characterAssets);
            compositor.AppendColor(bodyColors.HeadColor, RECT_HEAD);
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
                string textureId = part.TextureID;
                if (textureId != null && textureId.Length > 0 && !overlainLimbs.Contains(limb))
                {
                    Asset overlay = Asset.GetByAssetId(textureId);

                    if (limb == Limb.Torso)
                        compositor.AppendTexture(overlay, RECT_TORSO,     3);
                    else if (limb == Limb.LeftArm)
                        compositor.AppendTexture(overlay, RECT_LEFT_ARM,  3);
                    else if (limb == Limb.RightArm)
                        compositor.AppendTexture(overlay, RECT_RIGHT_ARM, 3);
                    else if (limb == Limb.LeftLeg)
                        compositor.AppendTexture(overlay, RECT_LEFT_LEG,  3);
                    else if (limb == Limb.RightLeg)
                        compositor.AppendTexture(overlay, RECT_RIGHT_LEG, 3);

                    overlainLimbs.Add(limb);
                }
            }

            return compositor;
        }

        public TextureAssembly AssembleTextures(TextureCompositor compositor, Dictionary<string,Material> materials)
        {
            TextureAssembly assembly = new TextureAssembly();
            assembly.Images = new Dictionary<string, Image>();
            assembly.MatLinks = new Dictionary<string, string>();

            Bitmap uvMap = compositor.BakeTextureMap();
            ImageAttributes blankAtt = new ImageAttributes();

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

                        Size size = cropRegion.Size;
                        int w = size.Width;
                        int h = size.Height;
                        Point origin = cropRegion.Location;
                        int x = origin.X;
                        int y = origin.Y;

                        Bitmap newImg = new Bitmap(w, h);
                        Graphics graphics = Graphics.FromImage(newImg);
                        Rectangle dest = new Rectangle(Point.Empty, size);

                        graphics.DrawImage(uvMap,dest,x,y,w,h,GraphicsUnit.Pixel,blankAtt);
                        graphics.Dispose();

                        image = newImg;
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
                {
                    assembly.Images.Add(materialName, image);
                    assembly.MatLinks.Add(materialName, materialName);
                }
                else
                    Rbx2Source.Print("Missing Image for Material {0}?", materialName);
            }

            return assembly;
        }
    }
}
