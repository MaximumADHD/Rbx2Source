using System.Collections.Generic;

using RobloxFiles;
using Rbx2Source.StudioMdl;

namespace Rbx2Source.Assembler
{
    public class BoneAssemblePrep
    {
        public List<Attachment> NonRigs = new List<Attachment>();
        public List<Attachment> Completed = new List<Attachment>();

        public List<StudioBone> Bones;
        public List<Node> Nodes;

        public bool AllowNonRigs;

        public BoneAssemblePrep(ref List<StudioBone> bones, ref List<Node> nodes)
        {
            Bones = bones;
            Nodes = nodes;
        }
    }
}
