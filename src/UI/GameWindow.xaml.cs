using GORE.Engine;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Windows.System;

namespace GORE.UI
{
    public sealed partial class GameWindow : Window
    {
        private RaycastEngine _raycastEngine;
        private Renderer3D _renderer;
        private bool _isGameLoopRunning;
        private GameConsole _console;
        private GameConfig _config;
        private bool _consoleVisible;

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
            InitializeConfig();
            InitializeConsole();
            _ = InitializeGameAsync();
        }

        private void InitializeConfig()
        {
            _config = new GameConfig();
            _config.LoadConfig();

            // Subscribe to important config changes
            var renderWidthVar = _config.Get("r_width");
            var renderHeightVar = _config.Get("r_height");

            renderWidthVar.OnChanged += OnRenderResolutionChanged;
            renderHeightVar.OnChanged += OnRenderResolutionChanged;
        }

        private void OnRenderResolutionChanged(ConfigVariable variable)
        {
            // Resolution change will require renderer recreation
            _console?.AddToHistory($"{variable.Name} changed to {variable}");
            _console?.AddToHistory("Resolution changes will take effect on map reload");
        }

        private void InitializeConsole()
        {
            _console = new GameConsole();
            _console.SetConfig(_config);
            _console.OnHistoryChanged += UpdateConsoleDisplay;
            _console.AddToHistory("GORE Engine Console");
            _console.AddToHistory("Type 'help' for available commands");
            _console.AddToHistory("Type 'cvarlist' to see config variables");
            _console.AddToHistory("");
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

            // Initialize renderer with resolution from config
            int renderWidth = _config.GetValue("r_width", 640);
            int renderHeight = _config.GetValue("r_height", 480);

            _renderer = new Renderer3D(renderWidth, renderHeight, _raycastEngine);

            _console.AddToHistory($"Renderer initialized at {renderWidth}x{renderHeight}");

            // Load textures
            foreach (var texMapping in mapData.TextureMapping)
            {
                await _renderer.LoadTextureAsync(texMapping.Key, texMapping.Value);
            }

            ViewportImage.Source = _renderer.GetBitmap();

            // Register game-specific console commands
            _console.RegisterGameCommands(_raycastEngine, 
                health => _health = health, 
                ammo => _ammo = ammo);

            // Register map loading command
            _console.RegisterCommand("map", "Load a map (usage: map <mapname>)", async args =>
            {
                if (args.Length == 0)
                {
                    _console.AddToHistory("Usage: map <mapname>");
                    _console.AddToHistory("Example: map e1m1");
                    return;
                }

                var mapName = args[0];
                await LoadMapAsync(mapName);
            });

            // Register map listing command
            _console.RegisterCommand("maps", "List available maps", args =>
            {
                try
                {
                    var baseDirectory = AppContext.BaseDirectory;
                    var mapsDirectory = System.IO.Path.Combine(baseDirectory, "gt1", "maps");

                    if (!Directory.Exists(mapsDirectory))
                    {
                        _console.AddToHistory("Maps directory not found");
                        return;
                    }

                    var mapFiles = Directory.GetFiles(mapsDirectory, "*.map");

                    if (mapFiles.Length == 0)
                    {
                        _console.AddToHistory("No maps found");
                        return;
                    }

                    _console.AddToHistory("Available maps:");
                    foreach (var mapFile in mapFiles.OrderBy(f => f))
                    {
                        var mapName = Path.GetFileNameWithoutExtension(mapFile);
                        _console.AddToHistory($"  {mapName}");
                    }
                    _console.AddToHistory($"Total: {mapFiles.Length} map(s)");
                }
                catch (Exception ex)
                {
                    _console.AddToHistory($"Error listing maps: {ex.Message}");
                }
            });

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

        private async System.Threading.Tasks.Task LoadMapAsync(string mapName)
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var mapPath = System.IO.Path.Combine(baseDirectory, "gt1", "maps", $"{mapName}.map");
                var textureCfgPath = System.IO.Path.Combine(baseDirectory, "gt1", "maps", "textures.cfg");

                _console.AddToHistory($"Loading map: {mapName}...");

                MapData mapData;
                try
                {
                    mapData = await MapLoader.LoadMapAsync(mapPath, textureCfgPath);
                    _console.AddToHistory($"✓ Map '{mapData.Name}' loaded successfully");
                }
                catch (FileNotFoundException)
                {
                    _console.AddToHistory($"✗ Map file not found: {mapName}.map");
                    _console.AddToHistory($"  Searched in: gt1/maps/");
                    return;
                }
                catch (Exception ex)
                {
                    _console.AddToHistory($"✗ Failed to load map: {ex.Message}");
                    return;
                }

                // Stop the game loop temporarily
                var wasRunning = _isGameLoopRunning;
                _isGameLoopRunning = false;

                // Update raycasting engine with new map
                _raycastEngine = new RaycastEngine(mapData.Grid);
                _raycastEngine.PlayerPosition = mapData.PlayerStart;

                // Update renderer with resolution from config
                int renderWidth = _config.GetValue("r_width", 640);
                int renderHeight = _config.GetValue("r_height", 480);
                _renderer = new Renderer3D(renderWidth, renderHeight, _raycastEngine);

                // Load textures for the new map
                foreach (var texMapping in mapData.TextureMapping)
                {
                    try
                    {
                        await _renderer.LoadTextureAsync(texMapping.Key, texMapping.Value);
                    }
                    catch (Exception ex)
                    {
                        _console.AddToHistory($"  Warning: Failed to load texture {texMapping.Value}: {ex.Message}");
                    }
                }

                ViewportImage.Source = _renderer.GetBitmap();

                // Re-register console commands with the new engine instance
                _console.RegisterGameCommands(_raycastEngine,
                    health => _health = health,
                    ammo => _ammo = ammo);

                // Restart the game loop if it was running
                if (wasRunning)
                {
                    _isGameLoopRunning = true;
                }

                _console.AddToHistory($"Map loaded: {mapData.Width}x{mapData.Height}");
                _console.AddToHistory($"Player spawned at ({mapData.PlayerStart.X:F2}, {mapData.PlayerStart.Y:F2})");
            }
            catch (Exception ex)
            {
                _console.AddToHistory($"✗ Unexpected error loading map: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Map load error: {ex}");
            }
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
            float mouseSensitivity = _config.GetValue("m_sensitivity", 0.002f);
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

