using System.Collections.Generic;
using System.IO;

using RobloxFiles.DataTypes;
using RobloxFiles;
using System.Diagnostics.Contracts;

namespace Rbx2Source.StudioMdl
{
    public class BoneKeyframe : IStudioMdlEntity<BoneKeyframe>
    {
        public string GroupName => "skeleton";

        public int Time;
        public List<StudioBone> Bones;
        public List<StudioBone> BaseRig;
        public bool DeltaSequence = false;

        public BoneKeyframe(int time = 0)
        {
            Time = time;
            Bones = new List<StudioBone>();
        }

        public void WriteStudioMdl(StringWriter fileBuffer, List<BoneKeyframe> skeleton)
        {
            Contract.Requires(fileBuffer != null && skeleton != null);
            fileBuffer.WriteLine("time " + Time);

            foreach (StudioBone bone in Bones)
            {
                int boneIndex = Bones.IndexOf(bone);
                fileBuffer.Write(boneIndex + " ");

                int parentIndex = bone.Node.ParentIndex;
                CFrame boneCFrame = bone.C0;

                if (DeltaSequence)
                {
                    StudioBone refBone = BaseRig[boneIndex];
                    boneCFrame = refBone.C0 * boneCFrame;

                    Node refNode = refBone.Node;
                    int refParentIndex = refNode.ParentIndex;

                    if (refParentIndex >= 0)
                    {
                        StudioBone refParent = BaseRig[refParentIndex];
                        boneCFrame = refParent.C1.Inverse() * boneCFrame;
                    }
                }
                else if (parentIndex >= 0)
                {
                    StudioBone parentBone = Bones[parentIndex];
                    boneCFrame *= parentBone.C1.Inverse();
                }

                Vector3 pos = boneCFrame.Position * Rbx2Source.MODEL_SCALE;
                Vector3 rot = new Vector3(boneCFrame.ToEulerAnglesXYZ());
                
                fileBuffer.Write(Format.FormatFloats
                (
                    pos.X, pos.Y, pos.Z,
                    rot.X, rot.Y, rot.Z
                ));

                fileBuffer.WriteLine();
            }
        }
    }
}
