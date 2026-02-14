using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GORE.Services;

namespace GORE.Pages
{
    /// <summary>
    /// Base page class providing common functionality for all game pages.
    /// Handles full screen mode and cursor management via GoreEngine.
    /// </summary>
    public abstract class BasePage : Page
    {
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // TEMPORARILY DISABLED to isolate navigation issue
            // Ensure game mode (fullscreen, cursor hidden) on every page navigation
            // in case user exited fullscreen with Alt+Enter or similar
            // GoreEngine.EnsureGameMode();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }
    }
}