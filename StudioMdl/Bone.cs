using Rbx2Source.DataTypes;
using Rbx2Source.StudioMdl;

namespace Rbx2Source.Reflection
{
    public class Bone : Instance
    {
        public BasePart Part0;
        public BasePart Part1;

        public CFrame C0;
        public CFrame C1;

        public Node Node;
        public bool IsAvatarBone;

        public Bone(string name, BasePart parent, BasePart attachTo = null)
        {
            Node = new Node();
            Node.Bone = this;

            Part0 = parent;
            Part1 = attachTo ?? parent;

            if (Part1 != null)
                Node.Name = Part1.Name;

            C0 = new CFrame();
            C1 = new CFrame();

            Parent = parent;
        }

        public Bone(Node node, CFrame interp)
        {
            Node = node;
            C0 = interp;
            C1 = new CFrame();
        }
    }
}
