using Microsoft.UI.Xaml.Navigation;
using GORE.Models;
using GORE.Services;

namespace GORE.Pages
{
    /// <summary>
    /// Base main menu page - provides core menu functionality
    /// </summary>
    public abstract class BaseMainMenuPage : BasePage
    {
        protected int selection = 0;
        protected GameConfiguration config;

        protected BaseMainMenuPage()
        {
            LoadConfiguration();
        }

        protected async void LoadConfiguration()
        {
            config = await ConfigurationService.LoadConfigurationAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Note: Input handling needs to be implemented for WinUI 3
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected abstract void UpdateSelectionCursor();
        protected abstract void OnNewGame();
        protected abstract void OnLoadGame();
        protected abstract void OnExitGame();
    }
}
