using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using Microsoft.Win32;

namespace RobloxToSourceEngine
{
    static class Program
    {
        static void fatalError(string msg)
        {
            MessageBox.Show(msg, "FATAL ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [STAThread]

        static void Main()
        {
            // First, we need to prevent this from being pinned to the taskbar.
            // The application won't automatically update if its not 
            // I accomplish this by adding a "NoStartPage" key under my application in Root\Applications
            RegistryKey root = Registry.ClassesRoot;
            RegistryKey applicationsSubKey = root.OpenSubKey("Applications", true);
            if (applicationsSubKey != null)
            {
                bool updateNoStartPageKey = false;
                var appNameSubKey = applicationsSubKey.OpenSubKey("RobloxToSourceEngine.exe", true);
                if (appNameSubKey != null)
                {
                    if (!appNameSubKey.GetValueNames().Contains("NoStartPage"))
                    {
                        updateNoStartPageKey = true;
                    }
                }
                else
                {
                    appNameSubKey = applicationsSubKey.CreateSubKey("RobloxToSourceEngine.exe", RegistryKeyPermissionCheck.Default);
                    if (appNameSubKey != null)
                    {
                        updateNoStartPageKey = true;
                    }
                }

                if (updateNoStartPageKey)
                {
                    appNameSubKey.SetValue("NoStartPage", string.Empty, RegistryValueKind.String);
                }
            }
            // Then make sure everything is up to date and the API is available.
            WebClient http = new WebClient();
            FileHandler FileHandler = new FileHandler();
            NameValueCollection settings = FileHandler.GetAppSettings();
            string currentVersion = settings["latestVersion"];
            string appData = Environment.GetEnvironmentVariable("AppData");
            string versionPath = Path.Combine(appData,"Rbx2SrcFiles","version");
            string localVersion = File.ReadAllText(versionPath);
            if (localVersion != currentVersion)
            {
                fatalError("You are running on an outdated version of the client.\nPlease run the launcher application and update.");
            }
            else
            {
                try
                {
                    string roblox = http.DownloadString("http://www.roblox.com");
                    if (roblox.Contains("ROBLOX is down for maintenance"))
                    {
                        // If the site is down, the API is down. No good reason for the application to run right now.
                        fatalError("Roblox is down for maintenance!\nPlease try again later.");
                    }
                    else
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new Rbx());
                    }
                }
                catch (WebException)
                {
                    fatalError("Something went wrong while trying to connect to roblox!\nPlease try again later.");
                }
            }
        }
    }
}
