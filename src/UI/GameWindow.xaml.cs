using GORE.Engine;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Numerics;
using Windows.System;

namespace GORE.UI
{
    public sealed partial class GameWindow : Window
    {
        private RaycastEngine _raycastEngine;
        private Renderer3D _renderer;
        private DispatcherTimer _gameTimer;

        private bool _moveForward;
        private bool _moveBackward;
        private bool _strafeLeft;
        private bool _strafeRight;
        private bool _turnLeft;
        private bool _turnRight;

        private DateTime _lastFrameTime;
        private int _health = 100;
        private int _ammo = 50;

        public GameWindow()
        {
            InitializeComponent();
            _ = InitializeGameAsync();
        }

        private async System.Threading.Tasks.Task InitializeGameAsync()
        {
            ExtendsContentIntoTitleBar = true;
            ScreenHelper.EnterFullScreenMode(this);

            // Load custom font
            await LoadCustomFontAsync();

            // Load map from file
            var baseDirectory = AppContext.BaseDirectory;
            var mapPath = System.IO.Path.Combine(baseDirectory, "gt1", "maps", "level1.map");
            var textureCfgPath = System.IO.Path.Combine(baseDirectory, "gt1", "maps", "textures.cfg");

            MapData mapData;
            try
            {
                mapData = await MapLoader.LoadMapAsync(mapPath, textureCfgPath);
                System.Diagnostics.Debug.WriteLine("✓ Map loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Failed to load map: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("Using default map");
                mapData = MapLoader.CreateDefaultMap();
            }

            // Initialize raycasting engine
            _raycastEngine = new RaycastEngine(mapData.Grid);
            _raycastEngine.PlayerPosition = mapData.PlayerStart;

            // Initialize renderer
            _renderer = new Renderer3D(800, 600, _raycastEngine);

            // Load textures
            foreach (var texMapping in mapData.TextureMapping)
            {
                await _renderer.LoadTextureAsync(texMapping.Key, texMapping.Value);
            }

            ViewportImage.Source = _renderer.GetBitmap();

            // Setup game loop
            _lastFrameTime = DateTime.Now;
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
        }

        private async System.Threading.Tasks.Task LoadCustomFontAsync()
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var fontPath = Path.Combine(baseDirectory, "gt1", "font.ttf");

                if (File.Exists(fontPath))
                {
                    var fontFamily = new FontFamily($"ms-appx:///gt1/font.ttf#GT1");

                    // Apply to HUD text elements
                    HealthText.FontFamily = fontFamily;
                    AmmoText.FontFamily = fontFamily;

                    System.Diagnostics.Debug.WriteLine($"✓ Custom font loaded: {fontPath}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Font not found: {fontPath} - using default");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Failed to load custom font: {ex.Message}");
            }

            await System.Threading.Tasks.Task.CompletedTask;
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            RootGrid.Focus(FocusState.Programmatic);
        }

        private void GameLoop(object sender, object e)
        {
            // Calculate delta time
            var currentTime = DateTime.Now;
            float deltaTime = (float)(currentTime - _lastFrameTime).TotalSeconds;
            _lastFrameTime = currentTime;

            // Update player movement
            UpdatePlayerMovement(deltaTime);

            // Render frame
            _renderer.Render();

            // Update HUD
            UpdateHUD();
        }

        private void UpdatePlayerMovement(float deltaTime)
        {
            Vector2 movement = Vector2.Zero;

            // Forward/Backward
            if (_moveForward)
                movement += _raycastEngine.PlayerDirection;
            if (_moveBackward)
                movement -= _raycastEngine.PlayerDirection;

            // Strafing
            if (_strafeLeft)
            {
                Vector2 right = new Vector2(_raycastEngine.PlayerDirection.Y, -_raycastEngine.PlayerDirection.X);
                movement -= right;
            }
            if (_strafeRight)
            {
                Vector2 right = new Vector2(_raycastEngine.PlayerDirection.Y, -_raycastEngine.PlayerDirection.X);
                movement += right;
            }

            // Apply movement
            if (movement != Vector2.Zero)
            {
                movement = Vector2.Normalize(movement);
                _raycastEngine.MovePlayer(movement, deltaTime);
            }

            // Rotation
            float rotSpeed = 2.0f * deltaTime;
            if (_turnLeft)
                _raycastEngine.RotatePlayer(rotSpeed);
            if (_turnRight)
                _raycastEngine.RotatePlayer(-rotSpeed);
        }

        private void UpdateHUD()
        {
            // Quake 3 style - just the numbers
            HealthText.Text = _health.ToString();
            AmmoText.Text = _ammo.ToString();
        }

        private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            HandleKeyInput(e.Key, true);
            e.Handled = true;
        }

        private void RootGrid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            HandleKeyInput(e.Key, false);
            e.Handled = true;
        }

        private void HandleKeyInput(VirtualKey key, bool isPressed)
        {
            switch (key)
            {
                // WASD movement
                case VirtualKey.W:
                    _moveForward = isPressed;
                    break;
                case VirtualKey.S:
                    _moveBackward = isPressed;
                    break;
                case VirtualKey.A:
                    _strafeLeft = isPressed;
                    break;
                case VirtualKey.D:
                    _strafeRight = isPressed;
                    break;

                // Arrow keys for rotation
                case VirtualKey.Left:
                    _turnLeft = isPressed;
                    break;
                case VirtualKey.Right:
                    _turnRight = isPressed;
                    break;

                // Space to shoot
                case VirtualKey.Space:
                    if (isPressed && _ammo > 0)
                    {
                        _ammo--;
                        // TODO: Implement shooting
                    }
                    break;

                // Escape to exit
                case VirtualKey.Escape:
                    if (isPressed)
                    {
                        _gameTimer?.Stop();
                        Close();
                    }
                    break;
            }
        }

        protected void OnWindowClosed()
        {
            _gameTimer?.Stop();
        }
    }
}
