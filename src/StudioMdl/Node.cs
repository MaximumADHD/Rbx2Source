#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using System.IO;

using Rbx2Source.Reflection;
using Rbx2Source.Geometry;

namespace Rbx2Source.StudioMdl
{
    class Node : IStudioMdlEntity
    {
        public string Name;
        public Bone Bone;
        public int NodeIndex;
        public bool UseParentIndex = false;
        public int ParentIndex = -1;
        public Polygon[] Geometry;

        public string GroupName
        {
            get { return "nodes"; }
        }

        private int FindParent(List<Node> nodes, Node node)
        {
            Bone bone = node.Bone;
            Part part0 = bone.Part0;
            Part part1 = bone.Part1;
            if (part0 != part1)
            {
                Node parent = null;
                foreach (Node n in nodes)
                {
                    Bone b = n.Bone;
                    if (b != bone && b.Part1 == part0)
                    {
                        parent = n;
                        break;
                    }
                }
                return nodes.IndexOf(parent);
            }
            return -1;
        }

        public void Write(StringWriter fileBuffer, IList rawNodes, object rawNode)
        {
            List<Node> nodes = rawNodes as List<Node>;
            Node node = rawNode as Node;

            int nodeIndex = nodes.IndexOf(node);
            node.NodeIndex = nodeIndex;

            string nodeName = '"' + node.Name + '"';
            int parentIndex = UseParentIndex ? ParentIndex : FindParent(nodes, node);
            ParentIndex = parentIndex;

            string joined = string.Join(" ", nodeIndex, nodeName, parentIndex);
            fileBuffer.WriteLine(joined);
        }
    }
}
