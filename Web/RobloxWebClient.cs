using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Rbx2Source.Web
{
    public class RobloxWebClient : WebClient
    {
        public RobloxWebClient()
        {
            Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip");
            Headers.Set(HttpRequestHeader.UserAgent, "RobloxStudio/WinInet");
        }

        public async Task<T> DownloadJson<T>(string url)
        {
            string json = await DownloadStringTaskAsync(url);
            return JsonConvert.DeserializeObject<T>(json);
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.GZip;

            return request;
        }
    }
}
