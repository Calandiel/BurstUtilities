local data = require 'types'
local f = require "file-utils"

local allowed_types = {
	"long", "int", "short", "sbyte",
	"ulong", "uint", "ushort", "byte",
	"double", "float", "bool", "bit", -- bit is a "bool" that takes exactly one bit.
	-- Collections
	"HashSet", "Dictionary", "List", "Array"
	-- Additionally, any Type name is allowed too
}

local int_counter = 0;

print("--- CODEGEN START ---")
f:Open()
f:Line("using UnityEngine;")
f:Line("using System;")
f:Line("using Calandiel.Collections;")
f:Line("")

local types = data["types"]
for k,v in pairs(types) do
	-- 'v' is a table containing our data.
	local name = v["name"]
	local namespace = v["namespace"]
	local components = v["components"]
	local type = v["type"]

	if name == nil then print("ERROR! A TYPE HAS NO NAME!") end

	if namespace ~= nil then
		f:Line("namespace " .. namespace)
		f:OpenScope()
	end
	-- First order of business is to declare the SOA

	if namespace ~= nil then
		f:CloseScope()
	end
end

-- #############
-- ### ENUMS ###
-- #############
do
local enums = data["enums"]
f:Line("")
f:Line("#region ENUMS")
for k,e in pairs(enums) do
	local name = e["name"]
	local namespace = e["namespace"]
	local values = e["values"]

	if name == nil then error("ERROR! AN ENUM HAS NO NAME!") end
	if values == nil then error("ERROR! AN ENUM HAS NO VALUES ARRAY!") end
	if #values == 0 then error("ERROR! AN ENUM HAS NO VALUES!") end
	if namespace ~= nil then
		f:Line("namespace " .. namespace)
		f:OpenScope()
	end
	local es = "byte"
	if #values > 250 then es = "ushort" end
	f:Line("public enum " .. name .. " : " .. es)
	f:OpenScope()


	for _,v in pairs(values) do
		f:Line(v .. ",")
	end

	f:CloseScope()
	if namespace ~= nil then
		f:CloseScope()
	end
end
f:Line("#endregion // ENUMS")
end

f:Close()
print("--- CODEGEN END ---")