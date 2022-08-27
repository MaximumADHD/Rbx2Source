using Microsoft.Win32;
using Rbx2Source.Assembler;
using Rbx2Source.Compiler;
using Rbx2Source.Resources;
using Rbx2Source.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiscordRPC;
using DiscordRPC.Logging;

namespace Rbx2Source
{
    public partial class Rbx2Source : Form
    {
        private class OutputLog
        {
            public string Message;
            public FontStyle FontStyle;

            public OutputLog()
            {
                Message = "";
                FontStyle = FontStyle.Regular;
            }

            public OutputLog(string message, FontStyle style = FontStyle.Regular)
            {
                Message = message;
                FontStyle = style;
            }
        }

        public const float MODEL_SCALE = 10;
        public Launcher baseProcess;

        private UserInfo currentUser;
        private long currentAssetId = 19027209;

        private GameInfo selectedGame;
        private string latestCompiledModel;
        private GameInfo latestCompiledOnGame;
        private readonly Dictionary<string, GameInfo> sourceGames = new Dictionary<string, GameInfo>();

        private Dictionary<Control, string> Links;
        private List<Control> CONTROLS_TO_DISABLE_WHEN_COMPILING;

        private static int stackLevel = 0;
        private static readonly List<OutputLog> outputQueue = new List<OutputLog>();
        private const string outputDivider = "---------------------------------------------------------------------------";

        private static readonly Dictionary<string, bool> progressQueue = new Dictionary<string, bool>();
        private static bool updateProgressQueue;

        private static readonly Image loadingImage = Properties.Resources.Loading;
        private static readonly Image brokenImage = Properties.Resources.BrokenPreview;

        private static Image debugImage;
        private string assetPreviewImage = "";

        public Rbx2Source()
        {
            UserAvatar defaultAvatar = UserAvatar.FromUserId(62601805);
            currentUser = defaultAvatar.UserInfo;

            InitializeComponent();
            InitializeRPC();

            if (!Debugger.IsAttached)
            {
                quickCompile.Visible = false;
                MainTab.Controls.Remove(Debug);
            }
        }
        public DiscordRpcClient rpcClient;
        void InitializeRPC()
        {
            rpcClient = new DiscordRpcClient("1012837153757208576");
            if (!Debugger.IsAttached)
            {
                rpcClient.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
                rpcClient.OnReady += (sender, e) =>
                {
                    Console.WriteLine("Received Ready from user {0}", e.User.Username);
                };

                rpcClient.OnPresenceUpdate += (sender, e) =>
                {
                    Console.WriteLine("Received Update! {0}", e.Presence);
                };
            }
            rpcClient.Initialize();
            rpcClient.SetPresence(new RichPresence()
            {
                Details = "Converting Assets",
                State = "Converting Roblox Assets to Source",
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    LargeImageText = $"Running Version {Settings.GetString("CurrentVersion")}",
             //       SmallImageKey = "image_small"
                }
            });
        }
        public static void ScheduleTasks(params string[] tasks)
        {
            foreach (string task in tasks)
            {
                if (!progressQueue.ContainsKey(task))
                {
                    progressQueue[task] = false;
                }
            }
        }

        public static void MarkTaskCompleted(string completedTask)
        {
            if (progressQueue.ContainsKey(completedTask))
            {
                progressQueue[completedTask] = true;
                updateProgressQueue = true;
            }
        }

        public static string GetEnumName<T>(T value)
        {
            if (value == null)
                return "null";

            return Enum.GetName(typeof(T), value);
        }

        private void WriteLine(string line)
        {
            output.AppendText(line + "\r\n");
        }

        private static void PrintInternal(OutputLog log)
        {
            outputQueue.Add(log);
        }

        public static void IncrementStack()
        {
            stackLevel++;
        }

        public static void DecrementStack()
        {
            stackLevel--;
        }

