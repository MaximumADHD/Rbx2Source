using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Rbx2Source.Geometry;
using RobloxFiles;
using Rbx2Source.Web;

namespace Rbx2Source.Textures
{
    public class TextureCompositor
    {
        private readonly List<CompositData> layers = new List<CompositData>();
        private string context = "Humanoid Texture Map";
        private readonly AvatarType avatarType;
        private Rectangle canvas;
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

        public static Rectangle GetBoundingBox(params Point[] points)
        {
            int min_X = int.MaxValue,
                min_Y = int.MaxValue;

            int max_X = int.MinValue,
                max_Y = int.MinValue;

            foreach (Point point in points)
            {
                int point_X = point.X,
                    point_Y = point.Y;

                min_X = Math.Min(min_X, point_X);
                min_Y = Math.Min(min_Y, point_Y);

                max_X = Math.Max(max_X, point_X);
                max_Y = Math.Max(max_Y, point_Y);
            }

            int width  = max_X - min_X,
                height = max_Y - min_Y;

            return new Rectangle(min_X, min_Y, width, height);
        }

        public void AppendColor(int brickColorId, string guide, Rectangle guideSize, byte layer = 0)
        {
            var composit = new CompositData(DrawFlags.Guide | DrawFlags.Color);
            composit.SetGuide(guide, guideSize, avatarType);
            composit.SetDrawColor(brickColorId);
            composit.Layer = layer;

            layers.Add(composit);
        }

        public void AppendTexture(object img, string guide, Rectangle guideSize, byte layer = 0)
        {
            var composit = new CompositData(DrawFlags.Guide | DrawFlags.Texture);
            composit.SetGuide(guide, guideSize, avatarType);
            composit.Texture = img;
            composit.Layer = layer;

            layers.Add(composit);
        }

        public void AppendColor(int brickColorId, Rectangle rect, byte layer = 0)
        {
            var composit = new CompositData(DrawFlags.Rect | DrawFlags.Color);
            composit.SetDrawColor(brickColorId);
            composit.Layer = layer;
            composit.Rect = rect;

            layers.Add(composit);
        }

        public void AppendTexture(object img, Rectangle rect, byte layer = 0, RotateFlipType flipMode = RotateFlipType.RotateNoneFlipNone)
        {
            var composit = new CompositData(DrawFlags.Rect | DrawFlags.Texture)
            {
                FlipMode = flipMode,
                Texture = img,
                Layer = layer,
                Rect = rect
            };

            layers.Add(composit);
        }

        public void SetContext(string newContext)
        {
            context = newContext;
        }

        public Bitmap BakeTextureMap()
        {
            var bitmap = new Bitmap(canvas.Width, canvas.Height);
            layers.Sort();

            composed = 0;

            Rbx2Source.Print("Composing " + context + "...");
            Rbx2Source.IncrementStack();

            foreach (CompositData composit in layers)
            {
                var buffer = Graphics.FromImage(bitmap);
                var drawFlags = composit.DrawFlags;
                var canvas = composit.Rect;

                if (drawFlags.HasFlag(DrawFlags.Rect))
                {
                    if (drawFlags.HasFlag(DrawFlags.Color))
                    {
                        composit.UseBrush(brush => buffer.FillRectangle(brush, canvas));
                    }
                    else if (drawFlags.HasFlag(DrawFlags.Texture))
                    {
                        Bitmap image = composit.GetTextureBitmap();

                        if (composit.FlipMode > 0)
                            image.RotateFlip(composit.FlipMode);

                        buffer.DrawImage(image, canvas);
                    }
                }
                else if (drawFlags.HasFlag(DrawFlags.Guide))
                {
                    Mesh guide = composit.Guide;

                    for (int face = 0; face < guide.NumFaces; face++)
                    {
                        Vertex[] verts = composit.GetGuideVerts(face);
                        Point offset = canvas.Location;
                        
                        Point[] poly = verts
                            .Select(vert => vert.ToPoint(canvas, offset))
                            .ToArray();
                        
                        if (drawFlags.HasFlag(DrawFlags.Color))
                        {
                            composit.UseBrush(brush => buffer.FillPolygon(brush, poly));
                        }
                        else if (drawFlags.HasFlag(DrawFlags.Texture))
                        {
                            Bitmap texture = composit.GetTextureBitmap();
                            Rectangle bbox = GetBoundingBox(poly);

                            Point origin = bbox.Location;
                            Bitmap drawLayer = new Bitmap(bbox.Width, bbox.Height);

                            Point[] uv = verts
                                .Select(vert => vert.ToUV(texture))
                                .ToArray();
                            
                            int origin_X = origin.X, 
                                origin_Y = origin.Y;

                            for (int x = bbox.Left; x < bbox.Right; x++)
                            {
                                for (int y = bbox.Top; y < bbox.Bottom; y++)
                                {
                                    var pixel = new Point(x, y);
                                    var bcPoint = new BarycentricPoint(pixel, poly);

                                    if (bcPoint.InBounds())
                                    {
                                        var uvPixel = bcPoint.ToCartesian(uv);
                                        Color color = texture.GetPixel(uvPixel.X, uvPixel.Y);
                                        drawLayer.SetPixel(x - origin_X, y - origin_Y, color);
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

            using (Graphics graphics = Graphics.FromImage(target))
                graphics.DrawImage(src, -crop.X, -crop.Y);

            return target;
        }

        public Bitmap BakeTextureMap(Rectangle crop)
        {
            Bitmap result;

            using (Bitmap src = BakeTextureMap())
                result = CropBitmap(src, crop);

            return result;
        }
    }
}
