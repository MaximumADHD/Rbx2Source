namespace Rbx2Source.Geometry
{
    public class BoneWeights
    {
        public byte[] Bones;
        public byte[] Weights;

        public override string ToString()
        {
            var bones = string.Join(", ", Bones);
            var weights = string.Join(", ", Weights);

            return $"{{Bones: [{bones}] | Weights: [{weights}]}}";
        }
    }
}
