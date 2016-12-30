using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
    class R6CharacterAssembler : CharacterAssembler, ICharacterAssembler
    {
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

        public byte[] CollisionModelScript
        {
            get { return ResourceUtility.GetResource("AvatarData/R6/CollisionJoints.qc"); }
        }

        public StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale)
        {
            StudioMdlWriter meshBuilder = new StudioMdlWriter();

            // Build Character

            Folder import = RbxReflection.LoadFromAsset(R6AssemblyAsset);
            Folder assembly = (Folder)import.FindFirstChild("ASSEMBLY");
            Part torso = (Part)assembly.FindFirstChild("Torso");
            Part head = (Part)assembly.FindFirstChild("Head");

            foreach (Instance asset in characterAssets.GetChildren())
            {
                if (asset.IsA("CharacterMesh"))
                {
                    CharacterMesh characterMesh = (CharacterMesh)asset;
                    string limbName = LimbMatcher[characterMesh.BodyPart];
                    MeshPart limb = (MeshPart)assembly.FindFirstChild(limbName);
                    if (limb != null) limb.MeshID = "rbxassetid://" + characterMesh.MeshId;
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

        public TextureAssembly AssembleTextures(UserAvatar avatar, Dictionary<string, Material> materials)
        {
            /*/ ASSEMBLING R6 CHARACTER TEXTURES IS VERY WEIRD
             *
             * This currently depends on a lucky observation I was able to make based on the
             * layout of the 3D thumbnail files.
             * 
             * To put it as simply as possible, there are "group" tags specified in the 3D Thumbnail's
             * obj file. Each group is prefixed with "Player1" followed by a numerical index for the group.
             * Each group is linked to a material in the mtl file, and the mtl file tells us what texture
             * that material should be linked to.
             * 
             * The number of numerical indexes depends on how many roblox parts exist on the character.
             * Accessories and gears always come first.
             * 
             * The last five groups will always be the following in-order:
             *  - Left Leg
             *  - Right Leg
             *  - Left Arm
             *  - Right Arm
             *  - Torso
             *  - Head
             *  
             * By cleverly exploiting this knowledge, I can extract the 3D obj file textures, and 
             * correctly link them to the right geometry in the generated R6 character.
            /*/

            TextureAssembly assembly = new TextureAssembly();
            assembly.Images = new Dictionary<string, Image>();
            assembly.MatLinks = new Dictionary<string, string>();

            Rbx3DThumbnailInfo info = TextureFetch.Get3DThumbnail(avatar.UserInfo.Id);
            string objHash = info.Obj;
            string objUrl = RbxWebUtility.ResolveHashUrl(objHash);
            string obj = RbxWebUtility.DownloadString(objUrl);
            StringReader reader = new StringReader(obj);

            List<int> matLookUp = new List<int>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("usemtl "))
                {
                    int value = int.Parse(line.Substring(14).Replace("Mtl", ""));
                    matLookUp.Add(value);
                }
            }

            int groupCount = matLookUp.Count;

            string materialHash = info.Mtl;
            string materialUrl = RbxWebUtility.ResolveHashUrl(materialHash);
            string material = RbxWebUtility.DownloadString(materialUrl);
            reader = new StringReader(material);

            int currentGroup = 0;
            Dictionary<int, string> textureIndex = new Dictionary<int, string>();

            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("newmtl"))
                    currentGroup = int.Parse(line.Substring(14).Replace("Mtl",""));
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
                string textureUrl = RbxWebUtility.ResolveHashUrl(textureHash);
                Bitmap baseImage = RbxWebUtility.DownloadImage(textureUrl);
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
            }

            return assembly;
        }
    }
}