        public static void Print(string msg)
        {
            int s = stackLevel + 1;

            while (s-- > 0)
                msg = "    " + msg;

            OutputLog log = new OutputLog(msg);
            PrintInternal(log);
        }

        public static void PrintHeader(string msg)
        {
            Contract.Requires(msg != null);
            PrintInternal(new OutputLog("[" + msg.ToUpperInvariant() + "]", FontStyle.Bold));
        }

        public static void Print(string msgFormat, params object[] values)
        {
            if (msgFormat == null)
                throw new ArgumentNullException(nameof(msgFormat));

            for (int i = 0; i < values.Length; i++)
            {
                string match = "{" + i + "}";
                msgFormat = msgFormat.Replace(match, values[i].ToInvariantString());
            }

            Print(msgFormat);
        }

        public static void SetDebugImage(Image img)
        {
            debugImage = img;
        }

        private static void showError(string msg, bool fatal = false)
        {
            MessageBox.Show(msg, (fatal ? "FATAL " : "") + "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (!fatal)
                return;

            Application.Exit();
        }

        private static string  ConfigLoader()
        {
            if (File.Exists("config.txt")) 
            {
                string configDir = File.ReadAllText("config.txt");
//                string steamPath = Path.Combine(configDir, "steamapps", "common"); // Previous steam folder logic
                return configDir;
                
            }
            else
            {
                string steamPath = @"C:\Program Files (x86)\Steam";
                return steamPath;
            }

        }

        private static string[] getStringsInQuotes(string str)
        {
            List<int> quoteLocs = new List<int>();
            char lastChar = '\0';

            for (int i = 0; i < str.Length; i++)
            {
                char next = str[i];

                if (next == '"' && lastChar != '\\')
                    quoteLocs.Add(i);

                lastChar = next;
            }

            if (quoteLocs.Count % 2 != 0)
                throw new Exception("Line " + str + " has an unclosed quote! Thanks Obama.");

            List<string> captures = new List<string>();

            for (int i = 0; i < quoteLocs.Count; i += 2)
            {
                int j = i + 1;

                int pos0 = quoteLocs[i];
                int pos1 = quoteLocs[j];

                string captured = str.Substring(pos0 + 1, pos1 - pos0 - 1);
                captures.Add(captured);
            }

            return captures.ToArray();
        }

        private void gatherSourceGames(string steamDir)
        {
            //string steamPath = Path.Combine(steamDir, "steamapps", "common");
            string configDir = ConfigLoader();
            string steamPath = Path.Combine(configDir, "steamapps", "common");
            if (Directory.Exists(steamPath))
            {
                foreach (string game in Directory.GetDirectories(steamPath))
                {
                    if (!game.Contains("Source SDK"))
                    {
                        string bin = Path.Combine(game, "bin");
                        bool usingAltRoute = false;
                        if (!Directory.Exists(bin))
                        {
                            string altRoute = Path.Combine(game, "game", "bin");

                            if (Directory.Exists(altRoute))
                            {
                                // Source Filmmaker
                                usingAltRoute = true;
                                bin = altRoute;
                            }
                            else
                            {
                                // Fistful of Frags
                                string altRoute2 = Path.Combine(game, "sdk", "bin");

                                if (Directory.Exists(altRoute2))
                                {
                                    usingAltRoute = true;
                                    bin = altRoute2;
                                }
                            }
                        }
                        if (Directory.Exists(bin))
                        {
                            string studioMdl = Path.Combine(bin, "studiomdl.exe");
                            if (File.Exists(studioMdl))
                            {
                                string path = "";

                                if (usingAltRoute)
                                {
                                    string userMod = Path.Combine(game, "game", "usermod");
                                    if (Directory.Exists(userMod))
                                    {
                                        path = Path.Combine(userMod, "gameinfo.txt");
                                    }
                                }

                                foreach (string gameDir in Directory.GetDirectories(game))
                                {
                                    if (!gameDir.Equals(bin, StringComparison.InvariantCulture))
                                    {
                                        string gameInfoPath = Path.Combine(gameDir, "gameinfo.txt");
                                        if (File.Exists(gameInfoPath))
                                        {
                                            path = gameInfoPath;
                                            break;
                                        }
                                    }
                                }

                                try
                                {
                                    GameInfo info = new GameInfo(path);

                                    if (info.ReadyToUse)
                                    {
                                        string gameName = info.GameName;
                                        sourceGames[gameName] = info;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("ERROR GETTING GAMEINFO");
                                    Console.WriteLine(e.Message);
                                    Console.WriteLine(e.StackTrace);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void updateDisplays()
        {
            assetPreview.Image = loadingImage;

            if (compilerTypeSelect.Text == "Avatar")
            {
                assetPreviewImage = "https://www.roblox.com/headshot-thumbnail/json?width=420&height=420&format=png&userId=" + currentUser.Id;
                compilerInput.Text = "Username:";
                compilerInputField.Text = currentUser.Username;
                compilerTypeIcon.Image = Properties.Resources.Humanoid_icon;
            }
            else if (compilerTypeSelect.Text == "Accessory/Gear")
            {
                assetPreviewImage = "https://www.roblox.com/asset-thumbnail/json?width=420&height=420&format=png&assetId=" + currentAssetId;
                compilerInput.Text = "AssetId:";
                compilerInputField.Text = currentAssetId.ToInvariantString();
                compilerTypeIcon.Image = Properties.Resources.Accoutrement_icon;
            }
        }

        private bool TrySetUsername(string userName)
        {
            UserAvatar avatar = UserAvatar.FromUsername(userName);

            if (avatar.UserExists)
            {
                Settings.SaveSetting("Username", userName);
                assetPreview.Image = loadingImage;
                currentUser = avatar.UserInfo;
                return true;
            }
            else
            {
                showError("An error occurred while trying to fetch this user!\n" +
                          "Either the user does not exist, is banned or something went wrong with the request.");

                return false;
            }
        }

        private bool TrySetAssetId(object value)
        {
            string text = value.ToString();

            if (long.TryParse(text, out long assetId))
            {
                Asset asset = null;

                try
                {
                    asset = Asset.Get(assetId);
                }
                catch
                {
                    showError("This AssetId isn't configured correctly on Roblox's end.\n\n" +
                              "This error usually happens if you input a very old AssetId that doesn't exist on their servers.\n\n" +
                              "Try something else!");
                }

                if (asset != null)
                {
                    AssetType assetType = asset.AssetType;
                    bool isAccessory = AssetGroups.IsTypeInGroup(assetType, AssetGroup.Accessories);

                    if (isAccessory || assetType == AssetType.Gear)
                    {
                        assetPreview.Image = loadingImage;
                        currentAssetId = assetId;

                        Settings.SaveSetting("AssetId", assetId.ToInvariantString());
                        return true;
                    }
                    else
                    {
                        showError("AssetType received: " + GetEnumName(assetType) + "\n\nExpected one of the following asset types:\n* Accessory\n* Gear\n\nTry again!");
                    }
                }
            }
            else
            {
                showError("Invalid AssetId!");
            }

            return false;
        }

        private void compilerInputField_Enter(object sender, EventArgs e)
        {
            compilerInputField.Clear();
        }

        private async void compilerInputField_Leave(object sender = null, EventArgs e = null)
        {
            if (compilerInputField.Enabled)
            {
                compilerInputField.Enabled = false;
                compilerTypeSelect.Enabled = false;

                if (compilerTypeSelect.Text == "Avatar")
                    TrySetUsername(compilerInputField.Text);
                else if (compilerTypeSelect.Text == "Accessory/Gear")
                    TrySetAssetId(compilerInputField.Text);

                updateDisplays();
                await Task.Delay(100);

                compilerTypeSelect.Enabled = true;
                compilerInputField.Enabled = true;
            }
        }

        private void compilerInputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ActiveControl = null;
            }
        }

        private async Task UpdateCompilerState()
        {
            while (outputQueue.Count > 0)
            {
                OutputLog next = outputQueue[0];
                outputQueue.RemoveAt(0);

                if (!output.IsDisposed) // Prevents a crash if the user quits during the compilation.
                {
                    output.SelectionFont = new Font(output.Font, next.FontStyle);
                    WriteLine(next.Message);
                }
            }

            if (updateProgressQueue)
            {
                int tasksDone = 0;
                int totalTasks = progressQueue.Keys.Count;

                foreach (string task in progressQueue.Keys)
                {
                    bool completed = progressQueue[task];
                    tasksDone += (completed ? 1 : 0);
                }

                double progress = ((double)tasksDone / totalTasks) * 100.0;
                compileProgress.Value = (int)progress;
                updateProgressQueue = false;
            }

            await Task.Delay(100);
        }

        private async void LogException(Task brokenTask, string context)
        {
            string baseErrorMsg = "Failed to " + context + " model!";

            string errorMsg = baseErrorMsg;
            string exceptionMsg = "No message was given.";

            AggregateException aException = brokenTask.Exception;

            if (aException != null)
            {
                Exception exception = aException.InnerException;
                if (exception != null)
                {
                    exceptionMsg = exception.Message;
                    errorMsg += "\nError Message: " + exceptionMsg + "\n\n" +
                                "If this error message has happened multiple times, and doesn't seem deliberate, you should totally send a screenshot of this error message to @qfoxbRBLX on Twitter.\n\n" +
                                "STACK TRACE:\n" + outputDivider + "\n" + exception.StackTrace + "\n" + outputDivider;
                }
            }

            Print(baseErrorMsg);
            Print("Message:" + exceptionMsg);

            await UpdateCompilerState();
            showError(errorMsg);

            if (Debugger.IsAttached)
                Debugger.Break();

            compileProgress.Value = 0;
        }

        private async void compile_Click(object sender, EventArgs e)
        {
            Stopwatch trackCompileTime = new Stopwatch();
            trackCompileTime.Start();

            output.Text = "";
            outputQueue.Clear();
            compileProgress.Value = 0;

            foreach (Control control in CONTROLS_TO_DISABLE_WHEN_COMPILING)
                control.Enabled = false;

            Compiler.UseWaitCursor = true;
            ModelCompiler.PreScheduleTasks();

            Func<AssemblerData> assemble;

            if (compilerTypeSelect.Text == "Avatar")
            {
                var assembler = new CharacterAssembler();
                var userAvatar = UserAvatar.FromUsername(currentUser.Username);
                assemble = new Func<AssemblerData>(() => assembler.Assemble(userAvatar));
            }
            else
            {
                var assembler = new CatalogItemAssembler();
                assemble = new Func<AssemblerData>(() => assembler.Assemble(currentAssetId));
            }

            Task<AssemblerData> buildModel = Task.Run(assemble);

            while (!buildModel.IsCompleted)
                await UpdateCompilerState();

            if (buildModel.IsFaulted)
            {
                LogException(buildModel, "assemble");
            }
            else
            {
                AssemblerData data = buildModel.Result;
                Task<string> compileModel = ModelCompiler.Compile(selectedGame, data);

                while (!compileModel.IsCompleted)
                    await UpdateCompilerState();

                if (compileModel.IsFaulted)
                {
                    LogException(compileModel, "compile");
                }
                else
                {
                    PrintHeader("FINISHED MODEL!");
                    trackCompileTime.Stop();

                    Print("Assembled in {0} seconds.", trackCompileTime.Elapsed.TotalSeconds);

                    await UpdateCompilerState();
                    compileModel.Wait();

                    latestCompiledModel = compileModel.Result;
                    latestCompiledOnGame = selectedGame;
                }
            }

            foreach (Control control in CONTROLS_TO_DISABLE_WHEN_COMPILING)
                control.Enabled = true;

            if (trackCompileTime.IsRunning)
                trackCompileTime.Stop();

            Compiler.UseWaitCursor = false;
            progressQueue.Clear();
        }

        private void viewCompiledModel_Click(object sender, EventArgs e)
        {
            if (latestCompiledOnGame != null)
            {
                string hlmvPath = latestCompiledOnGame.HLMVPath;

                if (File.Exists(hlmvPath))
                {
                    ThirdPartyUtility hlmv = new ThirdPartyUtility(hlmvPath);
                    hlmv.AddParameter("game", latestCompiledOnGame.GameDirectory);
                    hlmv.AddParameter("model", latestCompiledModel);
                    hlmv.Run();
                }
                else
                {
                    showError("This Source Engine game doesn't have a model viewer for some reason :P");
                }
            }
            else
            {
                showError("No model has compiled successfully yet!");
                viewCompiledModel.Enabled = false;
            }
        }

        private static void loadComboBox(ComboBox comboBox, string settingsKey, int defaultValue = 0)
        {
            string value = Settings.GetString(settingsKey);

            if (value != null && comboBox.Items.Contains(value))
            {
                comboBox.Text = value;
            }
            else
            {
                comboBox.SelectedIndex = defaultValue;
            }
        }

        private void compilerTypeSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.SaveSetting("CompilerType", compilerTypeSelect.Text);
            updateDisplays();
        }

        private void gameSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            GameInfo game = sourceGames[gameSelect.Text];
            selectedGame = game;

            if (game.GameIcon != null)
                gameIcon.Image = game.GameIcon.ToBitmap();
            else
                gameIcon.Image = gameIcon.InitialImage;

            Settings.SaveSetting("SelectedGame", gameSelect.Text);
        }

        private void onLinkClicked(object sender, EventArgs e)
        {
            Control control = (Control)sender;

            if (control != null && Links.ContainsKey(control))
            {
                string link = Links[control];
                Process.Start(link);
            }
        }

        private void Rbx2Source_Load(object sender, EventArgs e)
        {
            string steamDir = null;
            Process[] steamProcesses = Process.GetProcessesByName("Steam");

            if (steamProcesses.Length > 0)
            {
                foreach (Process process in steamProcesses)
                {
                    try
                    {
                        ProcessModule module = process.MainModule;
                        string exePath = module.FileName;

                        FileInfo info = new FileInfo(exePath);
                        string directory = info.DirectoryName;

                        if (info.Name == "Steam.exe")
                        {
                            string apps = Path.Combine(directory, "steamapps");
                            if (Directory.Exists(apps))
                            {
                                steamDir = directory;
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Can't access this steam process.");
                    }
                }
            }

            if (steamDir == null)
            {
                try
                {
                    RegistryKey currentUser = Registry.CurrentUser;
                    RegistryKey steam = currentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
                    steamDir = steam.GetValue("SteamPath") as string;
                }
                catch
                {
                    string programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                    steamDir = Path.Combine(programFilesX86, "Steam");
                    if (!Directory.Exists(steamDir))
                    {
                        string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
                        steamDir = Path.Combine(programFiles, "Steam");
                        if (!Directory.Exists(steamDir))
                        {
                            showError("Cannot find Steam on this PC!\nTry running Steam so Rbx2Source can detect it.", true);
                        }
                    }
                }
            }

            string steamPath = steamDir.Replace('/', '\\');
            gatherSourceGames(steamPath);

            string steamApps = Path.Combine(steamPath, "steamapps");

            if (Directory.Exists(steamApps))
            {
                string libraryFolders = Path.Combine(steamApps, "libraryfolders.vdf");

                if (File.Exists(libraryFolders))
                {
                    string file = File.ReadAllText(libraryFolders);

                    string[] newlines = new string[] { "\r\n", "\n" };
                    string[] lines = file.Split(newlines, StringSplitOptions.None);

                    foreach (string line in lines)
                    {
                        string[] kvPair = getStringsInQuotes(line);

                        if (kvPair.Length == 2)
                        {
                            string key = kvPair[0];
                            string value = kvPair[1];

                            int index = -1;

                            if (int.TryParse(key, out index))
                            {
                                value = value.Replace("\\\\", "\\");
                                gatherSourceGames(value);
                            }
                        }
                    }
                }
            }

            string savedGameSelection = Settings.GetString("SelectedGame");

            int gameCount = 0;
            gameSelect.Items.Clear();

            foreach (string key in sourceGames.Keys)
            {
                gameSelect.Items.Add(key);
                gameCount++;
            }

            if (gameCount == 0)
                showError("No Source Engine games were found in current directory! Is your config.txt file setup correctly?", true);

            gameSelect.Enabled = true;
            compile.Enabled = true;

            loadComboBox(gameSelect, "SelectedGame");
            loadComboBox(compilerTypeSelect, "CompilerType", 1);

            string userName = Settings.GetString("Username");
            if (userName != null)
                TrySetUsername(userName);

            string assetId = Settings.GetString("AssetId");
            TrySetAssetId(assetId);

            selectedGame = sourceGames[gameSelect.Text];
            updateDisplays();

            CONTROLS_TO_DISABLE_WHEN_COMPILING = new List<Control>() { compile, compilerInputField, gameSelect, viewCompiledModel, compilerTypeSelect, quickCompile };

            Links = new Dictionary<Control, string>()
            {
                {cloneTwitter,  "https://www.twitter.com/MaximumADHD"},
                {qfoxb,         "https://www.github.com/qfoxb"},
                {AJLink,        "https://www.github.com/RedTopper"},
                {egoMooseLink,  "https://www.github.com/EgoMoose"},
                {rileyLink,     "https://www.github.com/rileywilliam08"},
                {nemsTools,     "https://web.archive.org/web/20200201044259/http://nemesis.thewavelength.net:80/index.php?p=40"}
            };

            foreach (Control link in Links.Keys)
                link.Click += new EventHandler(onLinkClicked);

            Task.Run(async () =>
            {
                while (!IsDisposed)
                {
                    if (assetPreview.ImageLocation != assetPreviewImage)
                    {
                        CdnPender check = WebUtility.DownloadJSON<CdnPender>(assetPreviewImage);

                        if (check.Final)
                        {
                            assetPreviewImage = check.Url;
                            assetPreview.ImageLocation = check.Url;
                        }
                        else
                        {
                            string currentPending = assetPreviewImage; // localize this in case it changes.
                            assetPreview.Image = loadingImage;

                            Task<string> pend = Task.Run(() => WebUtility.PendCdn(currentPending, false));

                            while (!pend.IsCompleted)
                            {
                                if (assetPreviewImage != currentPending)
                                    break;

                                if (pend.IsFaulted)
                                    break;

                                await Task.Delay(100);
                            }

                            if (assetPreviewImage == currentPending)
                            {
                                if (pend.IsFaulted) // mark the preview as broken.
                                {
                                    assetPreviewImage = "";
                                    assetPreview.ImageLocation = "";
                                    assetPreview.Image = brokenImage;
                                }
                                else
                                {
                                    string result = pend.Result;
                                    assetPreviewImage = result;
                                    assetPreview.ImageLocation = result;
                                }
                            }
                        }
                    }

                    if (Debugger.IsAttached && debugImg.Image != debugImage)
                        debugImg.Image = debugImage;

                    await Task.Delay(10);
                }
            });
        }

        private void Rbx2Source_FormClosed(object sender, FormClosedEventArgs e)
        {
            baseProcess?.Dispose();
            rpcClient.Dispose();
        }

        private void quickCompile_CheckedChanged(object sender, EventArgs e)
        {
            CharacterAssembler.DEBUG_RAPID_ASSEMBLY = quickCompile.Checked;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void qfoxb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
        }

        private void TwitterIcon_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }
    }
}