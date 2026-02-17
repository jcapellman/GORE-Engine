using System.Collections.Generic;
using System.Numerics;

namespace GORE.Engine
{
    public class MapData
    {
        public int[,] Grid { get; set; }
        public Vector2 PlayerStart { get; set; }
        public Dictionary<int, string> TextureMapping { get; set; }
        public string Name { get; set; } = "Unnamed Level";
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
