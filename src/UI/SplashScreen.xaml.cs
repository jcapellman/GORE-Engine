using Microsoft.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using System.Numerics;

namespace GORE.UI
{
    public sealed partial class SplashScreen : Window
    {
        private readonly Window _mainWindow;
        private const int FadeInDuration = 1500;
        private const int DisplayDuration = 2000;
        private const int FadeOutDuration = 1500;

        private CanvasBitmap _logoBitmap;
        private float _blurAmount = 15f;
        private float _glowAmount = 0.5f;
        private bool _isLoaded = false;

        public SplashScreen(Window mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            ExtendsContentIntoTitleBar = true;

            // Enter fullscreen mode
            ScreenHelper.EnterFullScreenMode(this);

            CanvasControl.CreateResources += CanvasControl_CreateResources;
        }

        private async void CanvasControl_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(LoadRandomLogo(sender).AsAsyncAction());
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_logoBitmap != null && _isLoaded)
            {
                var ds = args.DrawingSession;

                // Create glow effect
                var glowBlur = new GaussianBlurEffect
                {
                    Source = _logoBitmap,
                    BlurAmount = 20f * _glowAmount
                };

                var glowComposite = new CompositeEffect
                {
                    Mode = CanvasComposite.Add
                };
                glowComposite.Sources.Add(_logoBitmap);
                glowComposite.Sources.Add(glowBlur);

                // Create blur effect that decreases over time
                var blurEffect = new GaussianBlurEffect
                {
                    Source = glowComposite,
                    BlurAmount = _blurAmount
                };

                // Center the image
                var canvasSize = sender.Size;
                var imageSize = _logoBitmap.Size;
                var scale = Math.Min(
                    (canvasSize.Width * 0.8) / imageSize.Width,
                    (canvasSize.Height * 0.8) / imageSize.Height
                );

                var x = (float)((canvasSize.Width - imageSize.Width * scale) / 2);
                var y = (float)((canvasSize.Height - imageSize.Height * scale) / 2);

                // Apply transform and draw
                ds.Transform = Matrix3x2.CreateScale((float)scale) * Matrix3x2.CreateTranslation(x, y);
                ds.DrawImage(blurEffect);
                ds.Transform = Matrix3x2.Identity;
            }
        }

        private async Task LoadRandomLogo(CanvasControl canvas)
        {
            const int logoCount = 5;
            var random = new Random();
            var logoNumber = random.Next(1, logoCount + 1);
            var logoFileName = $"LogoAlt{logoNumber}.png";

            // Get the app's base directory
            var baseDirectory = AppContext.BaseDirectory;
            var logoPath = System.IO.Path.Combine(baseDirectory, "Assets", logoFileName);

            // Load from file path
            _logoBitmap = await CanvasBitmap.LoadAsync(canvas, logoPath);
            _isLoaded = true;

            _ = ShowSplashSequence();
        }

        private async Task ShowSplashSequence()
        {
            await Task.Delay(100);

            // Fade in canvas
            CanvasControl.Opacity = 1.0;

            // Animate blur clearing up
            var startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < FadeInDuration)
            {
                var progress = (float)(DateTime.Now - startTime).TotalMilliseconds / FadeInDuration;
                _blurAmount = 15f * (1 - progress);
                _glowAmount = 0.5f + (0.5f * progress);
                CanvasControl.Invalidate();
                await Task.Delay(16); // ~60 FPS
            }

            _blurAmount = 0f;
            _glowAmount = 1f;
            CanvasControl.Invalidate();

            await Task.Delay(DisplayDuration);

            // Fade out
            CanvasControl.Opacity = 0.0;

            await Task.Delay(FadeOutDuration);

            // Transition to main menu
            var mainMenuScreen = new MainMenuScreen(_mainWindow);
            ScreenHelper.TransitionTo(this, mainMenuScreen);
        }
    }
}
