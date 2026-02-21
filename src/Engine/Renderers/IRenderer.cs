using System;
using System.Threading.Tasks;

namespace GORE.Engine.Renderers
{
    public interface IRenderer : IDisposable
    {
        void Initialize(int width, int height, RaycastEngine raycastEngine);
        void InitializeResources(object device, int canvasWidth, int canvasHeight);
        object GetRenderTarget();
        void Render(object drawingSession, float width, float height);
        Task LoadTextureAsync(int id, string path, object device);
    }
}