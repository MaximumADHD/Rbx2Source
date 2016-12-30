#pragma warning disable 0649

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Rbx2Source.Assembler;
using Rbx2Source.Resources;

namespace Rbx2Source.Compiler
{

    class ModelCompiler
    {
        private static ThirdPartyUtility vtfCompiler;
		private static string vtfCompilerPath;
        private static string utilityDir;

        static ModelCompiler()
        {
            string appData = Environment.GetEnvironmentVariable("AppData");
            string rbx2Source = Path.Combine(appData, "Rbx2Source");
            utilityDir = Path.Combine(rbx2Source, "Utility");
            Directory.CreateDirectory(utilityDir);
            vtfCompilerPath = Path.Combine(utilityDir, "vtfcmd.exe");
        }

        public static void PreScheduleTasks()
        {
            // This is called before the model is built or compiled.
            Rbx2Source.ScheduleTasks("CompileModel", "CompileTextures", "MoveTextures");
        }

        public static async Task<string> Compile(GameInfo gameInfo, AssemblerData data)
        {
            if (!gameInfo.ReadyToUse)
                throw new Exception("This gameinfo.txt file isn't ready to use!");

            Rbx2Source.PrintHeader("COMPILING MODEL");
            string studioMdlPath = gameInfo.StudioMdlPath;
			
            ThirdPartyUtility studioMdl = new ThirdPartyUtility(studioMdlPath);
			studioMdl.AddParameter("game",gameInfo.GameDirectory);
            studioMdl.AddParameter("nop4");
			studioMdl.AddParameter(UtilParameter.FilePush(data.CompilerScript));
			await studioMdl.Run();
            Rbx2Source.MarkTaskCompleted("CompileModel");

            Rbx2Source.PrintHeader("COMPILING TEXTURES");
            if (!File.Exists(vtfCompilerPath))
            {
                byte[] vtfZip = ResourceUtility.GetResource("VTFCmd.zip");
                MemoryStream extract = new MemoryStream(vtfZip);
                ZipArchive archive = new ZipArchive(extract);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string name = entry.Name;
                    string path = Path.Combine(utilityDir, name);
                    Stream stream = entry.Open();
                    byte[] file = FileUtility.ReadFullStream(stream);
                    FileUtility.WriteFile(path, file);
                }
            }

            string pngWildcard = Path.Combine(data.TextureDirectory,"*.png");
			vtfCompiler = new ThirdPartyUtility(vtfCompilerPath);
			vtfCompiler.AddParameter("folder",pngWildcard);
			vtfCompiler.AddParameter("resize");
			vtfCompiler.AddParameter("format", "ABGR8888"); // No compression? THIS IS FINE.png
			vtfCompiler.AddParameter("output",data.MaterialDirectory);
            await vtfCompiler.Run();
            Rbx2Source.MarkTaskCompleted("CompileTextures");

            string gameDirectory = gameInfo.GameDirectory;
            string modelPath = Path.Combine(gameDirectory, "models", data.ModelName);
            string materialPath = Path.Combine(gameDirectory, "materials", "models", data.CompileDirectory);
            FileUtility.InitiateEmptyDirectories(materialPath);

            foreach (string filePath in Directory.GetFiles(data.MaterialDirectory))
            {
                FileInfo info = new FileInfo(filePath);
                string fileName = info.Name;
                string destFileName = Path.Combine(materialPath, fileName);
                info.CopyTo(destFileName);
            }

            Rbx2Source.MarkTaskCompleted("MoveTextures");
            return modelPath;
        }
    }
}
