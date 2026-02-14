using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace GORE.Engine
{
    /// <summary>
    /// GORE Engine launcher - call this from App.xaml.cs to start your game!
    /// </summary>
    public static class GOREEngine
    {
        private static bool _initialized = false;
        private static Window _mainWindow;

        /// <summary>
        /// Initialize and start the GORE Engine with splash screen.
        /// Call this from your App.xaml.cs OnLaunched method.
        /// </summary>
        /// <param name="mainWindow">The main application window</param>
        public static async System.Threading.Tasks.Task StartAsync(Window mainWindow)
        {
            if (_initialized) return;

            _mainWindow = mainWindow;

            // Load game configuration
            var config = await Services.ConfigurationService.LoadConfigurationAsync();

            // Initialize music system
            // Note: Music files are optional, system will gracefully handle missing files

            // Enter fullscreen and hide cursor
            EnterFullScreenMode();
            HideCursor();

            // Show GORE Engine splash screen
            var splashScreen = new UI.SplashScreen(_mainWindow);
            splashScreen.Activate();

            _initialized = true;
        }

        /// <summary>
        /// Synchronous version of Start (for compatibility)
        /// </summary>
        /// <param name="mainWindow">The main application window</param>
        public static void Start(Window mainWindow)
        {
            var task = StartAsync(mainWindow);
            task.Wait();
        }

        private static void EnterFullScreenMode()
        {
            var hwnd = WindowNative.GetWindowHandle(_mainWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.SetBorderAndTitleBar(false, false);
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
            }
        }

        private static void HideCursor()
        {
            // In WinUI 3, cursor visibility is handled differently
            // This is a placeholder - WinUI 3 doesn't have direct CoreWindow access
            // You may need to use Win32 APIs via P/Invoke to hide the cursor
            // For now, we'll leave this as a no-op or use InputCursor
            if (_mainWindow != null)
            {
                _mainWindow.Activated += (sender, args) =>
                {
                    // Set cursor to null to hide it
                    // Note: This may need platform-specific implementation
                };
            }
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
