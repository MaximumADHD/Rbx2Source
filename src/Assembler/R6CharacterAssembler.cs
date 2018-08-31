using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Rbx2Source.Reflection;
using Rbx2Source.Resources;
using Rbx2Source.StudioMdl;
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

        private static string COMPOSIT_BASE_TEXTURE    = "BaseTexture";
        private static string COMPOSIT_OVERLAY_TEXTURE = "OverlayTexture";

        private static Rectangle CANVAS_RECT = new Rectangle(0, 0, 1024, 768);
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        private static Dictionary<string,int> refs = new Dictionary<string,int>() 
        {
            {"Torso",1},
            {"LeftArm",2},
            {"RightArm",3},
            {"LeftLeg",4},
            {"RightLeg",5}
        };

        public byte[] CollisionModelScript => ResourceUtility.GetResource("AvatarData/R6/CollisionJoints.qc");

        public StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale)
        {
            StudioMdlWriter meshBuilder = new StudioMdlWriter();

            // Build Character

            Folder import = RBXM.LoadFromAsset(R6AssemblyAsset);
            Folder assembly = import.FindFirstChild<Folder>("ASSEMBLY");
            Part torso = assembly.FindFirstChild<Part>("Torso");
            Part head = assembly.FindFirstChild<Part>("Head");

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
                    PrepareAccessory(asset, assembly);
                else if (asset.IsA("DataModelMesh")) 
                    OverwriteHead(asset, head);
            }

            BoneKeyframe keyframe = AssembleBones(meshBuilder, torso);

            foreach (Bone bone in keyframe.Bones)
                BuildAvatarGeometry(meshBuilder, bone);

            return meshBuilder;
        }

        public TextureCompositor ComposeTextureMap(Folder characterAssets, BodyColors bodyColors)
        {
            TextureCompositor compositor = new TextureCompositor(AvatarType.R6, CANVAS_RECT);

            // Append BodyColors
            compositor.AppendColor(bodyColors.TorsoColor,    COMPOSIT_TORSO,     CANVAS_RECT);
            compositor.AppendColor(bodyColors.LeftArmColor,  COMPOSIT_LEFT_ARM,  CANVAS_RECT);
            compositor.AppendColor(bodyColors.LeftLegColor,  COMPOSIT_LEFT_LEG,  CANVAS_RECT);
            compositor.AppendColor(bodyColors.RightArmColor, COMPOSIT_RIGHT_ARM, CANVAS_RECT);
            compositor.AppendColor(bodyColors.RightLegColor, COMPOSIT_RIGHT_LEG, CANVAS_RECT);

            // Append Shirt
            Shirt shirt = characterAssets.FindFirstChildOfClass<Shirt>();
            if (shirt != null)
            {
                Asset shirtAsset = Asset.GetByAssetId(shirt.ShirtTemplate);
                compositor.AppendTexture(shirtAsset, COMPOSIT_TORSO,     CANVAS_RECT, 2);
                compositor.AppendTexture(shirtAsset, COMPOSIT_LEFT_ARM,  CANVAS_RECT, 1);
                compositor.AppendTexture(shirtAsset, COMPOSIT_RIGHT_ARM, CANVAS_RECT, 1);
            }

            // Append Pants
            Pants pants = characterAssets.FindFirstChildOfClass<Pants>();
            if (pants != null)
            {
                Asset pantsAsset = Asset.GetByAssetId(pants.PantsTemplate);
                compositor.AppendTexture(pantsAsset, COMPOSIT_TORSO,     CANVAS_RECT, 1);
                compositor.AppendTexture(pantsAsset, COMPOSIT_LEFT_LEG,  CANVAS_RECT, 1);
                compositor.AppendTexture(pantsAsset, COMPOSIT_RIGHT_LEG, CANVAS_RECT, 1);
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

            /*Rbx3DThumbnailInfo info = TextureFetch.Get3DThumbnail(avatar.UserInfo.Id);
            string objHash = info.Obj;
            string objUrl = WebUtility.ResolveHashUrl(objHash);
            string obj = WebUtility.DownloadString(objUrl);
            StringReader reader = new StringReader(obj);

            List<int> matLookUp = new List<int>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("usemtl "))
                {
                    int value = int.Parse(line.Substring(14).Replace("Mtl", ""), Rbx2Source.NormalParse);
                    matLookUp.Add(value);
                }
            }

            int groupCount = matLookUp.Count;

            string materialHash = info.Mtl;
            string materialUrl = WebUtility.ResolveHashUrl(materialHash);
            string material = WebUtility.DownloadString(materialUrl);
            reader = new StringReader(material);

            int currentGroup = 0;
            Dictionary<int, string> textureIndex = new Dictionary<int, string>();

            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("newmtl"))
                    currentGroup = int.Parse(line.Substring(14).Replace("Mtl",""), Rbx2Source.NormalParse);
                else if (line.StartsWith("map_d"))
                {
                    string textureHash = line.Substring(6);
                    if (!textureIndex.ContainsKey(currentGroup))
                         textureIndex.Add(currentGroup, textureHash);
                }
            }

            Bitmap head = new Bitmap(113, 113);
            bool gotHead = false;

            foreach (int group in textureIndex.Keys)
            {
                string mtlName = "AvatarMap_Id" + group;
                string textureHash = textureIndex[group];
                string textureUrl = WebUtility.ResolveHashUrl(textureHash);
                Bitmap baseImage = WebUtility.DownloadImage(textureUrl);
                Size baseImgSize = baseImage.Size;
                int aspectRatio = baseImgSize.Width / baseImgSize.Height;
                if (aspectRatio == 2) // If it isn't 2:1, then we can safely assume this isn't a relevant texture map.
                {
                    Bitmap image = new Bitmap(768, 512);
                    Graphics graphics = Graphics.FromImage(image);
                    graphics.DrawImage(baseImage, Point.Empty);
                    assembly.Images.Add(mtlName, image);
                    if (!gotHead)
                    {
                        Graphics headGraphics = Graphics.FromImage(head);
                        headGraphics.DrawImage(baseImage, new Point(-903, -8));
                        gotHead = true;
                    }
                }
            }

            assembly.Images.Add("Head", head);
            assembly.MatLinks.Add("Head", "Head");

            foreach (string mtlName in materials.Keys)
            {
                if (!assembly.MatLinks.ContainsKey(mtlName))
                {
                    if (refs.ContainsKey(mtlName))
                    {
                        int i = refs[mtlName];
                        int g = groupCount - i - 1;
                        int v = matLookUp[g];
                        string textureLink = "AvatarMap_Id" + v;
                        assembly.MatLinks.Add(mtlName, textureLink);
                    }
                    else
                    {
                        Material mat = materials[mtlName];
                        Asset texture = mat.TextureAsset;
                        byte[] textureData = texture.GetContent();
                        MemoryStream textureStream = new MemoryStream(textureData);
                        Image image = Image.FromStream(textureStream);
                        assembly.Images.Add(mtlName, image);
                        assembly.MatLinks.Add(mtlName, mtlName);
                    }
                }
            }*/

            return assembly;
        }
    }
}
