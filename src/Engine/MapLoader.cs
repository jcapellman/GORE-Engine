using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GORE.Engine
{
    public class MapData
    {
        public int[,] Grid { get; set; }
        public Vector2 PlayerStart { get; set; }
        public Dictionary<int, string> TextureMapping { get; set; }
    }

    public class MapLoader
    {
        public static async Task<MapData> LoadMapAsync(string mapPath, string textureCfgPath)
        {
            var mapData = new MapData
            {
                PlayerStart = new Vector2(2.5f, 2.5f),
                TextureMapping = new Dictionary<int, string>()
            };

            // Load texture mapping first
            if (File.Exists(textureCfgPath))
            {
                var textureLines = await File.ReadAllLinesAsync(textureCfgPath);
                foreach (var line in textureLines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                        continue;

                    var parts = line.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int id))
                    {
                        mapData.TextureMapping[id] = parts[1].Trim();
                    }
                }
            }

            // Load map grid
            if (!File.Exists(mapPath))
            {
                throw new FileNotFoundException($"Map file not found: {mapPath}");
            }

            var lines = await File.ReadAllLinesAsync(mapPath);
            var gridLines = lines.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#')).ToList();

            if (gridLines.Count == 0)
            {
                throw new InvalidDataException("Map file is empty or contains only comments");
            }

            int height = gridLines.Count;
            int width = gridLines[0].Split(',').Length;

            mapData.Grid = new int[height, width];

            for (int y = 0; y < height; y++)
            {
                var values = gridLines[y].Split(',');
                for (int x = 0; x < width && x < values.Length; x++)
                {
                    if (int.TryParse(values[x].Trim(), out int value))
                    {
                        mapData.Grid[y, x] = value;
                    }
                }
            }

            return mapData;
        }

        public static MapData CreateDefaultMap()
        {
            // Fallback map if file loading fails
            int[,] defaultGrid = new int[,]
            {
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,2,2,2,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
                {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,3,0,0,0,3,0,0,0,1},
                {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,2,2,0,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,4,0,0,0,0,1,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,4,0,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };

            return new MapData
            {
                Grid = defaultGrid,
                PlayerStart = new Vector2(2.5f, 2.5f),
                TextureMapping = new Dictionary<int, string>()
            };
        }
    }
}
