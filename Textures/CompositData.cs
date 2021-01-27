using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using RobloxFiles.Enums;
using RobloxFiles.DataTypes;

using Rbx2Source.Geometry;
using Rbx2Source.Web;

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

        public TextureAllocation(MemoryStream stream)
        {
            Stream = stream;
        }
    }

    public class CompositData : IComparable, IDisposable
    {
        public object Texture;
        public Color DrawColor;
        
        public int Layer;
        public Mesh Guide;

        public Rectangle Rect;
        public RotateFlipType FlipType = RotateFlipType.RotateNoneFlipNone;

        public readonly DrawFlags DrawFlags;
        private static readonly Dictionary<long, TextureAllocation> TextureAlloc = new Dictionary<long, TextureAllocation>();
        public Brush DrawBrush { get; private set; }

        public CompositData(DrawFlags drawFlags)
        {
            DrawFlags = drawFlags;
            Rect = new Rectangle();
        }

        public void SetGuide(string guideName, Rectangle guideSize, HumanoidRigType avatarType)
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
            DrawBrush = new SolidBrush(DrawColor);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            FreeAllocatedTextures();
            DrawBrush?.Dispose();
        }

        public Bitmap GetTextureBitmap()
        {
            if (Texture is Asset)
            {
                var asset = Texture as Asset;
                long assetId = asset.Id;

                if (!TextureAlloc.TryGetValue(assetId, out TextureAllocation alloc))
                {
                    byte[] buffer = asset.GetContent();

                    var stream = new MemoryStream(buffer);
                    alloc = new TextureAllocation(stream);
                    
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

                return alloc.Texture;
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
