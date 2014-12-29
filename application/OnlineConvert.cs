////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// I didn't write this. I obtained this sample from Online-Convert's github page.
// Url here:
// https://github.com/onlineconvert/onlineconvert-api-example-codes/blob/master/DotNet/OnlineConvert/OnlineConvert/OnlineConvert.cs
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RobloxToSourceEngine
{
    public class OnlineConvert
    {
        // response constants
        public const int PARAMS = 2;
        public const int STATUS = 1;
        // request constants
        public const int FILE = 1;
        public const int COMMAN = 0;

        /**
        * Online Converter API URL
        */
        const string URL = "http://api.online-convert.com";

        /**
         * Source type constant value for URL type.
         */
        public const string SOURCE_TYPE_URL = "URL";

        /**
         * Source type constant value for FILE_BASE64 type.
         */
        public const string SOURCE_TYPE_FILE_BASE64 = "FILE_BASE64";

        /**
         * Source type constant value for FILE_PATH type.
         */
        public const string SOURCE_TYPE_FILE_PATH = "FILE_PATH";

        /**
         * @var string Online Converter API Key
         */
        private string apiKey;

        /**
         * @var boolean
         */
        private bool testMode;

        /**
         * @var string
         */
        private string url = OnlineConvert.URL;

        /**
         * @var string
         */
        private string targetType;

        /**
         * @var string
         */
        private string sourceType;

        /**
         * @var string
         */
        public string hash;

        /**
         * @var string
         */
        public string downloadUrl;

        /**
         * @var array
         */
        private Dictionary<string, string> targetTypeOptions;

        /**
         * @var array
         */
        private Dictionary<string, string> sourceTypeOptions;

        private OnlineConvert(String targetType = "")
        {
            targetTypeOptions = new Dictionary<string, string>();
            sourceTypeOptions = new Dictionary<string, string>();
            this.targetTypeOptions.Add("convert-to-7z", "archive");
            this.targetTypeOptions.Add("convert-to-bz2", "archive");
            this.targetTypeOptions.Add("convert-to-gz", "archive");
            this.targetTypeOptions.Add("convert-to-zip", "archive");
            this.targetTypeOptions.Add("convert-to-aac", "audio");
            this.targetTypeOptions.Add("convert-to-aiff", "audio");
            this.targetTypeOptions.Add("convert-to-flac", "audio");
            this.targetTypeOptions.Add("convert-to-m4a", "audio");
            this.targetTypeOptions.Add("convert-to-mp3", "audio");
            this.targetTypeOptions.Add("convert-to-ogg", "audio");
            this.targetTypeOptions.Add("convert-to-opus", "audio");
            this.targetTypeOptions.Add("convert-to-wav", "audio");
            this.targetTypeOptions.Add("convert-to-wma", "audio");
            this.targetTypeOptions.Add("convert-to-doc", "document");
            this.targetTypeOptions.Add("convert-to-docx", "document");
            this.targetTypeOptions.Add("convert-to-flash", "document");
            this.targetTypeOptions.Add("convert-to-html", "document");
            this.targetTypeOptions.Add("convert-to-odt", "document");
            this.targetTypeOptions.Add("convert-to-ppt", "document");
            this.targetTypeOptions.Add("convert-to-rtf", "document");
            this.targetTypeOptions.Add("convert-to-txt", "document");
            this.targetTypeOptions.Add("convert-to-azw3", "ebook");
            this.targetTypeOptions.Add("convert-to-epub", "ebook");
            this.targetTypeOptions.Add("convert-to-fb2", "ebook");
            this.targetTypeOptions.Add("convert-to-lit", "ebook");
            this.targetTypeOptions.Add("convert-to-lrf", "ebook");
            this.targetTypeOptions.Add("convert-to-mobi", "ebook");
            this.targetTypeOptions.Add("convert-to-pdb", "ebook");
            this.targetTypeOptions.Add("convert-to-pdf", "ebook");
            this.targetTypeOptions.Add("convert-to-tcr", "ebook");
            this.targetTypeOptions.Add("convert-to-bmp", "image");
            this.targetTypeOptions.Add("convert-to-jpg", "image");
            this.targetTypeOptions.Add("convert-to-png", "image");
            this.targetTypeOptions.Add("convert-to-tga", "image");
            this.targetTypeOptions.Add("convert-to-mp4", "video");
            this.targetTypeOptions.Add("convert-to-wmv", "video");
            //
            this.sourceTypeOptions.Add(OnlineConvert.SOURCE_TYPE_URL, "URL");
            this.sourceTypeOptions.Add(OnlineConvert.SOURCE_TYPE_FILE_BASE64, "FILE_BASE64");
            this.sourceTypeOptions.Add(OnlineConvert.SOURCE_TYPE_FILE_PATH, "FILE_PATH");
            this.targetType = targetType;
        }

        /**
     * convert
     * Make a API call to convert file/url/hash based on parameters and return xml response.
     *
     * @param string targetType To which file you want to convert (like convert-to-jpg, convert-to-mp3)
     * @param string sourceType 3 source types you can set (URL, FILE_PATH and FILE_BASE64)
     * @param string source Source can be provide based on sourceType if sourceType = URL you have to provide url string to this param.
     * @param string sourceName Provide file Name. This param used only with sourceType = FILE_PATH or FILE_BASE64
     * @param string sourceOptions Provide file conversion required extra parameters as array using this param.
     * @param string notificationUrl For set notification url for api actions.
     */
        public string convert(string targetType, string sourceType, string source, string sourceName = null, string sourceOptions = null, string notificationUrl = null)
        {
            if (!this.targetTypeOptions.ContainsKey(targetType))
            {
                throw new Exception("Invalid Target Type.");
            }
            this.targetType = targetType;

            if (!this.sourceTypeOptions.ContainsKey(sourceType))
            {
                throw new Exception("Invalid Source Type.");
            }
            this.sourceType = sourceType;

            if (this.sourceType == OnlineConvert.SOURCE_TYPE_FILE_BASE64 || this.sourceType == OnlineConvert.SOURCE_TYPE_FILE_PATH)
            {
                if (sourceName == null || sourceName.Length < 1)
                {
                    throw new Exception("Invalid Source Name.");
                }
            }

            if (this.sourceType == OnlineConvert.SOURCE_TYPE_FILE_PATH)
            {
                if (!System.IO.File.Exists(source))
                {
                    throw new Exception("File not found: " + source);
                }
                source = Convert.ToBase64String(System.IO.File.ReadAllBytes(source));
            }

            Dictionary<int, Dictionary<string, string>> dic = new Dictionary<int, Dictionary<string, string>>();
            var d = new Dictionary<string, string>();
            var f = new Dictionary<string, string>();
            d.Add("apiKey", this.apiKey);


            d.Add("targetType", this.targetTypeOptions[targetType]);
            d.Add("targetMethod", targetType);
            d.Add("testMode", this.testMode.ToString());
            d.Add("notificationUrl", notificationUrl);

            if (sourceOptions != null)
                d.Add("format", sourceOptions);

            var response = "";

            if (this.sourceType == OnlineConvert.SOURCE_TYPE_URL)
            {
                d.Add("sourceUrl", source);
                response = this.apiCall("queue-insert", d);
            }
            else
            {
                f.Add("fileName", sourceName);
                f.Add("fileData", source);
                dic.Add(OnlineConvert.COMMAN, d);
                dic.Add(OnlineConvert.FILE, f);
                response = this.apiCall("queue-insert", dic);
            }
            var responseDic = this.getXml2Dic(response);
            if (responseDic[2].ContainsKey("code") && responseDic[2]["code"] == "0")
            {
                this.hash = responseDic[1]["hash"];
            }
            this.url = URL;
            return response;
        }

        /**
         * create
         * Create instance and set required parameters.
         *
         * @param string apiKey
         * @param string testMode
         * @return self
         */
        public static OnlineConvert create(string apiKey, bool testMode = true, String targetType = "")
        {
            OnlineConvert instance = new OnlineConvert(targetType);
            instance.apiKey = apiKey;
            instance.testMode = testMode;
            return instance;
        }

        /**
         * getProgress
         * Provide process status for created instance or you can manually check status for specific hash.
         *
         * @param string hash Hash value of process which you want to check status.
         * @return string xml response string from server.
         */
        public string getProgress(string hash = "")
        {
            if (this.hash.Length < 1 && hash.Length < 1)
            {
                throw new Exception("get Job Status: Hash not found.");
            }
            var d = new Dictionary<string, string>();
            d.Add("apiKey", this.apiKey);
            d.Add("hash", (!string.IsNullOrEmpty(hash)) ? hash : this.hash);
            return this.apiCall("queue-status", d);
        }

        /**
         * deleteFile
         * Delete file from server after process or you can manually delete for specific hash.
         *
         * @param string hash Hash value of process which you want to delete.
         * @return string xml response string from server.
         */
        public string deleteFile(string hash = "")
        {
            if (this.hash.Length < 1 && hash.Length < 1)
            {
                throw new Exception("Delete File: Hash not found.");
            }

            var d = new Dictionary<string, string>();
            d.Add("apiKey", this.apiKey);
            d.Add("hash", (!string.IsNullOrEmpty(hash)) ? hash : this.hash);
            d.Add("method", "deleteFile");

            return this.apiCall("queue-manager", d);
        }

        /**
         * createToken
         * Create one time token to use API.
         *
         * @return string xml response string from server.
         */
        public string createToken()
        {
            var d = new Dictionary<string, string>();
            d.Add("apiKey", this.apiKey);
            return this.apiCall("request-token", d);
        }

        /**
         * getServer
         * Get free api server information.
         *
         * @param string targetType To which file you want to convert (like audio, video)
         * @return string xml response string from server.
         */
        public string getServer(string targetType)
        {
            var d = new Dictionary<string, string>();
            d.Add("apiKey", this.apiKey);
            d.Add("targetType", targetType);
            return this.apiCall("get-queue", d);
        }

        public string getDic2Xml(Dictionary<string, string> xmlDic)
        {
            string xmlString = "";
            foreach (var node in xmlDic)
            {
                xmlString += "<" + node.Key + ">" + node.Value + "</" + node.Key + ">";
            }
            return xmlString;
        }

        /**
         * apiCall
         * Make an API call to server based on action and parameters.
         *
         * @param string action API action name
         * @param Dictionary  xmlData Dictionary of xml data
         * @return string xml response string from server.
         */
        private string apiCall(string action, Dictionary<string, string> xmlData)
        {

            if (action != "get-queue")
            {
                if (String.IsNullOrEmpty(this.targetType))
                {
                    throw new Exception("Target type is null");
                }
                this.getServer(this.targetTypeOptions[this.targetType]);
            }

            using (var wb = new System.Net.WebClient())
            {
                var data = new System.Collections.Specialized.NameValueCollection();
                data["queue"] = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><queue>" + this.getDic2Xml(xmlData) + "</queue>";
                var response = wb.UploadValues(OnlineConvert.URL + "/" + action, "POST", data);
                string result = System.Text.Encoding.UTF8.GetString(response);
                return result;
            }
        }

        /**
        * apiCall
        * Make an API call to server based on action and parameters.
        *
        * @param string action API action name
        * @param Dictionary  xmlData Dictionary of xml data
        * @return string xml response string from server.
        */
        private string apiCall(string action, Dictionary<int, Dictionary<string, string>> xmlData)
        {

            if (action != "get-queue")
            {
                if (String.IsNullOrEmpty(this.targetType))
                {
                    throw new Exception("Target type is null");
                }
                this.getServer(this.targetTypeOptions[this.targetType]);
            }

            using (var wb = new System.Net.WebClient())
            {
                var data = new System.Collections.Specialized.NameValueCollection();
                data["queue"] = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><queue>" + this.getDic2Xml(xmlData) + "</queue>";
                var response = wb.UploadValues(OnlineConvert.URL + "/" + action, "POST", data);
                string result = System.Text.Encoding.UTF8.GetString(response);
                return result;
            }
        }


        public string getDic2Xml(Dictionary<int, Dictionary<string, string>> xmlDic)
        {
            var fileDic = xmlDic[OnlineConvert.FILE];
            var commandDic = xmlDic[OnlineConvert.COMMAN];
            string xmlString = this.getDic2Xml(commandDic);
            xmlString += "<file>" + this.getDic2Xml(fileDic) + "</file>";

            return xmlString;
        }

        public Dictionary<int, Dictionary<string, string>> getXml2Dic(string xmlData)
        {
            Dictionary<int, Dictionary<string, string>> dic2 = new Dictionary<int, Dictionary<string, string>>();

            var data = System.Xml.Linq.XElement.Parse(xmlData);
            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            Dictionary<string, string> dicStatus = new Dictionary<string, string>();
            foreach (var a in data.Elements())
            {
                foreach (var b in a.Elements())
                {
                    if (a.Name.LocalName == "params")
                    {
                        dicParams.Add(b.Name.LocalName, b.Value.ToString());
                    }
                    if (a.Name.LocalName == "status")
                    {
                        dicStatus.Add(b.Name.LocalName, b.Value.ToString());
                    }
                }
            }

            dic2.Add(1, dicParams);
            dic2.Add(2, dicStatus);

            return dic2;
        }
    }
}