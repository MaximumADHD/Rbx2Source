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