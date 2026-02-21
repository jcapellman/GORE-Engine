using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Numerics;

namespace GORE.Engine
{
    public class Renderer3D
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly RaycastEngine _raycastEngine;
        private readonly GORE.Engine.Systems.ResourceLoader _resourceLoader;

        public Renderer3D(int width, int height, RaycastEngine raycastEngine, GORE.Engine.Systems.ResourceLoader resourceLoader)
        {
            _screenWidth = width;
            _screenHeight = height;
            _raycastEngine = raycastEngine;
            _resourceLoader = resourceLoader;
        }

        // All Win2D rendering and resource management removed for OpenGL-only build
        public void Dispose() { }
    }
}
