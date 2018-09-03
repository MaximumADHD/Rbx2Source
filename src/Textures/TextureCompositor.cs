using System.Collections.Generic;
using System.Drawing;

using Rbx2Source.Geometry;
using Rbx2Source.Reflection;
using Rbx2Source.Web;

namespace Rbx2Source.Textures
{
    class TextureCompositor
    {
        private List<CompositData> layers = new List<CompositData>();
        private Rectangle canvas;
        private AvatarType avatarType;
        private string context = "Humanoid Texture Map";
        private int composed;

        public Folder CharacterAssets;

        public TextureCompositor(AvatarType at, int width, int height)
        {
            avatarType = at;
            canvas = new Rectangle(0, 0, width, height);
        }

        public TextureCompositor(AvatarType at, Rectangle rect)
        {
            avatarType = at;
            canvas = rect;
        }

        public void AppendColor(int brickColorId, string guide, Rectangle guideSize, byte layer = 0)
        {
            CompositData composit = new CompositData(DrawMode.Guide, DrawType.Color);
            composit.SetDrawColor(brickColorId);
            composit.SetGuide(guide, guideSize, avatarType);
            composit.Layer = layer;

            layers.Add(composit);
        }

        public void AppendTexture(object img, string guide, Rectangle guideSize, byte layer = 0)
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

        public void AppendTexture(object img, Rectangle rect, byte layer = 0, RotateFlipType flipMode = RotateFlipType.RotateNoneFlipNone)
        {
            CompositData composit = new CompositData(DrawMode.Rect, DrawType.Texture);
            composit.Texture = img;
            composit.Layer = layer;
            composit.Rect = rect;
            composit.FlipMode = flipMode;

            layers.Add(composit);
        }

        public void SetContext(string newContext)
        {
            context = newContext;
        }

        public Bitmap BakeTextureMap()
        {
            Bitmap bitmap = new Bitmap(canvas.Width, canvas.Height);
            layers.Sort();

            composed = 0;

            Rbx2Source.Print("Composing " + context + "...");
            Rbx2Source.IncrementStack();

            foreach (CompositData composit in layers)
            {
                Graphics buffer = Graphics.FromImage(bitmap);

                DrawMode drawMode = composit.DrawMode;
                DrawType drawType = composit.DrawType;

                Rectangle compositCanvas = composit.Rect;

                if (drawMode == DrawMode.Rect)
                {
                    if (drawType == DrawType.Color)
                    {
                        using (Brush brush = new SolidBrush(composit.DrawColor))
                            buffer.FillRectangle(brush, compositCanvas);
                    }
                    else if (drawType == DrawType.Texture)
                    {
                        Bitmap image = composit.GetTextureBitmap();
                        if (composit.FlipMode > 0)
                            image.RotateFlip(composit.FlipMode);

                        buffer.DrawImage(image, compositCanvas);
                    }
                }
                else if (drawMode == DrawMode.Guide)
                {
                    Mesh guide = composit.Guide;

                    for (int face = 0; face < guide.FaceCount; face++)
                    {
                        Vertex[] verts = composit.GetGuideVerts(face);
                        Point offset = compositCanvas.Location;

                        Point vert_a = CompositUtil.VertexToPoint(verts[0], compositCanvas, offset);
                        Point vert_b = CompositUtil.VertexToPoint(verts[1], compositCanvas, offset);
                        Point vert_c = CompositUtil.VertexToPoint(verts[2], compositCanvas, offset);

                        Point[] polygon = new Point[3] { vert_a, vert_b, vert_c };

                        if (drawType == DrawType.Color)
                        {
                            using (Brush brush = new SolidBrush(composit.DrawColor))
                                buffer.FillPolygon(brush, polygon);
                        }
                        else if (drawType == DrawType.Texture)
                        {
                            Bitmap texture = composit.GetTextureBitmap();
                            Rectangle bbox = CompositUtil.GetBoundingBox(vert_a, vert_b, vert_c);

                            Point origin = bbox.Location;
                            int width = bbox.Width;
                            int height = bbox.Height;

                            Bitmap drawLayer = new Bitmap(width, height);

                            Point uv_a = CompositUtil.VertexToUV(verts[0], texture);
                            Point uv_b = CompositUtil.VertexToUV(verts[1], texture);
                            Point uv_c = CompositUtil.VertexToUV(verts[2], texture);

                            for (int x = bbox.Left; x < bbox.Right; x++)
                            {
                                for (int y = bbox.Top; y < bbox.Bottom; y++)
                                {
                                    Point pixel = new Point(x, y);
                                    BarycentricPoint bcPixel = CompositUtil.ToBarycentric(pixel, vert_a, vert_b, vert_c);

                                    if (CompositUtil.InTriangle(bcPixel))
                                    {
                                        Point uvPixel = CompositUtil.ToCartesian(bcPixel, uv_a, uv_b, uv_c);
                                        Color color = texture.GetPixel(uvPixel.X, uvPixel.Y);
                                        drawLayer.SetPixel(x - origin.X, y - origin.Y, color);
                                    }
                                }
                            }

                            buffer.DrawImage(drawLayer, origin);
                            drawLayer.Dispose();
                        }
                    }
                }

                Rbx2Source.Print("{0}/{1} layers composed...", ++composed, layers.Count);

                if (layers.Count > 2)
                    Rbx2Source.SetDebugImage(bitmap);

                buffer.Dispose();
            }

            Rbx2Source.Print("Done!");
            Rbx2Source.DecrementStack();

            return bitmap;
        }

        public static Bitmap CropBitmap(Bitmap src, Rectangle crop)
        {
            Bitmap target = new Bitmap(crop.Width, crop.Height);

            Graphics graphics = Graphics.FromImage(target);
            graphics.DrawImage(src, -crop.X, -crop.Y);
            graphics.Dispose();

            return target;
        }

        public Bitmap BakeTextureMap(Rectangle crop)
        {
            Bitmap src = BakeTextureMap();
            return CropBitmap(src, crop);
        }
    }
}