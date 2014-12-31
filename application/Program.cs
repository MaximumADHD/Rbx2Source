using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;

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
            try
            {
                // Best way to test for an internet connection is to see if you can get a response from Google.
                string response = http.DownloadString("http://www.google.com");
            }
            catch
            {
                MessageBox.Show("Unable to connect to the internet.\nPlease check your connection and try again.", "FATAL ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            string currentVersion = settings["latestVersion"];
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Rbx());
        }
    }
}
