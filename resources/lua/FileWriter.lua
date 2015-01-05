----------------------------------------------------------------------------------------------------------------------------------------------
-- @ Max G, 2014-2015
-- File Writer
-- A basic string writer which lets me add new lines to a string without a hassle.
----------------------------------------------------------------------------------------------------------------------------------------------

function NewFileWriter()
	local file
	local writer = {}
	local lineQueue = {}
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
	function writer:Queue(...)
		for _,line in pairs{...} do
			table.insert(lineQueue,line)
		end
	end
	function writer:SortAndDump(sortFunc)
		table.sort(lineQueue,sortFunc)
		for _,line in pairs(lineQueue) do
			writer:Add(line)
		end
		lineQueue = {}
	end
	function writer:Dump()
		return file
	end
	return writer
end

----------------------------------------------------------------------------------------------------------------------------------------------
