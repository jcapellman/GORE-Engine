using System;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.System;
using GORE.Services;
using GORE.Models;

namespace GORE.Pages
{
    /// <summary>
    /// Base main menu page - fully driven by game.json configuration.
    /// Games just provide assets, no code needed!
    /// </summary>
    public abstract class BaseMainMenuPage : BasePage
    {
        // Menu state
        protected int menuSelection = 0;
        protected const int MenuItemCount = 3;
        protected bool isDialogOpen = false;

        // Animation state
        protected DispatcherTimer animationTimer;
        protected float cloudOffset1 = 0;
        protected float cloudOffset2 = 0;
        protected float mistOffset = 0;
        protected float animationTime = 0;

        // Configuration
        protected GameConfiguration config;

        protected BaseMainMenuPage()
        {
      //      LoadConfiguration();
        //    InitializeAnimationTimer();
        }

        private async void LoadConfiguration()
        {
            config = await ConfigurationService.LoadConfigurationAsync();
        }

        private void InitializeAnimationTimer()
        {
            animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60fps
            };
            animationTimer.Tick += AnimationTimer_Tick;
          //  animationTimer.Start();
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Initialize on first navigation only
            if (animationTimer == null)
            {
                LoadConfiguration();
                InitializeAnimationTimer();
                animationTimer.Start();
            }

            // Safely register keyboard handler - check for null to avoid AccessViolationException
            var coreWindow = Window.Current?.CoreWindow;
            if (coreWindow != null)
            {
                coreWindow.KeyDown -= CoreWindow_KeyDown; // Remove first to avoid duplicates
                coreWindow.KeyDown += CoreWindow_KeyDown;
            }

            UpdateMenuCursor();
            // TEMPORARILY DISABLED to isolate navigation issue
            // MusicManager.PlayMusic(MusicTrack.MainMenu);
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            animationTimer?.Stop();
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
        }

        protected virtual void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            args.Handled = true;

            if (isDialogOpen) return;

            if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.W)
            {
                menuSelection--;
                if (menuSelection < 0)
                    menuSelection = MenuItemCount - 1;
                UpdateMenuCursor();
            }
            else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.S)
            {
                menuSelection++;
                if (menuSelection >= MenuItemCount)
                    menuSelection = 0;
                UpdateMenuCursor();
            }
            else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.Space)
            {
                ExecuteMenuSelection();
            }
        }

        protected virtual void ExecuteMenuSelection()
        {
            switch (menuSelection)
            {
                case 0: OnNewGame(); break;
                case 1: OnLoadGame(); break;
                case 2: OnExit(); break;
            }
        }

        protected virtual void AnimationTimer_Tick(object sender, object e)
        {
            animationTime += 0.016f;
            cloudOffset1 += 0.3f;
            cloudOffset2 += 0.5f;
            mistOffset += 0.2f;

            OnAnimationFrame();
        }

        // Abstract methods for UI updates (XAML controls)
        protected abstract void UpdateMenuCursor();
        protected abstract void OnAnimationFrame();

        // Menu action hooks
        protected abstract void OnNewGame();
        protected abstract void OnLoadGame();
        protected abstract void OnExit();

        // Optional: Custom animation rendering (for games with Canvas)
        // REMOVE OR COMMENT OUT THESE ENTIRE METHODS:
        /*
        protected virtual void DrawCloudLayer(Microsoft.Graphics.Canvas.CanvasDrawingSession session,
            float width, float height, float offset, float yPosition, float alpha)
        {
           ...

        protected virtual void DrawMistLayer(Microsoft.Graphics.Canvas.CanvasDrawingSession session,
            float width, float height, float offset)
        {
           ...
        }*/
    }
}
