using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rbx2Source.Reflection
{
    enum AnimationPriority
    {
        Idle = 0,
        Movement = 1,
        Action = 2,
        Core = 1000
    }

    enum EasingDirection
    {
        In = 0,
        Out = 1,
        InOut = 2,
    }

    enum EasingStyle
    {
        Linear = 0,
        Constant = 1,
        Elastic = 2,
        Cubic = 3,
        Bounce = 4
    }

    enum MeshType
    {
        Head = 0,
        Torso = 1,
        Wedge = 2,
        Sphere = 3,
        Cylinder = 4,
        FileMesh = 5,
        Brick = 6,
    }

    enum PartType
    {
        Ball = 0,
        Block = 1,
        Cylinder = 2
    }
}
