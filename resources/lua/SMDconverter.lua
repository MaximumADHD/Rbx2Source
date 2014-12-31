----------------------------------------------------------------------------------------------------------------------------------------------
-- Max G, 2014
-- This code is in charge of pulling .obj files from roblox.com and outputting them as .smd files.
----------------------------------------------------------------------------------------------------------------------------------------------

import("System")
import("System.Net")

require("FileWriter")
require("Vector3")
require("ObjReader")
require("JSON")

----------------------------------------------------------------------------------------------------------------------------------------------

function print(...)
	for _,v in pairs{...} do
		Console.WriteLine(v)
	end
end

----------------------------------------------------------------------------------------------------------------------------------------------

local bones = 
{
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

local http = WebClient()

----------------------------------------------------------------------------------------------------------------------------------------------
-- Utility functions for writing the SMD data.
----------------------------------------------------------------------------------------------------------------------------------------------

function ridiculousJSONAsync(url,tag,decodeAgain)
	local t = JSON:DecodeJSON(http:DownloadString(url))
	local async = http:DownloadString(t[tag])
	return (decodeAgain and JSON:DecodeJSON(async) or async)
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

function getTorsoCenter(torsoAsset)
	-- Calculates the actual center of the torso as an offset to the origin of the torso's bounding box 
	local data = ridiculousJSONAsync("http://www.roblox.com/asset-thumbnail-3d/json?assetId=" .. torsoAsset,"Url",true)
	local objFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/"..data.obj,"Url")
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
	return offset * Vector3.new(1,-1,-1)
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
		local avatar = http:DownloadString("http://www.roblox.com/Asset/AvatarAccoutrements.ashx?userId=" .. userId)
		local gearId = string.match(avatar,"?id=(%d+)&equipped=1")
		if gearId then
			local gearData = ridiculousJSONAsync("http://www.roblox.com/asset-thumbnail-3d/json?assetId=" .. gearId,"Url",true)
			local hashTex = gearData.textures[1]
			if hashTex then
				ignoreHash = hashTex
			end
		end
	end
	print("Ignore Hash: " .. ignoreHash)
	-- Queue every material
	local mtlData = {}
	local mtlFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/"..data.mtl,"Url")
	local mtl = parseMTL(mtlFile)
	for _,material in pairs(mtl) do
		mtlData[material.Material] = material.HashTex
	end
	print("Materials list: " .. JSON:EncodeJSON(mtlData))
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
		local avatar = http:DownloadString("http://www.roblox.com/Asset/AvatarAccoutrements.ashx?userId="..userId)
		for id in string.gmatch(avatar,"/?id=(%d+)") do
			table.insert(assets,id)
		end
		local actualOrigin = Vector3.new()
		local torsoAsset
		for _,asset in pairs(assets) do
			local info = JSON:DecodeJSON(http:DownloadString("http://api.roblox.com/marketplace/productinfo?assetId=" .. asset))
			if info.AssetTypeId == 27 then
				torsoAsset = asset
				break
			end
		end
		if torsoAsset then
			torsoCenter = getTorsoCenter(torsoAsset)
		else
			print("Could not get torsoAsset")
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
	return JSON:EncodeJSON(data)
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
	return JSON:EncodeJSON(data)
end

----------------------------------------------------------------------------------------------------------------------------------------------