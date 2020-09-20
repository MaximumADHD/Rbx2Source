using RobloxFiles;
using RobloxFiles.DataTypes;

namespace Rbx2Source.Geometry
{
    public class MeshBone : Instance
    {
        public int NameIndex;

        public short Id;
        public short ParentId;

        public float Unknown;
        public CFrame CFrame;

        public override string ToString()
        {
            return $"[Bone: {Name}]";
        }
    }
}
