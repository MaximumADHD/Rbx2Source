using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Rbx2Source.Compiler
{
    public class GameInfo
    {
        public string GameInfoPath = null;
        public string GameDirectory = null;
        public string RootDirectory = null;
        public string GameName = null;
        public string StudioMdlPath = null;
        public string HLMVPath;
        public Icon GameIcon;

        private static Dictionary<string, string> PREFERRED_GAMEINFO_DIRECTORIES = new Dictionary<string, string>()
        {
            {"Half-Life 2","hl2"}
        };

        public GameInfo(string path)
        {
            string fileName = System.IO.Path.GetFileName(path);
            if (fileName != "gameinfo.txt")
                throw new Exception("Expected gameinfo.txt file.");

            string gameInfo = File.ReadAllText(path);

            StringReader reader = new StringReader(gameInfo);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Replace("\t", "");
                line = line.TrimStart(' ');
                line = line.Replace("\"game\"", "game");
                if (line.StartsWith("game"))
                {
                    int firstQuote = line.IndexOf('"');
                    if (firstQuote > 0)
                    {
                        firstQuote++;
                        int lastQuote = line.IndexOf('"', firstQuote);
                        if (lastQuote > 0)
                        {
                            GameName = line.Substring(firstQuote, lastQuote - firstQuote);
                            break;
                        }
                    }
                }
            }

            if (GameName == null)
                throw new Exception("Invalid gameinfo.txt file: Couldn't identify the name of the game.");

            // Fix cases where Valve likes to SCREAM the name of their game.
            MatchCollection matches = Regex.Matches(GameName, "[A-Z]+");

            foreach (Match match in matches)
            {
                foreach (Group group in match.Groups)
                {
                    string allCapStr = group.ToString();
                    if (allCapStr.Length > 2)
                    {
                        string newStr = allCapStr.Substring(0,1) + allCapStr.Substring(1).ToLower();
                        GameName = GameName.Replace(allCapStr, newStr);
                    }
                }
            }

            GameName = GameName.Replace(" [Beta]", "");
            GameName = GameName.Replace(" Source", ": Source");
            GameName = GameName.Replace(" DM", ": Deathmatch");

            GameInfoPath = path;
            GameDirectory = Directory.GetParent(path).ToString();
            RootDirectory = Directory.GetParent(GameDirectory).ToString();

            if (PREFERRED_GAMEINFO_DIRECTORIES.ContainsKey(GameName))
            {
                string preferred = PREFERRED_GAMEINFO_DIRECTORIES[GameName];
                GameDirectory = Path.Combine(RootDirectory, preferred);
                GameInfoPath = Path.Combine(GameDirectory, "gameinfo.txt");
            }
            

            string binPath = Path.Combine(RootDirectory, "bin");
            if (Directory.Exists(binPath))
            {
                string studioMdlPath = Path.Combine(binPath, "studiomdl.exe");
                if (File.Exists(studioMdlPath))
                    StudioMdlPath = studioMdlPath;

                string hlmvPath = Path.Combine(binPath, "hlmv.exe");
                if (File.Exists(hlmvPath))
                    HLMVPath = hlmvPath;
            }

            string resources = Path.Combine(GameDirectory, "resource");
            if (Directory.Exists(resources))
            {
                string gameIco = Path.Combine(resources,"game.ico");
                if (File.Exists(gameIco))
                    GameIcon = Icon.ExtractAssociatedIcon(gameIco);
            }
            if (GameIcon == null)
            {
                foreach (string file in Directory.GetFiles(RootDirectory))
                {
                    if (file.EndsWith(".exe") || file.EndsWith(".ico"))
                    {
                        GameIcon = Icon.ExtractAssociatedIcon(file);
                        break;
                    }
                }
            }
        }

        public bool ReadyToUse => (GameInfoPath != null && StudioMdlPath != null);
    }
}
