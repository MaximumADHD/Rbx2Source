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

        private static Dictionary<AssetGroup, List<AssetType>> groups = new Dictionary<AssetGroup, List<AssetType>>();

        static AssetGroups()
        {
            groups.Add(AssetGroup.PackageLimbs, new List<AssetType>()
            {
                AssetType.LeftArm,
                AssetType.RightArm,
                AssetType.LeftLeg,
                AssetType.RightLeg,
                AssetType.Torso
            });

            groups.Add(AssetGroup.Accessories, new List<AssetType>()
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

            groups.Add(AssetGroup.Animations, new List<AssetType>()
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
        }

        public static bool IsTypeInGroup(AssetType type, AssetGroup group)
        {
            return groups[group].Contains(type);
        }
    }
}
