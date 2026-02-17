using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GORE.Engine
{
    // MapData moved to src/Engine/Data/MapData.cs

    public class MapLoader
    {
        /// <summary>
        /// Load a map from a .map file with embedded metadata and textures
        /// </summary>
        public static async Task<MapData> LoadMapAsync(string mapPath)
        {
            if (!File.Exists(mapPath))
            {
                throw new FileNotFoundException($"Map file not found: {mapPath}");
            }

            var lines = await File.ReadAllLinesAsync(mapPath);

            // Validate format
            bool hasMetadataSection = lines.Any(l => l.Trim().Equals("[Metadata]", StringComparison.OrdinalIgnoreCase));
            bool hasTexturesSection = lines.Any(l => l.Trim().Equals("[Textures]", StringComparison.OrdinalIgnoreCase));
            bool hasGridSection = lines.Any(l => l.Trim().Equals("[Grid]", StringComparison.OrdinalIgnoreCase));

            if (!hasMetadataSection || !hasTexturesSection || !hasGridSection)
            {
                throw new InvalidDataException(
                    "Invalid map format. Map must contain [Metadata], [Textures], and [Grid] sections. " +
                    "Legacy format is no longer supported.");
            }

            return ParseMapFile(lines);
        }

        private static MapData ParseMapFile(string[] lines)
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

                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
                    continue;

                // Check for section headers
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    continue;
                }

                // Parse based on current section
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

            // Parse grid data
            if (gridLines.Count == 0)
            {
                throw new InvalidDataException("Map file contains no grid data in [Grid] section");
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

            // Validate that we have texture mappings
            if (mapData.TextureMapping.Count == 0)
            {
                throw new InvalidDataException("Map file contains no texture definitions in [Textures] section");
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
