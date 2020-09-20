using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using RobloxFiles.DataTypes;
using Rbx2Source.Geometry;
using Rbx2Source.Web;
using System.Diagnostics.Contracts;

namespace Rbx2Source.Textures
{
    [Flags]
    public enum DrawFlags
    {
        Guide = 0b00,
        Rect  = 0b10,

        Texture = 0b00,
        Color   = 0b01,
    }

    public class TextureAllocation
    {
        public MemoryStream Stream;
        public Bitmap Texture;
    }

    public class CompositData : IComparable
    {
        public object Texture;
        public Color DrawColor;

        public int Layer;
        public Mesh Guide;

        public Rectangle Rect;
        public RotateFlipType FlipMode = RotateFlipType.RotateNoneFlipNone;

        public readonly DrawFlags DrawFlags;
        private static readonly Dictionary<long, TextureAllocation> TextureAlloc = new Dictionary<long, TextureAllocation>();

        public CompositData(DrawFlags drawFlags)
        {
            DrawFlags = drawFlags;
            Rect = new Rectangle();
        }

        public void SetGuide(string guideName, Rectangle guideSize, AvatarType avatarType)
        {
            string guidePath = "AvatarData/" + avatarType + "/Compositing/" + guideName + ".mesh";
            Asset guideAsset = Asset.FromResource(guidePath);

            Guide = Mesh.FromAsset(guideAsset);
            Rect = guideSize;
        }

        public void SetOffset(Point offset)
        {
            Rect.Location = offset;
        }

        public void SetDrawColor(int id)
        {
            BrickColor brick = id;
            Color3uint8 clr = brick.Color;
            DrawColor = Color.FromArgb(clr.R, clr.G, clr.B);
        }

        public int CompareTo(object other)
        {
            if (other is CompositData)
            {
                var otherComp = other as CompositData;
                return (Layer - otherComp.Layer);
            }

            throw new NotImplementedException();
        }

        public Vertex[] GetGuideVerts(int faceIndex)
        {
            var result = Guide.Faces[faceIndex]
                .Select((face) => Guide.Verts[face])
                .ToArray();

            return result;
        }

        public void UseBrush(Action<Brush> callback)
        {
            using (var brush = new SolidBrush(DrawColor))
            callback?.Invoke(brush);
        }

        public Bitmap GetTextureBitmap()
        {
            if (Texture is Asset)
            {
                var asset = Texture as Asset;
                long assetId = asset.Id;

                if (!TextureAlloc.ContainsKey(assetId))
                {
                    byte[] buffer = asset.GetContent();

                    var alloc = new TextureAllocation()
                    {
                        Stream = new MemoryStream(buffer)
                    };
                    
                    try
                    {
                        alloc.Texture = Image.FromStream(alloc.Stream) as Bitmap;
                    }
                    catch
                    {
                        alloc.Texture = new Bitmap(512, 512);
                    }

                    TextureAlloc.Add(assetId, alloc);
                }

                return TextureAlloc[assetId].Texture;
            }
            else if (Texture is Bitmap)
            {
                return Texture as Bitmap;
            }
            else
            {
                throw new InvalidDataException("The Texture of this CompositData was assigned a garbage value.");
            }
        }

        public static void FreeAllocatedTextures()
        {
            long[] assetIds = TextureAlloc.Keys.ToArray();

            foreach (long assetId in assetIds)
            {
                TextureAllocation alloc = TextureAlloc[assetId];
                TextureAlloc.Remove(assetId);

                alloc.Texture.Dispose();
                alloc.Texture = null;

                alloc.Stream.Dispose();
                alloc.Stream = null;
            }
        }
    }
}
