using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace GORE.Engine.Systems
{
    public class MapSystem
    {
        private readonly ResourceLoader _resourceLoader;
        public MapData CurrentMap { get; private set; }

        public MapSystem(ResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        public async Task<MapData> LoadInitialMapAsync(string mapName)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var mapPath = Path.Combine(baseDirectory, "gt1", "maps", $"{mapName}.map");
            CurrentMap = await MapLoader.LoadMapAsync(mapPath);
            return CurrentMap;
        }        
        
        public event Action<MapData> MapLoaded;

        /// <summary>
        /// Loads a map by name from the maps directory. Throws on error.
        /// </summary>
        public async Task<MapData> LoadMapByNameAsync(string mapName)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var mapPath = Path.Combine(baseDirectory, "gt1", "maps", $"{mapName}.map");
            var mapData = await MapLoader.LoadMapAsync(mapPath);
            CurrentMap = mapData;
            MapLoaded?.Invoke(mapData);
            return mapData;
        }

        /// <summary>
        /// Verifies that all textures referenced in the map exist on disk. Returns a list of missing textures.
        /// </summary>
        public List<string> VerifyTextures(MapData mapData)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var missingTextures = new List<string>();
            foreach (var texMapping in mapData.TextureMapping)
            {
                var texturePath = Path.Combine(baseDirectory, texMapping.Value);
                if (!File.Exists(texturePath))
                {
                    missingTextures.Add($"  Texture {texMapping.Key}: {texMapping.Value}");
                }
            }
            return missingTextures;
        }

        /// <summary>
        /// Checks if a given (x, y) position is walkable (not a wall or closed door).
        /// </summary>
        public bool IsWalkable(float x, float y)
        {
            if (CurrentMap == null || CurrentMap.Grid == null)
                return false;
            int mapX = (int)x;
            int mapY = (int)y;
            if (mapX < 0 || mapX >= CurrentMap.Width || mapY < 0 || mapY >= CurrentMap.Height)
                return false;
            int cellValue = CurrentMap.Grid[mapY, mapX];
            // 0 = empty, 5 = door (optionally allow if open), others = wall
            return cellValue == 0;
        }
    }
}