#pragma warning disable 0649

using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Rbx2Source.Web
{
    public struct WebApiError
    {
        public int Code;
        public string Message;
    }

    public struct CdnPender
    {
        public string Url;
        public bool Final;
    }

    public class WebUtility
    {
        private static byte[] ReadFullStream(Stream stream, bool close = true)
        {
            byte[] result;

            using (MemoryStream streamBuffer = new MemoryStream())
            {
                stream.CopyTo(streamBuffer);
                result = streamBuffer.GetBuffer();
            }

            if (close)
            {
                stream.Close();
                stream.Dispose();
            }

            return result;
        }

        private static void wait(float time)
        {
            int ms = (int)(time * 1000);
            Task waitTask = Task.Delay(ms);
            waitTask.Wait();
        }

        public static byte[] DownloadData(string url)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip");

            request.UserAgent = "Roblox";
            request.Proxy = null;

            request.UseDefaultCredentials = true;
            request.Method = "GET";


            var response = request.GetResponse() as HttpWebResponse;
            var responseStream = response.GetResponseStream();

            byte[] result = null;

            if (response.ContentEncoding == "gzip")
            { 
                GZipStream decompressor = new GZipStream(responseStream, CompressionMode.Decompress);
                result = ReadFullStream(decompressor);
            }
            else
            {
                result = ReadFullStream(responseStream);
            }

            return result;
        }

        public static string DownloadString(string url)
        {
            byte[] data = DownloadData(url);
            return Encoding.UTF8.GetString(data);
        }

        public static Bitmap DownloadImage(string url)
        {
            byte[] data = DownloadData(url);

            using (Stream imgStream = new MemoryStream(data))
            {
                return new Bitmap(imgStream);
            }
        }

        public static T DownloadJSON<T>(string url)
        {
            byte[] content = DownloadData(url);
            var json = Encoding.UTF8.GetString(content);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T DownloadRbxApiJSON<T>(string subUrl, string apiServer = "api")
        {
            string url = "https://" + apiServer + ".roblox.com/" + subUrl;
            return DownloadJSON<T>(url);
        }

        public static string PendCdnUrl(string url, bool log = true)
        {
            string result = null;
            bool final = false;
            string dots = "..";

            while (!final && dots.Length <= 13)
            {
                CdnPender pender = DownloadJSON<CdnPender>(url);
                final = pender.Final;
                result = pender.Url;
                
                if (!final)
                {
                    dots += ".";

                    if (log)
                        Rbx2Source.Print("Waiting for finalization of " + url + dots);

                    wait(1f);
                }
            }

            if (dots.Length > 13)
                throw new Exception("CdnPender timed out after 10 retries! Roblox's servers may be overloaded right now.\nTry again after a few minutes!");

            return result;
        }

        public static string ResolveHashUrl(string hash)
        {
            int comp = 31;

            foreach (char c in hash)
                comp ^= (byte)c;

            int id = comp % 8;
            return "https://t" + id + ".rbxcdn.com/" + hash;
        }
    }
}
