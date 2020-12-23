--[[
This file contains definitions for the codegen to, well, generate.


--]]
return {
	types = {
		{
			name = "ExampleType",
			namespace = "Example.Namespace",
			vars = {
				{
					type = "int",
					name = "example_variable"
					-- checksum <- should this be included in checksum calculations? Default: true
					-- saved <- should this variable be saved? Default: true
					-- public <- should this variable be public? Default: true
				},{
					type = "ExampleEnum",
					name = "example_enum_variable"
				},{
					type = "Dictionary",
					name = "example_dictionary",
					generic_1 = "int",
					generic_2 = "ExampleType",
					capacity = 100
				}
			},
			type = {
				-- "dynamic" <- entities that can be created and destroyed. Characters, units.
				-- "data" <- entities that are loaded once at game start from game files and are used by other entities. Recipes, unit definitions.
				-- "static" <- entities that always exist. Tiles.
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