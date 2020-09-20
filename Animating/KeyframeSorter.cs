using System.Collections.Generic;
using System.Diagnostics.Contracts;
using RobloxFiles;

namespace Rbx2Source.Animating
{
    public class KeyframeSorter : IComparer<Keyframe>
    {
        public int Compare(Keyframe a, Keyframe b)
        {
            Contract.Requires(a != null && b != null);

            int aFrameTime = AnimationBuilder.ToFrameRate(a.Time);
            int bFrameTime = AnimationBuilder.ToFrameRate(b.Time);

            return aFrameTime - bFrameTime;
        }
    }
}
