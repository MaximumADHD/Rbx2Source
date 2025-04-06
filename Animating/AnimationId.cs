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
            return Asset.Get(AssetId);
        }

        public override string ToString()
        {
            return Rbx2Source.GetEnumName(AnimationType) + ' ' + AssetId;
        }
    }
}
