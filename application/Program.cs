using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace RobloxToSourceEngine
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            WebClient http = new WebClient();
            FileHandler FileHandler = new FileHandler();
            NameValueCollection settings = FileHandler.GetAppSettings();
            string currentVersion = settings["latestVersion"];
            string appData = Environment.GetEnvironmentVariable("AppData");
            string versionPath = Path.Combine(appData,"Rbx2SrcFiles","version");
            string localVersion = File.ReadAllText(versionPath);
            if (localVersion != currentVersion)
            {
                MessageBox.Show("You are running on an outdated version of the client.\nPlease run the launcher application and update.", "FATAL ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Rbx());
            }
        }
    }
}
