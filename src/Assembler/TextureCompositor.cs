using System;
using System.Collections.Generic;
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
        private Timer timer;
        private int composed;

        public TextureCompositor(AvatarType avatar_type, int width, int height)
        {
            avatarType = avatar_type;

            layers = new List<CompositData>();
            canvas = new Rectangle(0, 0, width, height);
            
            timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(onTimerElapsed);
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

        private void onTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Rbx2Source.Print("{0}/{1} ...", composed, layers.Count);
        }

        private PointF VertexToUV(Vertex vert)
        {
            Vector3 uv = vert.UV;
            float x = uv.x * canvas.Width;
            float y = uv.y * canvas.Height;
            return new PointF(x, y);
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

        private static bool PointInTriangle(PointF p, PointF p0, PointF p1, PointF p2)
        {
            float s = p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X + (p0.X - p2.X) * p.Y;
            float t = p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X + (p1.X - p0.X) * p.Y;

            if ((s < 0) != (t < 0))
                return false;

            var a = -p1.Y * p2.X + p0.Y * (p2.X - p1.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y;
            if (a < 0.0)
            {
                s = -s;
                t = -t;
                a = -a;
            }

            return s > 0 && t > 0 && (s + t) <= a;
        }

        private static PointF ProjectToLine(PointF p, PointF line0, PointF line1)
        {
            float m = (line1.Y - line0.Y) / (line1.X / line0.X);
            float b = (line0.Y - (m * line0.X));

            float x = (m * p.Y + p.X - m * b) / (m * m + 1);
            float y = (m * m * p.Y + m * p.X + b) / (m * m + 1);

            return new PointF(x, y);
        }

        private static double MagnitudeOf(PointF p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        private static PointF Subtract(PointF a, PointF b)
        {
            return new PointF(a.X - b.X, a.Y - b.Y);
        }

        private static double DistAlongLine(PointF p, PointF line0, PointF line1)
        {
            PointF online = ProjectToLine(p, line0, line1);

            PointF a = Subtract(online, line0);
            PointF b = Subtract(line1, line0);

            return MagnitudeOf(a) / MagnitudeOf(b);
        }

        private static PointF Lerp(PointF a, PointF b, float t)
        {
            float x = a.X + ((b.X - a.X) * t);
            float y = a.Y + ((b.Y - a.Y) * t);

            return new PointF(x, y);
        }

        private static RectangleF GetBoundingBox(params PointF[] points)
        {
            float min_X =  10e7f;
            float min_Y =  10e7f;
            float max_X = -10e7f;
            float max_Y = -10e7f;

            foreach (PointF point in points)
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

            float width = max_X - min_X;
            float height = max_Y - min_Y;

            return new RectangleF(min_X, min_Y, width, height);
        }

        public Bitmap BakeTextureMap()
        {
            Bitmap bitmap = new Bitmap(canvas.Width, canvas.Height);
            layers.Sort();

            composed = 0;
            timer.Start();

            Rbx2Source.Print("Composing Humanoid Texture Map...");
            Rbx2Source.IncrementStack();

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
                        Image image = CompositData.GetTextureBuffer(composit.Texture);
                        buffer.DrawImage(image, canvas);
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
                            Point[] polygon = new Point[3] { vert_a, vert_b, vert_c };
                            using (Brush brush = new SolidBrush(composit.DrawColor))
                                buffer.FillPolygon(brush, polygon);
                        }
                        else if (drawType == DrawType.Texture)
                        {
                            using (SolidBrush brush = new SolidBrush(Color.Black))
                            {
                                Pen pen = new Pen(brush);

                                //PointF uv_a = VertexToUVPoint(verts[0]);
                                //PointF uv_b = VertexToUVPoint(verts[1]);
                                //PointF uv_c = VertexToUVPoint(verts[2]);

                                //buffer.DrawLine(pen, uv_a, uv_b);
                                //buffer.DrawLine(pen, uv_b, uv_c);
                                //buffer.DrawLine(pen, uv_c, uv_a);

                                RectangleF bbox = GetBoundingBox(vert_a, vert_b, vert_c);

                                for (float x = bbox.Left; x <= bbox.Right; x++)
                                {
                                    for (float y = bbox.Top; y < bbox.Bottom; y++)
                                    {
                                        PointF pixel = new PointF(x, y);

                                        if (PointInTriangle(pixel, vert_a, vert_b, vert_c))
                                        {
                                            if ((x+y) % 2 == 0)
                                            {
                                                bitmap.SetPixel((int)x, (int)y, Color.Purple);
                                            }
                                            else
                                            {
                                                bitmap.SetPixel((int)x, (int)y, Color.Black);
                                            }
                                        }
                                    }
                                }

                                //buffer.DrawLine(pen, vert_a, vert_b);
                                //buffer.DrawLine(pen, vert_b, vert_c);
                                //buffer.DrawLine(pen, vert_c, vert_a);
                            }
                        }
                    }

                    composed++;
                }

                buffer.Dispose();
            }

            timer.Stop();

            Rbx2Source.Print("Done!");
            Rbx2Source.DecrementStack();

            CompositData.FreeAllocatedTextures();

            return bitmap;
        }
    }
}
