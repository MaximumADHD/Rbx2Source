using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rbx2Source.Web
{
    enum AssetGroup
    {
        PackageLimbs = 0,
        Accessories = 1
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
                AssetType.BackAccessory
            });
        }

        public static bool IsTypeInGroup(AssetType type, AssetGroup group)
        {
            return groups[group].Contains(type);
        }
    }
}
