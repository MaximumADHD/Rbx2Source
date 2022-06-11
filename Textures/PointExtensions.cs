namespace System.Drawing
{
    public static class PointExtensions
    {
        public static double Dot(this Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Point Subtract(this Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }
    }
}
