using GORE.Engine;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Windows.System;

namespace GORE.UI
{
    public sealed partial class GameWindow : Window
    {
        private RaycastEngine _raycastEngine;
        private Renderer3D _renderer;
        private bool _isGameLoopRunning;

        private bool _moveForward;
        private bool _moveBackward;
        private bool _strafeLeft;
        private bool _strafeRight;
        private bool _turnLeft;
        private bool _turnRight;

        private Stopwatch _frameTimer;
        private int _health = 100;
        private int _ammo = 50;

        // FPS tracking
        private int _frameCount = 0;
        private DateTime _lastFpsUpdate = DateTime.Now;

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
            var mapPath = System.IO.Path.Combine(baseDirectory, "gt1", "maps", "e1m1.map");
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

            // Initialize renderer with lower resolution for better performance
            // Image will be scaled up by the Image control
            _renderer = new Renderer3D(640, 480, _raycastEngine);

            // Load textures
            foreach (var texMapping in mapData.TextureMapping)
            {
                await _renderer.LoadTextureAsync(texMapping.Key, texMapping.Value);
            }

            ViewportImage.Source = _renderer.GetBitmap();

            // Setup game loop using CompositionTarget for better frame timing
            _frameTimer = Stopwatch.StartNew();
            _isGameLoopRunning = true;
            CompositionTarget.Rendering += OnRendering;
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

        private void OnRendering(object sender, object e)
        {
            if (!_isGameLoopRunning || _renderer == null)
                return;

            // Calculate delta time using high-precision stopwatch
            float deltaTime = (float)_frameTimer.Elapsed.TotalSeconds;
            _frameTimer.Restart();

            // Cap delta time to avoid large jumps
            if (deltaTime > 0.1f)
                deltaTime = 0.016f; // Fallback to ~60 FPS

            // Update player movement
            UpdatePlayerMovement(deltaTime);

            // Render frame
            _renderer.Render();

            // Update HUD (less frequently to save performance)
            _frameCount++;
            if (_frameCount % 5 == 0) // Update HUD every 5 frames
            {
                UpdateHUD();
            }
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

            // Update FPS counter
            _frameCount++;
            var elapsed = (DateTime.Now - _lastFpsUpdate).TotalSeconds;
            if (elapsed >= 1.0)
            {
                var fps = (int)(_frameCount / elapsed);
                FpsText.Text = $"{fps}fps";
                _frameCount = 0;
                _lastFpsUpdate = DateTime.Now;
            }
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
                        _isGameLoopRunning = false;
                        CompositionTarget.Rendering -= OnRendering;
                        Close();
                    }
                    break;
            }
        }
    }
}
