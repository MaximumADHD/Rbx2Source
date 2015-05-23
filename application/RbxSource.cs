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
using System.Diagnostics;
using System.Net;
using Newtonsoft;
using Newtonsoft.Json;
using LuaInterface;

namespace RobloxToSourceEngine
{
    public partial class Rbx : Form
    {
        GameDataManager DataManager = new GameDataManager();
        FileHandler FileHandler = new FileHandler();
        WebClient http = new WebClient();

        long assetId = 19027209;
        long currentAssetId = 19027209;
        string userName = "CloneTrooper1019";

        bool assetToggled = true;
        bool controlsActive = false;

        private void refreshGameList()
        {
            List<NameValueCollection> GameData = DataManager.GetGameData();
            setControlsActive(GameData.Count > 0);
            gameList.Items.Clear();
            string selectedGame = DataManager.GetConfigValue("SelectedGame");
            foreach (NameValueCollection game in GameData)
            {
                string name = game["Name"];
                if (selectedGame.Length == 0)
                {
                    selectedGame = name;
                    DataManager.SetConfigValue("SelectedGame",name);
                }
                gameList.Items.Add(name);
            }
            gameList.Text = selectedGame;
            if (!gameList.Enabled)
            {
                gameList.Items.Add("No games loaded!");
                gameList.SelectedIndex = 0;
            }
        }