            // Update FPS counter based on config
            bool showFps = _config.GetValue("r_showfps", true);
            FpsText.Visibility = showFps ? Visibility.Visible : Visibility.Collapsed;

            if (showFps)
            {
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
        }

        private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Toggle console with ~ key (grave accent, key code 192)
            if ((int)e.Key == 192 || e.Key == (VirtualKey)192)
            {
                ToggleConsole();
                e.Handled = true;
                return;
            }

            // Don't process game input if console is open
            if (_consoleVisible)
            {
                e.Handled = false;
                return;
            }

            HandleKeyInput(e.Key, true);
            e.Handled = true;
        }

        private void RootGrid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // Don't process game input if console is open
            if (_consoleVisible)
            {
                e.Handled = false;
                return;
            }

            HandleKeyInput(e.Key, false);
            e.Handled = true;
        }

        private void ToggleConsole()
        {
            _consoleVisible = !_consoleVisible;

            if (_consoleVisible)
            {
                // Show console with slide-down animation
                ConsoleOverlay.Visibility = Visibility.Visible;
                var animation = new DoubleAnimation
                {
                    From = -400,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, ConsoleTransform);
                Storyboard.SetTargetProperty(animation, "Y");
                storyboard.Begin();

                // Focus the input box
                ConsoleInput.Focus(FocusState.Programmatic);
            }
            else
            {
                // Hide console with slide-up animation
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = -400,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, ConsoleTransform);
                Storyboard.SetTargetProperty(animation, "Y");
                storyboard.Completed += (s, e) =>
                {
                    ConsoleOverlay.Visibility = Visibility.Collapsed;
                };
                storyboard.Begin();

                // Return focus to game
                RootGrid.Focus(FocusState.Programmatic);
            }
        }

        private void ConsoleInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                var input = ConsoleInput.Text;
                ConsoleInput.Text = string.Empty;
                _console.ExecuteCommand(input);
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Up)
            {
                ConsoleInput.Text = _console.GetPreviousCommand();
                ConsoleInput.SelectionStart = ConsoleInput.Text.Length;
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Down)
            {
                ConsoleInput.Text = _console.GetNextCommand();
                ConsoleInput.SelectionStart = ConsoleInput.Text.Length;
                e.Handled = true;
            }
            else if ((int)e.Key == 192 || e.Key == (VirtualKey)192) // ~ key
            {
                ToggleConsole();
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Escape)
            {
                ToggleConsole();
                e.Handled = true;
            }
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
                        // If console is open, just close it, don't exit the game
                        if (_consoleVisible)
                        {
                            ToggleConsole();
                        }
                        else
                        {
                            _isGameLoopRunning = false;
                            CompositionTarget.Rendering -= OnRendering;

                            // Save config on exit
                            _config?.SaveConfig();

                            Close();
                        }
                    }
                    break;
            }
        }

        private void UpdateConsoleDisplay()
        {
            ConsoleHistoryText.Text = string.Join("\n", _console.History);

            // Auto-scroll to bottom
            ConsoleScrollViewer.UpdateLayout();
            ConsoleScrollViewer.ChangeView(null, ConsoleScrollViewer.ScrollableHeight, null);
        }
    }
}
