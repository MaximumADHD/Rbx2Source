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

    class BarycentricPoint
    {
        public double U;
        public double V;
        public double W;
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
        private List<CompositData> layers = new List<CompositData>();
        private Rectangle canvas;
        private AvatarType avatarType;
        private int composed;

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
            int x = (int)vert.Pos.X;
            int y = (int)vert.Pos.Y;

            return new Point(x, y);
        }

        private static Point VertexToPoint(Vertex vert, Rectangle canvas, Point offset)
        {
            int x = (int)vert.Pos.X;
            int y = (int)vert.Pos.Y;

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

        public Bitmap ComputeScanlineMask(Rectangle canvas, Rectangle bbox, Point[] points)
        {
            Bitmap mask = new Bitmap(canvas.Width, canvas.Height);
            Graphics graphics = Graphics.FromImage(mask);

            using (SolidBrush black = new SolidBrush(Color.Black))
            {
                graphics.FillPolygon(black, points);
                graphics.Dispose();
            }

            /*var scanlineMask = new Dictionary<int, Tuple<int, int>>();

            for (int y = bbox.Top; y < bbox.Bottom; y++)
            {
                int x0 = bbox.Left;
                int x1 = bbox.Right;

                while (mask.GetPixel(x0, y).A == 0 && x0 < x1)
                    x0++;
                
                while (mask.GetPixel(x1, y).A == 0 && x1 > x0)
                    x1--;

                int width = (x1 - x0);

                if (width > 0)
                {
                    var x = new Tuple<int, int>(x0, x1);
                    scanlineMask.Add(y, x);
                }
            }

            mask.Dispose();
            return scanlineMask;*/

            return mask;
        }

        public bool InTriangle(BarycentricPoint bc)
        {
            // Use int approximation to avoid floating point errors.
            int u = (int)bc.U * 10000;
            int v = (int)bc.V * 10000;

            return u >= 0 && v >= 0 && u+v <= 10000;
        }

        public double Dot(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public BarycentricPoint ToBarycentric(Point p, Point a, Point b, Point c)
        {
            Point v0 = Subtract(b, a);
            Point v1 = Subtract(c, a);
            Point v2 = Subtract(p, a);

            double d00 = Dot(v0, v0);
            double d01 = Dot(v0, v1);
            double d11 = Dot(v1, v1);
            double d20 = Dot(v2, v0);
            double d21 = Dot(v2, v1);

            double denom = d00 * d11 - d01 * d01;

            BarycentricPoint bp = new BarycentricPoint();
            bp.V = (d11 * d20 - d01 * d21) / denom;
            bp.W = (d00 * d21 - d01 * d20) / denom;
            bp.U = 1.0 - bp.V - bp.W;

            return bp;
        }

        public Point ToCartesian(BarycentricPoint bp, Point a, Point b, Point c)
        {
            double x = (a.X * bp.U) + (b.X * bp.V) + (c.X * bp.W);
            double y = (a.Y * bp.U) + (b.Y * bp.V) + (c.Y * bp.W);

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
                            Bitmap texture = CompositData.GetTextureBuffer(composit.Texture) as Bitmap;

                            Rectangle bbox = GetBoundingBox(vert_a, vert_b, vert_c);
                            //var scanlineMask = ComputeScanlineMask(canvas, bbox, polygon);

                            Point origin = compositCanvas.Location;
                            int width = compositCanvas.Width;
                            int height = compositCanvas.Height;

                            Bitmap drawLayer = new Bitmap(width, height);

                            Point uv_a = VertexToUV(verts[0], texture);
                            Point uv_b = VertexToUV(verts[1], texture);
                            Point uv_c = VertexToUV(verts[2], texture);
                            
                            for (int y = bbox.Top; y < bbox.Bottom; y++)
                            {
                                //bool scanY = scanlineMask.ContainsKey(y);

                                for (int x = bbox.Left; x < bbox.Right; x++)
                                {
                                    Point pixel = new Point(x, y);
                                    BarycentricPoint bcPixel = ToBarycentric(pixel, vert_a, vert_b, vert_c);

                                    bool valid = InTriangle(bcPixel);
                                    bool passedByColor = false;

                                    /*if (!valid)
                                    {
                                        var scanline = scanlineMask[y];
                                        int left = scanline.Item1;
                                        int right = scanline.Item2;
                                        valid = (left <= x && x <= right);
                                        Color check = scanlineMask.GetPixel(x, y);
                                        valid = (check.A > 0);
                                        passedByColor = valid;
                                    }*/

                                    if (valid)
                                    {
                                        if (passedByColor)
                                            Debugger.Break();

                                        Point uvPixel = ToCartesian(bcPixel, uv_a, uv_b, uv_c);
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
                Rbx2Source.SetDebugImage(bitmap);

                buffer.Dispose();
            }

            Rbx2Source.Print("Done!");
            Rbx2Source.DecrementStack();

            CompositData.FreeAllocatedTextures();

            return bitmap;
        }
    }
}