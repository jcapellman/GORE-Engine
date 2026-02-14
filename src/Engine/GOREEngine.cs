using System;
using Windows.UI.Xaml;

namespace GORE.Engine
{
    /// <summary>
    /// GORE Engine launcher - call this from App.xaml.cs to start your game!
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

            // Load game configuration
            var config = await Services.ConfigurationService.LoadConfigurationAsync();

            // Initialize music system
            // Note: Music files are optional, system will gracefully handle missing files

            // Enter fullscreen and hide cursor
            EnterFullScreenMode();
            HideCursor();

            _initialized = true;
        }

        /// <summary>
        /// Synchronous version of Start (for compatibility)
        /// </summary>
        public static void Start()
        {
            var task = StartAsync();
            task.Wait();
        }

        private static void EnterFullScreenMode()
        {
            var view = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            view.TryEnterFullScreenMode();
        }

        private static void HideCursor()
        {
            Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = null;
        }

        /// <summary>
        /// Get the loaded game configuration
        /// </summary>
        public static Models.GameConfiguration GetConfiguration()
        {
            return Services.ConfigurationService.GetConfiguration();
        }
    }
}
