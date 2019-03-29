using System.Collections.Generic;
using Rbx2Source.Reflection;

namespace Rbx2Source.Animating
{
    public class KeyframeSorter : IComparer<Keyframe>
    {
        public int Compare(Keyframe a, Keyframe b)
        {
            int aFrameTime = Animator.ToFrameRate(a.Time);
            int bFrameTime = Animator.ToFrameRate(b.Time);

            return (aFrameTime - bFrameTime);
        }
    }
}
