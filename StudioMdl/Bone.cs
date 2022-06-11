using Rbx2Source.StudioMdl;

using RobloxFiles;
using RobloxFiles.DataTypes;

namespace RobloxFiles
{
    public class StudioBone : JointInstance
    {
        public Node Node;
        public bool IsAvatarBone;

        public StudioBone(string name, BasePart parent, BasePart attachTo = null)
        {
            Name = name;
            Node = new Node() { StudioBone = this };
            
            Part0 = parent;
            Part1 = attachTo ?? parent;

            if (Part1 != null)
                Node.Name = Part1.Name;

            C0 = new CFrame();
            C1 = new CFrame();

            Parent = parent;
        }

        public StudioBone(Node node, CFrame interp)
        {
            Node = node;
            C0 = interp;
            C1 = new CFrame();
        }
    }
}
