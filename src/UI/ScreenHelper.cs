using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace GORE.UI
{
    /// <summary>
    /// Helper class for common screen/window operations
    /// </summary>
    public static class ScreenHelper
    {
        /// <summary>
        /// Enters fullscreen mode by removing borders and maximizing to the primary display.
        /// </summary>
        public static void EnterFullScreenMode(Window window)
        {
            var hwnd = WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.SetBorderAndTitleBar(false, false);
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
                presenter.IsResizable = false;
            }

            // Get the primary display area
            var displayArea = DisplayArea.Primary;

            // Position at top-left corner and fill entire screen
            if (appWindow != null)
            {
                appWindow.Move(new Windows.Graphics.PointInt32(0, 0));
                appWindow.Resize(new Windows.Graphics.SizeInt32(
                    displayArea.OuterBounds.Width,
                    displayArea.OuterBounds.Height));
            }
        }

        /// <summary>
        /// Transitions from one window to another by activating the next and closing the current.
        /// </summary>
        public static void TransitionTo(Window currentWindow, Window nextWindow)
        {
            nextWindow.Activate();
            currentWindow.Close();
        }
    }
}
