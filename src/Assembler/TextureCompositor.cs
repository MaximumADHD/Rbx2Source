using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Rbx2Source.Coordinates;
using Rbx2Source.Geometry;
using Rbx2Source.Reflection;
using Rbx2Source.Web;

namespace Rbx2Source.Assembler
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

    class CompositData : IComparable
    {
        public DrawMode DrawMode { get; private set; }
        public DrawType DrawType { get; private set; }

        public Asset Texture;
        public Color DrawColor;

        public Mesh Guide;
        public int Layer;

        public Rectangle Rect;

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
    }

    class TextureCompositor
    {
        private List<CompositData> layers = new List<CompositData>();
        private Rectangle canvas;
        private AvatarType avatarType;

        public TextureCompositor(AvatarType avType, int width, int height)
        {
            avatarType = avType;
            canvas = new Rectangle(0, 0, width, height);
        }

        public void AppendColor(int brickColorId, string guide, Rectangle guideSize, byte layer = 0)
        {
            CompositData composit = new CompositData(DrawMode.Guide, DrawType.Color);
            composit.SetDrawColor(brickColorId);
            composit.SetGuide(guide, guideSize, avatarType);
            composit.Layer = layer;

            layers.Add(composit);
        }

        public void AppendTexture(Asset img, string guide, Rectangle guideSize, byte layer = 0)
        {
            CompositData composit = new CompositData(DrawMode.Guide, DrawType.Texture);
            composit.SetGuide(guide, guideSize, avatarType);
            composit.Texture = img;
            composit.Layer = layer;

            layers.Add(composit);
        }

        public void AppendColor(int brickColorId, Rectangle rect, byte layer = 0)
        {
            CompositData composit = new CompositData(DrawMode.Rect, DrawType.Color);
            composit.SetDrawColor(brickColorId);
            composit.Layer = layer;
            composit.Rect = rect;

            layers.Add(composit);
        }

        public void AppendTexture(Asset img, Rectangle rect, byte layer = 0)
        {
            CompositData composit = new CompositData(DrawMode.Rect, DrawType.Texture);
            composit.Texture = img;
            composit.Layer = layer;
            composit.Rect = rect;

            layers.Add(composit);
        }

        public PointF VertexToUVPoint(Vertex vert)
        {
            Vector3 uv = vert.UV;
            float x = uv.x * canvas.Width;
            float y = uv.y * canvas.Height;
            return new PointF(x, y);
        }

        public Point VertexToPoint(Vertex vert)
        {
            int x = (int)vert.Pos.x;
            int y = (int)vert.Pos.y;

            return new Point(x, y);
        }

        public Point VertexToPoint(Vertex vert, Rectangle canvas, Point offset)
        {
            int x = (int)vert.Pos.x;
            int y = (int)vert.Pos.y;

            Point result = new Point(offset.X + x, (offset.Y - y) + canvas.Height);
            return result;
        }

        public Bitmap BakeTextureMap()
        {
            Bitmap bitmap = new Bitmap(canvas.Width, canvas.Height);
            layers.Sort();

            foreach (CompositData composit in layers)
            {
                Graphics buffer = Graphics.FromImage(bitmap);

                DrawMode drawMode = composit.DrawMode;
                DrawType drawType = composit.DrawType;

                Rectangle canvas = composit.Rect;

                if (drawMode == DrawMode.Rect)
                {
                    if (drawType == DrawType.Color)
                    {
                        using (Brush brush = new SolidBrush(composit.DrawColor))
                            buffer.FillRectangle(brush, canvas);
                    }
                    else if (drawType == DrawType.Texture)
                    {
                        Asset texture = composit.Texture;
                        byte[] payload = texture.GetContent();

                        using (MemoryStream stream = new MemoryStream(payload))
                        {
                            Image image = Image.FromStream(stream);
                            buffer.DrawImage(image, canvas);
                        }
                    }
                }
                else if (drawMode == DrawMode.Guide)
                {
                    Mesh guide = composit.Guide;

                    for (int f = 0; f < guide.FaceCount; f++)
                    {
                        Vertex[] verts = composit.GetGuideVerts(f);
                        Point offset = canvas.Location;

                        Point vert_a = VertexToPoint(verts[0], canvas, offset);
                        Point vert_b = VertexToPoint(verts[1], canvas, offset);
                        Point vert_c = VertexToPoint(verts[2], canvas, offset);

                        if (drawType == DrawType.Color)
                        {
                            Point[] poly = new Point[3] { A, B, C };
                            using (Brush brush = new SolidBrush(composit.DrawColor))
                                buffer.FillPolygon(brush, poly);
                        }
                        else if (drawType == DrawType.Texture)
                        {
                            Asset texture = composit.Texture;
                            byte[] payload = texture.GetContent();

                            using (MemoryStream stream = new MemoryStream(payload))
                            {
                                Image image = Image.FromStream(stream);
                                using (TextureBrush brush = new TextureBrush(image))
                                {
                                    Pen pen = new Pen(brush);

                                    PointF uvA = GetUVPoint(verts[0]);
                                    PointF uvB = GetUVPoint(verts[1]);
                                    PointF uvC = GetUVPoint(verts[2]);

                                    buffer.DrawPolygon(pen, 
                                    buffer.DrawImage(image, canvas);
                                }
                            }
                        }

                    }
                }

                buffer.Dispose();
            }

            return bitmap;
        }
    }
}
