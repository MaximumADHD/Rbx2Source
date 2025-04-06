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

    public class AssetContentRepresentationSpecifier
    {
        public string Format;
        public string MajorVersion;
        public string Fidelity;
    }

    public class AssetResponseItem
    {
        public Uri Location;
        public bool IsHashDynamic;
        public bool IsCopyrightProtected;
        public bool IsArchived;
        public AssetType AssetTypeId;
        public AssetContentRepresentationSpecifier ContentRepresentationSpecifier;
    }

    public class Asset
    {
        public long Id;

        public AssetType AssetType;
        public ProductInfo ProductInfo;
        public AssetResponseItem ResponseItem;

        public bool Loaded;
        public bool IsLocal;

        public Uri CdnUri;
        public string CdnCacheId;

        public byte[] Content;
        public bool ContentLoaded;

        private static readonly Dictionary<long, Asset> assetCache = new Dictionary<long, Asset>();

        public static AssetResponseItem GetResponseItem(long assetId, string apiKey)
        {
            Uri uri = new Uri("https://apis.roblox.com/asset-delivery-api/v1/assetId/" + assetId);
            AssetResponseItem responseItem;

            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Headers.Add("x-api-key", apiKey);
            request.AllowAutoRedirect = false;
            request.UserAgent = "Rbx2Source";
            request.Method = "GET";

            using (var response = request.GetResponse() as HttpWebResponse)
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                string json = reader.ReadToEnd();
                responseItem = JsonConvert.DeserializeObject<AssetResponseItem>(json);
            }

            return responseItem;
        }

        public Instance OpenAsModel()
        {
            byte[] content = GetContent();
            return RobloxFile.Open(content);
        }

        public override int GetHashCode()
        {
            return Id.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Asset asset)
                return asset.Id == Id;

            return false;
        }

        public byte[] GetContent()
        {
            if (!ContentLoaded)
            {
                try
                {
                    HttpWebRequest request = WebRequest.CreateHttp(CdnUri);
                    request.UserAgent = "Rbx2Source";
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

        public static Asset Get(long assetId)
        {
            if (!assetCache.ContainsKey(assetId))
            {
                string appData = Environment.GetEnvironmentVariable("LocalAppData");

                string assetCacheDir = Path.Combine(appData, "Rbx2Source", "AssetCache");
                Directory.CreateDirectory(assetCacheDir);

                // Ping Roblox to figure out what this asset's cdn url is
                var apiKey = Rbx2Source.GetApiKey();

                if (apiKey == "")
                    throw new Exception("Invalid API key!");

                var responseItem = GetResponseItem(assetId, apiKey);
                Uri uri = new Uri("https://apis.roblox.com/asset-delivery-api/v1/assetId/" + assetId);

                HttpWebRequest request = WebRequest.CreateHttp(uri);
                request.Headers.Add("x-api-key", apiKey);
                request.UserAgent = "Rbx2Source";
                request.AllowAutoRedirect = false;
                request.Method = "GET";

                Asset asset = null;
                Uri location = responseItem.Location;

                var identifier = location.Segments[1];
                var cachedFile = assetCacheDir + '\\' + identifier;

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

                if (asset == null)
                {
                    var http = new WebClient()
                    {
                        Headers = {{ HttpRequestHeader.UserAgent, "Rbx2Source" }},
                        UseDefaultCredentials = true,
                        Proxy = null
                    };

                    asset = new Asset() 
                    {
                        Id = assetId,
                        ResponseItem = responseItem,
                    };
                    
                    try
                    {
                        string productInfoJson = http.DownloadString($"https://economy.roblox.com/v2/assets/{assetId}/details");
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

                    asset.CdnUri = location;
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
                if (address == "rbxasset://textures/face.png")
                    return FromResource("Images/face.png");


            return Get(assetId);
        }
    }
}
