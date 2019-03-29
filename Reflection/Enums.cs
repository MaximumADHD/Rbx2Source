namespace Rbx2Source.Reflection
{
    public enum AnimationPriority
    {
        Idle,
        Movement,
        Action,
        Core = 1000
    }

    public enum EasingDirection
    {
        In,
        Out,
        InOut,
    }

    public enum EasingStyle
    {
        Linear,
        Constant,
        Elastic,
        Cubic,
        Bounce
    }

    public enum MeshType
    {
        Head,
        Torso,
        Wedge,
        Sphere,
        Cylinder,
        FileMesh,
        Brick,
    }

    public enum NormalId
    {
        Right,
        Top,
        Back,
        Left,
        Bottom,
        Front
    }

    public enum PartType
    {
        Ball,
        Block,
        Cylinder
    }
}
