#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using System.IO;

using RobloxFiles;
using Rbx2Source.Geometry;
using System.Diagnostics.Contracts;

namespace Rbx2Source.StudioMdl
{
    public class Node : IStudioMdlEntity<Node>
    {
        public string GroupName => "nodes";

        public int Index;
        public string Name;

        public StudioBone StudioBone;
        public Mesh Mesh;

        public int ParentIndex = -1;
        public bool UseParentIndex = false;

        public Node(StudioBone bone)
        {
            StudioBone = bone;
        }

        public override string ToString()
        {
            return Name;
        }

        private int FindParent(List<Node> nodes)
        {
            BasePart part0 = StudioBone.Part0,
                     part1 = StudioBone.Part1;

            if (part0 != part1)
            {
                Node parent = null;

                foreach (Node n in nodes)
                {
                    StudioBone b = n.StudioBone;

                    if (b != StudioBone && b.Part1 == part0)
                    {
                        parent = n;
                        break;
                    }
                }

                return nodes.IndexOf(parent);
            }

            return -1;
        }

        public void WriteStudioMdl(StringWriter fileBuffer, List<Node> nodes)
        {
            Contract.Requires(fileBuffer != null && nodes != null);

            Index = nodes.IndexOf(this);
            ParentIndex = UseParentIndex ? ParentIndex : FindParent(nodes);

            string joined = string.Join(" ", Index, '"' + Name + '"', ParentIndex);
            fileBuffer.WriteLine(joined);
        }
    }
}
