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
        public string Name { get; set; } = "Unnamed Level";
        public int Width { get; set; }
        public int Height { get; set; }
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

            if (!File.Exists(mapPath))
            {
                throw new FileNotFoundException($"Map file not found: {mapPath}");
            }

            var lines = await File.ReadAllLinesAsync(mapPath);

            // Check if this is the new format (contains sections)
            bool isNewFormat = lines.Any(l => l.Trim().StartsWith("["));

            if (isNewFormat)
            {
                return await LoadMapWithMetadataAsync(mapPath, lines);
            }
            else
            {
                // Legacy format - load texture mapping from separate file
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
                var gridLines = lines.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#')).ToList();

                if (gridLines.Count == 0)
                {
                    throw new InvalidDataException("Map file is empty or contains only comments");
                }

                int height = gridLines.Count;
                int width = gridLines[0].Split(',').Length;

                mapData.Grid = new int[height, width];
                mapData.Width = width;
                mapData.Height = height;

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
        }

        private static async Task<MapData> LoadMapWithMetadataAsync(string mapPath, string[] lines)
        {
            var mapData = new MapData
            {
                PlayerStart = new Vector2(2.5f, 2.5f),
                TextureMapping = new Dictionary<int, string>()
            };

            string currentSection = "";
            var gridLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
                    continue;

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    continue;
                }

                switch (currentSection)
                {
                    case "Metadata":
                        ParseMetadata(trimmedLine, mapData);
                        break;
                    case "Textures":
                        ParseTexture(trimmedLine, mapData);
                        break;
                    case "Grid":
                        gridLines.Add(trimmedLine);
                        break;
                }
            }

            // Parse grid
            if (gridLines.Count == 0)
            {
                throw new InvalidDataException("Map file contains no grid data");
            }

            int height = gridLines.Count;
            int width = gridLines[0].Split(',').Length;

            mapData.Grid = new int[height, width];

            // Use metadata dimensions if specified, otherwise use actual grid size
            if (mapData.Width == 0) mapData.Width = width;
            if (mapData.Height == 0) mapData.Height = height;

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

        private static void ParseMetadata(string line, MapData mapData)
        {
            var parts = line.Split('=', 2);
            if (parts.Length != 2)
                return;

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            switch (key)
            {
                case "Name":
                    mapData.Name = value;
                    break;
                case "Width":
                    if (int.TryParse(value, out int width))
                        mapData.Width = width;
                    break;
                case "Height":
                    if (int.TryParse(value, out int height))
                        mapData.Height = height;
                    break;
                case "PlayerStartX":
                    if (float.TryParse(value, out float x))
                        mapData.PlayerStart = new Vector2(x, mapData.PlayerStart.Y);
                    break;
                case "PlayerStartY":
                    if (float.TryParse(value, out float y))
                        mapData.PlayerStart = new Vector2(mapData.PlayerStart.X, y);
                    break;
            }
        }

        private static void ParseTexture(string line, MapData mapData)
        {
            var parts = line.Split('=', 2);
            if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int id))
            {
                mapData.TextureMapping[id] = parts[1].Trim();
            }
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
