--[[
This file contains definitions for the codegen to, well, generate.


--]]
return {
	types = {
		{
			name = "Tile",
			namespace = "Apocrypha.Data",
			-- Each component is it's own array in the SOA
			components = {
				{
					vars = {
						{
							type = "int",
							name = "elevation"
						},
						{
							type = "BIOME",
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
			name = "BIOME",
			namespace = "Apocrypha.Data",
			values = {
				"SAVANNA", "GLACIER"
			}
		}
	},
	structs = {

	}

}