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
                        gameList.Items.Add(game["Name"]);
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

        private void addButton_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            openFileDialog.Filter = "Game Info File | gameinfo.txt";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string gameInfoPath = openFileDialog.FileName;
                string gameInfo = File.ReadAllText(gameInfoPath);
                string gameName = DataManager.IdentifyGameName(gameInfo);
                string studiomdlPath = "";
                /*/
                   Try to locate studiomdl.exe if possible.
                   To do this, we get the parent of the parent of the gameinfo.txt file, and try to find the bin folder in there (where those exes are located)
                   So for example: 

                   Steam/steamapps/common/Team Fortress 2/tf/gameinfo.txt -> Steam/steamapps/common/Team Fortress 2/tf/ ->
                   Steam/steamapps/common/Team Fortress 2/ -> Steam/steamapps/common/Team Fortress 2/bin ->
                
                   Steam/steamapps/common/Team Fortress 2/bin/studiomdl.exe 
                /*/
                string root = Directory.GetParent(Directory.GetParent(gameInfoPath).ToString()).ToString();
                string bin = Path.Combine(root, "bin");
                if (Directory.Exists(bin))
                {
                    // :D!
                    string vtex = Path.Combine(bin, "vtex.exe");
                    string studiomdl = Path.Combine(bin, "studiomdl.exe");
                    if (File.Exists(studiomdl))
                    {
                        studiomdlPath = studiomdl;
                    }
                }
                List<NameValueCollection> GameData = DataManager.GetGameData();
                DataManager.PushChange(GameData, gameName, gameInfoPath, studiomdlPath);
                DataManager.Save(DataManager.GameDataToJSON(GameData));
                applyDirectories(GameData, gameName);
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
    }
}
