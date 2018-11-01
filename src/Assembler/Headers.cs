using System;
using System.Collections.Generic;
using System.Drawing;

using Rbx2Source.Coordinates;
using Rbx2Source.Reflection;
using Rbx2Source.StudioMdl;
using Rbx2Source.Textures;
using Rbx2Source.Web;

namespace Rbx2Source.Assembler
{
    enum AnimationType
    {
        KeyframeSequence,
        R15AnimFolder
    }

    struct TextureAssembly
    {
        public Dictionary<string, Image> Images;
        public Dictionary<string, string> MatLinks;
        public string MaterialDirectory;

        public void LinkDirectly(string name, Image img)
        {
            Images.Add(name, img);
            MatLinks.Add(name, name);
        }
    }

    class Material
    {
        public BasePart LinkedTo;
        public bool UseAvatarMap = false;
        public Vector3 VertexColor = new Vector3(1, 1, 1);
        public double Transparency = 0.0;
        public double Reflectance = 0.0;
        public bool UseReflectance = false;
        public Asset TextureAsset;
    }

    class AnimationId
    {
        public AnimationType AnimationType;
        public long AssetId;

        public Asset GetAsset()
        {
            if (AnimationType == AnimationType.R15AnimFolder)
                return Asset.Get(AssetId, "/asset/?assetversionid=");
            else if (AnimationType == AnimationType.KeyframeSequence)
                return Asset.Get(AssetId);
            else
                throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Rbx2Source.GetEnumName(AnimationType) + ' ' + AssetId;
        }
    }

    class AssemblerData
    {
        public StudioMdlWriter ModelData;
        public TextureAssembly TextureData;
        public string CompilerScript;
        public string RootDirectory;
        public string TextureDirectory;
        public string MaterialDirectory;
        public string CompileDirectory;
        public string ModelName;
    }

    interface ICharacterAssembler
    {
        StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale, bool collisionModel = false);
        TextureCompositor ComposeTextureMap(Folder characterAssets, BodyColors bodyColors);
        TextureAssembly AssembleTextures(TextureCompositor compositor, Dictionary<string, Material> materials);
        Dictionary<string, AnimationId> CollectAnimationIds(UserAvatar avatar);
        byte[] CollisionModelScript { get; }
    }

    interface IAssembler
    {
        AssemblerData Assemble(object metadata);
    }
}
