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
        public bool Visited { get; set; }
    }

    public class WorldMapTerrain
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public bool IsWalkable { get; set; }
        public int EncounterRate { get; set; }
    }
}
