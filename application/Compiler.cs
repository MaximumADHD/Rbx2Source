using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Newtonsoft;
using Newtonsoft.Json;
using LuaInterface;

namespace RobloxToSourceEngine
{
    public partial class Compiler : Form
    {
        static GameDataManager DataManager = new GameDataManager();
        List<NameValueCollection> GameData = DataManager.GetGameData();
        FileHandler FileHandler = new FileHandler();
        
        WebClient http = new WebClient();
        string finalCompilePath = "";
        string finalModelName = "";
        string id = "";
        string username = "";
        bool isAsset = true;
        bool debugMode = false;
        int logCharLimit = 68;

        private void log(params string[] logTxts)
        {
            foreach (string logTxt in logTxts)
            {
                if (logTxt != null)
                {
                    if (logTxt.Length > logCharLimit)
                    {
                        string clean = logTxt;
                        while (clean.Length > logCharLimit)
                        {
                            string chunk = clean.Substring(0, logCharLimit);
                            log(chunk);
                            clean = clean.Substring(logCharLimit);
                        }
                        log(clean);
                    }
                    else
                    {
                        ConsoleDisp.Items.Add(logTxt);
                        ConsoleDisp.SelectedIndex = this.ConsoleDisp.Items.Count - 1;
                    }
                }
            }
        }

        public string GetFile(string dir, string creationPath, string name = "")
        {
            // Creates a file if it doesn't exist already.
            string filePath = Path.Combine(dir, name);
            if (!File.Exists(filePath))
            {
                byte[] fileData = FileHandler.GetResource_ByteArray(creationPath);
                FileHandler.WriteToFileFromBuffer(filePath, fileData);
            }
            return filePath;
        }

        public string GetDirectory(params string[] dir)
        {
            string fullPath = "";
            foreach (string block in dir)
            {
                fullPath = Path.Combine(fullPath, block);
                if (!Directory.Exists(fullPath))
                {
                    log("Creating Directory: " + fullPath);
                    Directory.CreateDirectory(fullPath);
                }
            }
            return fullPath;
        }

