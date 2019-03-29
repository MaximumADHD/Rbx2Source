namespace Rbx2Source.Animating
{
    public class PosePair
    {
        public PoseMapEntity Min;
        public PoseMapEntity Max;

        public PosePair(int minFrame, int maxFrame)
        {
            Min = new PoseMapEntity(minFrame);
            Max = new PoseMapEntity(maxFrame);
        }
    }
}
