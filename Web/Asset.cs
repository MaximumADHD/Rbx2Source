using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Rbx2Source.Assembler;
using Rbx2Source.Resources;

using RobloxFiles;

namespace Rbx2Source.Web
{
    public class ProductInfo
    {
        public string Name;
        public string WindowsSafeName;
        public AssetType AssetTypeId;
    }

    public class Asset
    {
        public long Id;

        public AssetType AssetType;
        public ProductInfo ProductInfo;

        public bool Loaded;
        public bool IsLocal;

        public string CdnUrl;
        public string CdnCacheId;

        public byte[] Content;
        public bool ContentLoaded;

        private static readonly Dictionary<long, Asset> assetCache = new Dictionary<long, Asset>();

        public Instance OpenAsModel()
        {
            byte[] content = GetContent();

            try
            {
                return RobloxFile.Open(content);
            }
            catch
            {
                return new Folder();
            }
        }

        public byte[] GetContent()
        {
            if (!ContentLoaded)
            {
                try
                {
                    HttpWebRequest request = WebRequest.CreateHttp(CdnUrl);
                    request.UserAgent = "RobloxStudio/WinInet";
                    request.Proxy = null;
                    request.UseDefaultCredentials = true;
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");

                    var response = request.GetResponse() as HttpWebResponse;
                    var responseStream = response.GetResponseStream();
                    string encoding = response.ContentEncoding;

                    if (encoding == "gzip")
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);

                    Content = FileUtility.ReadFullStream(responseStream);
                    ContentLoaded = true;

                    responseStream.Close();
                    response.Close();
                }
                catch
                {
                    Content = Array.Empty<byte>();
                    ContentLoaded = false;
                }
            }

            return Content;
        }

        public static Asset Get(long assetId, string idPiece = "/asset/?ID=")
        {
            if (!assetCache.ContainsKey(assetId))
            {
                string appData = Environment.GetEnvironmentVariable("LocalAppData");

                string assetCacheDir = Path.Combine(appData, "Rbx2Source", "AssetCache");
                Directory.CreateDirectory(assetCacheDir);

                // Ping Roblox to figure out what this asset's cdn url is
                Uri uri = new Uri("https://assetdelivery.roblox.com/v1" + idPiece + assetId);
                HttpWebRequest ping = WebRequest.CreateHttp(uri);
                ping.UserAgent = "RobloxStudio/WinInet";
                ping.AllowAutoRedirect = false;

                Asset asset = null;

                string location = "";
                string identifier = "";
                string cachedFile = "";

                try
                {
                    using (var response = ping.GetResponse() as HttpWebResponse)
                    {
                        location = response.GetResponseHeader("Location");
                        identifier = location.Remove(0, 8).Replace(".rbxcdn.com/", "-");
                        cachedFile = assetCacheDir + '\\' + identifier.Replace('/', '\\');

                        if (File.Exists(cachedFile))
                        {
                            string cachedContent = File.ReadAllText(cachedFile);

                            try
                            {
                                asset = JsonConvert.DeserializeObject<Asset>(cachedContent);

                                if (asset.Content.Length == 0)
                                {
                                    asset = null;
                                    throw new Exception();
                                }

                                Rbx2Source.Print("Fetched pre-cached asset {0}", assetId);
                            }
                            catch
                            {
                                // Corrupted file?
                                if (File.Exists(cachedFile))
                                {
                                    Rbx2Source.Print("Deleting corrupted file {0}", cachedFile);
                                    File.Delete(cachedFile);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to fetch {0}?", assetId);
                }

                if (asset == null)
                {
                    WebClient http = new WebClient()
                    {
                        UseDefaultCredentials = true,
                        Proxy = null
                    };

                    http.Headers.Set(HttpRequestHeader.UserAgent, "RobloxStudio/WinInet");
                    asset = new Asset() { Id = assetId };
                    
                    try
                    {
                        string productInfoJson = http.DownloadString("http://api.roblox.com/marketplace/productinfo?assetId=" + assetId);
                        asset.ProductInfo = JsonConvert.DeserializeObject<ProductInfo>(productInfoJson);
                        asset.ProductInfo.WindowsSafeName = FileUtility.MakeNameWindowsSafe(asset.ProductInfo.Name);
                        asset.AssetType = asset.ProductInfo.AssetTypeId;
                    }
                    catch
                    {
                        string name = "unknown_" + asset.Id;

                        ProductInfo dummyInfo = new ProductInfo()
                        {
                            Name = name,
                            WindowsSafeName = name,
                            AssetTypeId = AssetType.Model
                        };

                        asset.ProductInfo = dummyInfo;
                    }

                    asset.CdnUrl = location;
                    asset.CdnCacheId = identifier;
                    asset.GetContent();
                    asset.Loaded = true;

                    string serialized = JsonConvert.SerializeObject(asset, Formatting.None);

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

                    http.Dispose();
                }

                assetCache[assetId] = asset;
            }

            return assetCache[assetId];
        }

        public static Asset FromResource(string path)
        {
            byte[] embedded = ResourceUtility.GetResource(path);

            return new Asset()
            {
                Content = embedded,
                ContentLoaded = true,
                AssetType = AssetType.Model,
                IsLocal = true,
                Loaded = true,
                Id = 0
            };
        }

        public static Asset GetByAssetId(string address = "")
        {
            if (address == null || address.Length == 0)
                address = "rbxassetid://9854798";

            long legacyId = LegacyAssets.Check(address);

            if (legacyId > 0)
                return Get(legacyId);
           
            var match = Regex.Match(address.Trim(), @"\d+$");
            string sAssetId = match.Value;

            if (!long.TryParse(sAssetId, out long assetId))
                System.Diagnostics.Debugger.Break();

            return Get(assetId);
        }
    }
}