        private void ConsoleDisp_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ConsoleDisp.SelectedIndex != -1)
            {
                ConsoleDisp.SelectedIndex = -1;
            }
        }

        public void fatalError(string msg)
        {
            // Fatal Error
            // Causes the window to close
            this.Enabled = false;
            MessageBox.Show(msg, "Fatal Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }

        public void error(string msg)
        {
            // Non-Fatal Error
            // Shows a message, but doesn't close the window.
            MessageBox.Show(msg, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void compileTexture(string mtlName, string texHash, string mtlDir)
        {
            string appData = Environment.GetEnvironmentVariable("AppData");
            string rootPath = GetDirectory(appData, "Rbx2SrcFiles","images");
            string toolsPath = GetDirectory(rootPath, "converterUtil"); 
            string readme = GetFile(toolsPath, "tools/READ ME PLEASE.txt", "READ ME PLEASE.txt");
            string DevIL = GetFile(toolsPath, "tools/DevIL.dll", "DevIL.dll");
            string VTFLib = GetFile(toolsPath, "tools/VTFLib.dll", "VTFLib.dll");
            string VTFCmd_Path = GetFile(toolsPath, "tools/vtfcmd.exe", "VTFcmd.exe");
            log("Getting PNG File: " + texHash);
            string png = FileHandler.GetFileFromHash(texHash,"png",mtlName,rootPath);
            log("Converting to .VTF");
            string parameters = " -file " + inQuotes(png) + " -output " + inQuotes(mtlDir) + " -resize";
            ProcessStartInfo VTFCmd = new ProcessStartInfo();
            VTFCmd.FileName = VTFCmd_Path;
            VTFCmd.Arguments = parameters;
            VTFCmd.CreateNoWindow = true;
            VTFCmd.UseShellExecute = false;
            VTFCmd.RedirectStandardOutput = true;
            Console.WriteLine(inQuotes(VTFCmd_Path) + parameters);
            Process VTFCmd_Run = Process.Start(VTFCmd);
            StreamReader output = VTFCmd_Run.StandardOutput;
            VTFCmd_Run.WaitForExit();
            bool reading = true;
            while (reading)
            {
                string line = output.ReadLine();
                if (line != null)
                {
                    log(line);
                }
                else
                {
                    reading = false;
                }
            }
        }

        public void LuaError(LuaException e)
        {
            NameValueCollection settings = FileHandler.GetAppSettings();
            fatalError("A fatal error occured! \n\nLine " + e.Message.Substring(17) + "\n"+e.StackTrace+"\nIf you can, please tweet this information to " + settings["twitterName"] + ", and it will be fixed ASAP.\n\nThanks!");
        }
        public NameValueCollection WriteCharacterSMD(string userId)
        {
            LuaClass lua = new LuaClass();
            log("Loading Converter API...");
            try
            {
                lua.load("lua/SMDconverter.lua");
                log("Writing SMD file", "This may take up to a minute, depending on how complex the character is.", "Please wait...");
                lua.DoString("response = WriteCharacterSMD(" + userId + ")");
                string fileJSON = lua.GetString("response");
                NameValueCollection data = FileHandler.JsonToNVC(fileJSON);
                return data;
            }
            catch (LuaException e)
            {
                LuaError(e);
                NameValueCollection data = new NameValueCollection();
                return data;
            }  
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                NameValueCollection data = new NameValueCollection();
                return data;
            }
        }

        public NameValueCollection WriteAssetSMD(string assetId)
        {
            LuaClass lua = new LuaClass();
            
            log("Loading Converter API...");
            try
            {
                lua.load("lua/SMDconverter.lua");
                log("Writing SMD file", "Please wait...");
                lua.DoString("response = WriteAssetSMD(" + assetId + ")");
                string fileJSON = lua.GetString("response");
                NameValueCollection data = FileHandler.JsonToNVC(fileJSON);
                return data;
            }
            catch (LuaException e)
            {
                LuaError(e);
                NameValueCollection data = new NameValueCollection();
                return data;
            }
        }
        
        public string inQuotes(string str)
        {
            return "\"" + str + "\"";
        }

        private void goToViewer_Click(object sender = null, EventArgs e = null)
        {
            NameValueCollection gameInfo = DataManager.GetGameInfo(GameData, Properties.Settings.Default.SelectedGame);
            string studioMdlPath = gameInfo["StudioMdlDir"];
            string gamePath = Directory.GetParent(gameInfo["GameInfoDir"]).ToString();
            string binPath = Directory.GetParent(studioMdlPath).ToString();
            string hlmv = Path.Combine(binPath, "hlmv.exe");
            if (File.Exists(hlmv))
            {
                this.Enabled = false;
                Process modelViewer = Process.Start(hlmv, " -game " + inQuotes(gamePath) + " -model " + inQuotes(finalCompilePath));
                this.Hide();
                modelViewer.WaitForExit();
                this.Show();
                this.Enabled = true;
            }                
            else
            {
                error("Could not find hlmv.exe in " + binPath);
            }
        }

        public string getPathName()
        {
            string name;
            if (isAsset)
            {
                string json = http.DownloadString("http://api.roblox.com/marketplace/productinfo?assetId=" + id);
                NameValueCollection itemInfo = FileHandler.JsonToNVC(json);
                name = itemInfo["Name"].ToLower();
            }
            else
            {
                name = username.ToLower();
            }
            name = name.Replace(" ","_");
            name = name.Replace(".", "_DOT_");
            name = name.Replace(",", "");
            name = name.Replace("'", "");
            return name;
        }

        public void addLine(string str, string line,out string str_)
        {
            if (str.Length == 0)
            {
                str = str + line;
            }
            else
            {
                str = str + "\n" + line;
            }
            str_ = str;
        }

        private void goToModel_Click(object sender = null, EventArgs e = null)
        {
            Process.Start(Directory.GetParent(finalCompilePath).ToString());
        }

        private void returnToMenu_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public Compiler(string id_, bool isAsset_, string username_ = null, bool debugMode_ = false)
        {
            InitializeComponent();
            id = id_;
            isAsset = isAsset_;
            debugMode = debugMode_;
            if (username_ != null)
            {
                username = username_;
            }
        }

        private async void Compiler_Load(object sender, EventArgs e)
        {
            string appDataPath = Environment.GetEnvironmentVariable("AppData");
            string storagePath = Path.Combine(appDataPath, "Rbx2SrcFiles");
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }
            // idle_anim.smd
            // All models require an animation sequence, even if they aren't doing anything
            // This basically represents a static animation that does nothing.
            string idle = Path.Combine(storagePath,"models", "idle_anim.smd");
            string data = FileHandler.GetResource("models/idle_anim.smd");
            FileHandler.WriteToFileFromString(idle, data);
            NameValueCollection mtlData;
            string name = getPathName();
            NameValueCollection gameInfo = DataManager.GetGameInfo(GameData, Properties.Settings.Default.SelectedGame);
            string studioMdlPath = gameInfo["StudioMdlDir"];
            string gamePath = Directory.GetParent(gameInfo["GameInfoDir"]).ToString();
            string smdPath = Path.Combine(storagePath, "models", name + ".smd");
            string qcPath = Path.ChangeExtension(smdPath, "qc");
            if (isAsset)
            {
                assetDisplay.ImageLocation = "http://www.roblox.com/Game/Tools/ThumbnailAsset.ashx?aid=" + id + "&fmt=png&wd=420&ht=420";
                await Task.Delay(1000); // Make sure the window has time to show.
                Console.WriteLine("Writing");
                NameValueCollection assetSMD = WriteAssetSMD(id);
                string file = assetSMD["File"];
                string mtlDataJson = assetSMD["MtlData"];
                mtlData = FileHandler.JsonToNVC(mtlDataJson);
                log("StudioMDL writing completed.", "Saving File as:", smdPath);
                FileHandler.WriteToFileFromString(smdPath, file);
                log("Saved.");
                string qcFile = "";
                log("Writing QC file: " + qcPath);
                addLine(qcFile, "$modelname " + inQuotes("roblox/" + name + ".mdl"), out qcFile);
                addLine(qcFile, "$bodygroup " + inQuotes(name) + "\n{\n\tstudio " + inQuotes(name + ".smd") + "\n}", out qcFile);
                addLine(qcFile, "$cdmaterials " + inQuotes("models\\roblox\\" + name + "\\"), out qcFile);
                addLine(qcFile, "$sequence \"idle\" \"idle_anim.smd\"{\n\tfps 1 \n\tloop\n}", out qcFile);
                addLine(qcFile, "$collisionmodel " + inQuotes(name + ".smd") + "\n{\n\t$automass\n}", out qcFile);
                Console.WriteLine(qcFile);
                FileHandler.WriteToFileFromString(qcPath, qcFile);
            }
            else
            {
                assetDisplay.ImageLocation = "http://www.roblox.com/Thumbs/Avatar.ashx?width=420&height=420&format=png&userid=" + id;
                await Task.Delay(1000);
                NameValueCollection characterSMD = WriteCharacterSMD(id);
                string file = characterSMD["File"];
                string mtlDataJson = characterSMD["MtlData"];
                mtlData = FileHandler.JsonToNVC(mtlDataJson);
                string isArmUp = characterSMD["IsArmUp"];
                log("StudioMDL writing completed.", "Saving File as:", smdPath);
                FileHandler.WriteToFileFromString(smdPath, file);
                // Load a specific physics model based on whether or not the player's right arm is up or not.
                string physicsMdl = "physics_mdl.smd";
                if (isArmUp == "true")
                {
                    physicsMdl = "physics_mdl_armup.smd";
                }
                string physicsDir = Path.Combine(storagePath, "models","physics_mdl.smd");
                string physics = FileHandler.GetResource("models/" + physicsMdl);
                FileHandler.WriteToFileFromString(physicsDir, physics);
                string robloxian_root = Path.Combine(storagePath, "models", "robloxian_root.qc");
                log("Loading robloxian_root.qc");
                string root = FileHandler.GetResource("models/robloxian_root.qc");
                FileHandler.WriteToFileFromString(robloxian_root, root);
                log("Saved to: " + robloxian_root);
                FileHandler.WriteToFileFromString(robloxian_root, root);
                string qcFile = "";
                log("Writing QC file: " + qcPath);
                addLine(qcFile, "$modelname " + inQuotes("roblox/" + name + ".mdl"), out qcFile);
                addLine(qcFile, "$bodygroup " + inQuotes(name) + "\n{\n\tstudio " + inQuotes(name + ".smd") + "\n}", out qcFile);
                addLine(qcFile, "$cdmaterials " + inQuotes("models\\roblox\\" + name + "\\"), out qcFile);
                addLine(qcFile, "$include robloxian_root.qc", out qcFile);
                FileHandler.WriteToFileFromString(qcPath, qcFile);
            }
            log("COMPILING MODEL...");
            finalCompilePath = Path.Combine(gamePath, "models", "roblox",name + ".mdl");
            string asWhole = Path.Combine(finalCompilePath,finalModelName);
            if (File.Exists(asWhole))
            {
                File.Delete(asWhole);
            }
            log("Executing studiomdl.exe: ");
            string parameters = " -game " + inQuotes(gamePath) + " -nop4 -verbose  " + qcPath;
            log(inQuotes(studioMdlPath) + parameters);
            Console.WriteLine(inQuotes(studioMdlPath) + parameters);
            ProcessStartInfo studioMdl = new ProcessStartInfo();
            studioMdl.FileName = studioMdlPath;
            studioMdl.Arguments = parameters;
            studioMdl.CreateNoWindow = true;
            studioMdl.UseShellExecute = false;
            studioMdl.RedirectStandardOutput = true;
            Process studioMdl_Run = Process.Start(studioMdl);
            StreamReader output = studioMdl_Run.StandardOutput;
            while (studioMdl_Run.HasExited != true)
            {
                string line = await output.ReadLineAsync();
                log(line);
            }
            if (!File.Exists(asWhole))
            {
                NameValueCollection settings = FileHandler.GetAppSettings();
                error("studiomdl.exe unfortunately failed to compile the model!\nIf you are seeing this, take a screenshot of the console and tweet it to " + settings["twitterName"] + ".\nIt'll get fixed ASAP.\nThanks!");
            }
            log("Compiling Textures...");
            string mtlPath = GetDirectory(gamePath, "materials", "models", "roblox", name);
            foreach (string mtlName in mtlData.AllKeys)
            {
                string texHash = mtlData[mtlName];
                string vmtPath = Path.Combine(mtlPath,mtlName + ".vmt");
                compileTexture(mtlName, texHash, mtlPath);
                string vmtFile = "";
                addLine(vmtFile, "\"VertexLitGeneric\"", out vmtFile);
                addLine(vmtFile, "{", out vmtFile);
                addLine(vmtFile, "\t\"$basetexture\" \"models/roblox/" + name + "/" + mtlName + "\"", out vmtFile);
                addLine(vmtFile, "}", out vmtFile);
                FileHandler.WriteToFileFromString(vmtPath, vmtFile);
            }
            log("====================================================================");
            log("FINISHED COMPILING MODEL!");
            log("====================================================================");
            this.returnToMenu.Enabled = true;
            this.goToViewer.Enabled = true;
            this.goToModel.Enabled = true;
        }
    }
}
