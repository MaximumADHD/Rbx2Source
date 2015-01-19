----------------------------------------------------------------------------------------------------------------------------------------------
-- @ Max G, 2014-2015
-- File Writer
-- A basic string writer which lets me add new lines to a string without a hassle.
----------------------------------------------------------------------------------------------------------------------------------------------

function NewFileWriter()
	local writer = {}
	local lines = {}
	local lineQueue = {}
	function writer:Add(...)
		for _,line in pairs{...} do
			table.insert(lines,line)
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
		return table.concat(lines,"\n")
	end
	return writer
end

----------------------------------------------------------------------------------------------------------------------------------------------
