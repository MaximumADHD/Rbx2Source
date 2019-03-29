using Rbx2Source.StudioMdl;

namespace Rbx2Source.Assembler
{
    public class AssemblerData
    {
        public string ModelName;
        public string CompilerScript;

        public StudioMdlWriter ModelData;
        public TextureBindings TextureData;

        public string RootDirectory;
        public string TextureDirectory;
        public string CompileDirectory;
        public string MaterialDirectory;
    }
}