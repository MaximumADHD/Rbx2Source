using System;
using Rbx2Source.Web;

namespace Rbx2Source.Animating
{
    public enum AnimationType
    {
        KeyframeSequence,
        R15AnimFolder
    }

    public class AnimationId
    {
        public AnimationType AnimationType;
        public long AssetId;

        public Asset GetAsset()
        {
            if (AnimationType == AnimationType.R15AnimFolder)
                return Asset.Get(AssetId, "/asset/?assetversionid=");
            else if (AnimationType == AnimationType.KeyframeSequence)
                return Asset.Get(AssetId);

            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Main.GetEnumName(AnimationType) + ' ' + AssetId;
        }
    }
}
