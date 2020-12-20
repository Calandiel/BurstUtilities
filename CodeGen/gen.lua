local types = require 'types'
local f = require "file-utils"

local allowed_types = {
	"long", "int", "short", "sbyte",
	"ulong", "uint", "ushort", "byte",
	"double", "float", "bool", "bit", -- bit is a "bool" that takes exactly one bit.
	-- Collections
	"HashSet", "Dictionary", "List", "Array"
	-- Additionally, any Type name is allowed too
}


print("--- CODEGEN START ---")
f:Open()
f:Line("using UnityEngine;")
f:Line("using System;")
f:Line("using Calandiel.Collections;")


f:Close()
print("--- CODEGEN END ---")