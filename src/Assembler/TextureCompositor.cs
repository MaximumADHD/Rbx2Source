using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Timers;

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

    class TextureAllocation
    {
        public MemoryStream Stream;
        public Image Texture;
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

        public static Image GetTextureBuffer(Asset asset)
        {
            long assetId = asset.Id;

            if (!TextureAlloc.ContainsKey(assetId))
            {
                byte[] buffer = asset.GetContent();

                TextureAllocation alloc = new TextureAllocation();
                alloc.Stream = new MemoryStream(buffer);
                alloc.Texture = Image.FromStream(alloc.Stream);

                TextureAlloc.Add(assetId, alloc);
            }

            return TextureAlloc[assetId].Texture;
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

    class TextureCompositor
    {
        private List<CompositData> layers;
        private Rectangle canvas;
        private AvatarType avatarType;
        private int composed;

        public TextureCompositor(AvatarType avatar_type, int width, int height)
        {
            avatarType = avatar_type;

            layers = new List<CompositData>();
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

        private Point VertexToUV(Vertex vert, Bitmap target)
        {
            Vector3 uv = vert.UV;

            float x = uv.x * target.Width;
            float y = uv.y * target.Height;

            int ix = (int)x;
            int iy = (int)y;

            return new Point(ix, iy);
        }

        private static Point VertexToPoint(Vertex vert)
        {
            int x = (int)vert.Pos.x;
            int y = (int)vert.Pos.y;

            return new Point(x, y);
        }

        private static Point VertexToPoint(Vertex vert, Rectangle canvas, Point offset)
        {
            int x = (int)vert.Pos.x;
            int y = (int)vert.Pos.y;

            Point result = new Point(offset.X + x, (offset.Y - y) + canvas.Height);
            return result;
        }

        private static Point Subtract(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        private static Rectangle GetBoundingBox(params Point[] points)
        {
            int min_X = int.MaxValue;
            int min_Y = int.MaxValue;

            int max_X = int.MinValue;
            int max_Y = int.MinValue;

            foreach (Point point in points)
            {
                if (point.X < min_X)
                    min_X = point.X;

                if (point.Y < min_Y)
                    min_Y = point.Y;

                if (point.X > max_X)
                    max_X = point.X;

                if (point.Y > max_Y)
                    max_Y = point.Y;
            }

            int width = max_X - min_X;
            int height = max_Y - min_Y;

            return new Rectangle(min_X, min_Y, width, height);
        }

        public Dictionary<int, Tuple<int,int>> ComputeScanlineMask(Rectangle canvas, Rectangle bbox, Point[] points)
        {
            Bitmap mask = new Bitmap(canvas.Width, canvas.Height);

            // Draw the polygon onto the mask
            Graphics graphics = Graphics.FromImage(mask);
            using (SolidBrush black = new SolidBrush(Color.Black))
            {
                graphics.FillPolygon(black, points);
                graphics.Dispose();
            }

            // Do some raycasts to compute the range of each horizontal scanline.
            // The amount of pixel lookups needed for this is dramatically reduced by scanning along the bbox.

            var scanlineMask = new Dictionary<int, Tuple<int, int>>();

            for (int y = bbox.Top; y < bbox.Bottom; y++)
            {
                int x0 = bbox.Left;
                int x1 = bbox.Right - 1;

                while (mask.GetPixel(x0, y).A == 0 && x0 < x1)
                    x0++;
                
                while (mask.GetPixel(x1, y).A == 0 && x1 > x0)
                    x1--;

                int width = (x1 - x0);

                if (width > 0)
                {
                    Tuple<int, int> x = new Tuple<int, int>(x0, x1);
                    scanlineMask.Add(y, x);
                }
            }

            mask.Dispose();
            return scanlineMask;
        }

        public float Dot(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public Vector3 Barycentric(Point p, Point a, Point b, Point c)
        {
            Point v0 = Subtract(b, a);
            Point v1 = Subtract(c, a);
            Point v2 = Subtract(p, a);

            float d00 = Dot(v0, v0);
            float d01 = Dot(v0, v1);
            float d11 = Dot(v1, v1);
            float d20 = Dot(v2, v0);
            float d21 = Dot(v2, v1);

            float denom = d00 * d11 - d01 * d01;

            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1.0f - v - w;

            return new Vector3(u, v, w);
        }

        public Point Cartesian(Vector3 bary, Point a, Point b, Point c)
        {
            float x = (a.X * bary.X) + (b.X * bary.Y) + (c.X * bary.Z);
            float y = (a.Y * bary.X) + (b.Y * bary.Y) + (c.Y * bary.Z);

            int ix = (int)x;
            int iy = (int)y;

            return new Point(ix, iy);
        }

        public Bitmap BakeTextureMap()
        {
            Bitmap bitmap = new Bitmap(canvas.Width, canvas.Height);
            layers.Sort();

            composed = 0;

            Rbx2Source.Print("Composing Humanoid Texture Map...");
            Rbx2Source.IncrementStack();

            Bitmap basePolyMask = new Bitmap(canvas.Width, canvas.Height);

            using (Graphics mask = Graphics.FromImage(basePolyMask))
            {
                using (SolidBrush black = new SolidBrush(Color.Black))
                    mask.FillRectangle(black, canvas);
            }

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
                        Image image = CompositData.GetTextureBuffer(composit.Texture);
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

                        Point vert_a = VertexToPoint(verts[0], compositCanvas, offset);
                        Point vert_b = VertexToPoint(verts[1], compositCanvas, offset);
                        Point vert_c = VertexToPoint(verts[2], compositCanvas, offset);

                        Point[] polygon = new Point[3] { vert_a, vert_b, vert_c };

                        if (drawType == DrawType.Color)
                        {
                            using (Brush brush = new SolidBrush(composit.DrawColor))
                                buffer.FillPolygon(brush, polygon);
                        }
                        else if (drawType == DrawType.Texture)
                        {
                            Rectangle bbox = GetBoundingBox(vert_a, vert_b, vert_c);
                            var scanlineMask = ComputeScanlineMask(canvas, bbox, polygon);

                            Bitmap texture = CompositData.GetTextureBuffer(composit.Texture) as Bitmap;

                            Point uv_a = VertexToUV(verts[0], texture);
                            Point uv_b = VertexToUV(verts[1], texture);
                            Point uv_c = VertexToUV(verts[2], texture);

                            foreach (int y in scanlineMask.Keys)
                            {
                                Tuple<int, int> line = scanlineMask[y];

                                for (int x = line.Item1; x <= line.Item2; x++)
                                {
                                    Point pixel = new Point(x, y);
                                    Vector3 bary = Barycentric(pixel, vert_a, vert_b, vert_c);
                                    Point uvProj = Cartesian(bary, uv_a, uv_b, uv_c);
                                    Color color = texture.GetPixel(uvProj.X, uvProj.Y);
                                    bitmap.SetPixel(x, y, color);
                                }
                            }
                        }
                    }
                }

                Rbx2Source.Print("{0}/{1} layers composed...", ++composed, layers.Count);

                string localAppData = Environment.GetEnvironmentVariable("LocalAppData");

                string debugMasks = Path.Combine(localAppData, "DebugMasks");
                Directory.CreateDirectory(debugMasks);

                string debugPath = Path.Combine(localAppData, "DebugMasks", composed + ".png");
                bitmap.Save(debugPath);

                buffer.Dispose();
            }

            Rbx2Source.Print("Done!");
            Rbx2Source.DecrementStack();

            CompositData.FreeAllocatedTextures();

            return bitmap;
        }
    }
}
