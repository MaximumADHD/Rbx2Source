namespace Rbx2Source.Geometry
{
    public class MeshSkinning
    {
        public byte[] SubsetIndices;
        public byte[] BoneWeights;

        public override string ToString()
        {
            var subsetIndices = string.Join(", ", SubsetIndices);
            var boneWeights = string.Join(", ", BoneWeights);

            return $"{{SubsetIndices: [{subsetIndices}] | BoneWeights: [{boneWeights}]}}";
        }
    }
}
