using System.Collections.Generic;

using Rbx2Source.Reflection;
using Rbx2Source.StudioMdl;

namespace Rbx2Source.Assembler
{
    public class BoneAssemblePrep
    {
        public List<Attachment> NonRigs = new List<Attachment>();
        public List<Attachment> Completed = new List<Attachment>();

        public List<Bone> Bones;
        public List<Node> Nodes;

        public bool AllowNonRigs;

        public BoneAssemblePrep(ref List<Bone> bones, ref List<Node> nodes)
        {
            Bones = bones;
            Nodes = nodes;
        }
    }
}
