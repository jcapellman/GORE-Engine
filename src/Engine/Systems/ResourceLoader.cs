using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GORE.Engine.Systems
{
    public class ResourceLoader : IDisposable
    {
        private readonly ConcurrentDictionary<string, CanvasBitmap> _textureCache = new();
        private readonly ConcurrentDictionary<string, FontFamily> _fontCache = new();
        private readonly ConcurrentDictionary<string, object> _jsonCache = new();

        public async Task<CanvasBitmap> LoadTextureAsync(string path, CanvasDevice device)
        {
            if (_textureCache.TryGetValue(path, out var cached))
                return cached;

            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture not found: {path}");

            using var stream = File.OpenRead(path);
            var bitmap = await CanvasBitmap.LoadAsync(device, stream.AsRandomAccessStream());
            _textureCache[path] = bitmap;
            return bitmap;
        }

        public FontFamily LoadFont(string path, string fontFamilyName)
        {
            if (_fontCache.TryGetValue(path, out var cached))
                return cached;

            if (!File.Exists(path))
                throw new FileNotFoundException($"Font not found: {path}");

            // XAML FontFamily expects a URI
            var fontFamily = new FontFamily($"ms-appx:///{path.Replace("\\", "/")}#{fontFamilyName}");
            _fontCache[path] = fontFamily;
            return fontFamily;
        }

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
            foreach (var tex in _textureCache.Values)
                tex.Dispose();
            _textureCache.Clear();
            _fontCache.Clear();
            _jsonCache.Clear();
        }
    }
}
