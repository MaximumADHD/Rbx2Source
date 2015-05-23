using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;
namespace RobloxToSourceEngine
{
    class GameDataManager
    {
        public string GetSaveFolder()
        {
            string appData = Environment.GetEnvironmentVariable("AppData");
            string saveData = Path.Combine(appData, "Rbx2SrcFiles","application","config");
            if (!Directory.Exists(saveData))
            {
                Directory.CreateDirectory(saveData);
            }
            return saveData;
        }

        public string GetConfigValue(string key, string defaultValue = "")
        {
            string saveFolder = GetSaveFolder();
            string keyFile = Path.Combine(saveFolder, key);
            if (!File.Exists(keyFile))
            {
                SetConfigValue(key, defaultValue);
                return defaultValue;
            }
            else
            {
                return File.ReadAllText(keyFile);
            }
        }

        public void SetConfigValue(string key, string value)
        {
            string currentKey = GetConfigValue(key);
            if (!currentKey.Equals(value))
            {
                string saveFolder = GetSaveFolder();
                string keyFile = Path.Combine(saveFolder, key);
                File.WriteAllText(keyFile, value);
                Console.WriteLine("Saved Key '" + key + "' as '" + value + "'");
            }
        }

        public List<NameValueCollection> GetGameData()
        {
            List<NameValueCollection> games = new List<NameValueCollection>();
            try
            {
                string gameData = GetConfigValue("GameData","[]");
                ListDictionary gameDump = JsonConvert.DeserializeObject<ListDictionary>(gameData);
                foreach (DictionaryEntry entry in gameDump)
                {
                    string json = (string)entry.Value;
                    ListDictionary data = JsonConvert.DeserializeObject<ListDictionary>(json);
                    NameValueCollection game = new NameValueCollection();
                    foreach (DictionaryEntry dataPair in data)
                    {
                        game.Add((string)dataPair.Key, (string)dataPair.Value);
                    }
                    string name = game["Name"];
                    if (name != null)
                    {
                        games.Add(game);
                    }
                }
            }
            catch {} // I don't need to do anything with this, but its required. 
                     // A protected call function would be nice.
            return games;
        }

        public void Save(string json)
        {
            SetConfigValue("GameData", json);
        }

        public string GameDataToJSON(List<NameValueCollection> GameData)
        {
            // This is basically the same as the GetGameData function, but done in a reverse process.
            ListDictionary gameDump = new ListDictionary();
            foreach (NameValueCollection game in GameData)
            {
                string packKey = gameDump.Count.ToString();
                ListDictionary packedGame = new ListDictionary();
                for (int i = 0; i < game.Count; i++ )
                {
                    string key = game.GetKey(i);
                    string value = game[i];
                    packedGame.Add(key, value);
                } 
                string packJson = JsonConvert.SerializeObject(packedGame);
                gameDump.Add(packKey,packJson);
            }
            string gameJson = JsonConvert.SerializeObject(gameDump);
            return gameJson;
        }

        public NameValueCollection GetGameInfo(List<NameValueCollection> GameData, string name)
        {
            foreach (NameValueCollection game in GameData)
            {
                if (game["Name"].Equals(name))
                {
                    return game;
                }
            }
            // WARNING: UGLY HACK BELOW
            // Someone needs to tell me how to avoid the "not all code paths return a value" issue.
            // Lua doesn't have problems like this because it doesn't require precise identification. 
            // Its lightweight :P
            NameValueCollection error = new NameValueCollection();
            error.Add("ERROR", "");
            return error;
        }

        public string IdentifyGameName(string gameInfo)
        {
            /*/
                Too lazy to write a Valve KeyValue parser
                Just read this to understand whats going on:
                https://developer.valvesoftware.com/wiki/Gameinfo.txt
            /*/
            string gameName = "";
            try
            {
                bool foundGame = false;
                while (foundGame != true)
                {
                    gameInfo = gameInfo.Substring(1);
                    if (gameInfo.StartsWith("game") || gameInfo.StartsWith("\"game\""))
                    {
                        foundGame = true;
                        if (gameInfo.StartsWith("\"game\""))
                        {
                            gameInfo = gameInfo.Substring(6);
                        }
                    }
                    if (foundGame)
                    {
                        bool foundQuote1 = false;
                        bool foundQuote2 = false;
                        while (foundQuote1 == false || foundQuote2 == false)
                        {
                            gameInfo = gameInfo.Substring(1);
                            string chunk = gameInfo.Substring(0, 1);
                            if (chunk == "\"")
                            {
                                if (foundQuote1 != true)
                                {
                                    foundQuote1 = true;
                                }
                                else if (foundQuote2 != true)
                                {
                                    foundQuote2 = true;
                                }
                            }
                            else
                            {
                                if (foundQuote1)
                                {
                                    gameName = gameName + chunk;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                gameName = "ERROR";
            }
            return gameName;
        }

        public bool NeedsInit(NameValueCollection data)
        {
            return data["ERROR"] != null;
        }

        public void PushChange(List<NameValueCollection> GameData, string gameName, string gameInfoDir, string studioMdlDir)
        {
            NameValueCollection game = this.GetGameInfo(GameData,gameName);
            if (this.NeedsInit(game))
            {
                game = new NameValueCollection();
                game.Add("Name", gameName);
                game.Add("GameInfoDir", gameInfoDir);
                game.Add("StudioMdlDir", studioMdlDir);
                GameData.Add(game);
            }
            else
            {
                game.Set("Name", gameName);
                game.Set("GameInfoDir", gameInfoDir);
                game.Set("StudioMdlDir", studioMdlDir);
            }
            this.Save(this.GameDataToJSON(GameData));
        }

        public void RemoveGameInfo(string gameName)
        {
            List<NameValueCollection> GameData = this.GetGameData();
            NameValueCollection game = this.GetGameInfo(GameData,gameName);
            if (this.NeedsInit(game) != true)
            {
                GameData.Remove(game);
                this.Save(this.GameDataToJSON(GameData));
            }
        }
    }
}
