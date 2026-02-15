using System;
using System.Runtime.InteropServices;
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

        // Win32 API for cursor visibility
        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        /// <summary>
        /// Initialize and start the GORE Engine with splash screen.
        /// Call this from your App.xaml.cs OnLaunched method.
        /// </summary>
        /// <param name="mainWindow">The main application window</param>
        public static async System.Threading.Tasks.Task StartAsync(Window mainWindow)
        {
            if (_initialized) return;

            _mainWindow = mainWindow;

            // Hide the mouse cursor
            HideCursor();

            // Load game configuration
            var config = await Services.ConfigurationService.LoadConfigurationAsync();

            // Initialize music system
            // Note: Music files are optional, system will gracefully handle missing files

            // Enter fullscreen and hide cursor
            EnterFullScreenMode();

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
            // Hide the cursor using Win32 API
            ShowCursor(false);
            System.Diagnostics.Debug.WriteLine("Cursor hidden");
        }

        /// <summary>
        /// Shows the mouse cursor (called on app exit)
        /// </summary>
        public static void ShowCursor()
        {
            ShowCursor(true);
            System.Diagnostics.Debug.WriteLine("Cursor shown");
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
