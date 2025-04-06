using RobloxFiles;
using RobloxFiles.DataTypes;

namespace Rbx2Source.Geometry
{
    public class MeshBone : Instance
    {
        public int NameIndex;

        public short ParentIndex;
        public short LodParentIndex;

        public float Culling;
        public CFrame CFrame;

        public override string ToString()
        {
            return $"[Bone: {Name}]";
        }
    }
}
