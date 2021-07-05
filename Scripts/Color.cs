using UnityEngine;

namespace Calandiel
{
	// For serialization cuz Color32 isn't serializable
	[System.Serializable]
	public struct CColor
	{
		public byte r;
		public byte g;
		public byte b;
		public byte a;

		public CColor(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; }
		public CColor(byte r, byte g, byte b) { this.r = r; this.g = g; this.b = b; this.a = 255; }

		public static bool operator ==(CColor a, CColor b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
		public static bool operator !=(CColor a, CColor b) => a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;
		public static implicit operator Color32(CColor d) => new Color32() { a = d.a, b = d.b, g = d.g, r = d.r };
		public static implicit operator CColor(Color32 d) => new CColor() { a = d.a, b = d.b, g = d.g, r = d.r };
		public static implicit operator Color(CColor q) => (Color32)q;
		public static implicit operator CColor(Color q) => (Color32)q;

		public static CColor ColorFromInt(int i)
		{
			i = i % 23;
			switch (i)
			{
				case 0:  return TAUPE_ROSE();
				case 1:  return CHESTNUT();
				case 2:  return RED();
				case 3:  return MAROON();
				case 4:  return RED_PURPLE();
				case 5:  return ROSE();
				case 6:  return VERMILION();
				case 7:  return RUSSET();
				case 8:  return SCARLET();
				case 9:  return BURNT_UMBER();
				case 10: return TAUPE_MEDIUM();
				case 11: return DARK_CHESTNUT();
				case 12: return BURNT_SIENNA();
				case 13: return RUST();
				case 14: return AUBURN();
				case 15: return MAHOGANY();
				case 16: return PUMPKIN();
				case 17: return CHOCOLATE();
				case 18: return TAUPE_PALE();
				case 19: return TAUPE_DARK();
				case 20: return DARK_PEACH();
				case 21: return COPPER();
				case 22: return LIGHT_BROWN();
				default: return CColor.BLACK();
			}
		}

		#region PREDEFINED COLORS
		// !!!
		// !!! Visual reference: http://dwarffortresswiki.org/index.php/40d:Color#Color_tokens
		// !!!
		public static Color32 TAUPE_ROSE() => new Color32(144, 93, 93, 255);
		public static Color32 CHESTNUT() => new Color32(205, 92, 92, 255);
		public static Color32 RED() => Color.red;
		public static Color32 MAROON() => new Color32(128, 0, 0, 255);
		public static Color32 RED_PURPLE() => new Color32(178, 0, 75, 255);
		public static Color32 ROSE() => new Color32(244, 194, 194, 255);
		public static Color32 VERMILION() => new Color32(227, 66, 52, 255);
		public static Color32 RUSSET() => new Color32(117, 90, 87, 255);
		public static Color32 SCARLET() => new Color32(255, 36, 0, 255);
		public static Color32 BURNT_UMBER() => new Color32(138, 51, 36, 255);
		public static Color32 TAUPE_MEDIUM() => new Color32(103, 76, 71, 255);
		public static Color32 DARK_CHESTNUT() => new Color32(152, 105, 96, 255);
		public static Color32 BURNT_SIENNA() => new Color32(233, 116, 81, 255);
		public static Color32 RUST() => new Color32(183, 65, 14, 255);
		public static Color32 AUBURN() => new Color32(111, 53, 26, 255);
		public static Color32 MAHOGANY() => new Color32(192, 64, 0, 255);
		public static Color32 PUMPKIN() => new Color(255, 117, 24, 255);
		public static Color32 CHOCOLATE() => new Color32(210, 105, 30, 255);
		public static Color32 TAUPE_PALE() => new Color32(188, 152, 126, 255);
		public static Color32 TAUPE_DARK() => new Color32(72, 60, 50, 255);
		public static Color32 DARK_PEACH() => new Color32(255, 218, 185, 255);
		public static Color32 COPPER() => new Color32(184, 115, 51, 255);
		public static Color32 LIGHT_BROWN() => new Color32(205, 133, 63, 255);
		public static Color32 BRONZE() => new Color32(205, 127, 50, 255);
		public static Color32 PALE_BROWN() => new Color32(152, 118, 84, 255);
		public static Color32 DARK_BROWN() => new Color32(101, 67, 33, 255);
		public static Color32 SEPIA() => new Color32(112, 66, 20, 255);
		public static Color32 OCHRE() => new Color32(204, 119, 34, 255);
		public static Color32 BROWN() => new Color32(150, 75, 0, 255);
		public static Color32 CINNAMON() => new Color32(123, 63, 0, 255);
		public static Color32 TAN() => new Color32(210, 180, 140, 255);
		public static Color32 RAW_UMBER() => new Color32(115, 74, 18, 255);
		public static Color32 ORANGE() => new Color32(115, 74, 18, 255);
		public static Color32 PEACH() => new Color32(255, 229, 180, 255);
		public static Color32 TAUPE_SANDY() => new Color32(150, 113, 23, 255);
		public static Color32 GOLDENRON() => new Color32(218, 165, 32, 255);
		public static Color32 AMBER() => new Color32(255, 191, 0, 255);
		public static Color32 DARK_TAN() => new Color32(145, 129, 81, 255);
		public static Color32 SAFFRON() => new Color32(244, 196, 48, 255);
		public static Color32 ECRU() => new Color32(194, 178, 128, 255);
		public static Color32 GOLD() => new Color32(212, 175, 55, 255);
		public static Color32 PEARL() => new Color32(240, 234, 214, 255);
		public static Color32 BUFF() => new Color32(240, 220, 130, 255);
		public static Color32 FLAX() => new Color32(238, 220, 130, 255);
		public static Color32 BRASS() => new Color32(181, 166, 66, 255);
		public static Color32 GOLDEN_YELLOW() => new Color32(255, 223, 0, 255);
		public static Color32 LEMON() => new Color32(253, 233, 16, 255);
		public static Color32 CREAM() => new Color32(255, 253, 208, 255);
		public static Color32 BEIGE() => new Color32(245, 245, 220, 255);
		public static Color32 OLIVE() => new Color32(128, 128, 0, 255);
		public static Color32 IVORY() => new Color32(255, 255, 240, 255);
		public static Color32 LIME() => new Color32(204, 255, 0, 255);
		public static Color32 YELLOW_GREEN() => new Color32(154, 205, 50, 255);
		public static Color32 DARK_OLIVE() => new Color32(85, 104, 50, 255);
		public static Color32 GREEN_YELLOW() => new Color32(173, 255, 47, 255);
		public static Color32 CHARTREUSE() => new Color32(127, 255, 0, 255);
		public static Color32 FERN_GREEN() => new Color32(79, 121, 66, 255);
		public static Color32 MOSS_GREEN() => new Color32(173, 223, 173, 255);
		public static Color32 MINT_GREEN() => new Color32(152, 255, 152, 255);
		public static Color32 ASH_GRAY() => new Color32(178, 190, 181, 255);
		public static Color32 EMERALD() => new Color32(80, 200, 120, 255);
		public static Color32 SEA_GREEN() => new Color32(46, 139, 87, 255);
		public static Color32 SPRING_GREEN() => new Color32(0, 255, 127, 255);
		public static Color32 DARK_GREEN() => new Color32(1, 50, 32, 255);
		public static Color32 JADE() => new Color32(0, 168, 107, 255);
		public static Color32 AQUAMARINE() => new Color32(127, 255, 212, 255);
		public static Color32 PINE_GREEN() => new Color32(1, 121, 111, 255);
		public static Color32 TURQUOISE() => new Color32(48, 213, 200, 255);
		public static Color32 PALE_BLUE() => new Color32(175, 238, 238, 255);
		public static Color32 TEAL() => new Color32(0, 128, 128, 255);
		public static Color32 LIGHT_BLUE() => new Color32(173, 216, 230, 255);
		public static Color32 CERULEAN() => new Color32(0, 123, 167, 255);
		public static Color32 SKY_BLUE() => new Color32(135, 206, 235, 255);
		public static Color32 CHARCOAL() => new Color32(54, 69, 79, 255);
		public static Color32 SLATE_GREY() => new Color32(112, 128, 144, 255);
		public static Color32 MIDNIGHT_BLUE() => new Color32(0, 51, 102, 255);
		public static Color32 AZURE() => new Color32(0, 127, 255, 255);
		public static Color32 COBALT() => new Color32(0, 71, 171, 255);
		public static Color32 LAVENDER() => new Color32(230, 230, 250, 255);
		public static Color32 DARK_BLUE() => new Color32(0, 0, 139, 255);
		public static Color32 PERIWINKLE() => new Color32(204, 204, 255, 255);
		public static Color32 DARK_VIOLET() => new Color32(66, 49, 137, 255);
		public static Color32 AMETHYST() => new Color32(153, 102, 204, 255);
		public static Color32 DARK_INDIGO() => new Color32(49, 0, 98, 255);
		public static Color32 VIOLET() => new Color32(139, 0, 255, 255);
		public static Color32 INDIGO() => new Color32(75, 0, 130, 255);
		public static Color32 PURPLE() => new Color32(102, 0, 153, 255);
		public static Color32 HELIOTROPE() => new Color32(223, 115, 255, 255);
		public static Color32 LILAC() => new Color32(200, 162, 200, 255);
		public static Color32 PLUM() => new Color32(102, 0, 102, 255);
		public static Color32 TAUPE_PURPLE() => new Color32(80, 64, 77, 255);
		public static Color32 TAUPE_GREY() => new Color32(139, 133, 137, 255);
		public static Color32 FUCHSIA() => new Color32(244, 0, 161, 255);
		public static Color32 MAUVE() => new Color32(153, 51, 102, 255);
		public static Color32 LAVENDER_BLUSH() => new Color32(255, 240, 245, 255);
		public static Color32 DARK_PINK() => new Color32(231, 84, 128, 255);
		public static Color32 MAUVE_TAUPE() => new Color32(145, 95, 109, 255);
		public static Color32 DARK_SCARLET() => new Color32(86, 3, 25, 255);
		public static Color32 PUCE() => new Color32(204, 136, 153, 255);
		public static Color32 CRIMSON() => new Color32(220, 20, 60, 255);
		public static Color32 PINK() => new Color32(255, 192, 203, 255);
		public static Color32 CARDINAL() => new Color32(196, 30, 58, 255);
		public static Color32 CARMINE() => new Color32(150, 0, 24, 255);
		public static Color32 PALE_PINK() => new Color32(250, 218, 221, 255);
		public static Color32 PALE_CHESTNUT() => new Color32(221, 173, 175, 255);
		public static Color32 MAGENTA() => Color.magenta;
		public static Color32 YELLOW() => Color.yellow;
		public static Color32 GREEN() => Color.green;
		public static Color32 BLUE() => Color.blue;
		public static Color32 CYAN() => Color.cyan;
		public static Color32 WHITE() => Color.white;
		public static Color32 SILVER() => new Color32(192, 192, 192, 255);
		public static Color32 GREY() => Color.grey;
		public static Color32 BLACK() => Color.black;

		#endregion



	}

}