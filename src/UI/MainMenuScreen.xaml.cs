using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;

namespace GORE.UI
{
    public sealed partial class MainMenuScreen : Window
    {
        private readonly Window _mainWindow;

        public MainMenuScreen(Window mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            ExtendsContentIntoTitleBar = true;

            // Enter fullscreen mode
            ScreenHelper.EnterFullScreenMode(this);

            // Load configuration and set up UI
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            var config = Engine.GOREEngine.GetConfiguration();

            // Set game title
            GameTitleText.Text = config.Game.Title;

            // Load background image
            if (!string.IsNullOrEmpty(config.UI.MainMenuBackground))
            {
                try
                {
                    var baseDirectory = AppContext.BaseDirectory;
                    var backgroundPath = Path.Combine(baseDirectory, config.UI.MainMenuBackground);

                    if (File.Exists(backgroundPath))
                    {
                        BackgroundImage.Source = new BitmapImage(new Uri(backgroundPath));
                    }
                }
                catch
                {
                    // If background image fails to load, use black background
                    // The grid already has no background, so it will be black by default
                }
            }

            // Apply font settings
            var fontSize = config.UI.FontSize;
            NewGameButton.FontSize = fontSize;
            LoadGameButton.FontSize = fontSize;
            SettingsButton.FontSize = fontSize;
            ExitButton.FontSize = fontSize;
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Transition to character creation screen
            // For now, just show a message
        }

        private void LoadGameButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Transition to load game screen
            // For now, just show a message
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Transition to settings screen
            // For now, just show a message
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Exit the application
            Application.Current.Exit();
        }
    }
}
