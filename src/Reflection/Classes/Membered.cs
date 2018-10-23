#pragma warning disable 0649

using Rbx2Source.Assembler;
using Rbx2Source.Coordinates;
using Rbx2Source.Web;

namespace Rbx2Source.Reflection
{
    class Accoutrement : Instance
    {
        public CFrame AttachmentPoint;
    }

    class Attachment : Instance
    {
        public CFrame CFrame;
        public bool Visible;
    }

    class CharacterMesh : CharacterAppearance
    {
        public long BaseTextureId;
        public Limb BodyPart;
        public long MeshId;
        public long OverlayTextureId;
    }

    class Shirt : CharacterAppearance
    {
        public string ShirtTemplate;
    }

    class Pants : CharacterAppearance
    {
        public string PantsTemplate;
    }

    class ShirtGraphic : CharacterAppearance
    {
        public string Graphic;
    }

    class BevelMesh : DataModelMesh
    {
        public double Bevel;
        public double Bevel_Roundness;
        public double Buldge;
    }

    class DataModelMesh : Instance
    {
        public Vector3 Scale;
        public Vector3 Offset;
        public Vector3 VertexColor;
    }

    class Keyframe : Instance
    {
        public float Time;
    }

    class KeyframeSequence : Instance
    {
        public bool Loop = true;
        public AnimationPriority Priority = AnimationPriority.Core;
        public AvatarType AvatarType = AvatarType.Unknown;
        public float TimeScale = 1f;
    }

    class MeshPart : Part
    {
        public string MeshID;
        public string MeshId;
        public string TextureID;
        public Vector3 InitialSize;
    }

    class Part : Instance
    {
        public float Transparency;
        public float Reflectance;
        public CFrame CFrame;
        public int BrickColor;

        // note: the XML reflection is case sensitive :/

        public Vector3 size;
        public PartType shape = PartType.Block;

        public Vector3 Size
        {
            get { return size; }
            set { size = value; }
        }

        public PartType Shape
        {
            get { return shape; }
            set { shape = value; }
        }
    }

    class Pose : Instance
    {
        public CFrame CFrame;
        public EasingDirection PoseEasingDirection;
        public EasingStyle PoseEasingStyle;
        public float Weight;
    }

    class SpecialMesh : DataModelMesh
    {
        public string MeshId;
        public string TextureId;
        public MeshType MeshType;
    }

    class Decal : Instance
    {
        public string Texture;
    }

    class StringValue : Instance
    {
        public string Value;
    }

    class Vector3Value : Instance
    {
        public Vector3 Value;
    }
}
