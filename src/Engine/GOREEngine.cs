
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
            // 90s-style loading screen
            log("GORE Engine (VERSION 2026.2.0)");
            await Task.Delay(300);

            log("[1/5] Loading resources...");
            var resourceLoader = new Systems.ResourceLoader();
            await Task.Delay(250);

            log("[2/5] Loading config...");
            var configSystem = new ConfigSystem(resourceLoader);
            configSystem.Load();
            await Task.Delay(250);

            log("[3/5] Loading maps...");
            var mapSystem = new MapSystem(resourceLoader);
            await Task.Delay(250);

            log("[4/5] Loading weapons...");
            var weaponSystem = new WeaponSystem(resourceLoader);
            await Task.Delay(250);

            log("[5/5] Initializing renderer...");
            // Load initial map to get grid
            await mapSystem.LoadMapByNameAsync("e1m1"); // Or use a config/default
            var raycastEngine = new RaycastEngine(mapSystem.CurrentMap.Grid, mapSystem);
            var rendererSystem = new RendererSystem(resourceLoader);
            rendererSystem.Initialize(640, 480, raycastEngine);
            await Task.Delay(250);

            log("All systems initialized!");

            var instance = new GOREEngineInstance(
                configSystem,
                mapSystem,
                weaponSystem,
                rendererSystem,
                log,
                logError
            );
            instance.RaycastEngine = raycastEngine;
            // Optionally, you could add further async initialization here if needed
            return instance;
        }
    }
}
