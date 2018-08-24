using System.Collections.Generic;
using System.Drawing;

using Rbx2Source.Coordinates;
using Rbx2Source.Reflection;
using Rbx2Source.StudioMdl;
using Rbx2Source.Web;

namespace Rbx2Source.Assembler
{
    struct TextureAssembly
    {
        public Dictionary<string, Image> Images;
        public Dictionary<string, string> MatLinks;
        public string MaterialDirectory;
    }

    class Material
    {
        public Part LinkedTo;
        public bool UseAvatarMap = false;
        public Vector3 VertexColor = new Vector3(1, 1, 1);
        public double Transparency = 0.0;
        public double Reflectance = 0.0;
        public bool UseReflectance = false;
        public Asset TextureAsset;
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
        StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale);
        TextureCompositor ComposeTextureMap(Folder characterAssets, BodyColors bodyColors);
        TextureAssembly AssembleTextures(TextureCompositor compositor, Dictionary<string, Material> materials);
        byte[] CollisionModelScript { get; }
    }

    interface IAssembler
    {
        AssemblerData Assemble(object metadata);
    }
}
