using System;
using System.Threading.Tasks;
using GORE.Engine.Renderers;

namespace GORE.Engine
{
    /// <summary>
    /// Wrapper/factory for IRenderer, selects implementation based on config.
    /// </summary>
    public class RendererSystem : IDisposable
    {
        private IRenderer _renderer;
        private readonly Systems.ResourceLoader _resourceLoader;
        private string _rendererType;

        public RendererSystem(Systems.ResourceLoader resourceLoader, string rendererType = "OpenGL")
        {
            _resourceLoader = resourceLoader;
            _rendererType = rendererType;
        }

        public void Initialize(int width, int height, RaycastEngine raycastEngine)
        {
            _renderer = new OpenGLRenderer(_resourceLoader);
            _renderer.Initialize(width, height, raycastEngine);
        }

        public void InitializeResources(object device, int canvasWidth, int canvasHeight)
        {
            _renderer?.InitializeResources(device, canvasWidth, canvasHeight);
        }

        public object GetRenderTarget() => _renderer?.GetRenderTarget();


        public void Render(object drawingSession, float width, float height)
        {
            _renderer?.Render(drawingSession, width, height);
        }

        public void Render(object drawingSession, float width, float height, float dt)
        {
            // Only OpenGLRenderer supports dt, so cast and call
            if (_renderer is OpenGLRenderer ogl)
                ogl.Render(drawingSession, width, height, dt);
            else
                _renderer?.Render(drawingSession, width, height);
        }

        public Task LoadTextureAsync(int id, string path, object device)
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
