using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using LuaInterface;

namespace RobloxToSourceEngine
{
    public class LuaClass : Lua
    {
        public event EventHandler<MessageOutEventArgs> MessageOut;
        private FileHandler FileHandler = new FileHandler();
        private bool loaded = false;

        private MethodBase GetMethodInfo(string methodName)
        {
            MethodBase info = this.GetType().GetMethod(methodName);
            return info;
        }

        public void require(string lib)
        {
            string contents = FileHandler.GetResource("lua/" + lib + ".lua");
            this.DoString(contents);
        }

        public void load(string path)
        {
            if (!loaded)
            {
                path = path.Replace("/", "\\");
                string file = FileHandler.GetResource(path);
                this.DoString(file);
                loaded = true;
            }
        }

        public void log(string text)
        {
            if (MessageOut != null)
            {
                MessageOut(this, new MessageOutEventArgs(text));
            }
        }

        public string downloadString(string url)
        {
            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
            GZipStream responseStream = new GZipStream(client.OpenRead(url), CompressionMode.Decompress);
            StreamReader reader = new StreamReader(responseStream);
            return reader.ReadToEnd();
        }

        public LuaClass()
        {
            require("CLRPackage");
            RegisterFunction("require", this, GetMethodInfo("require"));
            RegisterFunction("log", this, GetMethodInfo("log"));
            RegisterFunction("downloadString", this, GetMethodInfo("downloadString"));
        }
    }
}
