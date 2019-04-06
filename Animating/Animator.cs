using System;
using System.Collections.Generic;
using System.Linq;

using Rbx2Source.DataTypes;
using Rbx2Source.StudioMdl;
using Rbx2Source.Reflection;
using Rbx2Source.Web;

namespace Rbx2Source.Animating
{
    public class Animator
    {
        public const int FrameRate = 60;
        private static KeyframeSorter sorter = new KeyframeSorter();

        public static int ToFrameRate(float time)
        {
            return (int)(time * FrameRate);
        }

        private static List<Pose> GatherPoses(Instance kf, List<Pose> poses = null)
        {
            if (poses == null)
                poses = new List<Pose>();

            foreach (Pose pose in kf.GetChildrenOfClass<Pose>())
            {
                poses.Add(pose);
                GatherPoses(pose, poses);
            }

            return poses;
        }

        public static PosePair GetClosestPoses(Dictionary<int, Dictionary<string, Pose>> keyFrameMap, int frame, string poseName)
        {
            // Get min.
            int minFrame = frame;

            while (minFrame >= 0)
            {
                if (keyFrameMap[minFrame].ContainsKey(poseName))
                    break;

                minFrame--;
            }
            
            // Get max.
            int maxFrame = frame;

            while (maxFrame < keyFrameMap.Count)
            {
                if (keyFrameMap[maxFrame].ContainsKey(poseName))
                    break;

                maxFrame++;
            }

            if (maxFrame == keyFrameMap.Count)
                maxFrame = minFrame;

            // Return data
            PosePair pair = new PosePair(minFrame, maxFrame);

            if (minFrame >= 0)
            {
                pair.Min.Pose = keyFrameMap[minFrame][poseName];
                pair.Max.Pose = keyFrameMap[maxFrame][poseName];
            }
            else
            {
                // Generate dummy data so we don't do anything with this bone.
                Pose stubPose = new Pose();
                stubPose.Name = poseName;
                stubPose.CFrame = new CFrame();

                pair.Min.Pose = stubPose;
                pair.Max.Pose = stubPose;
            }

            return pair;
        }

        public static void PatchAngles(ref CFrame applyTo, int axis, float[] angles)
        {
            float halfPi = (float)Math.PI / 2f;

            float[] applyRepair = new float[3];
            applyRepair[axis] = halfPi;

            float[] applyRotate = new float[3];
            applyRotate[axis] = angles[axis];

            CFrame repair = CFrame.Angles(applyRepair);
            CFrame rotate = CFrame.Angles(applyRotate);

            applyTo *= repair * rotate * repair.Inverse();
        }

