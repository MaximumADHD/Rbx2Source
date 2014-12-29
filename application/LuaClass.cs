using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using LuaInterface;

namespace RobloxToSourceEngine
{
    // This is a minor expansion to the LuaInterface.Lua class.
    // Imports the CLRPackage library, so we can use the "using" statement.
    // Also lets the lua script quickly load resource files from the github repository.
    class LuaClass : Lua
    {
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

        public LuaClass()
        {
            require("CLRPackage");
            RegisterFunction("require", this, GetMethodInfo("require"));
        }
    }
}
