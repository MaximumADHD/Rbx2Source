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
    
    public partial class CdnPender
    {
        [JsonProperty("data")]
        public Datum[] Data { get; set; }
    }

    public partial class Datum
    {
        [JsonProperty("targetId")]
        public long TargetId { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }
    }

    public static class WebUtility
    {
        private const string V = "application/json";

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

        public static byte[] DownloadData(string address, string body = "", string method = "GET")
        {
            HttpWebRequest request = WebRequest.CreateHttp(new Uri(address));
            request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip");

            request.UserAgent = "Roblox";
            request.Proxy = null;

            request.UseDefaultCredentials = true;
            request.Method = method;

            if (method.ToUpper() != "GET") {
                request.ContentLength = Encoding.Default.GetBytes(body).Length;
                request.ContentType = V;
                request.GetRequestStream().Write(Encoding.Default.GetBytes(body), 0, Encoding.Default.GetBytes(body).Length);

            }

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

        public static T DownloadJSON<T>(string address,string body = "", string method = "GET")
        {
            byte[] content = DownloadData(address, body, method);
            var json = Encoding.UTF8.GetString(content);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T DownloadRbxApiJSON<T>(string subAddress, string apiServer = "api", string body = "", string method = "GET") // TODO: Replace this code to use the newer roblox API endpoints
        {
            string url = "https://" + apiServer + ".roblox.com/" + subAddress;
            return DownloadJSON<T>(url,body,method);
        }

        public static string PendCdn(string address, bool log = true) // This is the image downloading Code
        {
            string result = null;
            bool final = false;
            string dots = "..";

            while (!final && dots.Length <= 13)
            {
                CdnPender pender = DownloadJSON<CdnPender>(address);
                final = pender.Data[0].State == "Final";
                result = pender.Data[0].ImageUrl.ToString();

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
