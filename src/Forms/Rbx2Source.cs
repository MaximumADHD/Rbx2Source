using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

using Rbx2Source.Assembler;
using Rbx2Source.Compiler;
using Rbx2Source.Geometry;
using Rbx2Source.Resources;
using Rbx2Source.Web;

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

        public static IFormatProvider NormalParse = CultureInfo.InvariantCulture;
        public Launcher baseProcess;

        private UserInfo currentUser;
        private long currentAssetId = 19027209;

        private Dictionary<string, GameInfo> sourceGames = new Dictionary<string, GameInfo>();
        private GameInfo selectedGame;
        private List<Control> CONTROLS_TO_DISABLE_WHEN_COMPILING;
        private string latestCompiledModel;
        private GameInfo latestCompiledOnGame;
        private Dictionary<Control, string> Links;
        private static int stackLevel = 0;

        private static List<OutputLog> outputQueue = new List<OutputLog>();
        private static Dictionary<string, bool> progressQueue = new Dictionary<string, bool>();
        private static bool updateProgressQueue = false;
        private static string dumbHeaderLineThing = "---------------------------------------------------------------------------";
        private string assetPreviewImage = "";
        private static Image loadingImage = Properties.Resources.Loading;
        private static Image brokenImage = Properties.Resources.BrokenPreview;

        public Rbx2Source()
        {
            InitializeComponent();
        }

        public static void ScheduleTasks(params string[] tasks)
        {
            foreach (string task in tasks)
                if (!progressQueue.ContainsKey(task))
                    progressQueue[task] = false;
        }

        public static void MarkTaskCompleted(string completedTask)
        {
            if (progressQueue.ContainsKey(completedTask))
            {
                progressQueue[completedTask] = true;
                updateProgressQueue = true;
            }
        }

        private void WriteLine(string line)
        {
            int selectedIndex = output.Text.Length;
            if (output.Text == "")
                output.AppendText(line);
            else
                output.AppendText("\r\n" + line);
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
                msg = "\t" + msg;

            OutputLog log = new OutputLog(msg);
            PrintInternal(log);
        }

        public static void PrintHeader(string msg)
        {
            PrintInternal(new OutputLog("[" + msg.ToUpper() + "]", FontStyle.Bold));
        }

        public static void Print(string msgFormat, params object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                string match = "{" + i + "}";
                msgFormat = msgFormat.Replace(match, values[i].ToString());
            }
            Print(msgFormat);
        }

        private void showError(string msg, bool fatal = false)
        {
            MessageBox.Show(msg, (fatal ? "FATAL " : "") + "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (fatal) Application.Exit();
        }

        private string[] getStringsInQuotes(string str)
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
                string captured = str.Substring(pos0+1, pos1-pos0-1);
                captures.Add(captured);
            }

            return captures.ToArray();
        }

        private void gatherSourceGames(string steamDir)
        {
            string steamPath = Path.Combine(steamDir, "steamapps", "common");
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
                                    bin = altRoute2;
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
                                        path = Path.Combine(userMod, "gameinfo.txt");
                                }
                                foreach (string gameDir in Directory.GetDirectories(game))
                                {
                                    if (!gameDir.Equals(bin))
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
                compilerInputField.Text = currentAssetId.ToString();
                compilerTypeIcon.Image = Properties.Resources.Accoutrement_icon;
            }
        }

        private bool TrySetUsername(string userName)
        {
            UserAvatar avatar = UserAvatar.FromUsername(userName);
            if (avatar.UserExists)
            {
                Settings.SetSetting("Username", userName, true);
                assetPreview.Image = loadingImage;
                currentUser = avatar.UserInfo;
                return true;
            }
            else
            {
                showError("An error occurred while trying to fetch this user!\nEither the user does not exist, or something went wrong with the request.");
                return false;
            }
        }

        private bool TrySetAssetId(object value)
        {
            string text = value.ToString();
            long assetId = -1;
            long.TryParse(text, out assetId);
            if (assetId > 0)
            {
                Asset asset = null;
                try
                {
                    asset = Asset.Get(assetId);
                }
                catch
                {
                    showError("This AssetId isn't configured correctly on Roblox's end.\n\nThis error usually happens if you input a very old AssetId that doesn't exist on their servers.\n\nTry something else!");
                }
                if (asset != null)
                {
                    AssetType assetType = asset.AssetType;
                    bool isAccessory = AssetGroups.IsTypeInGroup(assetType, AssetGroup.Accessories);
                    if (isAccessory || assetType == AssetType.Gear)
                    {
                        assetPreview.Image = loadingImage;
                        Settings.SetSetting("AssetId64", assetId, true);
                        currentAssetId = assetId;
                        return true;
                    }
                    else
                    {
                        showError("AssetType received: " + Enum.GetName(typeof(AssetType), assetType) + "\n\nExpected one of the following asset types:\n* Accessory\n* Gear\n\nTry again!");
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
                await Task.Delay(1);

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
                    if (completed) tasksDone++;
                }

                double progress = ((double)tasksDone / (double)totalTasks) * 100.0;
                compileProgress.Value = (int)progress;
                updateProgressQueue = false;
            }

            await Task.Delay(1);
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
                    errorMsg += "\nError Message: " + exceptionMsg + "\n\nIf this error message has happened multiple times, and doesn't seem deliberate, you should totally send a screenshot of this error message to @MaxGeee1019 on Twitter.\n\nSTACK TRACE:\n" + dumbHeaderLineThing + "\n" + exception.StackTrace + "\n" + dumbHeaderLineThing;
                }
            }

            Print(baseErrorMsg);
            Print("Message:" + exceptionMsg);

            await UpdateCompilerState();
            showError(errorMsg);
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

            IAssembler assembler;
            object metadata;

            if (compilerTypeSelect.Text == "Avatar")
            {
                assembler = new CharacterAssembler();
                metadata = UserAvatar.FromUsername(currentUser.Username);
            }
            else
            {
                assembler = new CatalogItemAssembler();
                metadata = currentAssetId;
            }

            Task<AssemblerData> buildModel = Task.Run(() => assembler.Assemble(metadata));

            while (!buildModel.IsCompleted)
                await UpdateCompilerState();

            if (buildModel.IsFaulted)
                LogException(buildModel,"assemble");
            else
            {
                AssemblerData data = buildModel.Result;
                Task<string> compileModel = ModelCompiler.Compile(selectedGame, data);

                while (!compileModel.IsCompleted)
                    await UpdateCompilerState();

                if (compileModel.IsFaulted)
                    LogException(compileModel, "compile");
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

            Compiler.UseWaitCursor = false;
            progressQueue.Clear();

            if (trackCompileTime.IsRunning)
                trackCompileTime.Stop();
        }

        private void viewCompiledModel_Click(object sender, EventArgs e)
        {
            if (latestCompiledOnGame != null)
            {
                string hlmvPath = latestCompiledOnGame.HLMVPath;
                if (File.Exists(hlmvPath))
                {
                    ThirdPartyUtility hlmv = new ThirdPartyUtility(hlmvPath);
                    Console.WriteLine(latestCompiledModel);
                    hlmv.AddParameter("-game", latestCompiledOnGame.GameDirectory);
                    hlmv.AddParameter("-model", latestCompiledModel);
                    hlmv.RunSimple();
                }
                else showError("This Source Engine game doesn't have a model viewer for some reason :P");
            }
            else
            {
                showError("No model has compiled successfully yet!");
                viewCompiledModel.Enabled = false;
            }
        }
        
        private void loadComboBox(ComboBox comboBox, string settingsKey, int defaultValue = 0)
        {
            string value = Settings.GetSetting<string>(settingsKey);
            if (value != null && comboBox.Items.Contains(value))
                comboBox.Text = value;
            else
                comboBox.SelectedIndex = defaultValue;
        }

        private void compilerTypeSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.SetSetting("CompilerType", compilerTypeSelect.Text, true);
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

            Settings.SetSetting("SelectedGame", gameSelect.Text, true);
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
                        if (info.Name == "Steam.exe" && info.Directory.Name == "Steam")
                        {
                            string apps = Path.Combine(directory, "steamapps");
                            if (Directory.Exists(apps))
                                steamDir = directory;
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
                    RegistryKey classesRoot = Registry.ClassesRoot;
                    RegistryKey steam = classesRoot.OpenSubKey(@"SOFTWARE\Valve\Steam");
                    steamDir = (string)steam.GetValue("SteamPath");
                    if (steamDir == null)
                        throw new Exception();
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
                    string[] newlines = new string[] {"\r\n","\n"};
                    string[] lines = file.Split(newlines,StringSplitOptions.None);
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

            string savedGameSelection = Settings.GetSetting<string>("SelectedGame");
            int gameCount = 0;
            gameSelect.Items.Clear();

            foreach (string key in sourceGames.Keys)
            {
                gameSelect.Items.Add(key);
                gameCount++;
            }

            if (gameCount == 0)
                showError("No Source Engine games were found on this PC!", true);

            gameSelect.Enabled = true;
            compile.Enabled = true;

            loadComboBox(gameSelect, "SelectedGame");
            loadComboBox(compilerTypeSelect, "CompilerType", 1);

            string userName = Settings.GetSetting<string>("Username");
            if (userName != null)
                TrySetUsername(userName);

            string assetId = Settings.GetSetting<string>("AssetId64");
            TrySetAssetId(assetId);

            selectedGame = sourceGames[gameSelect.Text];
            updateDisplays();

            CONTROLS_TO_DISABLE_WHEN_COMPILING = new List<Control>() { compile, compilerInputField, gameSelect, viewCompiledModel, compilerTypeSelect };
            
            Links = new Dictionary<Control, string>() 
            {
                {twitterLink,   "https://www.twitter.com/MaxGeee1019"},
                {AJLink,        "https://www.github.com/RedInquisitive"},
                {egoMooseLink,  "https://www.github.com/EgoMoose"},
                {nemsTools,     "http://nemesis.thewavelength.net/index.php?p=40"}
            };

            foreach (Control link in Links.Keys)
                link.Click += new EventHandler(onLinkClicked);

            Task.Run(async() =>
            {
                while (true)
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
                            Task<string> pend = Task.Run(() => WebUtility.PendCdnUrl(currentPending,false));
                            while (!pend.IsCompleted)
                            {
                                if (assetPreviewImage != currentPending) break;
                                if (pend.IsFaulted) break;
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
                    await Task.Delay(10);
                }
            });
        }

        private void Rbx2Source_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (baseProcess != null && !baseProcess.IsDisposed)
                baseProcess.Dispose();
        }
    }
}