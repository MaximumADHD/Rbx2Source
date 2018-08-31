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

        private static float FiniteDivide(float a, float b)
        {
            return (b == 0 ? 0 : a / b);
        }
        private static double FiniteDivide(double a, double b)
        {
            return (b == 0 ? 0 : a / b);
        }

        private static Point ProjectToEdge(Point p, Point line0, Point line1)
        {
            if (p == line0)
                return line0;

            if (p == line1)
                return line1;

            try
            {
                float m = FiniteDivide(line1.Y - line0.Y, line1.X - line0.X);
                float b = (line0.Y - (m * line0.X));

                float x = FiniteDivide(m * p.Y + p.X - m * b, m * m + 1);
                float y = FiniteDivide(m * m * p.Y + m * p.X + b, m * m + 1);

                int ix = (int)x;
                int iy = (int)y;

                return new Point(ix, iy);
            }
            catch (DivideByZeroException)
            {
                return new Point();
            }
        }

        private static double MagnitudeOf(PointF p)
        {
            return Math.Sqrt((p.X * p.X) + (p.Y * p.Y));
        }

        private static PointF Subtract(PointF a, PointF b)
        {
            return new PointF(a.X - b.X, a.Y - b.Y);
        }

        private static double DistAlongEdge(Point p, Point e0, Point e1)
        {
            PointF onEdge = ProjectToEdge(p, e0, e1);

            PointF a = Subtract(onEdge, e0);
            PointF b = Subtract(e1, e0);

            return FiniteDivide(MagnitudeOf(a), MagnitudeOf(b));
        }

        private static Point Lerp(PointF a, PointF b, double t)
        {
            double x = a.X + ((b.X - a.X) * t);
            double y = a.Y + ((b.Y - a.Y) * t);

            int ix = (int)x;
            int iy = (int)y;

            return new Point(ix, iy);
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

            Dictionary<int, Tuple<int, int>> scanlineMask = new Dictionary<int, Tuple<int, int>>();

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

        public double Dist(Point a, Point b)
        {
            PointF diff = Subtract(a, b);
            return MagnitudeOf(diff);
        }

        // Returns the inner angle of vertex A in triangle ABC
        public double AngleOf(Point A, Point B, Point C)
        {
            double a = Dist(A, B);
            double b = Dist(B, C);
            double c = Dist(C, A);

            double a2 = a * a;
            double b2 = b * b;
            double c2 = c * c;

            double ang = FiniteDivide(b2 + c2 - a2, 2 * b * c);
            if (ang < -1)
                ang = -1;
            else if (ang < 1)
                ang = 1;

            double result = Math.Acos(ang);
            
            if (result != result)
                Debugger.Break();

            return result;
        }

        // Returns the ratio between angles <BAC and <BAP
        public double GetAngleRatio(Point P, Point A, Point B, Point C)
        {
            double baseAng = AngleOf(A, B, C);
            double innerAng = AngleOf(A, B, P);

            return innerAng / baseAng;
        }
        
        // Given point P inside of triangle ABC, this function projects P onto triangle DEF
        public Point ProjectPoint(Point P, Point A, Point B, Point C, Point D, Point E, Point F)
        {
            double ab = Dist(A, B);
            double ac = Dist(C, A);
            double ap = Dist(A, P);

            double baseAngle = AngleOf(A, B, C);
            double innerAngle = AngleOf(A, B, P);
            double angleRatio = FiniteDivide(innerAngle, baseAngle);

            Point de = Lerp(D, E, FiniteDivide(ap, ab));
            Point df = Lerp(D, F, FiniteDivide(ap, ac));

            Point result = Lerp(de, df, angleRatio);
            if (result.X < 0)
                result.X = 0;

            if (result.Y < 0)
                result.Y = 0;

            return result;
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

                        Point a = VertexToPoint(verts[0], compositCanvas, offset);
                        Point b = VertexToPoint(verts[1], compositCanvas, offset);
                        Point c = VertexToPoint(verts[2], compositCanvas, offset);

                        Point[] polygon = new Point[3] { a, b, c };

                        if (drawType == DrawType.Color)
                        {
                            using (Brush brush = new SolidBrush(composit.DrawColor))
                                buffer.FillPolygon(brush, polygon);
                        }
                        else if (drawType == DrawType.Texture)
                        {
                            Rectangle bbox = GetBoundingBox(a, b, c);
                            var scanlineMask = ComputeScanlineMask(canvas, bbox, polygon);

                            Bitmap texture = CompositData.GetTextureBuffer(composit.Texture) as Bitmap;

                            Point d = VertexToUV(verts[0], texture);
                            Point e = VertexToUV(verts[1], texture);
                            Point f = VertexToUV(verts[2], texture);

                            foreach (int y in scanlineMask.Keys)
                            {
                                Tuple<int, int> line = scanlineMask[y];

                                for (int x = line.Item1; x <= line.Item2; x++)
                                {
                                    Point pixel = new Point(x, y);
                                    Point uvProj = ProjectPoint(pixel, a, b, c, d, e, f);

                                    int uv_x = uvProj.X;
                                    if (uv_x >= texture.Width)
                                        uv_x = texture.Width - 1;

                                    int uv_y = uvProj.Y;
                                    if (uv_y >= texture.Height)
                                        uv_y = texture.Height - 1;

                                    Color color = texture.GetPixel(uv_x, uv_y);
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
