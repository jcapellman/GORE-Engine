# Fix GORE Engine Page Files
# Run this script to replace the corrupted .xaml.cs files

Write-Host "Fixing GORE Engine page files..." -ForegroundColor Green

# Remove old files
Remove-Item "C:\Users\jcape\source\repos\GORE-Engine\src\Pages\MainMenuPage.xaml.cs" -Force -ErrorAction SilentlyContinue
Remove-Item "C:\Users\jcape\source\repos\GORE-Engine\src\Pages\CharacterCreationPage.xaml.cs" -Force -ErrorAction SilentlyContinue

# Create MainMenuPage.xaml.cs
$mainMenuContent = @'
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI.Xaml;
using GORE.Services;
using GORE.Models;

namespace GORE.Pages
{
    public sealed partial class MainMenuPage : BaseMainMenuPage
    {
        public MainMenuPage()
        {
            this.InitializeComponent();
            LoadGameConfiguration();
        }

        private async void LoadGameConfiguration()
        {
            config = await ConfigurationService.LoadConfigurationAsync();

            if (config != null)
            {
                txtGameTitle.Text = config.Game.Title;
                txtVersion.Text = $"v{config.Game.Version}";
                txtDeveloper.Text = $"by {config.Game.Developer}";
            }
        }

        protected override void UpdateMenuCursor()
        {
            cursorNewGame.Visibility = menuSelection == 0 ? Visibility.Visible : Visibility.Collapsed;
            cursorLoadGame.Visibility = menuSelection == 1 ? Visibility.Visible : Visibility.Collapsed;
            cursorExit.Visibility = menuSelection == 2 ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnAnimationFrame()
        {
            animationCanvas?.Invalidate();
        }

        protected override void OnNewGame()
        {
            this.Frame.Navigate(typeof(CharacterCreationPage));
        }

        protected override async void OnLoadGame()
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;

            bool saveExists = await SaveGameManager.SaveExists();

            if (saveExists)
            {
                var saveData = await SaveGameManager.LoadGame();
                if (saveData != null)
                {
                    this.Frame.Navigate(typeof(MysticChronicles.GamePage), saveData);
                }
                else
                {
                    await ShowDialogAsync("Load Failed", "Failed to load saved game.");
                }
            }
            else
            {
                await ShowDialogAsync("No Save Data", "No saved game found.");
            }
            
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        protected override void OnExit()
        {
            Application.Current.Exit();
        }

        private void AnimationCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var session = args.DrawingSession;
            float width = (float)sender.ActualWidth;
            float height = (float)sender.ActualHeight;

            DrawCloudLayer(session, width, height, cloudOffset1, 0.3f, 25);
            DrawCloudLayer(session, width, height, cloudOffset2, 0.5f, 40);
            DrawMistLayer(session, width, height, mistOffset);
        }

        private async System.Threading.Tasks.Task ShowDialogAsync(string title, string content)
        {
            isDialogOpen = true;
            
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK"
            };
            
            await dialog.ShowAsync();
            isDialogOpen = false;
        }
    }
}
'@

Set-Content -Path "C:\Users\jcape\source\repos\GORE-Engine\src\Pages\MainMenuPage.xaml.cs" -Value $mainMenuContent

# Create CharacterCreationPage.xaml.cs
$charCreateContent = @'
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GORE.Services;
using GORE.Models;

namespace GORE.Pages
{
    public sealed partial class CharacterCreationPage : BaseCharacterCreationPage
    {
        public CharacterCreationPage()
        {
            this.InitializeComponent();
            LoadGameConfiguration();
        }

        private async void LoadGameConfiguration()
        {
            config = await ConfigurationService.LoadConfigurationAsync();
        }

        protected override void UpdateSelectionCursor()
        {
            cursorConfirm.Visibility = selection == 0 ? Visibility.Visible : Visibility.Collapsed;
            cursorCancel.Visibility = selection == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnCharacterNameChanged(string name)
        {
            txtHeroName.Text = string.IsNullOrWhiteSpace(name) ? "Hero" : name;
        }

        protected override void OnConfirm(string heroName)
        {
            MusicManager.PlayMusic(MusicTrack.Exploration);
            this.Frame.Navigate(typeof(MysticChronicles.GamePage), heroName);
        }

        protected override void OnCancel()
        {
            this.Frame.Navigate(typeof(MainMenuPage));
        }
    }
}
'@

Set-Content -Path "C:\Users\jcape\source\repos\GORE-Engine\src\Pages\CharacterCreationPage.xaml.cs" -Value $charCreateContent

Write-Host "✅ Files created successfully!" -ForegroundColor Green
Write-Host "Now rebuild the solution in Visual Studio" -ForegroundColor Cyan
