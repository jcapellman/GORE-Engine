using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace GORE.Engine
{
    /// <summary>
    /// Thin wrapper around Renderer3D to allow future separation and testing.
    /// </summary>
    public class RendererSystem : IDisposable
    {
        private Renderer3D _renderer;
        private RaycastEngine _raycastEngine;
        private readonly GORE.Engine.Systems.ResourceLoader _resourceLoader;

        public RendererSystem(GORE.Engine.Systems.ResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        public void Initialize(int width, int height, RaycastEngine raycastEngine)
        {
            _raycastEngine = raycastEngine;
            _renderer = new Renderer3D(width, height, raycastEngine, _resourceLoader);
        }

        public void InitializeResources(CanvasDevice device, int canvasWidth, int canvasHeight)
        {
            _renderer?.InitializeResources(device, canvasWidth, canvasHeight);
        }

        public object GetRenderTarget() => _renderer?.GetRenderTarget();

        public void Render(Microsoft.Graphics.Canvas.CanvasDrawingSession session, float width, float height)
        {
            _renderer?.Render(session, width, height);
        }

        public Task LoadTextureAsync(int id, string path, CanvasDevice device)
        {
            if (_renderer == null) throw new InvalidOperationException("Renderer not initialized");
            return _renderer.LoadTextureAsync(id, path, device);
        }

        public void Dispose()
        {
            _renderer?.Dispose();
        }
    }
}