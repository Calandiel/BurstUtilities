--[[
This file contains definitions for the codegen to, well, generate.


--]]
return {
	types = {
		{
			name = "ExampleType",
			namespace = "Example.Namespace",
			-- Each component is it's own array in the SOA
			components = {
				{
					vars = {
						{
							type = "int",
							name = "example_variable"
						},
						{
							type = "ExampleEnum",
							name = "example_enum_variable"
						}
					}
					-- checksum <- should this be included in checksum calculations? Default: false
					-- saved <- should this variable be saved? Default: true
				}
			},
			type = {
				-- "dynamic" <- entities that can be created and destroyed. Characters, units.
				-- "data" <- entities that are loaded once at game start from game files and are used by other entities. Recipes, unit definitions.
				-- "static" <- entities that always exist. Tiles.
				-- "struct" <- a helper structure
				"static"
			}
		}
	},
	enums = {
		{
			name = "ExampleEnum",
			namespace = "Example.Namespace",
			values = {
				"EXAMPLE_1", "EXAMPLE_2"
			}
		}
	},
	structs = {
		{
			name = "TESTstruct",
			namespace = "Apocrypha.Data",
			vars = {
				{

				}
			}
		}
	}

}