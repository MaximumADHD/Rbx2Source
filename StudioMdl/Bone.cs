using Rbx2Source.StudioMdl;

using RobloxFiles;
using RobloxFiles.DataTypes;

namespace RobloxFiles
{
    public class StudioBone : JointInstance
    {
        public Node Node;
        public bool IsAvatarBone;

        public int NodeIndex => Node?.Index ?? -1;

        public StudioBone(string name, BasePart part)
        {
            Name = name;

            Node = new Node(this);
            Node.Name = name;

            Part0 = part;
            Part1 = part;

            Parent = part;
        }

        public StudioBone(Attachment a0, Attachment a1)
        {
            Part0 = a0.Parent as BasePart;
            Part1 = a1.Parent as BasePart;

            Name = Part1.Name;
            Node = new Node(this);
            
            C0 = a0.CFrame;
            C1 = a1.CFrame;

            Node.Name = Name;
            Parent = Part1;
        }

        public StudioBone(Node node, CFrame interp)
        {
            Node = node;
            C0 = interp;
            C1 = new CFrame();
        }
    }
}
