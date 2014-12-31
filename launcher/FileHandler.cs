using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Windows.Forms;

namespace Rbx2SourceLauncher
{
    class FileHandler
    {
        bool useLocalPath = false;
        string localPath = "";
        string gitPath = "https://raw.githubusercontent.com/CloneTrooper1019/Rbx2Source/master/resources/";
        WebClient http = new WebClient();

        public void WriteToFileFromUrl(FileStream file, string url)
        {
            byte[] fileBuffer = http.DownloadData(url);
            file.Write(fileBuffer, 0, fileBuffer.Length);
            file.Close();
        }

        public void WriteToFileFromBuffer(string path, byte[] fileBuffer)
        {
            FileStream file = File.Create(path);
            file.Write(fileBuffer, 0, fileBuffer.Length);
            file.Close();
        }

        public void WriteToFileFromString(string path, string contents)
        {
            FileStream file = File.Create(path);
            byte[] contentStream = Encoding.Default.GetBytes(contents);
            file.Write(contentStream, 0, contentStream.Length);
            file.Close();
        }

        public byte[] GetResource(string path)
        {
            byte[] contents = null;
            if (useLocalPath)
            {
                path = path.Replace("/", "\\");
                string dir = Path.Combine(localPath, path);
                if (File.Exists(dir))
                {
                    contents = File.ReadAllBytes(dir);
                }
                else
                {
                    throw new Exception("Cannot find '" + dir + "'");
                }
            }
            else
            {
                string dir = gitPath + path;
                contents = http.DownloadData(dir);
            }
            return contents;
        }

        public FileHandler()
        {
            // See if we should use a repository clone saved to the desktop rather than github itself.
            string[] search = new string[] { "bin", "application", "Rbx2Source" };
            string currentPath = Environment.CurrentDirectory;
            int count = 0;
            foreach (string expectedParent in search)
            {
                string parent = Directory.GetParent(currentPath).Name;
                if (expectedParent == parent)
                {
                    currentPath = Directory.GetParent(currentPath).ToString();
                    count = count + 1;
                }
            }
            if (count == search.Length)
            {
                currentPath = Path.Combine(currentPath, "resources");
                if (Directory.Exists(currentPath))
                {
                    useLocalPath = true;
                    localPath = currentPath;
                }
            }
        }
    }
}