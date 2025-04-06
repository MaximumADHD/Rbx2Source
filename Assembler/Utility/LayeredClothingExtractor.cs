using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Rbx2Source.Web;
using Microsoft.Win32;

using RobloxFiles;
using RobloxFiles.Enums;
using RobloxFiles.DataTypes;

using Rbx2Source.Geometry;
using Rbx2Source.Resources;

namespace Rbx2Source.Assembler
{
    public class LayeredClothingExtractor
    {
        public readonly UserAvatar Avatar;
        public static bool UseExistingObj = false;
        public ObjFile Output { get; private set; }

        public LayeredClothingExtractor(UserAvatar avatar)
        {
            Avatar = avatar;
        }

        public async Task Extract()
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (UseExistingObj)
            {
                var existing = Path.Combine(desktopPath, "Rbx2SourceRig (SAVE TO DESKTOP).obj");

                if (File.Exists(existing))
                {
                    string contents = File.ReadAllText(existing);
                    Output = new ObjFile(contents);
                    return;
                }
            }

            var temp = Environment.GetEnvironmentVariable("TEMP");
            var bin = new BinaryRobloxFile();

            var workspace = new Workspace();
            workspace.SetAttribute<double>("UserId", Avatar.UserInfo.Id);
            workspace.Tags.Add("Rbx2Source_LayeredClothingExtractor");
            workspace.Parent = bin;

            // !! Silence legacy chat deprecation
            var textChatService = new TextChatService();
            textChatService.ChatVersion = ChatVersion.TextChatService;
            textChatService.Parent = bin;

            // !! Silence compatibility lighting deprecation
            var lighting = new Lighting();
            lighting.Technology = Technology.Voxel;
            lighting.Parent = bin;

            // Save place file to temp.
            var placeFile = Path.Combine(temp, "Rbx2Source_LayeredClothingExtractor.rbxl");
            await bin.SaveAsync(placeFile);

            // Write plugin to local plugins folder.
            string localAppData = Environment.GetEnvironmentVariable("localappdata");
            var pluginPath = Path.Combine(localAppData, "Roblox", "Plugins", "Rbx2Source_LayeredClothingExtractor.lua");

            var plugin = ResourceUtility.GetResource("Plugin/LayeredClothingExtractor.lua");
            File.WriteAllBytes(pluginPath, plugin);


            var startTime = DateTime.Now;
            Process studioProc = null;

            try
            {
                // Try to find Roblox Studio's location directly.
                var currentUser = Registry.CurrentUser;
                var software = currentUser.OpenSubKey("SOFTWARE");

                var roblox = software.OpenSubKey("Roblox");
                var robloxStudio = roblox.OpenSubKey("RobloxStudio");
                var contentFolder = robloxStudio.GetValue("ContentFolder") as string;

                var studioPath = Path.Combine(contentFolder, "..", "RobloxStudioBeta.exe");
                var studioInfo = new FileInfo(studioPath);

                if (!studioInfo.Exists)
                    throw new Exception("Studio not found!");

                var startInfo = new ProcessStartInfo()
                {
                    FileName = studioInfo.FullName,
                    Arguments = placeFile,
                };

                studioProc = Process.Start(startInfo);
            }
            catch
            {
                // Alright, start it directly by file, and wait for a new RobloxStudioBeta process.
                Process.Start(placeFile);

                while (true)
                {
                    foreach (var process in Process.GetProcessesByName("RobloxStudioBeta.exe"))
                    {
                        if (process.StartTime > startTime)
                        {
                            studioProc = process;
                            break;
                        }
                    }

                    if (studioProc != null)
                        break;

                    await Task.Delay(200);
                }
            }
            
            string objFile = "";
            
            var objFilePath = Path.Combine(desktopPath, "Rbx2SourceRig (SAVE TO DESKTOP).obj");
            var fileInfo = new FileInfo(objFilePath);

            var fileWatcher = new FileSystemWatcher()
            {
                Path = desktopPath,
                IncludeSubdirectories = false,
                Filter = "Rbx2SourceRig (SAVE TO DESKTOP).obj"
            };

            fileWatcher.Changed += new FileSystemEventHandler(async (_, eventArgs) =>
            {
                await Task.Delay(1000);

                if (objFile.Length > 0)
                    return;

                objFile = File.ReadAllText(objFilePath);
                studioProc.Kill();
            });

            fileWatcher.EnableRaisingEvents = true;

            while (objFile.Length == 0)
                if (studioProc.HasExited)
                    break;

                await Task.Delay(500);

            if (objFile.Length > 0)
                Output = new ObjFile(objFile);

            fileWatcher.Dispose();
        }
    }
}
