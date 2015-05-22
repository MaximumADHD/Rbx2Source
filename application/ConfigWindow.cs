using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;
using LuaInterface;

namespace RobloxToSourceEngine
{
    public partial class ConfigWindow : Form
    {
        bool inChangeEvent = false;

        GameDataManager DataManager = new GameDataManager();

        public ConfigWindow()
        {
            InitializeComponent();
            Properties.Settings.Default.SettingChanging += new System.Configuration.SettingChangingEventHandler(onSettingChanged);
        }

        public void onSettingChanged(object sender, SettingChangingEventArgs e)
        {
            updateGameList();
        }

        public void applyDirectories(List<NameValueCollection> GameData, string gameName)
        {
            NameValueCollection gameInfo = DataManager.GetGameInfo(GameData, gameName);
            if (!DataManager.NeedsInit(gameInfo))
            {
                gameList.Text = gameName;
                inputGameInfo.Text = gameInfo["GameInfoDir"];
                inputStudioMDL.Text = gameInfo["StudioMdlDir"];
            }
        }

        public void error(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            
            DialogResult result = MessageBox.Show("Are you sure you'd like to delete this?\nThis cannot be undone.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                inChangeEvent = false;
                DataManager.RemoveGameInfo(gameList.Text);
                updateGameList();
                List<NameValueCollection> GameData = DataManager.GetGameData();
                try
                {
                    NameValueCollection firstGame = GameData[0];
                    applyDirectories(GameData, firstGame["Name"]);
                }
                catch
                {
                    this.inputStudioMDL.Text = "";
                    this.inputStudioMDL.Enabled = false;
                    this.inputGameInfo.Text = "";
                    this.inputGameInfo.Enabled = false;
                    this.removeButton.Enabled = false;
                }
            }
        }

        public void updateGameList(object sender = null, EventArgs e = null)
        {
            if (!inChangeEvent)
            {
                List<NameValueCollection> GameData = DataManager.GetGameData();
                inChangeEvent = true;
                if (gameList.Text != "")
                {
                    Properties.Settings.Default.SelectedGame = gameList.Text;
                    Properties.Settings.Default.Save();
                }
                gameList.Items.Clear();
                studiomdlSearch.Enabled = (GameData.Count > 0);
                removeButton.Enabled = (GameData.Count > 0);
                if (GameData.Count > 0)
                {
                    gameList.Enabled = true;
                    string selectedGame = Properties.Settings.Default.SelectedGame;
                    foreach (NameValueCollection game in GameData)
                    {
                        string gameName = game["Name"];
                        if (gameName != null)
                        {
                            gameList.Items.Add(gameName);
                        }
                        
                    }
                    applyDirectories(GameData, selectedGame);
                }
                else
                {
                    gameList.Enabled = false;
                    gameList.Items.Add("No games loaded!");
                    gameList.SelectedIndex = 0;
                }
                inChangeEvent = false;
            }
        }

        public string getSteamPath()
        {
            string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            string steamPath = Path.Combine(programFiles, "Steam", "steamapps", "common");
            if (Directory.Exists(steamPath))
            {
                return steamPath;
            }
            throw new Exception();
        }

        public void addGame(string gameInfoPath, bool showFail = true)
        {
            string gameInfo = File.ReadAllText(gameInfoPath);
            string gameName = DataManager.IdentifyGameName(gameInfo);
            if (gameName != "ERROR")
            {
                string studiomdlPath = "";
                string root = Directory.GetParent(Directory.GetParent(gameInfoPath).ToString()).ToString();
                string bin = Path.Combine(root, "bin");
                bool success = false;
                if (Directory.Exists(bin))
                {
                    string studiomdl = Path.Combine(bin, "studiomdl.exe");
                    if (File.Exists(studiomdl))
                    {
                        success = true;
                        studiomdlPath = studiomdl;
                        List<NameValueCollection> GameData = DataManager.GetGameData();
                        DataManager.PushChange(GameData, gameName, gameInfoPath, studiomdlPath);
                        DataManager.Save(DataManager.GameDataToJSON(GameData));
                        applyDirectories(GameData, gameName);     
                    }
                }
                if (!success && showFail)
                {
                    error("Could not find studiomdl.exe for '" + gameName + "'");
                }
            }
            else if (showFail)
            {
                error("Invalid gameinfo.txt file (COULD NOT IDENTIFY GAME NAME)");
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            try
            {
                string steamPath = getSteamPath();
                openFileDialog.InitialDirectory = steamPath;
            }
            catch
            {
                openFileDialog.InitialDirectory = programFiles;
            }
            openFileDialog.Filter = "Game Info File | gameinfo.txt";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                addGame(openFileDialog.FileName);
            }
            this.Enabled = true;
        }

