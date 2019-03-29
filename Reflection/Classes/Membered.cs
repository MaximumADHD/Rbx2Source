#pragma warning disable 0649

using Rbx2Source.Assembler;
using Rbx2Source.Coordinates;
using Rbx2Source.Web;

namespace Rbx2Source.Reflection
{
    public class Accoutrement : Instance
    {
        public CFrame AttachmentPoint;
    }

    public class Attachment : Instance
    {
        public CFrame CFrame;
        public bool Visible;
    }

    public class CharacterMesh : CharacterAppearance
    {
        public long BaseTextureId;
        public Limb BodyPart;
        public long MeshId;
        public long OverlayTextureId;
    }

    public class Shirt : CharacterAppearance
    {
        public string ShirtTemplate;
    }

    public class Pants : CharacterAppearance
    {
        public string PantsTemplate;
    }

    public class ShirtGraphic : CharacterAppearance
    {
        public string Graphic;
    }

    public class BevelMesh : DataModelMesh
    {
        public double Bevel;
        public double Bevel_Roundness;
        public double Buldge;
    }

    public class DataModelMesh : Instance
    {
        public Vector3 Scale;
        public Vector3 Offset;
        public Vector3 VertexColor;
    }

    public class Keyframe : Instance
    {
        public float Time;
    }

    public class KeyframeSequence : Instance
    {
        public bool Loop = true;
        public AnimationPriority Priority = AnimationPriority.Core;
        public AvatarType AvatarType = AvatarType.Unknown;
        public float TimeScale = 1f;
    }

    public class BasePart : Instance
    {
        public int BrickColor;

        public CFrame CFrame;
        public Vector3 Position => CFrame.Position;

        public float Reflectance;
        public float Transparency;

        public Vector3 Size;
    }

    public class MeshPart : BasePart
    {
        public string MeshId;
        public string TextureId;
        public Vector3 InitialSize;
    }

    public class Part : BasePart
    {
        public PartType Shape = PartType.Block;
    }

    public class Pose : Instance
    {
        public CFrame CFrame;
        public EasingDirection PoseEasingDirection;
        public EasingStyle PoseEasingStyle;
        public float Weight;
    }

    public class SpecialMesh : DataModelMesh
    {
        public string MeshId;
        public string TextureId;
        public MeshType MeshType;
    }

    public class Decal : Instance
    {
        public string Texture;
    }

    public class ValueBase<T> : Instance
    {
        public T Value;
    }

    public class Animation : Instance
    {
        public string AnimationId;

        // Hack to make it easier for me to do a Linq query of weighted user animations
        public double Weight
        {
            get
            {
                NumberValue weight = FindFirstChild<NumberValue>("Weight");
                return weight != null ? weight.Value : 1;
            }
        }
    }
}
