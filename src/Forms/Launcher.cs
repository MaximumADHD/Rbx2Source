using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Rbx2Source.Properties;
using Rbx2Source.Resources;

namespace Rbx2Source
{
    public partial class Launcher : Form
    {
        private WebClient http = new WebClient();

        public Launcher()
        {
            InitializeComponent();
        }

        public async Task setStatus(string status)
        {
            statusLbl.Text = status + "...";
            await Task.Delay(1);
        }

        public async Task<byte[]> GetGitHubFile(string localPath)
        {
            if (Environment.CurrentDirectory.Contains(@"Rbx2Source\src\bin"))
            {
                string path = Path.Combine(Environment.CurrentDirectory, @"..\..\..", localPath);
                return File.ReadAllBytes(path);
            }
            else
            {
                string gitPath = "https://raw.githubusercontent.com/CloneTrooper1019/Rbx2Source/master/" + localPath;
                byte[] response = await http.DownloadDataTaskAsync(gitPath);
                return response;
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
            try
            {
                // Check if the current process is attached to the legacy launcher, so we can phase it out.
                Process self = Process.GetCurrentProcess();
                ManagementObjectSearcher search = new ManagementObjectSearcher(@"root\CIMV2", "SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = " + self.Id);
                ManagementObjectCollection results = search.Get();
                var scanResults = results.GetEnumerator(); // using var because the type name is ridiculous.
                if (scanResults.MoveNext())
                {
                    ManagementBaseObject query = scanResults.Current;
                    uint parentId = (uint)query.GetPropertyValue("ParentProcessId");
                    Process parent = Process.GetProcessById((int)parentId);
                    
                    string parentPath = parent.MainModule.FileName;
                    FileInfo info = new FileInfo(parentPath);
                    if (info.Name == "Rbx2Source.exe" || info.Name == "Rbx2SourceLauncher.exe")
                    {
                        await setStatus("Phasing out the legacy launcher");
                        parent.Kill();
                        parent.WaitForExit();
                        File.Copy(myPath, parentPath, true);
                        Process.Start(parentPath);
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            string myName = myInfo.Name;
            string dir = myInfo.DirectoryName;

            if (myName.StartsWith("NEW_"))
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
                    if (fileName.StartsWith("NEW_") && info.Extension.ToLower() == "exe")
                    {
                        File.Delete(info.FullName);
                        break;
                    }
                }
            }

            await setStatus("Checking for updates");
            string latestVersion = await GetGitHubString("version.txt");
            string myVersion = Settings.GetSetting<string>("CurrentVersion");
            if (latestVersion != myVersion)
            {
                await setStatus("Updating Rbx2Source");
                Settings.SetSetting("CurrentVersion", latestVersion);
                Settings.Save();
                byte[] newVersion = await GetGitHubFile("Rbx2Source.exe");
                string updatePath = Path.Combine(dir,"NEW_" + myName);
                File.WriteAllBytes(updatePath, newVersion);
                Process.Start(updatePath);
                Application.Exit();
            }

            await setStatus("Checking for required DLL files");
            bool addedLibraries = false;

            foreach (string library in ResourceUtility.GetFiles("Libraries"))
            {
                string libName = library.Replace("Libraries/", "");
                string libPath = Path.Combine(dir, libName);
                if (!File.Exists(libPath))
                {
                    byte[] content = ResourceUtility.GetResource(library);
                    addedLibraries = true;
                    File.WriteAllBytes(libPath, content);
                }
            }

            if (addedLibraries)
                Application.Restart();

            await setStatus("Starting Rbx2Source");
            Rbx2Source rbx2Source = new Rbx2Source();
            rbx2Source.baseProcess = this;
            await Task.Delay(1000);
            rbx2Source.Show();
            await Task.Delay(50);
            Hide();
        }
    }
}
