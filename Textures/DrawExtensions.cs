namespace System.Drawing
{
    public static class DrawExtensions
    {
        public static float Dot(this PointF a, PointF b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static PointF Subtract(this PointF a, PointF b)
        {
            return new PointF(a.X - b.X, a.Y - b.Y);
        }

        public static Color Lerp(this Color a, Color b, float alpha)
        {
            return Color.FromArgb
            (
                (int)(a.A + ((b.A - a.A) * alpha)),
                (int)(a.R + ((b.R - a.R) * alpha)),
                (int)(a.G + ((b.G - a.G) * alpha)),
                (int)(a.B + ((b.B - a.B) * alpha))
            );
        }

        public static Color GetPixel(this Bitmap bitmap, PointF point)
        {
            float x = point.X,
                  y = point.Y;

            int x0 = (int)x,
                y0 = (int)y,
                x1 = (int)(x + .5f),
                y1 = (int)(y + .5f);

            Color c00 = bitmap.GetPixel(x0, y0),
                  c01 = bitmap.GetPixel(x0, y1),
                  c10 = bitmap.GetPixel(x1, y0),
                  c11 = bitmap.GetPixel(x1, y1);

            Color c0 = c00.Lerp(c01, y - y0);
            Color c1 = c10.Lerp(c11, y - y0);

            return c0.Lerp(c1, x - x0);
        }
    }
}
