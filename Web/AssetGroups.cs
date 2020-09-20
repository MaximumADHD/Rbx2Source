using System.Collections.Generic;

namespace Rbx2Source.Web
{
    public enum AssetGroup
    {
        PackageLimbs,
        Accessories,
        Animations
    }

    static class AssetGroups
    {

        private static readonly IReadOnlyDictionary<AssetGroup, List<AssetType>> groups;

        static AssetGroups()
        {
            var result = new Dictionary<AssetGroup, List<AssetType>>();

            result.Add(AssetGroup.PackageLimbs, new List<AssetType>()
            {
                AssetType.LeftArm,
                AssetType.RightArm,
                AssetType.LeftLeg,
                AssetType.RightLeg,
                AssetType.Torso
            });

            result.Add(AssetGroup.Accessories, new List<AssetType>()
            {
                AssetType.Hat,
                AssetType.HairAccessory,
                AssetType.FaceAccessory,
                AssetType.NeckAccessory,
                AssetType.ShoulderAccessory,
                AssetType.FrontAccessory,
                AssetType.BackAccessory,
                AssetType.WaistAccessory
            });

            result.Add(AssetGroup.Animations, new List<AssetType>()
            {
                AssetType.ClimbAnimation,
                AssetType.DeathAnimation,
                AssetType.FallAnimation,
                AssetType.IdleAnimation,
                AssetType.JumpAnimation,
                AssetType.RunAnimation,
                AssetType.SwimAnimation,
                AssetType.WalkAnimation,
                AssetType.PoseAnimation
            });

            groups = result;
        }

        public static bool IsTypeInGroup(AssetType type, AssetGroup group)
        {
            return groups[group].Contains(type);
        }
    }
}
