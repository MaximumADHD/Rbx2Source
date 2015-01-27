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
using Newtonsoft;
using Newtonsoft.Json;
using LuaInterface;

namespace RobloxToSourceEngine
{
    class FileHandler
    {
        bool useLocalPath = false;
        string localPath = "";
        WebClient http = new WebClient();
        public NameValueCollection JsonToNVC(string json)
        {
            ListDictionary parse = JsonConvert.DeserializeObject<ListDictionary>(json);
            NameValueCollection NVC = new NameValueCollection();
            foreach (DictionaryEntry pair in parse)
            {
                if (pair.Value != null)
                {
                    NVC.Add(pair.Key.ToString(), pair.Value.ToString());
                }
            }
            return NVC;
        }

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
            // Write to a file if the file doesn't exist yet,
            // or if the current contents of the file aren't the same as the provided ones.
            bool proceedToWrite = false;
            if (File.Exists(path))
            {
                string currentContents = File.ReadAllText(path);
                if (!contents.Equals(currentContents))
                {
                    proceedToWrite = true;
                }
            }
            else
            {
                proceedToWrite = true;
            }
            if (proceedToWrite)
            {
                FileStream file = File.Create(path); // Creates or overwrites it. Doesn't really matter in this case.
                byte[] contentStream = Encoding.Default.GetBytes(contents);
                file.Write(contentStream, 0, contentStream.Length);
                file.Close();
            }
        }

        public string GetFileFromHash(string hash, string extension = "", string customName = null, string customPath = null)
        {
            string json = http.DownloadString("http://www.roblox.com/thumbnail/resolve-hash/" + hash);
            NameValueCollection response = JsonToNVC(json);
            string url = response["Url"];
            string filePath;
            if (customPath != null)
            {
                string roaming = Environment.GetEnvironmentVariable("AppData");
                filePath = Path.Combine(roaming, "Rbx2SrcFiles");
            }
            else
            {
                filePath = customPath;
            }
            string name;
            if (customName != null)
            {
                name = customName;
            }
            else
            {
                name = hash;
            }
            filePath = Path.ChangeExtension(Path.Combine(filePath, name), extension);
            FileStream file = File.Create(filePath);
            WriteToFileFromUrl(file, url);
            return filePath;
        }

        public NameValueCollection GetAppSettings()
        {
            string json = null;
            if (useLocalPath)
            {
                string path = Path.Combine(localPath, "settings.json");
                json = File.ReadAllText(path);
            }
            else
            {
                string path = "https://raw.githubusercontent.com/" + Properties.Settings.Default.GitPath + "/settings.json";
                json = http.DownloadString(path);
                
            }
            return JsonToNVC(json);
        }

        public byte[] GetResource_ByteArray(string path)
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
                string dir = "https://raw.githubusercontent.com/" + Properties.Settings.Default.GitPath + "/" + path;
                contents = http.DownloadData(dir);
            }
            return contents;
        }

        public string GetResource(string path)
        {
            string contents = null;
            if (useLocalPath)
            {
                path = path.Replace("/", "\\");
                string dir = Path.Combine(localPath, path);
                if (File.Exists(dir))
                {
                    contents = File.ReadAllText(dir);
                }
                else
                {
                    throw new Exception("Cannot find '" + dir + "'");
                }
            }
            else
            {
                string dir = "https://raw.githubusercontent.com/" + Properties.Settings.Default.GitPath + "/" + path;
                contents = http.DownloadString(dir);
            }
            return contents;
        }
        public FileHandler()
        {
            // Check and see if we're running inside of a cloned github repository.
            // If we are, use the resource files stored here rather than the ones on the cloud.
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
            if (count == 3)
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
