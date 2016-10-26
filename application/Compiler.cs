﻿using System;
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
using System.Text.RegularExpressions;
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
        string id = "";
        string username = "";
        bool isAsset = true;
        bool debugMode = false;
        int logCharLimit = 55;
        int activeVTFCompilers = 0;

        private void log(params string[] logTxts)
        {
            foreach (string logTxt in logTxts)
            {
                if (logTxt != null && logTxt != "1/1 files completed.")
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

        private void onMessageOut(Object sender, MessageOutEventArgs e)
        {
            log(e.Message);
        }

        private void onFinished(Object sender, EventArgs e)
        {
            activeVTFCompilers = activeVTFCompilers - 1;
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
            activeVTFCompilers++;
            VTFProcessor processor = new VTFProcessor();
            processor.mtlName = mtlName;
            processor.texHash = texHash;
            processor.mtlDir = mtlDir;
            processor.ProcessingFinished += new EventHandler(onFinished);
            processor.MessageOut += new EventHandler<MessageOutEventArgs>(onMessageOut);
            processor.BeginProcessing();
        }

        public void LuaError(LuaException e)
        {
            NameValueCollection settings = FileHandler.GetAppSettings();
            fatalError("A fatal error occured! \n\nLine " + e.Message.Substring(17) + "\n" + e.StackTrace);
        }

        public NameValueCollection WriteCharacterSMD(string userId)
        {
            LuaClass lua = new LuaClass();
            lua.MessageOut += new EventHandler<MessageOutEventArgs>(onMessageOut);
            log("Loading Converter API...");
            try
            {
                lua.load("lua/SMDconverter.lua");
                lua.DoString("response = WriteCharacterSMD(" + userId + ")");
                string fileJSON = lua.GetString("response");
                NameValueCollection data = FileHandler.JsonToNVC(fileJSON);
                return data;
            }
            catch (LuaException e)
            {
                LuaError(e);
            }
            NameValueCollection error = new NameValueCollection();
            error.Add("ERROR", "");
            return error;
        }

        public NameValueCollection WriteAssetSMD(string assetId)
        {
            LuaClass lua = new LuaClass();
            lua.MessageOut += new EventHandler<MessageOutEventArgs>(onMessageOut);
            log("Loading Converter API...");
            try
            {
                lua.load("lua/SMDconverter.lua");
                lua.DoString("response = WriteAssetSMD(" + assetId + ")");
                string fileJSON = lua.GetString("response");
                NameValueCollection data = FileHandler.JsonToNVC(fileJSON);
                return data;
            }
            catch (LuaException e)
            {
                LuaError(e);
            }
            NameValueCollection error = new NameValueCollection();
            error.Add("ERROR", "");
            return error;
        }

        public string inQuotes(string str)
        {
            return "\"" + str + "\"";
        }

        private void goToViewer_Click(object sender = null, EventArgs e = null)
        {
            string currentGame = DataManager.GetConfigValue("SelectedGame");
            NameValueCollection gameInfo = DataManager.GetGameInfo(GameData, currentGame);
            string studioMdlPath = gameInfo["StudioMdlDir"];
            string gamePath = Directory.GetParent(gameInfo["GameInfoDir"]).ToString();
            string binPath = Directory.GetParent(studioMdlPath).ToString();
            string hlmv = Path.Combine(binPath, "hlmv.exe");
            if (File.Exists(hlmv))
            {
                Process modelViewer = Process.Start(hlmv, " -game " + inQuotes(gamePath) + " -model " + inQuotes(finalCompilePath));
                this.Close();
            }
            else
            {
                error("Could not find hlmv.exe in " + binPath);
            }
        }

        public NameValueCollection KeyValue(string key, string value)
        {
            NameValueCollection pair = new NameValueCollection();
            pair.Add(key,value);
            return pair;
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
            name = Regex.Replace(name, @"[^\w\.@-]","_",RegexOptions.None,TimeSpan.FromSeconds(1.5));
            return name;
        }

        public void logFancy(string text)
        {
            log("=======================================================");
            log(text.ToUpper());
            log("=======================================================");
        }

        private async void CompileModel(object sender = null, EventArgs e = null)
        {
            string appDataPath = Environment.GetEnvironmentVariable("AppData");
            string storagePath = GetDirectory(appDataPath, "Rbx2SrcFiles");
            string mdlPath = GetDirectory(storagePath, "models");
            NameValueCollection mtlData;
            string name = getPathName();
            string currentGame = DataManager.GetConfigValue("SelectedGame");
            NameValueCollection gameInfo = DataManager.GetGameInfo(GameData, currentGame);
            string studioMdlPath = gameInfo["StudioMdlDir"];
            string gamePath = Directory.GetParent(gameInfo["GameInfoDir"]).ToString();
            string smdPath = Path.Combine(mdlPath, name + ".smd");
            string qcPath = Path.ChangeExtension(smdPath, "qc");
            await Task.Delay(30);
            logFancy("Building Model");
            if (isAsset)
            {
                string idle = Path.Combine(storagePath, "models", "static_prop.smd");
                string data = FileHandler.GetResource("models/static_prop.smd");
                FileHandler.WriteToFileFromString(idle, data);
                assetDisplay.ImageLocation = "http://www.roblox.com/Game/Tools/ThumbnailAsset.ashx?aid=" + id + "&fmt=png&wd=420&ht=420";
                await Task.Delay(50);
                NameValueCollection assetSMD = WriteAssetSMD(id);
                if (assetSMD["ERROR"] == "") return;
                string file = assetSMD["File"];
                string mtlDataJson = assetSMD["MtlData"];
                mtlData = FileHandler.JsonToNVC(mtlDataJson);
                log("FINISHED BUILDING MESH!", "Saving File as:", smdPath);
                FileHandler.WriteToFileFromString(smdPath, file);
                log("Saved.");
                log("Writing QC file: " + qcPath);
                FileWriter qcFile = new FileWriter();
                NameValueCollection commands = new NameValueCollection();
                qcFile.WriteCommand("modelname", "roblox/" + name + ".mdl");
                qcFile.WriteCommand("bodygroup", name);
                qcFile.WriteInBrackets(false,"studio " + inQuotes(name + ".smd"));
                qcFile.WriteCommand("sequence", "static", "static_prop.smd");
                qcFile.WriteInBrackets(true, "fps 1", "loop");
                qcFile.WriteCommand("collisionmodel", name + ".smd");
                qcFile.WriteCommand("cdmaterials", "models/roblox/" + name + "/");
                FileHandler.WriteToFileFromString(qcPath, qcFile.ToString());
            }
            else
            {
                assetDisplay.ImageLocation = "http://www.roblox.com/Thumbs/Avatar.ashx?width=420&height=420&format=png&userid=" + id;
                await Task.Delay(50);
                NameValueCollection characterSMD = WriteCharacterSMD(id);
                if (characterSMD["ERROR"] == "") return;
                string file = characterSMD["File"];
                string[] animations = new string[] { "reference", "walk", "idle", "jump", "falling", "toolup" };
                foreach (string animation in animations)
                {
                    string path = Path.Combine(storagePath, "models", animation + "_anim.smd");
                    string data = FileHandler.GetResource("models/animations/" + animation + ".smd");
                    FileHandler.WriteToFileFromString(path, data);
                }
                string mtlDataJson = characterSMD["MtlData"];
                mtlData = FileHandler.JsonToNVC(mtlDataJson);
                log("FINISHED BUILDING MESH!", "Saving File as:", smdPath);
                FileHandler.WriteToFileFromString(smdPath, file);
                string robloxian_root = Path.Combine(storagePath, "models", "robloxian_root.qc");
                log("Loading robloxian_root.qc");
                string root = FileHandler.GetResource("models/robloxian_root.qc");
                log("Saved to: " + robloxian_root);
                FileHandler.WriteToFileFromString(robloxian_root, root);
                string collision = FileHandler.GetResource("models/collision.qc");
                collision = collision.Replace("physics_mdl", name);
                log("Writing QC file: " + qcPath);
                FileWriter qcFile = new FileWriter();
                qcFile.WriteCommand("modelname", "roblox/" + name + ".mdl");
                qcFile.WriteCommand("bodygroup", name);
                qcFile.WriteInBrackets(false, "studio " + inQuotes(name + ".smd"));
                qcFile.WriteCommand("cdmaterials", "models/roblox/" + name + "/");
                qcFile.WriteLine(collision);
                qcFile.WriteCommand("include", "robloxian_root.qc");
                FileHandler.WriteToFileFromString(qcPath, qcFile.ToString());
            }
            logFancy("Compiling Model...");
            finalCompilePath = Path.Combine(gamePath, "models", "roblox", name + ".mdl");
            log("Executing studiomdl.exe: ");
            string parameters = " -game " + inQuotes(gamePath) + " -nop4 -verbose " + qcPath;
            log(inQuotes(studioMdlPath) + parameters);
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
            if (!File.Exists(finalCompilePath))
            {
                NameValueCollection settings = FileHandler.GetAppSettings();
                error("studiomdl.exe unfortunately failed to compile the model! Try to compile it again!\nIf you see this message multiple times, take a screenshot of the console and tweet it to " + settings["twitterName"] + ".\nIt'll get fixed ASAP.\nThanks!");
                logFancy("Failed to compile model!");
            }
            else
            {
                logFancy("Preparing Textures...");
                string mtlPath = GetDirectory(gamePath, "materials", "models", "roblox", name);
                foreach (string mtlName in mtlData.AllKeys)
                {
                    string texHash = mtlData[mtlName];
                    string vmtPath = Path.Combine(mtlPath, mtlName + ".vmt");
                    log("Found Texture: " + texHash);
                    compileTexture(mtlName, texHash, mtlPath);
                    FileWriter vmtFile = new FileWriter();
                    vmtFile.AddLine(inQuotes("VertexLitGeneric"));
                    vmtFile.WriteInBrackets(false, vmtFile.FormatCommand("basetexture", "models/roblox/" + name + "/" + mtlName));
                    FileHandler.WriteToFileFromString(vmtPath, vmtFile.ToString());
                }
                logFancy("Compiling Textures...");
                while (activeVTFCompilers > 0)
                {
                    await Task.Delay(250);
                }
                logFancy("Finished compiling model!");
                this.goToModel.Enabled = true;
            }
            this.returnToMenu.Enabled = true;
            this.recompileModel.Enabled = true;
        }

        private void recompileModel_Click(object sender = null, EventArgs e = null)
        {
            DialogResult result = MessageBox.Show("Are you sure you'd like to compile again?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                this.returnToMenu.Enabled = false;
                this.recompileModel.Enabled = false;
                this.goToModel.Enabled = false;
                this.ConsoleDisp.Items.Clear();
                CompileModel();
            }
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
    }
}
