----------------------------------------------------------------------------------------------------------------------------------------------
-- @ Max G, 2014-2015
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

if not Vector3 then
	require("Vector3")
end

local tonumber = tonumber
local table = table

local function readTags(file)
	local tags = {}
	for line in file:gmatch("[^\r\n]+") do
		if #line > 0 then
			local block = { Items = {} }
			for split in line:gmatch("([^ ]+)") do
				if not block.Tag then
					block.Tag = split
				else
					table.insert(block.Items,tonumber(split) or split)
				end
			end
			table.insert(tags,block)
		end
	end
	return tags
end

function parseOBJ(objFile,origin)
	local origin = origin or Vector3.new()
	local obj = {
		Verts = {};
		Norms = {};
		Texs = {};
		Faces = {};
	}
	local currentMtl = "";
	local currentGroup = "root";
	local tags = readTags(objFile)
	for _,v in pairs(tags) do
		local tag = v.Tag
		local info = v.Items
		if tag == "usemtl" then -- Current Material in use
			currentMtl = info[1]
		elseif tag == "g" then
			local group = info[1]
			currentGroup = info[1]
		elseif tag == "v" then -- A new Vert 
			local vec = Vector3.new(unpack(info))
			vec = (vec - origin) * meshScale
			table.insert(obj.Verts,{vec.X,vec.Y,vec.Z})
		elseif tag == "vn" then -- A new Normal
			local vec = Vector3.new(unpack(info))
			table.insert(obj.Norms,{vec.X,vec.Y,vec.Z})
		elseif tag == "vt" then -- A new Texture Coordinate
			table.insert(obj.Texs,info)
		elseif tag == "f" then -- A face for the mesh
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
	return obj
end

function parseMTL(mtlFile)
	local mtl = {}
	local tags = readTags(mtlFile)
	local currentMtl = ""
	local dump
	for _,v in pairs(tags) do
		local tag = v.Tag
		local info = v.Items
		if tag == "newmtl" then -- A new material
			if dump then
				table.insert(mtl,dump);
			end
			dump = {};
			dump.Material = info[1];
		elseif tag == "map_d" then -- A texture map for the mesh
			dump.HashTex = info[1];
		end
	end
	if dump then
		table.insert(mtl,dump)
	end
	return mtl;
end

----------------------------------------------------------------------------------------------------------------------------------------------
