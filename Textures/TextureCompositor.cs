using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Rbx2Source.Geometry;
using RobloxFiles;
using RobloxFiles.Enums;

namespace Rbx2Source.Textures
{
    public class TextureCompositor : IDisposable
    {
        private readonly List<CompositData> layers = new List<CompositData>();
        private string context = "Humanoid Texture Map";
        private readonly HumanoidRigType avatarType;
        private Rectangle canvas;
        private int composed;

        public Folder CharacterAssets;

        public TextureCompositor(int width, int height, HumanoidRigType at = HumanoidRigType.R6, Folder characterAssets = null)
        {
            CharacterAssets = characterAssets;
            canvas = new Rectangle(0, 0, width, height);
            avatarType = at;
        }

        public TextureCompositor(Rectangle rect, HumanoidRigType at = HumanoidRigType.R6, Folder characterAssets = null)
        {
            CharacterAssets = characterAssets;
            avatarType = at;
            canvas = rect;
        }

        public static Rectangle GetBoundingBox(params PointF[] points)
        {
            int min_X = int.MaxValue,
                min_Y = int.MaxValue;

            int max_X = int.MinValue,
                max_Y = int.MinValue;

            foreach (PointF point in points)
            {
                float point_X = point.X,
                      point_Y = point.Y;

                min_X = (int)Math.Min(min_X, point_X);
                min_Y = (int)Math.Min(min_Y, point_Y);

                max_X = (int)Math.Max(max_X, point_X);
                max_Y = (int)Math.Max(max_Y, point_Y);
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

        public void AppendTexture(object img, Rectangle rect, byte layer = 0, RotateFlipType flipType = RotateFlipType.RotateNoneFlipNone)
        {
            var composit = new CompositData(DrawFlags.Rect | DrawFlags.Texture)
            {
                FlipType = flipType,
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

        public Bitmap BakeTextureMap(bool log = true)
        {
            var bitmap = new Bitmap(canvas.Width, canvas.Height);
            layers.Sort();

            composed = 0;

            if (log)
            {
                Main.Print($"Composing {context}...");
                Main.IncrementStack();
            }

            foreach (CompositData composit in layers)
            {
                var buffer = Graphics.FromImage(bitmap);
                var drawFlags = composit.DrawFlags;
                var canvas = composit.Rect;

                if (drawFlags.HasFlag(DrawFlags.Rect))
                {
                    if (drawFlags.HasFlag(DrawFlags.Color))
                    {
                        var brush = new SolidBrush(composit.DrawColor);
                        buffer.FillRectangle(brush, canvas);
                        brush.Dispose();
                    }
                    else if (drawFlags.HasFlag(DrawFlags.Texture))
                    {
                        Bitmap image = composit.GetTextureBitmap();

                        if (composit.FlipType > 0)
                            image.RotateFlip(composit.FlipType);

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
                        
                        PointF[] poly = verts
                            .Select(vert => vert.ToPoint(canvas, offset))
                            .ToArray();
                        
                        if (drawFlags.HasFlag(DrawFlags.Color))
                        {
                            buffer.FillPolygon(composit.DrawBrush, poly);
                        }
                        else if (drawFlags.HasFlag(DrawFlags.Texture))
                        {
                            Bitmap texture = composit.GetTextureBitmap();
                            Rectangle bbox = GetBoundingBox(poly);

                            Point origin = bbox.Location;
                            Bitmap drawLayer = new Bitmap(bbox.Width, bbox.Height);

                            PointF[] uv = verts
                                .Select(vert => vert.ToUV(texture))
                                .ToArray();
                            
                            int origin_X = origin.X, 
                                origin_Y = origin.Y;

                            for (int x = bbox.Left; x < bbox.Right; x++)
                            {
                                for (int y = bbox.Top; y < bbox.Bottom; y++)
                                {
                                    var pixel = new PointF(x + .5f, y + .5f);
                                    var point = new BarycentricPoint(pixel, poly);

                                    if (point.InBounds)
                                    {
                                        var uvPixel = point.ToCartesian(uv);
                                        Color color = texture.GetPixel(uvPixel);
                                        drawLayer.SetPixel(x - origin_X, y - origin_Y, color);
                                    }
                                }
                            }

                            buffer.DrawImage(drawLayer, origin);
                            drawLayer.Dispose();
                        }
                    }
                }

                if (log)
                    Main.Print($"{++composed}/{layers.Count} layers composed...");

                if (layers.Count > 2)
                    Main.SetDebugImage(bitmap);

                buffer.Dispose();
            }

            if (log)
            {
                Main.Print("Done!");
                Main.DecrementStack();
            }
            
            return bitmap;
        }

        public static Bitmap CropBitmap(Bitmap src, Rectangle crop)
        {
            Bitmap target = new Bitmap(crop.Width, crop.Height);

            using (Graphics graphics = Graphics.FromImage(target))
                graphics.DrawImage(src, -crop.X, -crop.Y);

            return target;
        }

        public Bitmap BakeTextureMap(Rectangle crop, bool log = true)
        {
            Bitmap result;

            using (Bitmap src = BakeTextureMap(log))
                result = CropBitmap(src, crop);

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (layers.Count > 0)
                {
                    var layer = layers.First();
                    layers.Remove(layer);
                    layer.Dispose();
                }
            }
        }
    }
}
