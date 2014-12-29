---------------------------------------------------------------------------------------------------------------------------------
-- Max G, 2014-2015
-- App Info
-- Will have more uses when I begin moving a majority of the code over to Lua.
---------------------------------------------------------------------------------------------------------------------------------

luanet.load_assembly("System.Net")

DATA =
{
  latestVersion = "1.00"; -- All copies locally store their version number. This is used to check if its outdated.
  twitterName = "@CloneTroper1019"; -- Used for error messages so people know where to contact me.
  meshScale = 10; -- The default scale of a ported model.
}

---------------------------------------------------------------------------------------------------------------------------------

http = luanet.import_type("System.Net.WebClient")()

function GetResource(fileName)
  local success,response = pcall(function ()
    local get = http:DownloadString("https://raw.githubusercontent.com/CloneTrooper1019/Rbx2Source/master/"..fileName)
    if get == "Not Found" then
      error("Could not load " .. fileName .."!")
    else
      return get
    end
  end
end

function GetSetting(key)
  if DATA[key] then
    return tostring(DATA[key])
  else
    error("attempt to index non-existant setting '"..key..'")
  end
end

---------------------------------------------------------------------------------------------------------------------------------
