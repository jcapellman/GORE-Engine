using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GORE.Services;
using GORE.Models;

namespace GORE.Pages
{
    public sealed partial class MainMenuPage : BaseMainMenuPage
    {
        public MainMenuPage()
        {
            this.InitializeComponent();
       //     LoadGameConfiguration();
        }

        private async void LoadGameConfiguration()
        {
            config = await ConfigurationService.LoadConfigurationAsync();

            if (config != null)
            {
             //   txtGameTitle.Text = config.Game.Title;
              //  txtVersion.Text = $"v{config.Game.Version}";
               // txtDeveloper.Text = $"by {config.Game.Developer}";
            }
        }

        protected override void UpdateMenuCursor()
        {
            // Temporarily disabled - no cursor controls in minimal test XAML
        }

        protected override void OnAnimationFrame()
        {
          //  animationCanvas?.Invalidate();
        }

        protected override void OnNewGame()
        {
            this.Frame.Navigate(typeof(CharacterCreationPage));
        }

        protected override async void OnLoadGame()
        {
            var coreWindow = Window.Current?.CoreWindow;
            if (coreWindow != null)
            {
                coreWindow.KeyDown -= CoreWindow_KeyDown;
            }

            bool saveExists = await SaveGameManager.SaveExists();

            if (saveExists)
            {
                var saveData = await SaveGameManager.LoadGame();
                if (saveData != null)
                {
                    this.Frame.Navigate(typeof(BaseGamePage), saveData);
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

            if (coreWindow != null)
            {
                coreWindow.KeyDown += CoreWindow_KeyDown;
            }
        }

        protected override void OnExit()
        {
            Application.Current.Exit();
        }

        /*

        private void AnimationCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var session = args.DrawingSession;
            float width = (float)sender.ActualWidth;
            float height = (float)sender.ActualHeight;

            DrawCloudLayer(session, width, height, cloudOffset1, 0.3f, 25);
            DrawCloudLayer(session, width, height, cloudOffset2, 0.5f, 40);
            DrawMistLayer(session, width, height, mistOffset);
        }
        */
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
