using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using Rbx2Source.Geometry;
using Rbx2Source.Web;

namespace Rbx2Source.Textures
{
    enum DrawMode
    {
        Guide,
        Rect
    }

    enum DrawType
    {
        Texture,
        Color,
    }

    class TextureAllocation
    {
        public MemoryStream Stream;
        public Bitmap Texture;
    }

    class CompositData : IComparable
    {
        public DrawMode DrawMode { get; private set; }
        public DrawType DrawType { get; private set; }

        public object Texture;
        public Color DrawColor;

        public Mesh Guide;
        public int Layer;
        public RotateFlipType FlipMode = RotateFlipType.RotateNoneFlipNone;

        public Rectangle Rect;

        private static Dictionary<long, TextureAllocation> TextureAlloc = new Dictionary<long, TextureAllocation>();

        public CompositData(DrawMode drawMode, DrawType drawType)
        {
            DrawMode = drawMode;
            DrawType = drawType;

            Rect = new Rectangle();
        }

        public void SetGuide(string guideName, Rectangle guideSize, AvatarType avatarType)
        {
            string avatarTypeName = Rbx2Source.GetEnumName(avatarType);
            string guidePath = "AvatarData/" + avatarType + "/Compositing/" + guideName + ".mesh";
            Asset guideAsset = Asset.FromResource(guidePath);

            Guide = Mesh.FromAsset(guideAsset);
            Rect = guideSize;
        }

        public void SetOffset(Point offset)
        {
            Rect.Location = offset;
        }

        public bool SetDrawColor(int brickColorId)
        {
            if (BrickColors.NumericalSearch.ContainsKey(brickColorId))
            {
                BrickColor bc = BrickColors.NumericalSearch[brickColorId];
                DrawColor = bc.Color;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int CompareTo(object other)
        {
            if (other is CompositData)
            {
                CompositData otherComp = other as CompositData;
                return (Layer - otherComp.Layer);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Vertex[] GetGuideVerts(int faceIndex)
        {
            int[] face = Guide.Faces[faceIndex];

            int a = face[0];
            int b = face[1];
            int c = face[2];

            return new Vertex[3]
            {
                Guide.Verts[a],
                Guide.Verts[b],
                Guide.Verts[c]
            };
        }

        public Bitmap GetTextureBitmap()
        {
            if (Texture is Asset)
            {
                Asset asset = Texture as Asset;
                long assetId = asset.Id;

                if (!TextureAlloc.ContainsKey(assetId))
                {
                    byte[] buffer = asset.GetContent();

                    TextureAllocation alloc = new TextureAllocation();
                    alloc.Stream = new MemoryStream(buffer);
                    alloc.Texture = Image.FromStream(alloc.Stream) as Bitmap;

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
