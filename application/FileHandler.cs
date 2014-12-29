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

        public string ConvertFile(string filePath, string newExtension)
        {
            Console.WriteLine("Converting " + Path.GetFileName(filePath) + " to a ." + newExtension);
            string apiKey = "8264be4c908b83fd81208fff47117e93"; // This is a personal API key. Don't share pls lol.
            string request = "convert-to-" + newExtension;
            string fileName = Path.GetFileName(filePath);
            OnlineConvert oc = OnlineConvert.create(apiKey, true, request);

            // Some of this was borrowed from Online-Convert's Github Sample.
            // https://github.com/onlineconvert/onlineconvert-api-example-codes/blob/master/DotNet/Example/Example/Example.cs

            Console.WriteLine("Requesting file...");
            string xml = oc.convert(request, "FILE_PATH", filePath, fileName);
            Console.WriteLine("Processing file...");
            var dica = oc.getXml2Dic(xml);
            var directDownloadURL = "";
            Dictionary<int, Dictionary<string, string>> dicb = new Dictionary<int, Dictionary<string, string>>();

            if (dica[1].ContainsKey("hash") && dica[1]["hash"] != "")
            {
                while (true)
                {
                    var b = oc.getProgress(dica[1]["hash"]);
                    dicb = oc.getXml2Dic(b);
                    if (dicb[2].ContainsKey("code") && dicb[2]["code"] == "100")
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(5000);
                }
                directDownloadURL = dicb[1]["directDownload"];
            }

            Console.WriteLine("Done!");
            if (directDownloadURL != "")
            {
                string finalPath = Path.ChangeExtension(filePath, newExtension);
                FileStream file = File.Create(finalPath);
                WriteToFileFromUrl(file, directDownloadURL);
                Console.WriteLine(finalPath);
                return finalPath;
            }
            else
            {
                return "error";
            }
        }

        public void WriteToFileFromUrl(FileStream file, string url)
        {
            byte[] fileBuffer = http.DownloadData(url);
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
    }
}
