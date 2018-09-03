using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.IO.Compression;

using Newtonsoft.Json;
using Rbx2Source.Assembler;
using Rbx2Source.Resources;

namespace Rbx2Source.Web
{
    struct ProductInfo
    {
        public string Name;
        public string WindowsSafeName;
        public AssetType AssetTypeId;
    }

    class Asset
    {
        public long Id;
        public AssetType AssetType;
        public ProductInfo ProductInfo;
        public bool Loaded = false;
        public bool IsLocal = false;
        public string CdnUrl;
        public string CdnCacheId;
        public byte[] Content;
        public bool ContentLoaded = false;

        private static Dictionary<long, long> avidToId = new Dictionary<long, long>();
        private static Dictionary<long, Asset> assetCache = new Dictionary<long, Asset>();

        public byte[] GetContent()
        {
            if (!ContentLoaded)
            {
                HttpWebRequest request = WebRequest.CreateHttp(CdnUrl);
                request.UserAgent = "Roblox";
                request.Proxy = null;
                request.UseDefaultCredentials = true;
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();

                string encoding = response.ContentEncoding;
                if (encoding == "gzip")
                    responseStream = new GZipStream(responseStream, CompressionMode.Decompress);

                Content = FileUtility.ReadFullStream(responseStream);
                ContentLoaded = true;

                responseStream.Close();
                response.Close();
            }

            return Content;
        }

        public static Asset Get(long assetId, string idPiece = "/asset/?ID=")
        {
            if (!assetCache.ContainsKey(assetId))
            {
                string appData = Environment.GetEnvironmentVariable("AppData");
                string assetCacheDir = Path.Combine(appData, "Rbx2Source", "AssetCache");
                Directory.CreateDirectory(assetCacheDir);

                // Ping Roblox to figure out what this asset's cdn url is
                HttpWebRequest ping = WebRequest.CreateHttp("https://assetgame.roblox.com" + idPiece + assetId);
                ping.UserAgent = "Roblox";
                ping.Method = "HEAD";
                ping.AllowAutoRedirect = false;

                HttpWebResponse response = (HttpWebResponse)ping.GetResponse();
                string location = response.GetResponseHeader("Location");
                string identifier = location.Remove(0, 7).Replace(".rbxcdn.com/", "-") + "_64";
                string cachedFile = Path.Combine(assetCacheDir, identifier);
                response.Close();

                Asset asset = null;
                if (File.Exists(cachedFile))
                {
                    string cachedContent = File.ReadAllText(cachedFile);
                    try
                    {
                        asset = JsonConvert.DeserializeObject<Asset>(cachedContent);
                        Rbx2Source.Print("Fetched pre-cached asset {0}", assetId);
                    }
                    catch
                    {
                        // Corrupted file?
                        if (File.Exists(cachedFile)) File.Delete(cachedFile);
                    }
                }

                if (asset == null)
                {
                    asset = new Asset();

                    WebClient http = new WebClient();
                    http.UseDefaultCredentials = true;
                    http.Headers.Set(HttpRequestHeader.UserAgent, "Roblox");
                    http.Proxy = null;
                    asset.Id = assetId;

                    try
                    {
                        string productInfoJson = http.DownloadString("http://api.roblox.com/marketplace/productinfo?assetId=" + assetId);
                        asset.ProductInfo = JsonConvert.DeserializeObject<ProductInfo>(productInfoJson);
                        asset.ProductInfo.WindowsSafeName = FileUtility.MakeNameWindowsSafe(asset.ProductInfo.Name);
                        asset.AssetType = asset.ProductInfo.AssetTypeId;
                    }
                    catch
                    {
                        ProductInfo dummyInfo = new ProductInfo();
                        dummyInfo.Name = "unknown_" + asset.Id;
                        dummyInfo.WindowsSafeName = dummyInfo.Name;
                        dummyInfo.AssetTypeId = AssetType.Model;
                    }

                    asset.CdnUrl = location;
                    asset.CdnCacheId = identifier;
                    asset.GetContent();
                    asset.Loaded = true;

                    string serialized = JsonConvert.SerializeObject(asset, Formatting.None, new JsonSerializerSettings());
                    try
                    {
                        File.WriteAllText(cachedFile, serialized);
                        Rbx2Source.Print("Precached AssetId {0}", assetId);
                    }
                    catch
                    {
                        // Oh well.
                        Rbx2Source.Print("Failed to cache AssetId {0}", assetId);
                    }
                }

                assetCache[assetId] = asset;
            }

            return assetCache[assetId];
        }

        public static Asset FromResource(string path)
        {
            byte[] embedded = ResourceUtility.GetResource(path);

            Asset local = new Asset();
            local.Content = embedded;
            local.ContentLoaded = true;
            local.AssetType = AssetType.Model;
            local.Id = 0;
            local.IsLocal = true;

            ProductInfo dummyInfo = new ProductInfo();
            dummyInfo.Name = "???";
            dummyInfo.AssetTypeId = AssetType.Model;

            local.Loaded = true;
            return local;
        }

        public static Asset GetByAssetId(string url = "")
        {
            if (url == null || url.Length == 0)
                url = "rbxassetid://9854798";

            long legacyId = LegacyAssets.Check(url);
            if (legacyId > 0)
            {
                return Get(legacyId);
            }
            else
            {
                string sAssetId = "";
                int endIndex = -1;
                for (int i = url.Length - 1; i >= 0; i--)
                {
                    char c = url[i];
                    if (char.IsNumber(c))
                    {
                        endIndex = i;
                        break;
                    }
                }
                for (int i = endIndex; i >= 0; i--)
                {
                    char c = url[i];
                    if (!char.IsNumber(c)) break;
                    sAssetId += c;
                }
                char[] charArray = sAssetId.ToCharArray();
                Array.Reverse(charArray);
                sAssetId = new string(charArray);

                long assetId = long.Parse(sAssetId);
                return Get(assetId);
            }
        }
    }
}
