----------------------------------------------------------------------------------------------------------------------------------------------
-- @ Max G, 2014-2015
-- Vector3
-- An XYZ coordinate system
-- Partially based on http://wiki.roblox.com/index.php?title=Vector3
----------------------------------------------------------------------------------------------------------------------------------------------

Vector3 = {}

import("System")

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
				assert(Vector3.IsVector(v),"Error: " .. tostring(v) .. " isn't a vector lol")
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
	function vec:Cross(vec2)
		assert(Vector3.IsVector(vec2));
		local nX = (vec.Y * vec2.Z) - (vec.Z * vec2.Y)
		local nY = (vec.Z * vec2.X) - (vec.X * vec2.Z)
		local nZ = (vec.X * vec2.Y) - (vec.Y * vec2.X)
		return Vector3.new(nX,nY,nZ)
	end
	setmetatable(vec,meta)
	return vec
end

----------------------------------------------------------------------------------------------------------------------------------------------
