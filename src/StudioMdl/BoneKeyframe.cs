using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rbx2Source.Coordinates;
using Rbx2Source.Reflection;

namespace Rbx2Source.StudioMdl
{
    class BoneKeyframe : IStudioMdlEntity
    {
        public int Time;
        public List<Bone> Bones;

        public bool DeltaSequence = false;
        public List<Bone> BaseRig;

        public string GroupName => "skeleton";

        public void Write(StringWriter fileBuffer, IList rawSkeleton, object rawKeyframe)
        {
            BoneKeyframe keyframe = rawKeyframe as BoneKeyframe;
            fileBuffer.WriteLine("time " + keyframe.Time);

            foreach (Bone bone in keyframe.Bones)
            {
                int boneIndex = keyframe.Bones.IndexOf(bone);
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
                        boneCFrame = refParent.C1.inverse() * boneCFrame;
                    }
                }
                else if (parentIndex >= 0)
                {
                    Bone parentBone = keyframe.Bones[parentIndex];
                    boneCFrame = parentBone.C1.inverse() * boneCFrame;
                }
                
                fileBuffer.Write(boneCFrame.ToStudioMdlString());
                fileBuffer.WriteLine();
            }
        }

        public BoneKeyframe(int time = 0)
        {
            Time = time;
            Bones = new List<Bone>();
        }
    }
}
