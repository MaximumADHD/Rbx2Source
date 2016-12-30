using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rbx2Source.Assembler
{
    public struct BrickColor
    {
        public string Name;
        public int R;
        public int G;
        public int B;
        public BrickColor(string name, int r, int g, int b)
        {
            Name = name.Replace(".", "").Replace(" ", "_").ToLower();
            R = r;
            G = g;
            B = b;
        }
    }

    public static class BrickColors
    {
        #region AUTO GENERATED BRICKCOLOR LIST

        public static List<BrickColor> Palette = new List<BrickColor>() 
        {
	        new BrickColor("Earth green", 39, 70, 45),
	        new BrickColor("Slime green", 80, 109, 84),
	        new BrickColor("Bright bluish green", 0, 143, 156),
	        new BrickColor("Black", 27, 42, 53),
	        new BrickColor("Deep blue", 33, 84, 185),
	        new BrickColor("Dark blue", 0, 16, 176),
	        new BrickColor("Navy blue", 0, 32, 96),
	        new BrickColor("Parsley green", 44, 101, 29),
	        new BrickColor("Dark green", 40, 127, 71),
	        new BrickColor("Teal", 18, 238, 212),
	        new BrickColor("Smoky grey", 91, 93, 105),
	        new BrickColor("Steel blue", 82, 124, 174),
	        new BrickColor("Storm blue", 51, 88, 130),
	        new BrickColor("Lapis", 16, 42, 220),
	        new BrickColor("Dark indigo", 61, 21, 133),
	        new BrickColor("Camo", 58, 125, 21),
	        new BrickColor("Sea green", 52, 142, 64),
	        new BrickColor("Shamrock", 91, 154, 76),
	        new BrickColor("Toothpaste", 0, 255, 255),
	        new BrickColor("Sand blue", 116, 134, 157),
	        new BrickColor("Medium blue", 110, 153, 202),
	        new BrickColor("Bright blue", 13, 105, 172),
	        new BrickColor("Really blue", 0, 0, 255),
	        new BrickColor("Mulberry", 89, 34, 89),
	        new BrickColor("Forest green", 31, 128, 29),
	        new BrickColor("Bright green", 75, 151, 75),
	        new BrickColor("Grime", 127, 142, 100),
	        new BrickColor("Lime green", 0, 255, 0),
	        new BrickColor("Pastel blue-green", 159, 243, 233),
	        new BrickColor("Fossil", 159, 161, 172),
	        new BrickColor("Electric blue", 9, 137, 207),
	        new BrickColor("Lavender", 140, 91, 159),
	        new BrickColor("Royal purple", 98, 37, 209),
	        new BrickColor("Eggplant", 123, 0, 123),
	        new BrickColor("Sand green", 120, 144, 130),
	        new BrickColor("Moss", 124, 156, 107),
	        new BrickColor("Artichoke", 138, 171, 133),
	        new BrickColor("Sage green", 185, 196, 177),
	        new BrickColor("Pastel light blue", 175, 221, 255),
	        new BrickColor("Cadet blue", 159, 173, 192),
	        new BrickColor("Cyan", 4, 175, 236),
	        new BrickColor("Alder", 180, 128, 255),
	        new BrickColor("Lilac", 167, 94, 155),
	        new BrickColor("Plum", 123, 47, 123),
	        new BrickColor("Bright violet", 107, 50, 124),
	        new BrickColor("Olive", 193, 190, 66),
	        new BrickColor("Br. yellowish green", 164, 189, 71),
	        new BrickColor("Olivine", 148, 190, 129),
	        new BrickColor("Laurel green", 168, 189, 153),
	        new BrickColor("Quill grey", 223, 223, 222),
	        new BrickColor("Ghost grey", 202, 203, 209),
	        new BrickColor("Pastel Blue", 128, 187, 219),
	        new BrickColor("Pastel violet", 177, 167, 255),
	        new BrickColor("Pink", 255, 102, 204),
	        new BrickColor("Hot pink", 255, 0, 191),
	        new BrickColor("Magenta", 170, 0, 170),
	        new BrickColor("Crimson", 151, 0, 0),
	        new BrickColor("Deep orange", 255, 175, 0),
	        new BrickColor("New Yeller", 255, 255, 0),
	        new BrickColor("Medium green", 161, 196, 140),
	        new BrickColor("Mint", 177, 229, 166),
	        new BrickColor("Pastel green", 204, 255, 204),
	        new BrickColor("Light stone grey", 229, 228, 223),
	        new BrickColor("Light blue", 180, 210, 228),
	        new BrickColor("Baby blue", 152, 194, 219),
	        new BrickColor("Carnation pink", 255, 152, 220),
	        new BrickColor("Persimmon", 255, 89, 89),
	        new BrickColor("Really red", 255, 0, 0),
	        new BrickColor("Bright red", 196, 40, 28),
	        new BrickColor("Maroon", 117, 0, 0),
	        new BrickColor("Gold", 239, 184, 56),
	        new BrickColor("Bright yellow", 245, 205, 48),
	        new BrickColor("Daisy orange", 248, 217, 109),
	        new BrickColor("Cool yellow", 253, 234, 141),
	        new BrickColor("Pastel yellow", 255, 255, 204),
	        new BrickColor("Pearl", 231, 231, 236),
	        new BrickColor("Fog", 199, 212, 228),
	        new BrickColor("Mauve", 224, 178, 208),
	        new BrickColor("Sunrise", 212, 144, 189),
	        new BrickColor("Terra Cotta", 190, 104, 98),
	        new BrickColor("Dusty Rose", 163, 75, 75),
	        new BrickColor("Cocoa", 86, 36, 36),
	        new BrickColor("Neon orange", 213, 115, 61),
	        new BrickColor("Bright orange", 218, 133, 65),
	        new BrickColor("Wheat", 241, 231, 199),
	        new BrickColor("Buttermilk", 254, 243, 187),
	        new BrickColor("Institutional white", 248, 248, 248),
	        new BrickColor("White", 242, 243, 243),
	        new BrickColor("Light reddish violet", 232, 186, 200),
	        new BrickColor("Pastel orange", 255, 201, 201),
	        new BrickColor("Salmon", 255, 148, 148),
	        new BrickColor("Tawny", 150, 85, 85),
	        new BrickColor("Rust", 143, 76, 42),
	        new BrickColor("CGA brown", 170, 85, 0),
	        new BrickColor("Br. yellowish orange", 226, 155, 64),
	        new BrickColor("Cashmere", 211, 190, 150),
	        new BrickColor("Khaki", 226, 220, 188),
	        new BrickColor("Lily white", 237, 234, 234),
	        new BrickColor("Seashell", 233, 218, 218),
	        new BrickColor("Pastel brown", 255, 204, 153),
	        new BrickColor("Light orange", 234, 184, 146),
	        new BrickColor("Medium red", 218, 134, 122),
	        new BrickColor("Burgundy", 136, 62, 62),
	        new BrickColor("Reddish brown", 105, 64, 40),
	        new BrickColor("Cork", 188, 155, 93),
	        new BrickColor("Burlap", 199, 172, 120),
	        new BrickColor("Beige", 202, 191, 163),
	        new BrickColor("Oyster", 187, 179, 178),
	        new BrickColor("Mid gray", 205, 205, 205),
	        new BrickColor("Brick yellow", 215, 197, 154),
	        new BrickColor("Nougat", 204, 142, 105),
	        new BrickColor("Brown", 124, 92, 70),
	        new BrickColor("Pine Cone", 108, 88, 75),
	        new BrickColor("Fawn brown", 160, 132, 79),
	        new BrickColor("Sand red", 149, 121, 119),
	        new BrickColor("Hurricane grey", 149, 137, 136),
	        new BrickColor("Cloudy grey", 171, 168, 158),
	        new BrickColor("Linen", 175, 148, 131),
	        new BrickColor("Copper", 150, 103, 102),
	        new BrickColor("Dark orange", 160, 95, 53),
	        new BrickColor("Dirt brown", 86, 66, 54),
	        new BrickColor("Bronze", 126, 104, 63),
	        new BrickColor("Dark stone grey", 99, 95, 98),
	        new BrickColor("Medium stone grey", 163, 162, 165),
	        new BrickColor("Flint", 105, 102, 92),
	        new BrickColor("Dark taupe", 90, 76, 66),
	        new BrickColor("Burnt Sienna", 106, 57, 9),
	        new BrickColor("Really black", 17, 17, 17)
        };

        public static List<int> NumericalSearch = new List<int>() { 141, 301, 107, 26, 1012, 303, 1011, 304, 28, 1018, 302, 305, 306, 307, 308, 1021, 309, 310, 1019, 135, 102, 23, 1010, 312, 313, 37, 1022, 1020, 1027, 311, 315, 1023, 1031, 316, 151, 317, 318, 319, 1024, 314, 1013, 1006, 321, 322, 104, 1008, 119, 323, 324, 325, 320, 11, 1026, 1016, 1032, 1015, 327, 1017, 1009, 29, 328, 1028, 208, 45, 329, 330, 331, 1004, 21, 332, 333, 24, 334, 226, 1029, 335, 336, 342, 343, 338, 1007, 339, 133, 106, 340, 341, 1001, 1, 9, 1025, 337, 344, 345, 1014, 105, 346, 347, 348, 349, 1030, 125, 101, 350, 192, 351, 352, 353, 354, 1002, 5, 18, 217, 355, 356, 153, 357, 358, 359, 360, 38, 361, 362, 199, 194, 363, 364, 365, 1003 };

        #endregion
    }
}
