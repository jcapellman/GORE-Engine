using System.Collections.Generic;

namespace GORE.Models
{
    public class WorldMap
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<WorldMapLayer> Layers { get; set; } = new();
        public List<WorldMapLocation> Locations { get; set; } = new();
        public string BackgroundMusic { get; set; }
    }

    public class WorldMapLayer
    {
        public string Name { get; set; }
        public int[][] Tiles { get; set; }
    }

    public class WorldMapLocation
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; }
        public string Texture { get; set; }
        public int Width { get; set; } = 1;  // Default to 1 tile
        public int Height { get; set; } = 1; // Default to 1 tile
        public bool Visited { get; set; }
    }

    public class WorldMapTerrain
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Texture { get; set; }
        public string BattleBackground { get; set; }
        public bool IsWalkable { get; set; }
        public int EncounterRate { get; set; }
        public int EncounterZone { get; set; }
        public TileEffect Effect { get; set; }
    }

    public class TileEffect
    {
        public string Type { get; set; } // "water", "lava", "snow", etc.
        public float Speed { get; set; } = 1.0f;
        public float Intensity { get; set; } = 1.0f;
    }
}
