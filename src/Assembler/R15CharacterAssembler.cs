﻿using System;
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
        private static Asset R15AssemblyAsset = Asset.FromResource("AvatarData/R15/ASSEMBLY.rbxmx");
        private static Dictionary<Limb, Rectangle> UVCrops = new Dictionary<Limb, Rectangle>()
        {
            {Limb.Torso, new Rectangle(0,0,388,272)},
            {Limb.LeftArm, new Rectangle(496,0,264,284)},
            {Limb.RightArm, new Rectangle(760,0,264,284)},
            {Limb.Head, new Rectangle(240,272,256,296)},
            {Limb.LeftLeg, new Rectangle(496,284,264,284)},
            {Limb.RightLeg, new Rectangle(760,284,264,284)}
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

        public TextureAssembly AssembleTextures(UserAvatar avatar, Dictionary<string,Material> materials)
        {
            TextureAssembly assembly = new TextureAssembly();
            assembly.Images = new Dictionary<string, Image>();
            assembly.MatLinks = new Dictionary<string, string>();

            // Figure out which image is the uvMap
            Bitmap uvMap = null;
            foreach (string uvMapUrl in TextureFetch.FromUser(avatar.UserInfo.Id))
            {
                Bitmap possibleUvMap = WebUtility.DownloadImage(uvMapUrl);
                if (possibleUvMap.Width == 1024 && possibleUvMap.Height == 1024)
                {
                    uvMap = possibleUvMap;
                    break;
                }
            }

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
