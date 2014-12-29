----------------------------------------------------------------------------------------------------------------------------------------------
-- @ Max G, 2014-2015
-- File Writer
-- A basic string writer which lets me add new lines to a string without a hassle.
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