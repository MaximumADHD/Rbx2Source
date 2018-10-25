using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rbx2Source.Coordinates;
using Rbx2Source.Reflection;
using Rbx2Source.StudioMdl;

namespace Rbx2Source.Reflection
{
    class Bone : Instance
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
            if (attachTo != null)
                Part1 = attachTo;
            else
                Part1 = parent;

            if (Part1 != null)
                Node.Name = Part1.Name;

            C0 = new CFrame();
            C1 = new CFrame();
            Parent = parent;
        }

        public Bone(string name, int frame, CFrame cframe)
        {
            C0 = cframe;
            C1 = new CFrame();
        }
    }
}
