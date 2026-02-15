using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace GORE.Engine
{
    /// <summary>
    /// GORE Engine - Wolfenstein 3D-style raycasting FPS
    /// </summary>
    public static class GOREEngine
    {
        private static bool _initialized = false;

        /// <summary>
        /// Initialize and start the GORE Engine.
        /// Call this from your App.xaml.cs OnLaunched method.
        /// </summary>
        public static async System.Threading.Tasks.Task StartAsync()
        {
            if (_initialized) return;

            // Launch the 3D game window directly
            var gameWindow = new UI.GameWindow();
            gameWindow.Activate();

            _initialized = true;

            await System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
