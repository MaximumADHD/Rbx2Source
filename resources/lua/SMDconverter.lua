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

local floor = math.floor
local sqrt = math.sqrt
local tostring = tostring
local tonumber = tonumber
local pairs = pairs
local table = table

----------------------------------------------------------------------------------------------------------------------------------------------

local function print(...)
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

local function ridiculousJSONAsync(url,tag,decodeAgain)
	local t = JSON:DecodeJSON(http:DownloadString(url))
	local async = http:DownloadString(t[tag])
	return (decodeAgain and JSON:DecodeJSON(async) or async)
end

local function avg(dump)
	local a = 0
	for _,v in pairs(dump) do
		a = a + v
	end
	a = a / #dump
	return a
end

local function calculateCentroid(obj,group)
	local x,y,z = {},{},{}
	for _,face in pairs(obj.Faces) do
		if not group or face.Group == group then
			for _,coord in pairs(face.Coords) do
				local vert = obj.Verts[coord.Vert]
				table.insert(x,vert[1]) 	
				table.insert(y,vert[2])
				table.insert(z,vert[3])
			end
		end
	end
	
	return Vector3.new(avg(x),avg(y),avg(z))
end

local function getGroupData(obj)
	local s = {}
	s.setup = {};
	s.groups = {};
	local centroidCache = {}
	for _,face in pairs(obj.Faces) do
		if not s.setup[face.Group] then
			s.setup[face.Group] = true;
			s.groups[#s.groups+1] = face.Group
		end
	end
	function s:GetLink(group)
		-- For some really dumb reason, recent versions of Roblox's 3D thumbnails have been using a new naming scheme.
		-- Luckily its in a very specific order, so it can be decoded into the old format.
		if bones[group] then
			return bones[group].Link
		elseif string.find(group,"Player1") then
			local num = tonumber(string.match(group,"Player1(%d+)"))
			local ref = { [0] = "Head1",[1] = "Torso1",[2] = "LeftArm1",[3] = "RightArm1",[4] = "LeftLeg1",[5] = "RightLeg1" }
			if num then
				local key = ref[#s.groups-num]
				if key then
					return bones[key].Link
				end
			end
		end
		return 5
	end
	function s:GetRealName(name)
		-- If the new group naming scheme is present, this function translates 
		-- the old naming scheme into the new naming scheme.
		if not s.setup.Player11 then
			return name
		else
			local refs = {Head1 = 0, Torso1 = 1, LeftArm1 = 2, RightArm1 = 3, LeftLeg1 = 4, RightLeg1 = 5}
			local ref = refs[name]
			return (ref and "Player1" .. #s.groups - refs[name] or name)
		end
	end
	function s:GetCentroid(name)
		local name = s:GetRealName(name)
		if not centroidCache[name] then
			centroidCache[name] = calculateCentroid(obj,name)
		end
		return centroidCache[name]
	end
	return s
end

local function float(num)
	-- Obj Files have some insanely low numbers sometimes.
	if floor(num) == num then
		return tostring(num)
	else
		local fix = floor(num*10000)/10000
		if fix == floor(fix) then
			return tostring(fix)
		else
			local fl = tostring(floor(fix))
			local str = tostring(fix)
			while (#str - #fl) < 7 do
				str = str .. "0"
			end
			return str
		end
	end
end

local function unwrap(this)
	local str = ""
	for _,v in pairs(this) do
		if str ~= "" then
			str = str .. " "
		end
		str = str .. float(v)
	end
	return str
end

local function dumpVector3(v3)
	return float(v3.X).." "..float(v3.Y).." "..float(v3.Z)
end

local function shouldFlipSkeleton(obj,torsoCenter)
	-- Recently, some meshes have been loading backwards.
	-- Roblox wtf are you doing lol.
	if not torsoCenter then
		torsoCenter = Vector3.new()
	end
	local groupData = getGroupData(obj)
	local bonePos = torsoCenter + (bones.LeftArm1.Offset * meshScale);
	local groupPos = groupData:GetCentroid("LeftArm1")
	local off = groupPos-bonePos
	local dist = sqrt(off.X^2+off.Y^2+off.Z^2)
	return dist > 15
end

local function getOrigin(data)
	local a = Vector3.new(data.aabb.min.x,data.aabb.min.y,data.aabb.min.z)
	local b = Vector3.new(data.aabb.max.x,data.aabb.max.y,data.aabb.max.z)
	return (a+b)/2	
end

local function getTorsoCenter(userId)
	local assets = {}
	local avatar = http:DownloadString("http://www.roblox.com/Asset/AvatarAccoutrements.ashx?userId="..userId)
	local torsoAsset
	for asset in string.gmatch(avatar,"/?id=(%d+)") do
		local info = JSON:DecodeJSON(http:DownloadString("http://api.roblox.com/marketplace/productinfo?assetId=" .. asset))
		if info.AssetTypeId == 27 then
			torsoAsset = asset
			break
		end
	end
	if torsoAsset then
		local data = ridiculousJSONAsync("http://www.roblox.com/asset-thumbnail-3d/json?assetId=" .. torsoAsset,"Url",true)
		local objFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/"..data.obj,"Url")
		local origin = getOrigin(data)
		local obj = parseOBJ(objFile)
		local groupData = getGroupData(obj)
		local leftArm = groupData:GetCentroid("LeftArm1")
		local rightArm = groupData:GetCentroid("RightArm1")
		local torsoOrigin = (leftArm+rightArm)/2
		local offset = (torsoOrigin - groupData:GetCentroid("Torso1")))
		return Vector3.new(-offset.X,-offset.Y,-offset.Z)
	else
		return Vector3.new()
	end
end

local function sortNumerical(a,b)
	local a = tonumber(string.match(a,"(%d+) "));
	local b = tonumber(string.match(b,"(%d+) "));
	return a < b
end

----------------------------------------------------------------------------------------------------------------------------------------------
-- File Writing Functions
----------------------------------------------------------------------------------------------------------------------------------------------

local function WriteCharacterSMD(userId)
	local data = ridiculousJSONAsync("http://www.roblox.com/avatar-thumbnail-3d/json?userId=" .. userId,"Url",true)
	local objFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/" .. data.obj,"Url")
	local origin = getOrigin(data)
	local obj = parseOBJ(objFile,origin)
	local file = NewFileWriter()
	file:Add("version 1","","nodes")
	for _,node in pairs(bones) do
		local stack = (node.Link == 0) and -1 or 0
		file:Queue(" "..node.Link .. [[ "]] .. node.Name .. [[" ]] .. stack)
	end
	file:SortAndDump(sortNumerical)
	local groupData = getGroupData(obj)
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
	local gearGroup do
		if not groupData.setup.Player11 and ignoreHash then
			for i = 2,5 do
				if not groupData.setup["Handle"..i] then
					gearGroup = "Handle" .. (i-1)
					break
				end
			end
		end
	end
	local mtlData = {}
	local mtlFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/"..data.mtl,"Url")
	local mtl = parseMTL(mtlFile)
	for _,material in pairs(mtl) do
		mtlData[material.Material] = material.HashTex
	end
	torsoCenter = getTorsoCenter(userId)
	if shouldFlipSkeleton(obj,torsoCenter) then
		-- Negate the XZ axis
		for i,vert in pairs(obj.Verts) do
			local vec = Vector3.new(unpack(vert)) * Vector3.new(-1,1,-1)
			obj.Verts[i] = {vec.X,vec.Y,vec.Z}
		end
		for i,norm in pairs(obj.Norms) do
			local vec = Vector3.new(unpack(norm)) * Vector3.new(-1,1,-1)
			obj.Norms[i] = {vec.X,vec.Y,vec.Z}
		end
		-- Recalculate the torso center
		torsoCenter = getTorsoCenter(userId)
	end
	file:Add("end","","skeleton","time 0")
	for name,data in pairs(bones) do
		name = groupData:GetRealName(name)
		local o = (data.Offset * meshScale)
		if name == groupData:GetRealName("Torso1") then
			o = o + calculateCentroid(obj,name)
			if torsoCenter then
				o = o - torsoCenter
			end
			torsoCenter = o
		end
		file:Queue(" "..data.Link .." " .. dumpVector3(o) .. " 0 0 0")
	end
	file:SortAndDump(sortNumerical)
	file:Add("end","","triangles")
	for _,face in pairs(obj.Faces) do
		if mtlData[face.Material] ~= ignoreHash and face.Group ~= "Humanoidrootpart1" and face.Group ~= gearGroup then
			file:Add(face.Material)
			local link = groupData:GetLink(face.Group)
			for index,coord in pairs(face.Coords) do
				local vert = obj.Verts[coord.Vert]
				local norm = obj.Norms[coord.Norm]
				local tex = obj.Texs[coord.Tex]
				if link == 2 and ignoreHash then
					-- This user was previously wearing a gear.
					-- We need to manually correct the arm.	
					local nx,ny,nz = unpack(norm)
					local v = Vector3.new(unpack(vert))
					local origin = torsoCenter + (bones.RightArm1.Offset * meshScale)
					local o = v-origin
					v = origin + Vector3.new(o.X,-o.Z,o.Y)
					vert = {v.X,v.Y,v.Z}
					norm = {nx,-nz,ny}
				end
				file:Add(" "..link .. " " .. unwrap(vert) .. " " .. unwrap(norm) .. " " .. unwrap(tex))
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
		IsArmUp = tostring(ignoreHash ~= nil);
	}
	return JSON:EncodeJSON(data)
end

local function WriteAssetSMD(assetId)
	local data = ridiculousJSONAsync("http://www.roblox.com/asset-thumbnail-3d/json?assetId=" .. assetId,"Url",true)
	local objFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/" .. data.obj,"Url")
	local origin = getOrigin(data)
	local obj = parseOBJ(objFile,origin)
	local file = NewFileWriter()
	file:Add("version 1","","nodes"," 0 \"root\" -1","end","","skeleton","time 0"," 0 0 0 0 0 0 0","end","","triangles") 
	local mtlData = {}
	local mtlFile = ridiculousJSONAsync("http://www.roblox.com/thumbnail/resolve-hash/"..data.mtl,"Url")
	local mtl = parseMTL(mtlFile)
	for _,material in pairs(mtl) do
		mtlData[material.Material] = material.HashTex
	end
	for _,face in pairs(obj.Faces) do
		file:Add(face.Material)
		for _,coord in pairs(face.Coords) do
			local vert = obj.Verts[coord.Vert];
			local norm = obj.Norms[coord.Norm];
			local tex = obj.Texs[coord.Tex];
			file:Add(" 0 " .. unwrap(vert) .. "  " .. unwrap(norm) .. "  " .. unwrap(tex))
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