        private void studiomdlSearch_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            openFileDialog.Filter = "SMD Compiler | studiomdl.exe";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string selectedGame = Properties.Settings.Default.SelectedGame;
                List<NameValueCollection> GameData = DataManager.GetGameData();
                NameValueCollection Game = DataManager.GetGameInfo(GameData, selectedGame);
                DataManager.PushChange(GameData, Game["Name"], Game["GameInfoDir"], openFileDialog.FileName);
                DataManager.Save(DataManager.GameDataToJSON(GameData));
                applyDirectories(GameData, Game["Name"]);
            }
            this.Enabled = true;
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void scanSteam_Click(object sender, EventArgs e)
        {
            try
            {
                string steamPath = getSteamPath();
                DialogResult result = MessageBox.Show("Are you sure you would like to scan Steam for gameinfo.txt files?\n\nThis will clear any games currently loaded into Rbx2Source, and will attempt to load Source Engine games from your Steam directory.\n\nTHIS CAN TAKE SEVERAL MOMENTS. YOU WILL NEED TO BE PATIENT.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    doneButton.Enabled = false;
                    gameList.Enabled = false;
                    Properties.Settings.Default.GameData = "{}"; // Quick Reset.
                    Properties.Settings.Default.SelectedGame = "";
                    Properties.Settings.Default.Save();
                    scanLabel.Text = "Scanning for Source\nEngine Games...";
                    await Task.Delay(500);
                    List<string> sourceGames = new List<string>();
                    foreach (string folder in Directory.GetDirectories(steamPath))
                    {
                        string myFolder = folder;
                        string game = Path.Combine(folder,"game");
                        if (Directory.Exists(game))
                        {
                            myFolder = game;
                        }
                        string appId = Path.Combine(myFolder, "steam_appid.txt");
                        string studioMdl = Path.Combine(myFolder, "bin","studiomdl.exe");
                        if (File.Exists(studioMdl) && File.Exists(appId))
                        {
                            string name = folder.Replace(steamPath + "\\", "");
                            if (name.Length > 12)
                            {
                                name = name.Substring(0, 12) + "...";
                            }
                            Console.WriteLine(name);
                            scanLabel.Text = "Found Game:\n" + name;
                            sourceGames.Add(myFolder);
                            await Task.Delay(100);
                        }
                    }
                    List<string> gameInfoPaths = new List<string>();
                    foreach (string sourceGame in sourceGames)
                    {
                        foreach(string gameInfoPath in Directory.GetFiles(sourceGame,"gameinfo.txt",SearchOption.AllDirectories))
                        {
                            bool canProceed = true;
                            if (gameInfoPath.Contains("SourceFilmmaker") && !gameInfoPath.Contains("usermod"))
                            {
                                canProceed = false;
                            }
                            if (canProceed)
                            {
                                if (!gameInfoPath.Contains("bin") && !gameInfoPath.Contains("movie"))
                                {
                                    gameInfoPaths.Add(gameInfoPath);
                                }
                                if (!gameInfoPath.Contains("Half-Life 2\\"))
                                {
                                    break;
                                }
                            }
                            
                        }
                    }
                    int current = 0;
                    foreach (string gameInfoPath in gameInfoPaths)
                    {
                        current++;
                        scanLabel.Text = "Loading Games (" + current + "/" + gameInfoPaths.Count + ")";
                        Console.WriteLine(scanLabel.Text);
                        await Task.Delay(100);
                        addGame(gameInfoPath, false);
                    }
                    scanLabel.Text = "";
                    doneButton.Enabled = true;
                    MessageBox.Show("Scan completed!\n" + current + " games were imported.","Success!",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                }
            }
            catch
            {
                error("Could not find Steam Directory");
            }
        }
    }
}
