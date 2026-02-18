
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System;
using System.Threading.Tasks;
using GORE.Engine.Systems;

namespace GORE.Engine
{
    /// <summary>
    /// GORE Engine - Wolfenstein 3D-style raycasting FPS
    /// </summary>
    public static class GOREEngine
    {
        private static bool _initialized = false;

        /// <summary>
        /// Create and initialize a new GOREEngineInstance.
        /// </summary>
        public static async Task<GOREEngineInstance> CreateAndInitializeAsync(Action<string> log, Action<string> logError)
        {
            // Dependency injection: create shared ResourceLoader and all systems
            var resourceLoader = new Systems.ResourceLoader();
            var configSystem = new ConfigSystem(resourceLoader);
            configSystem.Load();
            var mapSystem = new MapSystem(resourceLoader);
            var weaponSystem = new WeaponSystem(resourceLoader);
            var dummyRaycast = new RaycastEngine(new int[1, 1], null);
            var rendererSystem = new RendererSystem(resourceLoader);
            rendererSystem.Initialize(640, 480, dummyRaycast);

            var instance = new GOREEngineInstance(
                configSystem,
                mapSystem,
                weaponSystem,
                rendererSystem,
                log,
                logError
            );
            // Optionally, you could add further async initialization here if needed
            return instance;
        }
    }
}
