
local FILE = { }

function FILE:Open(str)
	self.file = io.open("exports.cs", "w+");
	self.scopeDepth = 0
end
function FILE:Close()
	self.file:close()
end

function FILE:Line(str)
	for i=0,self.scopeDepth,1 do
		self.file:write("\t")
	end
	self.file:write(str)
	self.file:write("\n")
end

function FILE:OpenScope()
	self.scopeDepth = self.scopeDepth + 1
	self:Line("{")
end
function FILE:OpenScope()
	self.scopeDepth = self.scopeDepth - 1
	self:Line("}")
end

return FILE