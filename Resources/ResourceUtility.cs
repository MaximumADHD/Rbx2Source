using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Rbx2Source.Assembler;

namespace Rbx2Source.Resources
{
    static class ResourceUtility
    {
        private static readonly string group;
        private static readonly Assembly rbx2Source;
        private static readonly List<string> manifestResourceNames;
        private static readonly Dictionary<string, byte[]> resourceCache;
        
        static ResourceUtility()
        {
            rbx2Source = Assembly.GetExecutingAssembly();
            manifestResourceNames = new List<string>(rbx2Source.GetManifestResourceNames());
            resourceCache = new Dictionary<string, byte[]>();

            Type program = typeof(Program);
            group = program.Namespace;
        }

        public static List<string> GetFiles(string localDirectory)
        {
            var files = new List<string>();
            string baseDir = group + ".Resources.";
            string lDirManifest = baseDir + localDirectory.Replace("/", ".");
            
            foreach (string file in manifestResourceNames)
            {
                if (file.StartsWith(lDirManifest, StringComparison.InvariantCulture))
                {
                    string fileName = localDirectory + "/" + file.Replace(lDirManifest + ".", "");
                    files.Add(fileName);
                }
            }

            return files;
        }

        public static byte[] GetResource(string localPath)
        {
            if (resourceCache.ContainsKey(localPath))
            {
                return resourceCache[localPath];
            }
            else
            {
                string[] traversal = (group + "/Resources/" + localPath).Split('/');
                string path = string.Join(".", traversal);
                
                using (Stream resource = rbx2Source.GetManifestResourceStream(path))
                {
                    byte[] result;

                    if (resource == null)
                        throw new Exception("Resource " + path + " does not exist!\nCheck if the resource was embedded properly.");
                    else
                        result = FileUtility.ReadFullStream(resource);

                    resourceCache[localPath] = result;
                    return result;
                }
            }
        }
    }
}
