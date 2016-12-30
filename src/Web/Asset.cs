using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public int Id;
        public AssetType AssetType;
        public ProductInfo ProductInfo;
        public bool Loaded = false;
        public bool IsLocal = false;
        public int VersionId;
        public bool HasVersionId;
        public byte[] Content;
        public bool ContentLoaded = false;

        private static Dictionary<int, int> avidToId = new Dictionary<int, int>();
        private static Dictionary<int, Asset> assetCache = new Dictionary<int, Asset>();

        public byte[] GetContent()
        {
            if (!ContentLoaded)
            {
                HttpWebRequest request = WebRequest.CreateHttp("https://assetgame.roblox.com/asset/?ID=" + Id);
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip");
                request.UserAgent = "Roblox";
                request.Proxy = null;
                request.UseDefaultCredentials = true;
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                GZipStream decompressor = new GZipStream(responseStream, CompressionMode.Decompress);

                Content = FileUtility.ReadFullStream(decompressor);
                ContentLoaded = true;

                responseStream.Close();
                response.Close();
            }

            return Content;
        }

        public Task<byte[]> GetContentAsync()
        {
            Task<byte[]> contentTask = Task.Run<byte[]>(() =>
            {
                byte[] content = GetContent();
                return content;
            });
            return contentTask;
        }

        public static Asset Get(int assetId, bool isVersionId = false)
        {
            int versionId = -1;

            string appData = Environment.GetEnvironmentVariable("AppData");
            string assetCacheDir = Path.Combine(appData, "Rbx2Source", "AssetCache");
            string cachedFile = Path.Combine(assetCacheDir, assetId.ToString());
            Directory.CreateDirectory(assetCacheDir);

            string cachedContent = "";
            bool gotCachedContent = false;

            if (isVersionId)
            {
                versionId = assetId;
                
                if (File.Exists(cachedFile))
                {
                    Rbx2Source.Print("Got precached resource " + versionId);
                    cachedContent = File.ReadAllText(cachedFile);
                    gotCachedContent = true;
                }
                else if (!avidToId.ContainsKey(assetId))
                {
                    HttpWebRequest request = WebRequest.CreateHttp("http://www.roblox.com/Item.aspx?avid=" + assetId);
                    request.Proxy = null;
                    request.UserAgent = "Roblox";

                    WebResponse response = request.GetResponse();
                    string sAssetId = response.ResponseUri.Segments[2].Replace("/", "");
                    int newAssetId = int.Parse(sAssetId);
                    avidToId[assetId] = newAssetId;
                    assetId = newAssetId;

                    response.Close();
                }
                else
                {
                    assetId = avidToId[assetId];
                }
            }

            if (!assetCache.ContainsKey(assetId))
            {
                Asset asset = null;
                if (gotCachedContent)
                {
                    try
                    {
                        asset = JsonConvert.DeserializeObject<Asset>(cachedContent);
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
                    if (isVersionId)
                    {
                        asset.HasVersionId = true;
                        asset.VersionId = versionId;
                    }

                    WebClient http = new WebClient();
                    http.UseDefaultCredentials = true;
                    http.Headers.Set(HttpRequestHeader.UserAgent, "Roblox");
                    http.Proxy = null;
                    asset.Id = assetId;
                    
                    string productInfoJson = http.DownloadString("http://api.roblox.com/marketplace/productinfo?assetId=" + assetId);
                    asset.ProductInfo = JsonConvert.DeserializeObject<ProductInfo>(productInfoJson);
                    asset.ProductInfo.WindowsSafeName = FileUtility.MakeNameWindowsSafe(asset.ProductInfo.Name);
                    asset.AssetType = asset.ProductInfo.AssetTypeId;
                    asset.Loaded = true;

                    if (isVersionId)
                    {
                        asset.GetContent();
                        string cacheContent = JsonConvert.SerializeObject(asset, Formatting.None, new JsonSerializerSettings());
                        try
                        {
                            File.WriteAllText(cachedFile, cacheContent);
                            Rbx2Source.Print("Precached resource {0}",versionId);
                        }
                        catch
                        {
                            // Oh well.
                            Console.ForegroundColor = ConsoleColor.Red;
                            Rbx2Source.Print("Failed to cache assetVersionId {0}!",versionId);
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                }

                return asset;
            }
            else
            {
                return assetCache[assetId];
            }
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

        public static Asset GetByAssetId(string url = "rbxassetid://9854798")
        {
            int legacyId = LegacyAssets.Check(url);
            if (legacyId > 0)
                return Asset.Get(legacyId);
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

                int assetId = int.Parse(sAssetId);
                return Get(assetId);
            }
        }

        public static Task<Asset> GetAsync(int assetId, bool isVersionId = false)
        {
            Task<Asset> asyncAsset = Task.Run<Asset>(() =>
            {
                Asset result = Get(assetId, isVersionId);
                return result;
            });
            return asyncAsset;
        }
    }
}
