using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Rbx2Source.Resources;

namespace Rbx2Source
{
    public partial class Launcher : Form
    {
        private readonly WebClient http = new WebClient();

        public Launcher()
        {
            InitializeComponent();
        }

        public void setStatus(string status)
        {
            statusLbl.Text = status + "...";
            statusLbl.Refresh();
        }

        public async Task<byte[]> GetGitHubFile(string localPath)
        {
            if (Environment.CurrentDirectory.Contains(@"Rbx2Source\bin"))
            {
                string path = Path.Combine(Environment.CurrentDirectory, @"..\..", localPath);
                return File.ReadAllBytes(path);
            }
            else
            {
                string gitPath = "https://raw.githubusercontent.com/CloneTrooper1019/Rbx2Source/master/" + localPath;
                return await http.DownloadDataTaskAsync(gitPath);
            }
        }

        public async Task<string> GetGitHubString(string localPath)
        {
            byte[] contents = await GetGitHubFile(localPath);
            return Encoding.UTF8.GetString(contents);
        }

        private async void Launcher_Load(object sender, EventArgs e)
        {
            string myPath = Application.ExecutablePath;
            FileInfo myInfo = new FileInfo(myPath);

            string myName = myInfo.Name;
            string dir = myInfo.DirectoryName;

            if (myName.StartsWith("NEW_", StringComparison.InvariantCulture))
            {
                string newPath = Path.Combine(dir,myName.Substring(4));
                File.Copy(myInfo.FullName, newPath, true);

                Process.Start(newPath);
                Application.Exit();
            }
            else
            {
                foreach (string filePath in Directory.GetFiles(dir))
                {
                    FileInfo info = new FileInfo(filePath);
                    string fileName = info.Name;

                    if (fileName.StartsWith("NEW_", StringComparison.InvariantCulture) && info.Extension.ToUpperInvariant() == ".EXE")
                    {
                        File.Delete(info.FullName);
                        break;
                    }
                }
            }

            setStatus("Checking for updates");

            string latestVersion = await GetGitHubString("version.txt");
            string myVersion = Settings.GetString("CurrentVersion");

            if (latestVersion != myVersion)
            {
                setStatus("Updating Rbx2Source");

                byte[] newVersion = await GetGitHubFile("Rbx2Source.exe");
                string updatePath = Path.Combine(dir, "NEW_" + myName);

                File.WriteAllBytes(updatePath, newVersion);
                Settings.SaveSetting("CurrentVersion", latestVersion);

                Process.Start(updatePath);
                Application.Exit();
            }
            
            setStatus("Starting Rbx2Source");
            await Task.Delay(500);

            Rbx2Source rbx2Source = null;

            Task startRbx2Source = Task.Run(() =>
            {
                rbx2Source = new Rbx2Source();
                rbx2Source.baseProcess = this;
            });

            await startRbx2Source;

            rbx2Source.Show();
            Hide();
        }
    }
}
