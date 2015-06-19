using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace RobloxToSourceEngine
{
    public class MessageOutEventArgs : EventArgs
    {
        private string message;

        public MessageOutEventArgs(string msg)
        {
            this.message = msg;
        }

        public string Message
        {
            get { return this.message; }
        }
    }

    public class VTFProcessor
    {
        public string texHash;
        public string mtlName;
        public string mtlDir;
        public event EventHandler<MessageOutEventArgs> MessageOut;
        public event EventHandler ProcessingFinished;
        private FileHandler FileHandler = new FileHandler();
        private bool begin = false;
        private Timer timer;
       
        public void BeginProcessing()
        {
            begin = true;
        }

        private void log(string text)
        {
            if (MessageOut != null)
            {
                MessageOut(this, new MessageOutEventArgs(text));
            }
        }

        private string GetDirectory(params string[] dir)
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

        private string GetFile(string dir, string creationPath, string name = "")
        {
            // Creates a file if it doesn't exist already.
            string filePath = Path.Combine(dir, name);
            if (!File.Exists(filePath))
            {
                byte[] fileData = FileHandler.GetResource_ByteArray(creationPath);
                FileHandler.WriteToFileFromBuffer(filePath, fileData);
            }
            return filePath;
        }

        private int toNearestPowerOfTwo(int value)
        {
            int power = 2;
            while (power <= value)
            {
                power = power + power;
            }
            return power;
        }

        private string inQuotes(string str)
        {
            return "\"" + str + "\"";
        }

        private void process()
        {
            string appData = Environment.GetEnvironmentVariable("AppData");
            string rootPath = GetDirectory(appData, "Rbx2SrcFiles", "images");
            string toolsPath = GetDirectory(rootPath, "converterUtil");
            string readme = GetFile(toolsPath, "tools/READ ME PLEASE.txt", "READ ME PLEASE.txt");
            string DevIL = GetFile(toolsPath, "tools/DevIL.dll", "DevIL.dll");
            string VTFLib = GetFile(toolsPath, "tools/VTFLib.dll", "VTFLib.dll");
            string VTFCmd_Path = GetFile(toolsPath, "tools/vtfcmd.exe", "VTFcmd.exe");
            log("Getting PNG File: " + texHash);
            string png = FileHandler.GetFileFromHash(texHash, "png", mtlName, rootPath);
            Image pngCanvas = Image.FromFile(png);
            int width = toNearestPowerOfTwo(pngCanvas.Width);
            int height = toNearestPowerOfTwo(pngCanvas.Height);
            string parameters = " -file " + inQuotes(png) + " -output " + inQuotes(mtlDir) + " -resize -rwidth " + width + " -rheight " + height;
            ProcessStartInfo VTFCmd = new ProcessStartInfo();
            VTFCmd.FileName = VTFCmd_Path;
            VTFCmd.Arguments = parameters;
            VTFCmd.CreateNoWindow = true;
            VTFCmd.UseShellExecute = false;
            VTFCmd.RedirectStandardOutput = true;
            Process VTFCmd_Run = Process.Start(VTFCmd);
            StreamReader output = VTFCmd_Run.StandardOutput;
            VTFCmd_Run.WaitForExit();
            while (true)
            {
                string line = output.ReadLine();
                if (line != null)
                {
                    log(line);
                }
                else
                {
                    break;
                }
            }
            ProcessingFinished(this, EventArgs.Empty);
        }

        public void checkToBegin(Object sender, EventArgs e)
        {
            if (begin)
            {
                timer.Stop();
                process();
            }
        }

        public VTFProcessor()
        {
            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += new EventHandler(checkToBegin);
            timer.Start();
        }
    }
}
