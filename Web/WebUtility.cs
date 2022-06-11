#pragma warning disable 0649

using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Rbx2Source.Web
{
    public class WebApiError
    {
        public int Code;
        public string Message;
    }

    public class CdnPender
    {
        public string Url;
        public bool Final;
    }

    public static class WebUtility
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

        public static byte[] DownloadData(string address)
        {
            HttpWebRequest request = WebRequest.CreateHttp(new Uri(address));
            request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip");

            request.UserAgent = "Roblox";
            request.Proxy = null;

            request.UseDefaultCredentials = true;
            request.Method = "GET";


            var response = request.GetResponse() as HttpWebResponse;
            var responseStream = response.GetResponseStream();

            byte[] result;

            if (response.ContentEncoding == "gzip")
            { 
                var decompressor = new GZipStream(responseStream, CompressionMode.Decompress);
                result = ReadFullStream(decompressor);
                decompressor.Dispose();
            }
            else
            {
                result = ReadFullStream(responseStream);
            }

            return result;
        }

        public static string DownloadString(string address)
        {
            byte[] data = DownloadData(address);
            return Encoding.UTF8.GetString(data);
        }

        public static Bitmap DownloadImage(string address)
        {
            byte[] data = DownloadData(address);
            Bitmap result;

            using (Stream imgStream = new MemoryStream(data))
                result = new Bitmap(imgStream);

            return result;
        }

        public static T DownloadJSON<T>(string address)
        {
            byte[] content = DownloadData(address);
            var json = Encoding.UTF8.GetString(content);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T DownloadRbxApiJSON<T>(string subAddress, string apiServer = "api")
        {
            string url = "https://" + apiServer + ".roblox.com/" + subAddress;
            return DownloadJSON<T>(url);
        }

        public static string PendCdn(string address, bool log = true)
        {
            string result = null;
            bool final = false;
            string dots = "..";

            while (!final && dots.Length <= 13)
            {
                CdnPender pender = DownloadJSON<CdnPender>(address);
                final = pender.Final;
                result = pender.Url;
                
                if (!final)
                {
                    dots += ".";

                    if (log)
                        Rbx2Source.Print("Waiting for finalization of " + address + dots);

                    wait(1f);
                }
            }

            if (dots.Length > 13)
                throw new Exception("CdnPender timed out after 10 retries! Roblox's servers may be overloaded right now.\nTry again after a few minutes!");

            return result;
        }

        public static string ResolveHash(string hash)
        {
            Contract.Requires(hash != null);
            int id = 31;

            foreach (char c in hash)
                id ^= (byte)c;

            return $"https://t{id % 8}.rbxcdn.com/{hash}";
        }
    }
}
