#pragma warning disable 0649

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Rbx2Source.Assembler;
using Rbx2Source.Resources;

namespace Rbx2Source.Compiler
{
    public static class ModelCompiler
    {
        private static ThirdPartyUtility vtfCompiler;
        private static readonly string vtfCompilerPath;
        private static readonly string utilityDir;

        static ModelCompiler()
        {
            string appData = Environment.GetEnvironmentVariable("LocalAppData");
            string rbx2Source = Path.Combine(appData, "Rbx2Source");
            utilityDir = Path.Combine(rbx2Source, "Utility");

            Directory.CreateDirectory(utilityDir);
            vtfCompilerPath = Path.Combine(utilityDir, "vtfcmd.exe");
        }

        public static void PreScheduleTasks()
        {
            // This is called before the model is built or compiled.
            Main.ScheduleTasks("CompileModel", "CompileTextures", "MoveTextures");
        }

        public static async Task<string> Compile(GameInfo gameInfo, AssemblerData data)
        {
            Contract.Requires(gameInfo != null && data != null);

            if (!gameInfo.ReadyToUse)
                throw new Exception("This gameinfo.txt file isn't ready to use!");

            Main.PrintHeader("COMPILING MODEL");
            #region Compile Model
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            string studioMdlPath = gameInfo.StudioMdlPath;
			
            ThirdPartyUtility studioMdl = new ThirdPartyUtility(studioMdlPath);
            studioMdl.AddParameter("game", gameInfo.GameDirectory);
            studioMdl.AddParameter("nop4");
            studioMdl.AddFile(data.CompilerScript);

            await studioMdl.RunWithOutput();
            Main.MarkTaskCompleted("CompileModel");

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Main.PrintHeader("COMPILING TEXTURES");
            #region Compile Textures
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (!File.Exists(vtfCompilerPath))
            {
                byte[] vtfZip = ResourceUtility.GetResource("VTFCmd.zip");

                using (MemoryStream extract = new MemoryStream(vtfZip))
                using (ZipArchive archive = new ZipArchive(extract))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string name = entry.Name;
                        string path = Path.Combine(utilityDir, name);

                        using (Stream stream = entry.Open())
                        {
                            byte[] file = FileUtility.ReadFullStream(stream);
                            FileUtility.WriteFile(path, file);
                        }
                    }
                }
            }

            string pngWildcard = Path.Combine(data.TextureDirectory, "*.png");
            vtfCompiler = new ThirdPartyUtility(vtfCompilerPath);

            vtfCompiler.AddParameter("resize");
            vtfCompiler.AddParameter("folder", pngWildcard);
            vtfCompiler.AddParameter("format", "ABGR8888"); // No compression? THIS IS FINE
            vtfCompiler.AddParameter("output", data.MaterialDirectory);

            await vtfCompiler.RunWithOutput();
            Main.MarkTaskCompleted("CompileTextures");
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            Main.PrintHeader("MOVING TEXTURES");
            #region Move Textures
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            string gameDirectory = gameInfo.GameDirectory;
            string modelPath = Path.Combine(gameDirectory, "models", data.ModelName);
            string materialPath = Path.Combine(gameDirectory, "materials", "models", data.CompileDirectory);

            FileUtility.InitiateEmptyDirectories(materialPath);

            foreach (string filePath in Directory.GetFiles(data.MaterialDirectory))
            {
                FileInfo info = new FileInfo(filePath);

                string fileName = info.Name;
                Main.Print($"Moving File: {fileName}");

                Main.IncrementStack();
                Main.Print($"From: {filePath}");

                string destFilePath = Path.Combine(materialPath, fileName);
                info.CopyTo(destFilePath);

                Main.Print($"To:   {destFilePath}");
                Main.DecrementStack();
            }

            Main.MarkTaskCompleted("MoveTextures");
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion

            return modelPath;
        }
    }
}
