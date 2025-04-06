using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using Rbx2Source.StudioMdl;
using Rbx2Source.Web;

using RobloxFiles;
using RobloxFiles.DataTypes;

namespace Rbx2Source.Animating
{
    public static class AnimationBuilder
    {
        public const int FrameRate = 60;
        private static readonly KeyframeSorter sorter = new KeyframeSorter();

        public static int ToFrameRate(float time)
        {
            return (int)(time * FrameRate);
        }

        private static List<Pose> GatherPoses(Instance kf)
        {
            return kf.GetDescendantsOfType<Pose>().ToList();
        }

        public static PosePair GetClosestPoses(Dictionary<int, Dictionary<string, Pose>> keyFrameMap, int frame, string poseName)
        {
            Contract.Requires(keyFrameMap != null);

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
                Pose stubPose = new Pose()
                {
                    Name = poseName,
                    CFrame = new CFrame()
                };

                pair.Min.Pose = stubPose;
                pair.Max.Pose = stubPose;
            }

            return pair;
        }

        public static void PatchAngles(ref CFrame applyTo, int axis, float angle)
        {
            const float halfPi = (float)(Math.PI / 2f);

            float[] applyRepair = new float[3];
            applyRepair[axis] = halfPi;

            float[] applyRotate = new float[3];
            applyRotate[axis] = angle;

            CFrame repair = CFrame.Angles(applyRepair);
            CFrame rotate = CFrame.Angles(applyRotate);

            applyTo *= repair * rotate * repair.Inverse();
        }

        public static string Assemble(KeyframeSequence sequence, List<StudioBone> rig)
        {
            Contract.Requires(sequence != null && rig != null);

            StudioMdlWriter animWriter = new StudioMdlWriter();
            List<Keyframe> keyframes = new List<Keyframe>();

            var boneLookup = new Dictionary<string, StudioBone>();
            var nodes = animWriter.Nodes;

            foreach (StudioBone bone in rig)
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

            foreach (Keyframe kf in sequence.GetChildrenOfType<Keyframe>())
            {
                Pose rootPart = kf.FindFirstChild<Pose>("HumanoidRootPart");

                if (rootPart != null)
                {
                    // We don't need the rootpart for this.
                    foreach (Pose subPose in rootPart.GetChildrenOfType<Pose>())
                        subPose.Parent = kf;

                    rootPart.Destroy();
                }
                
                keyframes.Add(kf);
            }

            keyframes = keyframes
                .OrderBy(keyframe => keyframe.Time)
                .ToList();

            float fLength = keyframes.Last().Time;
            int frameCount = ToFrameRate(fLength);

            // As far as I can tell, models in Source require you to store poses for every
            // single frame. I need to fill in the gaps with interpolated pose CFrames.

            var keyframeMap = new Dictionary<string, LinkedList<PoseMapEntity>>();

            foreach (Keyframe kf in keyframes)
            {
                int frame = ToFrameRate(kf.Time);
                var poses = kf.GetDescendantsOfType<Pose>();

                foreach (var pose in poses)
                {
                    var name = pose.Name;

                    if (!keyframeMap.TryGetValue(name, out LinkedList<PoseMapEntity> list))
                    {
                        list = new LinkedList<PoseMapEntity>();
                        keyframeMap[name] = list;
                    }

                    var entry = new PoseMapEntity(frame) { Pose = pose };
                    list.AddLast(entry);
                }
            }

            List<BoneKeyframe> boneKeyframes = animWriter.Skeleton;
            var avatarTypeId = sequence.FindFirstChild<StringValue>("AvatarType");
            var avatarType = avatarTypeId?.Value ?? "R15";

            for (int i = 0; i < frameCount; i++)
            {
                var frame = new BoneKeyframe(i);
                List<StudioBone> bones = frame.Bones;
                
                if (avatarType == "R15")
                {
                    frame.BaseRig = rig;
                    frame.DeltaSequence = true;
                }

                foreach (Node node in nodes)
                {
                    var name = node.Name;

                    if (!keyframeMap.TryGetValue(name, out LinkedList<PoseMapEntity> list))
                    {
                        var dummyBone = new StudioBone(node, CFrame.identity);
                        bones.Add(dummyBone);

                        continue;
                    }

                    var node0 = list.First;
                    var node1 = node0.Next ?? node0;

                    var ent0 = node0.Value;
                    var ent1 = node1.Value;

                    var frame0 = ent0.Frame;
                    var frame1 = ent1.Frame;

                    var pose0 = ent0.Pose;
                    var pose1 = ent1.Pose;

                    var lastCFrame = pose0.CFrame;
                    var nextCFrame = pose1.CFrame;

                    float alpha = frame0 != frame1
                        ? (float)(i - frame0) / (frame1 - frame0)
                        : 0;

                    StudioBone baseBone = boneLookup[name];
                    CFrame interp = lastCFrame.Lerp(nextCFrame, alpha);

                    var quat = new Quaternion(interp);
                    var angles = quat.ToEulerAngles();

                    interp = CFrame.FromEulerAnglesXYZ(angles.Roll, 0, 0)
                           * CFrame.FromEulerAnglesXYZ(0, 0, angles.Yaw)
                           * CFrame.FromEulerAnglesXYZ(0, angles.Pitch, 0) 
                           * new CFrame(interp.Position);

                    var bone = new StudioBone(node, interp);
                    bones.Add(bone);

                    if (alpha >= 1)
                    {
                        // We're about to go past the next node, pop the front.
                        list.RemoveFirst();
                    }
                }

                boneKeyframes.Add(frame);
            }

            string result = animWriter.BuildFile();
            animWriter.Dispose();

            return result;
        }
    }
}
