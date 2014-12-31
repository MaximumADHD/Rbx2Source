using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Rbx2SourceLauncher
{
    public partial class Launcher : Form
    {
        string status = "Loading";
        string dotEffect = ""; // Dots are added to this as the timer ticks.
        string localVersion = "1.20";
        bool debugMode = false;



        Timer update = new Timer();
        WebClient http = new WebClient();
        FileHandler FileHandler = new FileHandler();

        public Launcher()
        {
            InitializeComponent();
            update.Interval = 150;
            update.Start();
            update.Tick += new System.EventHandler(timerUpdate);
        }

        public async void log(string line)
        {
            await Task.Delay(30);
            this.logDisplay.Text = line;
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
        public void terminate(string error)
        {
            this.Hide();
            MessageBox.Show(error, "Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
        public void timerUpdate(object sender, EventArgs e)
        {
            if (dotEffect.Length == 5)
            {
                dotEffect = "";
            }
            else
            {
                dotEffect = dotEffect + ".";
            }
            this.loadingStatus.Text = status + dotEffect;
        }

        public void stopRunningCopies()
        {
            Process[] running = Process.GetProcessesByName("RobloxToSourceEngine");
            foreach (Process p in running)
            {
                try
                {
                    Console.WriteLine("Killing");
                    p.Kill();
                }
                catch
                {
                    // Sometimes it won't let us :/
                }
            }
        }

        public void readCommand(string line, out string command, out string path)
        {
            string cmd = "";
            string pth = "";
            bool gotCommand = false;
            while (line.Length > 0)
            {
                string chunk = line.Substring(0, 1);
                line = line.Substring(1);
                if (chunk.Equals("\""))
                {
                    gotCommand = true;
                    cmd = cmd.Replace(" ", "");
                }
                else
                {
                    if (gotCommand)
                    {
                        pth = pth + chunk;
                    }
                    else
                    {
                        cmd = cmd + chunk;
                    }
                } 
            }
            command = cmd;
            path = pth;
        }

        public List<string> readLines(string contents)
        {
            List<string> lines = new List<string>();
            string line = "";
            while (contents.Length > 0)
            {
                string chunk = contents.Substring(0,1);
                if (chunk.Equals("\n"))
                {
                    lines.Add(line);
                    line = "";
                }
                else
                {
                    line = line + chunk;
                }
                contents = contents.Substring(1);
            }
            return lines;
        }

        public string getResourceStr(string path)
        {
            byte[] data = FileHandler.GetResource(path);
            return Encoding.UTF8.GetString(data);
        }

        public string getVersion()
        {
            // Brutally parse the json key/value I'm looking for.
            // This is the only time I work with JSON in the launcher.
            string settings = getResourceStr("settings.json");
            string version = "";
            while (version == "")
            {
                settings = settings.Substring(1);
                if (settings.StartsWith("latestVersion"))
                {
                    settings = settings.Substring(13);
                    bool reading = true;
                    bool firstQuote = false;
                    while (reading)
                    {
                        settings = settings.Substring(1);
                        if (settings.StartsWith("\""))
                        {
                            if (firstQuote)
                            {
                                reading = false;
                            }
                            else
                            {
                                firstQuote = true;
                            }
                        }
                        else if (firstQuote)
                        {
                            version = version + settings.Substring(0, 1);
                        }
                    }
                }
            }
            Console.WriteLine(version);
            return version;
        }

        public void writeToPossiblyActiveFile(string export, string resourcePath)
        {
            // Crafty users may try to run the exe while we're writing over DLL files.
            // If it runs into an IOException, it will terminate the exe and try again until it succeeds.
            try
            {
                byte[] file = FileHandler.GetResource(resourcePath);
                FileHandler.WriteToFileFromBuffer(export, file);
            }
            catch (IOException)
            {
                stopRunningCopies();
                writeToPossiblyActiveFile(export, resourcePath);
            }
            catch (WebException)
            {
                terminate("Could not find resource '" + resourcePath + "'\nPlease tweet @CloneTroper1019 about this and it will be fixed asap.");
            }
        }


        private async void Launcher_Load(object sender, EventArgs e)
        {
            await Task.Delay(100);
            bool newVersion = false;
            status = "Checking connection";
            try
            {
                http.DownloadString("http://www.google.com");
            }
            catch
            {
                terminate("Could not start.\nRbx2Source requires an internet connection to run :(");
            }
            string appData = Environment.GetEnvironmentVariable("AppData");
            string root = Path.Combine(appData, "Rbx2SrcFiles");
            string localVersion = null;
            string versionPath = Path.Combine(root, "version");
            if (!File.Exists(versionPath))
            {
                newVersion = true;
                status = "Installing Rbx2Source";
                localVersion = "null";
            }
            else
            {
                localVersion = File.ReadAllText(versionPath);
            }
            string version = getVersion();
            Console.WriteLine(localVersion);
            if (!newVersion)
            {
                if (version != localVersion && !debugMode)
                {
                    newVersion = true;
                    status = "Shutting down Rbx2Source";
                    stopRunningCopies();
                    status = "Installing Version " + version;
                }
                else
                {
                    status = "Checking files";
                }
            }
            string installData = getResourceStr("install.dat");
            string localPath = "";
            string gitPath = "";
            List<string> lines = readLines(installData);
            this.loadingBar.Style = ProgressBarStyle.Continuous;
            Console.WriteLine(lines.Count);
            foreach (string line in readLines(installData))
            {
                Console.WriteLine(line);
                if (line.Length > 0)
                {
                    if (!line.StartsWith("--"))
                    {
                        string command, path;
                        readCommand(line, out command, out path);
                        if (command == "setLocalPath")
                        {
                            localPath = path;
                        }
                        else if (command == "setGitPath")
                        {
                            gitPath = path;
                        }
                        else if (command == "import")
                        {
                            string exportPath = GetDirectory(appData, localPath);
                            string export = Path.Combine(exportPath, path);
                            if (!File.Exists(export) || newVersion)
                            {
                                log("Importing resource " + path);
                                string resourcePath = gitPath + "/" + path;
                                writeToPossiblyActiveFile(export, resourcePath);
                                await Task.Delay(300); // Prevent spam on github servers.
                            }
                            else
                            {
                                log("Checked resource " + path);
                                await Task.Delay(100);
                            }
                            int nextValue = (int)this.loadingBar.Value + (300 / lines.Count);
                            if (nextValue > 100)
                            {
                                nextValue = 100;
                            }
                            this.loadingBar.Value = nextValue;
                        }
                    }
                }
            }
            FileHandler.WriteToFileFromString(versionPath, version);
            // LET THEM GAZE UPON THIS BEAUTIFUL LOADING SCREEN
            // Only doing this so its not such a surprising start after loading.
            status = "Starting Rbx2Source";
            this.loadingBar.Style = ProgressBarStyle.Marquee;
            this.logDisplay.Text = "";
            await Task.Delay(2000); 
            string exePath = Path.Combine(root, "application", "RobloxToSourceEngine.exe");
            Process rbx2Source = Process.Start(exePath);
            this.Visible = false;
            //this.WindowState = FormWindowState.Minimized;
            rbx2Source.WaitForExit();
            this.Close();
        }
    }
}