        private void showUserError(string errorMsg)
        {
            MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void onWindowClosed(object sender, FormClosedEventArgs e)
        {
            this.Text = "Rbx2Source Converter Tool";
            this.ControlBox = true;
            this.Enabled = true;
            this.Focus();
            refreshGameList();
        }

        public void setControlsActive(bool active)
        {
            gameList.Enabled = active;
            controlsActive = active;
            compile.Enabled = active;
            inputAssetID.Enabled = (assetToggled && active);
            inputUsername.Enabled = (assetToggled != true && active);
            enterAssetId.Enabled = (assetToggled && active);
            enterUsername.Enabled = (assetToggled != true && active);
            toggleAssetId.Enabled = (active && !assetToggled);
            toggleUserId.Enabled = (active && assetToggled);
        }

        public Rbx()
        {
            InitializeComponent();
            refreshGameList();
        }

        public string userIdFromUsername(string username)
        {
            try
            {
                string userInfo = http.DownloadString("http://api.roblox.com/users/get-by-username?username=" + username);
                if (!userInfo.Contains("\"success\":false"))
                {
                    NameValueCollection data = FileHandler.JsonToNVC(userInfo);
                    Console.WriteLine(data);
                    Console.WriteLine(data["Id"]);
                    return data["Id"];
                }
                else
                {
                    return "-1";
                }
  
            }
            catch
            {
                return "-1";
            }
        }

        private void toggleAssetId_Click(object sender, EventArgs e)
        {
            assetToggled = true;

            inputAssetID.Enabled = true;
            enterAssetId.Enabled = true;

            inputUsername.Enabled = false;
            enterUsername.Enabled = false;

            toggleAssetId.Enabled = false;
            toggleUserId.Enabled  = true;

            assetDisplay.ImageLocation = "http://www.roblox.com/Game/Tools/ThumbnailAsset.ashx?aid=" + assetId + "&fmt=png&wd=420&ht=420";
        }

        private void toggleUserId_Click(object sender, EventArgs e)
        {
            assetToggled = false;

            inputAssetID.Enabled = false;
            enterAssetId.Enabled = false;

            inputUsername.Enabled = true;
            enterUsername.Enabled = true;

            toggleAssetId.Enabled = true;
            toggleUserId.Enabled = false;

            assetDisplay.ImageLocation = "http://www.roblox.com/Thumbs/Avatar.ashx?width=420&height=420&format=png&username=" + userName;
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            this.ControlBox = false;
            this.Text = "(Locked while config window is active)";
            ConfigWindow window = new ConfigWindow();
            window.Owner = this;
            window.Show();
            window.Focus();
            window.FormClosed += new FormClosedEventHandler(onWindowClosed);
        }

        bool inInputEvent = false;
        private void inputAssetID_TextChanged(object sender, EventArgs e)
        {
            if (!inInputEvent)
            {
                inInputEvent = true;
                int test = 0;
                Int32.TryParse(inputAssetID.Text, out test);
                if (test != 0)
                {
                    Int64.TryParse(inputAssetID.Text, out currentAssetId);
                }
                inputAssetID.Text = currentAssetId.ToString();
                inputAssetID.SelectionStart = inputAssetID.Text.Length;
                inInputEvent = false;
            }
        }

        private void enterUsername_Click(object sender, EventArgs e)
        {
            string userId = userIdFromUsername(inputUsername.Text);
            Console.WriteLine("USERID: " + userId);
            if (userId != "-1")
            {
                try
                {
                    string json = http.DownloadString("http://www.roblox.com/avatar-thumbnail-3d/json?userId=" + userId);
                    if (!json.Contains("null"))
                    {
                        userName = inputUsername.Text;
                        assetDisplay.ImageLocation = "http://www.roblox.com/Thumbs/Avatar.ashx?width=420&height=420&format=png&username=" + userName;
                        MessageBox.Show("This character is fully compatable!\n If your selected game is configured correctly, you may now compile!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                    }
                    else
                    {
                        showUserError("Could not load mesh for this user!\nRoblox may still be loading the character.\nTry again in a few moments.");
                    }
                }
                catch
                {
                    showUserError("Could not load mesh for this user!\nRoblox may still be loading the character.\nTry again in a few moments.");
                }
            }
            else
            {
                showUserError("This user does not exist!");
            }
            inputUsername.Text = userName;
        }

        private void enterAssetId_Click(object sender, EventArgs e)
        {
            string test = inputAssetID.Text;
            string json = http.DownloadString("http://api.roblox.com/marketplace/productinfo?assetId=" + test);
            if (json.Length > 0)
            {
                ListDictionary data = JsonConvert.DeserializeObject<ListDictionary>(json);
                NameValueCollection assetInfo = new NameValueCollection();
                foreach (DictionaryEntry entry in data)
                {
                    if (entry.Value != null)
                    {
                        assetInfo.Add(entry.Key.ToString(), entry.Value.ToString());
                    }
                }
                string type = assetInfo["AssetTypeId"];
                if (type == "8" || type == "19") // 8 = Hat, 19 = Gear
                {
                    string json3D = http.DownloadString("http://www.roblox.com/asset-thumbnail-3d/json?assetId=" + test);
                    if (!(json3D.Contains("null")))
                    {
                        assetDisplay.ImageLocation = "http://www.roblox.com/Game/Tools/ThumbnailAsset.ashx?aid=" + test + "&fmt=png&wd=420&ht=420";
                        MessageBox.Show("This item is fully compatable!\n If your selected game is configured correctly, you may now compile!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        Int64.TryParse(test, out assetId);
                    }
                    else
                    {
                        showUserError("Could not load mesh for this asset!\nTry again in a few moments.");
                    }
                }
                else
                {
                    showUserError("This AssetId must be a Hat or a Gear.");
                }
            }
            else
            {
                showUserError("This AssetId does not exist!");
            }
            inputAssetID.Text = assetId.ToString();
        }

        private void compile_Click(object sender, EventArgs e)
        {
            // Make sure that the game we're using is properly configured.
            List<NameValueCollection> GameData = DataManager.GetGameData();
            string selectedGame = DataManager.GetConfigValue("SelectedGame");
            NameValueCollection GameInfo = DataManager.GetGameInfo(GameData, selectedGame);
            if (!DataManager.NeedsInit(GameInfo))
            {
                if (GameInfo["StudioMdlDir"] != "")
                {
                    string id;
                    if (assetToggled)
                    {
                        id = assetId.ToString();
                    }
                    else
                    {
                        id = userIdFromUsername(userName);
                    }
                    Compiler compile = new Compiler(id, assetToggled,userName);
                    this.Enabled = false;
                    this.ControlBox = false;
                    this.Text = "(Locked while compiler window is active)";
                    compile.Owner = this;
                    try
                    {
                        compile.Show();
                        compile.Focus();
                        compile.FormClosed += new FormClosedEventHandler(onWindowClosed);
                    }
                    catch
                    {
                        Application.Exit();
                    }
                }
                else
                {
                    showUserError("studiomdl.exe is not defined for " + selectedGame + "!\nFix this and try again.");
                }
            }
            else
            {
                showUserError("The configuration for " + selectedGame + " is corrupted!");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            DialogResult result = MessageBox.Show("Are you sure you'd like to exit?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
            else
            {
                this.Enabled = true;
            }
        }

        private void gameList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gameList.Text != "No games loaded!" )
            {
                DataManager.SetConfigValue("SelectedGame",gameList.Text);
            }
        }
    }
}