        public static string Assemble(KeyframeSequence sequence, List<Bone> rig)
        {
            StudioMdlWriter animWriter = new StudioMdlWriter();
            List<Keyframe> keyframes = new List<Keyframe>();

            var boneLookup = new Dictionary<string, Bone>();
            var nodes = animWriter.Nodes;

            foreach (Bone bone in rig)
            {
                Node node = bone.Node;

                if (node != null)
                {
                    string boneName = node.Name;

                    if (!boneLookup.ContainsKey(boneName))
                        boneLookup.Add(boneName, bone);

                    nodes.Add(node);
                }
            }

            foreach (Keyframe kf in sequence.GetChildrenOfClass<Keyframe>())
            {
                Pose rootPart = kf.FindFirstChild<Pose>("HumanoidRootPart");

                if (rootPart != null)
                {
                    // We don't need the rootpart for this.
                    foreach (Pose subPose in rootPart.GetChildrenOfClass<Pose>())
                        subPose.Parent = kf;

                    rootPart.Destroy();
                }

                kf.Time /= sequence.TimeScale;
                keyframes.Add(kf);
            }

            keyframes.Sort(0, keyframes.Count, sorter);

            Keyframe lastKeyframe = keyframes[keyframes.Count - 1];

            float fLength = lastKeyframe.Time;
            int frameCount = ToFrameRate(fLength);

            // As far as I can tell, models in Source require you to store poses for every
            // single frame, so I need to fill in the gaps with interpolated pose CFrames.

            var keyframeMap = new Dictionary<int, Dictionary<string, Pose>>();

            foreach (Keyframe kf in keyframes)
            {
                int frame = ToFrameRate(kf.Time);
                var poses = GatherPoses(kf);

                var poseMap = poses.ToDictionary(pose => pose.Name);
                keyframeMap[frame] = poseMap;
            }

            // Make sure there are no holes in the data.
            for (int i = 0; i < frameCount; i++)
            {
                if (!keyframeMap.ContainsKey(i))
                {
                    var emptyState = new Dictionary<string, Pose>();
                    keyframeMap.Add(i, emptyState);
                }
            }

            List<BoneKeyframe> boneKeyframes = animWriter.Skeleton;

            for (int i = 0; i < frameCount; i++)
            {
                BoneKeyframe frame = new BoneKeyframe(i);
                List<Bone> bones = frame.Bones;

                if (sequence.AvatarType == AvatarType.R15)
                {
                    frame.BaseRig = rig;
                    frame.DeltaSequence = true;
                }

                foreach (Node node in nodes)
                {
                    PosePair closestPoses = GetClosestPoses(keyframeMap, i, node.Name);

                    float min = closestPoses.Min.Frame;
                    float max = closestPoses.Max.Frame;

                    float alpha = (min == max ? 0 : (i - min) / (max - min));

                    Pose pose0 = closestPoses.Min.Pose;
                    Pose pose1 = closestPoses.Max.Pose;

                    CFrame lastCFrame = pose0.CFrame;
                    CFrame nextCFrame = pose1.CFrame;

                    Bone baseBone = boneLookup[node.Name];
                    CFrame interp = lastCFrame.Lerp(nextCFrame, alpha);

                    // Make some patches to the interpolation offsets. Unfortunately I can't
                    // identify any single fix that I can apply to each joint, so I have to get crafty.
                    // At some point in the future, I want to find a more practical solution for this problem,
                    // but it is extremely difficult to isolate if any single solution exists.

                    if (sequence.AvatarType == AvatarType.R6)
                    {
                        Vector3 pos = interp.Position;
                        CFrame rot = interp - pos;
                        
                        if (node.Name == "Torso")
                        {
                            // Flip the YZ axis of the Torso.
                            float[] ang = interp.ToEulerAnglesXYZ();
                            rot = CFrame.Angles(ang[0], ang[2], ang[1]);
                            pos = new Vector3(pos.X, pos.Z, pos.Y);
                        }
                        else if (node.Name.StartsWith("Right"))
                        {
                            // X-axis is inverted for the right arm/leg.
                            pos *= new Vector3(-1, 1, 1);
                        }

                        if (node.Name.EndsWith("Arm") || node.Name.EndsWith("Leg"))
                        {
                            // Rotate position offset of the arms & legs 90* counter-clockwise.
                            pos = new Vector3(-pos.Z, pos.Y, pos.X);
                        }

                        interp = new CFrame(pos) * rot;
                    }
                    else if (sequence.AvatarType == AvatarType.R15)
                    {
                        float[] ang = interp.ToEulerAnglesXYZ();

                        // Cancel out the rotations
                        interp *= CFrame.Angles(-ang[0], -ang[1], -ang[2]);

                        // Patch the Y-axis
                        PatchAngles(ref interp, 1, ang);

                        // Patch the Z-axis
                        PatchAngles(ref interp, 2, ang);

                        // Patch the X-axis
                        PatchAngles(ref interp, 0, ang);
                    }

                    Bone bone = new Bone(node, interp);
                    bones.Add(bone);
                }

                boneKeyframes.Add(frame);
            }

            return animWriter.BuildFile();
        }
    }
}