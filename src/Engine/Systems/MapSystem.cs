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
        }
    }
}