using System;
using Windows.UI.Xaml;
using Windows.UI.ViewManagement;

namespace GORE.Services
{
    /// <summary>
    /// Main GORE Engine initialization and management.
    /// Call ApplyGameMode() after Window.Current.Activate() and navigation.
    /// </summary>
    public static class GoreEngine
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// Applies fullscreen mode and hides cursor.
        /// Call this once in App.OnLaunched AFTER Window.Current.Activate() and navigation.
        /// </summary>
        public static void ApplyGameMode()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            // Enter fullscreen mode for immersive experience
            var view = ApplicationView.GetForCurrentView();
            view.TryEnterFullScreenMode();

            // Hide system cursor (game uses keyboard/gamepad only)
            if (Window.Current.CoreWindow != null)
            {
                Window.Current.CoreWindow.PointerCursor = null;
            }
        }

        /// <summary>
        /// DEPRECATED: Use ApplyGameMode() instead after activating window separately.
        /// </summary>
        [Obsolete("Use ApplyGameMode() instead. Window.Current.Activate() should be called before navigation.")]
        public static void Initialize()
        {
            ApplyGameMode();
        }

        /// <summary>
        /// Ensures fullscreen and cursor remain hidden (called on page navigations).
        /// Only runs if ApplyGameMode() has been called.
        /// </summary>
        public static void EnsureGameMode()
        {
            // Don't do anything if not initialized yet
            if (!_isInitialized)
                return;

            var view = ApplicationView.GetForCurrentView();
            if (!view.IsFullScreenMode)
            {
                view.TryEnterFullScreenMode();
            }

            if (Window.Current.CoreWindow?.PointerCursor != null)
            {
                Window.Current.CoreWindow.PointerCursor = null;
            }
        }

        /// <summary>
        /// Cleanup on app exit.
        /// </summary>
        public static void Shutdown()
        {
            MusicManager.Cleanup();
        }
    }
}
