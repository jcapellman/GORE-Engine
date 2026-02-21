using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;

namespace GORE.Engine.Systems
{
    public class ResourceLoader : IDisposable
    {
        private readonly ConcurrentDictionary<string, object> _jsonCache = new();

        public T LoadJson<T>(string path)
        {
            if (_jsonCache.TryGetValue(path, out var cached) && cached is T tCached)
                return tCached;

            if (!File.Exists(path))
                throw new FileNotFoundException($"JSON file not found: {path}");

            var jsonText = File.ReadAllText(path);
            var obj = JsonSerializer.Deserialize<T>(jsonText);
            _jsonCache[path] = obj;
            return obj;
        }

        public void Dispose()
        {
            _jsonCache.Clear();
        }
    }
}
