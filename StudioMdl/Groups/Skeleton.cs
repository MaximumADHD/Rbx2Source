using System.Collections.Generic;
using System.IO;

using Rbx2Source.DataTypes;
using Rbx2Source.Reflection;

namespace Rbx2Source.StudioMdl
{
    public class BoneKeyframe : IStudioMdlEntity<BoneKeyframe>
    {
        public string GroupName => "skeleton";

        public int Time;
        public List<Bone> Bones;
        public List<Bone> BaseRig;
        public bool DeltaSequence = false;

        public BoneKeyframe(int time = 0)
        {
            Time = time;
            Bones = new List<Bone>();
        }

        public void WriteStudioMdl(StringWriter fileBuffer, List<BoneKeyframe> skeleton)
        {
            fileBuffer.WriteLine("time " + Time);

            foreach (Bone bone in Bones)
            {
                int boneIndex = Bones.IndexOf(bone);
                fileBuffer.Write(boneIndex + " ");

                int parentIndex = bone.Node.ParentIndex;
                CFrame boneCFrame = bone.C0;

                if (DeltaSequence)
                {
                    Bone refBone = BaseRig[boneIndex];
                    boneCFrame = refBone.C0 * boneCFrame;

                    Node refNode = refBone.Node;
                    int refParentIndex = refNode.ParentIndex;

                    if (refParentIndex >= 0)
                    {
                        Bone refParent = BaseRig[refParentIndex];
                        boneCFrame = refParent.C1.Inverse() * boneCFrame;
                    }
                }
                else if (parentIndex >= 0)
                {
                    Bone parentBone = Bones[parentIndex];
                    boneCFrame *= parentBone.C1.Inverse();
                }

                string studioMdl = boneCFrame.WriteStudioMdl();
                fileBuffer.Write(studioMdl);
                fileBuffer.WriteLine();
            }
        }
    }
}
