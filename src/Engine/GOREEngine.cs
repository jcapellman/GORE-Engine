using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System;
using System.Threading.Tasks;
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
        /// Create and initialize a new GOREEngineInstance.
        /// </summary>
        public static async Task<GOREEngineInstance> CreateAndInitializeAsync(Action<string> log, Action<string> logError)
        {
            var instance = new GOREEngineInstance();
            var ok = await instance.InitializeAsync(log, logError);
            return ok ? instance : null;
        }
    }
}
