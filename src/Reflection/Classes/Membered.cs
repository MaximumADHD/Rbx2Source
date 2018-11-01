﻿#pragma warning disable 0649

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

    class BasePart : Instance
    {
        public int BrickColor;

        public CFrame CFrame;
        public Vector3 Position => CFrame.p;

        public float Reflectance;
        public float Transparency;

        public Vector3 Size;
    }

    class MeshPart : BasePart
    {
        public string MeshId;
        public string TextureId;
        public Vector3 InitialSize;
    }

    class Part : BasePart
    {
        public PartType Shape = PartType.Block;
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

    class ValueBase<T> : Instance
    {
        public T Value;
    }

    class Animation : Instance
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
