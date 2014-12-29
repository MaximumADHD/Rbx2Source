----------------------------------------------------------------------------------------------------------------------------------------------
-- Max G, 2014
-- This code is in charge of pulling .obj files from roblox.com and outputting them as .smd files.
-- Yes... I could write this in C#, but Lua's table system is WAYYYYY MORE FLEXABLE, and I can use string patterns <3
-- Syntax Highlighting is available here (in case your eyes are strained by this c:)
-- http://pastebin.com/qNXEZdD1
----------------------------------------------------------------------------------------------------------------------------------------------
-- Some manually written Roblox API ports.
----------------------------------------------------------------------------------------------------------------------------------------------

function print(...)
	local Console = luanet.import_type("System.Console")
	for _,v in pairs{...} do
		Console.WriteLine(v)
	end
end

Vector3 = {} -- Vector3 ( Partially based on: http://wiki.roblox.com/index.php?title=Vector3 )

function Vector3.IsVector(otherVec)
	local is = pcall(function ()
		-- If this is legitimate, it should concatenate without a problem.
		return otherVec.X .. otherVec.Y .. otherVec.Z
	end)
	return is
end

function Vector3.new(x,y,z)
	local x,y,z = x or 0, y or 0, z or 0
	local meta = {}
	local function insert(k,v)
		meta["__"..k] = v
	end
	local vec = {X = x; Y = y; Z = z}
	local vecStr = function ()
		return vec.X .. ", " .. vec.Y .. ", " .. vec.Z
	end
	insert("newindex",error)
	insert("tostring",vecStr)
	insert("add",function (_,v)
		local x,y,z do
			if type(v) == "number" then
				x,y,z = v,v,v
			else
				assert(Vector3.IsVector(v))
				x,y,z = v.X,v.Y,v.Z
			end
		end
		return Vector3.new(vec.X+x,vec.Y+y,vec.Z+z)
	end)
	insert("sub",function (_,v)
		local x,y,z do
			if type(v) == "number" then
				x,y,z = v,v,v
			else
				assert(Vector3.IsVector(v))
				x,y,z = v.X,v.Y,v.Z
			end
		end
		return Vector3.new(vec.X-x,vec.Y-y,vec.Z-z)
	end)
	insert("mul",function (_,v)
		local x,y,z do
			if type(v) == "number" then
				x,y,z = v,v,v
			else
				assert(Vector3.IsVector(v))
				x,y,z = v.X,v.Y,v.Z
			end
		end
		return Vector3.new(vec.X*x,vec.Y*y,vec.Z*z)
	end)
	insert("div",function (_,v)
		local x,y,z do
			if type(v) == "number" then
				x,y,z = v,v,v
			else
				assert(Vector3.IsVector(v))
				x,y,z = v.X,v.Y,v.Z
			end
		end
		return Vector3.new(vec.X/x,vec.Y/y,vec.Z/z)
	end)		
	function vec:lerp(vec2,alpha)
		assert(Vector3.IsVector(vec2))
		assert(type(alpha) == "number")
		local nX = vec.X + (vec2.X - vec.X) * alpha
		local nY = vec.Y + (vec2.Y - vec.Y) * alpha
		local nZ = vec.Z + (vec2.Z - vec.Z) * alpha
		return Vector3.new(nX,nY,nZ)
	end
	setmetatable(vec,meta)
	return vec
end

----------------------------------------------------------------------------------------------------------------------------------------------
h = {} -- HttpService ( http://wiki.roblox.com/index.php?title=HttpService )

function h:GetAsync(url)
	if not loadedNet then
		loadedNet = true
		luanet.load_assembly("System.Net")
	end
	local http = luanet.import_type("System.Net.WebClient")()
	return http:DownloadString(url)
end

local JSON = loadstring(h:GetAsync("http://pastebin.com/raw.php?i=S7CmEtAy"))()
h.DecodeJSON = JSON.Decode
h.EncodeJSON = JSON.Encode

----------------------------------------------------------------------------------------------------------------------------------------------
-- Bone Structure
-- This is a predefined skeleton that roblox characters use.
----------------------------------------------------------------------------------------------------------------------------------------------
-- The key (Torso1, LeftArm1, etc) refers to a naming scheme that roblox uses in their meshes.
-- "Name" is the bone's name.
-- "Offset" is the bone's offset relative to the Torso's origin
-- "Link" is the reference index used to refer to that bone (used in the triangle/skeleton data)

bones = {
	Torso1 = 
	{
		Name = "Torso";
		Offset = Vector3.new();
		Link = 0;
	},
	LeftArm1 = 
	{
		Name = "Left Shoulder";
		Offset = Vector3.new(1,0.5,0);
		Link = 1;
	},
	RightArm1 = 
	{
		Name = "Right Shoulder";
		Offset = Vector3.new(-1,0.5,0);
		Link = 2;	
	},
	LeftLeg1 = 
	{
		Name = "Left Hip";
		Offset = Vector3.new(1,-1,0);
		Link = 3;
	},
	RightLeg1 = 
	{
		Name = "Right Hip";
		Offset = Vector3.new(-1,-1,0);
		Link = 4;
	},
	Head1 = 
	{
		Name = "Neck";
		Offset = Vector3.new(0,1,0);
		Link = 5;
	}
}

----------------------------------------------------------------------------------------------------------------------------------------------
-- OBJ/MTL Parser functions
----------------------------------------------------------------------------------------------------------------------------------------------
-- The best way I can explain these two functions is:
-- 
-- INPUT LINE:     v 0.123 0.234 0.345 1.0
--
-- RESULT:
--		TAG = "v"
-- 		ITEMS = {"0.123","0.234,"0.345,"1.0"}
--			
--	* PROCEED TO DO A SPECIFIC OPERATION ON THE ITEMS BASED ON THE TAG*
-- 
----------------------------------------------------------------------------------------------------------------------------------------------

meshScale = 10

function parseOBJ(objFile,origin)
	-- Parses an OBJ File into a data array.
	local origin = origin or Vector3.new()
	local obj = {
		Verts = {};
		Norms = {};
		Texs = {};
		Faces = {};
	}
	local currentMtl = "";
	local currentGroup = "root";
	for line in objFile:gmatch("[^\r\n]+") do
		if #line > 0 then
			local info = {}
			local tag = ""
			local process = ""
			local readChars = 0
			for char in line:gmatch(".") do
				readChars = readChars + 1
				if char == " " then
					if tag == "" then
						tag = process
					else
						table.insert(info,tonumber(process) or process)
					end
					process = ""
				else
					process = process .. char
					if readChars == #line then
						table.insert(info,tonumber(process) or process)
					end
				end
			end
			if tag == "usemtl" then
				currentMtl = info[1]
			elseif tag == "g" then
				local group = info[1]
				currentGroup = info[1]
			elseif tag == "v" then
				local vec = Vector3.new(unpack(info))
				vec = (vec - origin) * meshScale
				table.insert(obj.Verts,{vec.X,vec.Y,vec.Z})
			elseif tag == "vn" then
				local vec = Vector3.new(unpack(info))
				table.insert(obj.Norms,{vec.X,vec.Y,vec.Z})
			elseif tag == "vt" then
				table.insert(obj.Texs,info)
			elseif tag == "f" then
				local face = {
					Material = currentMtl;
					Group = currentGroup;
					Coords = {};
				}
				for _,pair in pairs(info) do
					local triangle = {}
					local v,t,n
					-- The face definition format has a weird pattern format. Can't really explain this.
					-- (%S+) is used to match an individual number using the Lua String Pattern algorithm stuff.
					if type(pair) == "number" then 
						v = tonumber(pair)
					elseif string.find(pair,"//") then
						v,n = string.match(pair,"(%S+)//(%S+)")
					else
						v,t,n = string.match(pair,"(%S+)/(%S+)/(%S+)")
						if not v or not t or not n then
							v,t = string.match(pair,"(%S+)/(%S+)")
						end
					end
					triangle.Vert = tonumber(v)
					triangle.Tex = tonumber(t)
					triangle.Norm = tonumber(n)	
					table.insert(face.Coords,triangle)
				end
				table.insert(obj.Faces,face)
			end
		end
	end
	return obj
end

function parseMTL(mtlFile)
	-- Parses an OBJ File into a data array.
	local mtl = {}
	local currentMtl = ""
	local dump
	for line in mtlFile:gmatch("[^\r\n]+") do
		if #line > 0 then
			local info = {}
			local tag = ""
			local process = ""
			local readChars = 0
			for char in line:gmatch(".") do
				readChars = readChars + 1
				if char == " " then
					if tag == "" then
						tag = process
					else
						table.insert(info,process)
					end
					process = ""
				else
					process = process .. char
					if readChars == #line then
						table.insert(info,process)
					end
				end
			end
			if tag == "newmtl" then
				if dump then
					table.insert(mtl,dump);
				end
				dump = {};
				dump.Material = info[1];
			elseif tag == "map_d" then
				dump.HashTex = info[1];
			end
		end
	end
	if dump then
		table.insert(mtl,dump)
	end
	return mtl;
end

----------------------------------------------------------------------------------------------------------------------------------------------
-- File Writer
-- A basic string writer which lets me add new lines without a hassle.
----------------------------------------------------------------------------------------------------------------------------------------------
function NewFileWriter()
	local file
	local writer = {}
	function writer:Add(...)
		for _,line in pairs{...} do
			if not file then
				-- No file yet? Just set the file to this line
				file = line
			else
				-- Add a new line below the current file.
				file = file .. "\n" .. line
			end
		end
	end
	function writer:Dump()
		return file
	end
	return writer
end

----------------------------------------------------------------------------------------------------------------------------------------------
-- Utility functions for writing the SMD data.
----------------------------------------------------------------------------------------------------------------------------------------------
function ridiculousJSONAsync(url,tag,decodeAgain)
	local t = h.DecodeJSON(h:GetAsync(url))
	local async = h:GetAsync(t[tag])
	return (decodeAgain and h.DecodeJSON(async) or async)
end

function float(num)
	-- Obj Files have some insanely low numbers sometimes.
	if math.floor(num) == num then
		return tostring(num)
	else
		local fix = math.floor(num*10000)/10000
		if fix == math.floor(fix) then
			return tostring(fix)
		else
			local fl = tostring(math.floor(fix))
			local str = tostring(fix)
			while (#str - #fl) < 7 do
				str = str .. "0"
			end
			return str
		end
	end
end

function calculateOrigin(obj,group)
	local x,y,z = {},{},{}
	for _,face in pairs(obj.Faces) do
		if face.Group == group then
			for _,coord in pairs(face.Coords) do
				local vert = obj.Verts[coord.Vert]
				table.insert(x,vert[1])
				table.insert(y,vert[2])
				table.insert(z,vert[3])
			end
		end
	end
	local function avg(dump)
		local a = 0
		for _,v in pairs(dump) do
			a = a + v
		end
		local total = #dump/2
		a = a / #dump
		return a
	end
	return Vector3.new(avg(x),avg(y),avg(z))
end

function debugPack(vec)
	local x,y,z = vec.X,vec.Y,vec.Z
	local debugFunc = loadstring(h:GetAsync("http://pastebin.com/raw.php?i=HWfVS3TE"))()
	return debugFunc(x,y,z)
end

function getTorsoCenter(torsoAsset)
	-- Calculates the actual center of the torso as an offset to the origin of the torso's bounding box 
	local data = ridiculousJSONAsync("http://roproxy.tk/asset-thumbnail-3d/json?assetId=" .. torsoAsset,"Url",true)
	local objFile = ridiculousJSONAsync("http://roproxy.tk/thumbnail/resolve-hash/"..data.obj,"Url")
	local origin do
		local a = Vector3.new(data.aabb.min.x,data.aabb.min.y,data.aabb.min.z)
		local b = Vector3.new(data.aabb.max.x,data.aabb.max.y,data.aabb.max.z)
		origin = (a+b)/2
	end
	local obj = parseOBJ(objFile)
	local setup = {}
	local groups = {}
	for _,face in pairs(obj.Faces) do
		if not setup[face.Group] then
			setup[face.Group] = true;
			table.insert(groups,face.Group)
		end
	end
	local function getRealName(name)
		if not setup["Player11"] then
			return name
		else
			local refs = {Head1 = 0, Torso1 = 1, LeftArm1 = 2, RightArm1 = 3, LeftLeg1 = 4, RightLeg1 = 5}
			local ref = refs[name]
			return (ref and "Player1" .. #groups - refs[name] or name)
		end
	end
	local leftArm = calculateOrigin(obj,getRealName("LeftArm1"))
	local rightArm = calculateOrigin(obj,getRealName("RightArm1"))
	local torsoOrigin = (leftArm+rightArm)/2
	local offset = (torsoOrigin - calculateOrigin(obj,getRealName("Torso1")))
	return offset * Vector3.new(1,-1,1)
end


function unwrap(this)
	local str = ""
	for _,v in pairs(this) do
		if str ~= "" then
			str = str .. " "
		end
		str = str .. float(v)
	end
	return str
end

function dumpVector3(v3)
	return float(v3.X).." "..float(v3.Y).." "..float(v3.Z)
end
----------------------------------------------------------------------------------------------------------------------------------------------
-- File Writing Functions
----------------------------------------------------------------------------------------------------------------------------------------------

function WriteCharacterSMD(userId)
	local data = ridiculousJSONAsync("http://www.roblox.com/avatar-thumbnail-3d/json?userId=" .. userId,"Url",true)
	local objFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/" .. data.obj,"Url")
	local origin do
		local a = Vector3.new(data.aabb.min.x,data.aabb.min.y,data.aabb.min.z)
		local b = Vector3.new(data.aabb.max.x,data.aabb.max.y,data.aabb.max.z)
		origin = (a+b)/2
	end
	local obj = parseOBJ(objFile,origin)
	local file = NewFileWriter()
	file:Add("version 1","nodes")
	for _,node in pairs(bones) do
		local stack = (node.Link == 0) and -1 or 0
		file:Add(node.Link .. [[ "]] .. node.Name .. [[" ]] .. stack)
	end
	file:Add("end","skeleton","time 0")
	local ignoreHash do
		local avatar = h:GetAsync("http://www.roblox.com/Asset/AvatarAccoutrements.ashx?userId=" .. userId)
		local gearId = string.match(avatar,"?id=(%d+)&equipped=1")
		if gearId then
			local gearData = ridiculousJSONAsync("http://www.roblox.com/asset-thumbnail-3d/json?assetId=" .. gearId,"Url",true)
			local hashTex = gearData.textures[1]
			if hashTex then
				ignoreHash = hashTex
			end
		end
	end
	-- Queue every material
	local mtlData = {}
	local mtlFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/"..data.mtl,"Url")
	local mtl = parseMTL(mtlFile)
	for _,material in pairs(mtl) do
		mtlData[material.Material] = material.HashTex
	end
	-- Queue every group.
	local setup = {}
	local groups = {}
	for _,face in pairs(obj.Faces) do
		if not setup[face.Group] then
			setup[face.Group] = true;
		end
	end
	for k in pairs(setup) do
		table.insert(groups,k)
	end
	local function getLink(group)
		-- For some really dumb reason, recent versions of Roblox's 3D thumbnails have been using a new naming scheme.
		-- Luckily its in a very specific order, so it can be decoded into the old format.
		if bones[group] then
			return bones[group].Link
		elseif string.find(group,"Player1") then
			local num = tonumber(string.match(group,"Player1(%d+)"))
			local ref = { [0] = "Head1",[1] = "Torso1",[2] = "LeftArm1",[3] = "RightArm1",[4] = "LeftLeg1",[5] = "RightLeg1" }
			if num then
				local key = ref[#groups-num]
				if key then
					return bones[key].Link
				end
			end
		end
		return 5
	end
	local function getRealName(name)
		-- If the new group naming scheme is present, this function translates 
		-- the old naming scheme into the new naming scheme.
		if not setup["Player11"] then
			return name
		else
			local refs = {Head1 = 0, Torso1 = 1, LeftArm1 = 2, RightArm1 = 3, LeftLeg1 = 4, RightLeg1 = 5}
			local ref = refs[name]
			return (ref and "Player1" .. #groups - refs[name] or name)
		end
	end
	local gearGroup do
		if not setup["Player11"] and ignoreHash then
			for i = 2,5 do
				if not setup["Handle"..i] then
					gearGroup = "Handle" .. (i-1)
					break
				end
			end
		end
	end
	local torsoCenter do
		local assets = {}
		local avatar = h:GetAsync("http://www.roblox.com/Asset/AvatarAccoutrements.ashx?userId="..userId)
		for id in string.gmatch(avatar,"/?id=(%d+)") do
			table.insert(assets,id)
		end
		local actualOrigin = Vector3.new()
		local torsoAsset
		for _,asset in pairs(assets) do
			local info = h.DecodeJSON(h:GetAsync("http://api.roblox.com/marketplace/productinfo?assetId=" .. asset))
			if info.AssetTypeId == 27 then
				torsoAsset = asset
				break
			end
		end
		if torsoAsset then
			torsoCenter = getTorsoCenter(torsoAsset)
		end
	end
	for name,data in pairs(bones) do
		name = getRealName(name)
		local o = (data.Offset * meshScale)
		if name == getRealName("Torso1") then
			o = o + calculateOrigin(obj,name)
			if torsoCenter then
				o = o - torsoCenter
				torsoCenter = o
			end
		end
		file:Add(data.Link .." " .. dumpVector3(o) .. " 0 0 0")
	end
	file:Add("end","triangles")
	for _,face in pairs(obj.Faces) do
		if mtlData[face.Material] ~= ignoreHash and face.Group ~= "Humanoidrootpart1" and face.Group ~= gearGroup then
			file:Add(face.Material)
			local link = getLink(face.Group)
			for _,coord in pairs(face.Coords) do
				local vert = obj.Verts[coord.Vert]
				local norm = obj.Norms[coord.Norm]
				local tex = obj.Texs[coord.Tex]
				if link == 2 and ignoreHash then
					local t = torsoCenter or Vector3.new()
					local midpoint = t - Vector3.new(meshScale,0,0)
					local v = Vector3.new(unpack(vert))
					local n = Vector3.new(unpack(norm))
					vert = debugPack(v)
					norm = debugPack(n)
				end
				file:Add(link .. " " .. unwrap(vert) .. " " .. unwrap(norm) .. " " .. unwrap(tex))
			end
		end
	end
	file:Add("end")
	if mtlData[ignoreHash] then
		mtlData[ignoreHash] = nil;
	end
	local data = {
		File = file:Dump();
		MtlData = mtlData;
		IsArmUp = "false";
	}
	return h.EncodeJSON(data)
end

function WriteAssetSMD(assetId)
	local data = ridiculousJSONAsync("http://www.roblox.com/asset-thumbnail-3d/json?assetId=" .. assetId,"Url",true)
	local objFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/" .. data.obj,"Url")
	local origin do
		local a = Vector3.new(data.aabb.min.x,data.aabb.min.y,data.aabb.min.z)
		local b = Vector3.new(data.aabb.max.x,data.aabb.max.y,data.aabb.max.z)
		origin = (a+b)/2
	end
	local obj = parseOBJ(objFile,origin)
	local file = NewFileWriter()
	file:Add("version 1","nodes","0 \"root\" -1","end","skeleton","time 0","0 0 0 0 0 0 0","end","triangles") 
	local mtlData = {}
	local mtlFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/"..data.mtl,"Url")
	local mtl = parseMTL(mtlFile)
	for _,material in pairs(mtl) do
		mtlData[material.Material] = material.HashTex
	end
	for _,face in pairs(obj.Faces) do
		file:Add(face.Material)
		for _,coord in pairs(face.Coords) do
			local Vert = obj.Verts[coord.Vert];
			local Norm = obj.Norms[coord.Norm];
			local Tex = obj.Texs[coord.Tex];
			file:Add("0 " .. unwrap(Vert) .. "  " .. unwrap(Norm) .. "  " .. unwrap(Tex))
		end
	end
	file:Add("end")
	local data = {
		File = file:Dump();
		MtlData = mtlData;
	}
	return h.EncodeJSON(data)
end

----------------------------------------------------------------------------------------------------------------------------------------------
