using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rbx2Source.Coordinates;
using Rbx2Source.StudioMdl;
using Rbx2Source.Reflection;
using Rbx2Source.Web;

namespace Rbx2Source.Animation
{
    class KeyframeSorter : IComparer<Keyframe>
    {
        public int Compare(Keyframe a, Keyframe b)
        {
            int aFrameTime = AnimationAssembler.ToFrameRate(a.Time);
            int bFrameTime = AnimationAssembler.ToFrameRate(b.Time);
            return (aFrameTime - bFrameTime);
        }
    }

    struct PoseMapEntity
    {
        public int Frame;
        public Pose Pose;
    }

    struct PosePair
    {
        public PoseMapEntity Min;
        public PoseMapEntity Max;
    }

    class AnimationAssembler
    {
        public static int FrameRate = 60;
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
                else
                    minFrame--;
            }
                

            // Get max.
            int maxFrame = frame;

            while (maxFrame < keyFrameMap.Count)
            {
                if (keyFrameMap[maxFrame].ContainsKey(poseName))
                    break;
                else
                    maxFrame++;
            }

            if (maxFrame == keyFrameMap.Count)
                maxFrame = minFrame;

            // Return data

            PosePair pair = new PosePair();
            pair.Min = new PoseMapEntity();
            pair.Min.Frame = minFrame;
            pair.Max = new PoseMapEntity();
            pair.Max.Frame = maxFrame;

            if (minFrame == -1 )
            {
                // Generate dummy data so we don't do anything with this bone.
                Pose stubPose = new Pose();
                stubPose.Name = poseName;
                stubPose.CFrame = new CFrame();
                pair.Min.Pose = stubPose;
                pair.Max.Pose = stubPose;
            }
            else
            {
                pair.Min.Pose = keyFrameMap[minFrame][poseName];
                if (keyFrameMap.ContainsKey(maxFrame))
                    pair.Max.Pose = keyFrameMap[maxFrame][poseName];
                else
                    pair.Max.Pose = pair.Min.Pose;
            }

            return pair;
        }

        private static void SetupNodeHierarchy(Pose pose, List<Node> nodes, int lastNode = -1)
        {
            Node node = new Node();
            node.Name = pose.Name;
            node.UseParentIndex = true;
            node.ParentIndex = lastNode;
            nodes.Add(node);

            int nodeIndex = nodes.IndexOf(node);
            node.NodeIndex = nodeIndex;
            
            foreach (Pose nextPose in pose.GetChildrenOfClass<Pose>())
                SetupNodeHierarchy(nextPose, nodes, nodeIndex);

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

            // Animations in source are kinda dumb, because there is no in-between frame interpolation.
            // I have to account for every single CFrame for every single frame.

            var keyframeMap = new Dictionary<int, Dictionary<string, Pose>>();
            for (int i = 0; i <= frameCount; i++)
                keyframeMap[i] = new Dictionary<string, Pose>();

            foreach (Keyframe kf in keyframes)
            {
                int frame = ToFrameRate(kf.Time);
                var poses = GatherPoses(kf);
                var poseMap = keyframeMap[frame];

                foreach (Pose pose in poses)
                {
                    poseMap[pose.Name] = pose;
                }
            }

            List<BoneKeyframe> boneKeyframes = animWriter.Skeleton;
            
            Keyframe baseFrame = keyframes[0];
            
            for (int i = 0; i < frameCount; i++)
            {
                BoneKeyframe frame = new BoneKeyframe();
                frame.Time = i;

                if (sequence.AvatarType == AvatarType.R15)
                {
                    frame.DeltaSequence = true;
                    frame.BaseRig = rig;
                }

                List<Bone> bones = frame.Bones;

                foreach (Node node in nodes)
                {
                    PosePair closestPoses = GetClosestPoses(keyframeMap, i, node.Name);

                    float current = i;
                    float min = closestPoses.Min.Frame;
                    float max = closestPoses.Max.Frame;

                    float alpha = (min == max ? 0 : (current - min) / (max - min));

                    Pose pose0 = closestPoses.Min.Pose;
                    Pose pose1 = closestPoses.Max.Pose;

                    CFrame lastCFrame = pose0.CFrame;
                    CFrame nextCFrame = pose1.CFrame;

                    Bone baseBone = boneLookup[node.Name];
                    CFrame interp = lastCFrame.lerp(nextCFrame, alpha);

                    // some ugly manual fixes.
                    // todo: make this unnecessary :(

                    /*if (sequence.AvatarType == AvatarType.R6)
                    {
                        Vector3 pos = interp.p;
                        CFrame rot = interp - pos;
                        if (node.Name == "Torso")
                        {
                            float[] ang = interp.toEulerAnglesXYZ();
                            rot = CFrame.Angles(ang[0], ang[2], ang[1]);
                            pos = new Vector3(pos.x, pos.z, pos.y);
                        }
                        else if (node.Name.StartsWith("Right"))
                            pos *= new Vector3(-1, 1, 1);

                        if (node.Name.Contains("Arm") || node.Name.Contains("Leg"))
                            pos = new Vector3(pos.z, pos.y, pos.x);

                        if (sequence.Name == "Climb" && node.Name.Contains("Leg")) // https://www.youtube.com/watch?v=vfJ7DqyDl9w
                            pos += new Vector3(-.1f, 0, 0);

                        interp = new CFrame(pos) * rot;
                    }
                    else if (sequence.AvatarType == AvatarType.R15)
                    {
                        if (node.Name.Contains("UpperArm") )
                        {
                            Vector3 pos = interp.p;
                            CFrame rot = interp - pos;

                            float[] ang = rot.toEulerAnglesXYZ();
                            rot = CFrame.Angles(ang[0], -ang[2], -ang[1]);
                            
                            interp = new CFrame(pos) * rot;
                        }
                    }*/

                    Bone bone = new Bone(node.Name, i, interp);
                    bone.Node = node;

                    bones.Add(bone);
                }

                boneKeyframes.Add(frame);
            }

            return animWriter.BuildFile();
        }
    }
}