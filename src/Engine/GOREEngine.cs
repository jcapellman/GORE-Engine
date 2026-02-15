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
        /// The engine will create its own window automatically.
        /// </summary>
        public static async System.Threading.Tasks.Task StartAsync()
        {
            if (_initialized) return;

            // Create the main window automatically
            _mainWindow = new Window();

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

        /// <summary>
        /// Get the loaded game configuration
        /// </summary>
        public static Models.GameConfiguration GetConfiguration() => Services.ConfigurationService.GetConfiguration();
    }
}
