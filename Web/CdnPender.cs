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

    public class CdnPender
    {
        public string Url;
        public bool Final;

        public static async Task<string> PendCdn(string address, bool log = true)
        {
            string result = null;
            string dots = "..";

            using (var http = new RobloxWebClient())
            {
                bool final = false;
                
                while (!final && dots.Length <= 13)
                {
                    CdnPender pender = await http.DownloadJson<CdnPender>(address);
                    final = pender.Final;
                    result = pender.Url;

                    if (final)
                        break;

                    dots += ".";
                }
            }
            
            if (dots.Length > 13)
                throw new Exception("CdnPender timed out after 10 retries! Roblox's servers may be overloaded right now.\nTry again after a few minutes!");

            return result;
        }
    }
}
