using System;
using System.IO;
using System.Threading.Tasks;

namespace GORE.Engine
{
    public class MapSystem
    {
        public MapData CurrentMap { get; private set; }

        public async Task<MapData> LoadInitialMapAsync(string mapName)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var mapPath = Path.Combine(baseDirectory, "gt1", "maps", $"{mapName}.map");
            CurrentMap = await MapLoader.LoadMapAsync(mapPath);
            return CurrentMap;
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

        }
    }
}