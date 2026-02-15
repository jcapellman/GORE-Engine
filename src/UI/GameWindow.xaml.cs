using GORE.Engine;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
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
        private Dictionary<int, string> _pendingTextureMapping;
        private bool _isLoadingTextures;
        private bool _resourcesInitialized;

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
        private double _fpsAccumulator = 0.0;
        private int _fpsFrameCount = 0;
        private int _lastFps = 0;

        // Delta time smoothing
        private float _lastDeltaTime = 0.016f;
        private const float MAX_DELTA_TIME = 0.05f; // Cap at 50ms (20 FPS minimum)

        // Cached config values (updated when config changes)
        private bool _showFps = true;
        private float _mouseSensitivity = 0.002f;

        // Cached vectors to reduce allocations
        private Vector2 _cachedRightVector;

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

            // Cache frequently accessed config values
            UpdateCachedConfigValues();

            // Subscribe to important config changes
            var renderWidthVar = _config.Get("r_width");
            var renderHeightVar = _config.Get("r_height");
            var showFpsVar = _config.Get("r_showfps");
            var mouseSensitivityVar = _config.Get("m_sensitivity");

            renderWidthVar.OnChanged += OnRenderResolutionChanged;
            renderHeightVar.OnChanged += OnRenderResolutionChanged;
            showFpsVar.OnChanged += OnConfigChanged;
            mouseSensitivityVar.OnChanged += OnConfigChanged;
        }

        private void UpdateCachedConfigValues()
        {
            _showFps = _config.GetValue("r_showfps", true);
            _mouseSensitivity = _config.GetValue("m_sensitivity", 0.002f);
        }

        private void OnConfigChanged(ConfigVariable variable)
        {
            UpdateCachedConfigValues();
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
            _console.AddToHistory("Win2D hardware acceleration enabled");

            // Note: Textures will be loaded when Win2D canvas is ready
            // Store texture mapping for later
            _pendingTextureMapping = mapData.TextureMapping;

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

            // Setup frame timer
            _frameTimer = Stopwatch.StartNew();
            _isGameLoopRunning = true;
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

                // Dispose old renderer
                _renderer?.Dispose();

                // Update raycasting engine with new map
                _raycastEngine = new RaycastEngine(mapData.Grid);
                _raycastEngine.PlayerPosition = mapData.PlayerStart;

                // Update renderer with resolution from config
                int renderWidth = _config.GetValue("r_width", 640);
                int renderHeight = _config.GetValue("r_height", 480);
                _renderer = new Renderer3D(renderWidth, renderHeight, _raycastEngine);

                // Reset initialization flags
                _resourcesInitialized = false;
                _isLoadingTextures = false;

                // Initialize Win2D resources if canvas is ready
                if (ViewportCanvas.Device != null)
                {
                    _renderer.InitializeResources(ViewportCanvas.Device);
                    _resourcesInitialized = true;

                    // Load textures for the new map
                    _isLoadingTextures = true;
                    foreach (var texMapping in mapData.TextureMapping)
                    {
                        try
                        {
                            await _renderer.LoadTextureAsync(texMapping.Key, texMapping.Value, ViewportCanvas.Device);
                        }
                        catch (Exception ex)
                        {
                            _console.AddToHistory($"  Warning: Failed to load texture {texMapping.Value}: {ex.Message}");
                        }
                    }
                    _isLoadingTextures = false;
                }
                else
                {
                    // Store for later loading
                    _pendingTextureMapping = mapData.TextureMapping;
                }

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

        private void ViewportCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            if (!_isGameLoopRunning || _renderer == null || _raycastEngine == null)
                return;

            // Initialize Win2D resources on first update if needed
            if (!_resourcesInitialized && _renderer.GetRenderTarget() == null)
            {
                try
                {
                    _renderer.InitializeResources(sender.Device);
                    _resourcesInitialized = true;

                    // Start loading textures asynchronously
                    if (_pendingTextureMapping != null && !_isLoadingTextures)
                    {
                        _isLoadingTextures = true;
                        _ = LoadTexturesAsync(sender.Device);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to initialize resources: {ex.Message}");
                    return;
                }
            }

            // Don't update game logic while loading textures
            if (_isLoadingTextures)
                return;

            // Calculate delta time with smoothing to prevent warping
            float rawDeltaTime = (float)args.Timing.ElapsedTime.TotalSeconds;

            // Clamp delta time to reasonable range
            float clampedDeltaTime = Math.Clamp(rawDeltaTime, 0.001f, MAX_DELTA_TIME);

            // Smooth delta time using exponential moving average to reduce jitter
            float deltaTime = _lastDeltaTime * 0.7f + clampedDeltaTime * 0.3f;
            _lastDeltaTime = deltaTime;

            // Update player movement
            UpdatePlayerMovement(deltaTime);

            // Update FPS counter
            UpdateFPS(deltaTime);

            // Update HUD (less frequently to save performance)
            if (_frameCount % 5 == 0) // Update HUD every 5 frames
            {
                UpdateHUD();
            }

            _frameCount++;
        }

        private async System.Threading.Tasks.Task LoadTexturesAsync(CanvasDevice device)
        {
            try
            {
                if (_pendingTextureMapping == null)
                {
                    _isLoadingTextures = false;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Loading {_pendingTextureMapping.Count} textures...");

                foreach (var texMapping in _pendingTextureMapping)
                {
                    try
                    {
                        await _renderer.LoadTextureAsync(texMapping.Key, texMapping.Value, device);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Failed to load texture {texMapping.Value}: {ex.Message}");
                    }
                }

                _pendingTextureMapping = null;
                _isLoadingTextures = false;
                System.Diagnostics.Debug.WriteLine("✓ All textures loaded");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading textures: {ex.Message}");
                _isLoadingTextures = false;
            }
        }

        private void ViewportCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            // Don't render if resources aren't initialized or textures are still loading
            if (_renderer == null || _renderer.GetRenderTarget() == null || _isLoadingTextures)
            {
                // Draw loading screen
                args.DrawingSession.Clear(Windows.UI.Color.FromArgb(255, 0, 0, 0));
                return;
            }

            try
            {
                // Render the 3D scene, scaled to fill the canvas
                _renderer.Render(args.DrawingSession, (float)sender.Size.Width, (float)sender.Size.Height);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Render error: {ex.Message}");
            }
        }

        private void UpdatePlayerMovement(float deltaTime)
        {
            Vector2 movement = Vector2.Zero;
            bool needsMovement = false;

            // Forward/Backward
            if (_moveForward)
            {
                movement += _raycastEngine.PlayerDirection;
                needsMovement = true;
            }
            if (_moveBackward)
            {
                movement -= _raycastEngine.PlayerDirection;
                needsMovement = true;
            }

            // Strafing - cache the right vector to avoid duplicate calculations
            if (_strafeLeft || _strafeRight)
            {
                _cachedRightVector = new Vector2(_raycastEngine.PlayerDirection.Y, -_raycastEngine.PlayerDirection.X);

                if (_strafeLeft)
                {
                    movement -= _cachedRightVector;
                    needsMovement = true;
                }
                if (_strafeRight)
                {
                    movement += _cachedRightVector;
                    needsMovement = true;
                }
            }

            // Apply movement only if needed
            if (needsMovement)
            {
                movement = Vector2.Normalize(movement);
                _raycastEngine.MovePlayer(movement, deltaTime);
            }

            // Rotation - use cached sensitivity
            if (_turnLeft || _turnRight)
            {
                float rotSpeed = 2.0f * deltaTime;
                if (_turnLeft)
                    _raycastEngine.RotatePlayer(rotSpeed);
                if (_turnRight)
                    _raycastEngine.RotatePlayer(-rotSpeed);
            }
        }

        private void UpdateFPS(float deltaTime)
        {
            if (_showFps)
            {
                _fpsFrameCount++;
                _fpsAccumulator += deltaTime;

                // Update FPS display once per second
                if (_fpsAccumulator >= 1.0)
                {
                    _lastFps = (int)(_fpsFrameCount / _fpsAccumulator);
                    _fpsFrameCount = 0;
                    _fpsAccumulator = 0.0;
                }
            }
        }

        private void UpdateHUD()
        {
            // Marshal UI updates to UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                // Quake 3 style - just the numbers
                HealthText.Text = _health.ToString();
                AmmoText.Text = _ammo.ToString();

                // Update FPS counter based on cached config
                FpsText.Visibility = _showFps ? Visibility.Visible : Visibility.Collapsed;

                if (_showFps && _lastFps > 0)
                {
                    FpsText.Text = $"{_lastFps}fps";
                }
            });
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

                            // Save config on exit
                            _config?.SaveConfig();

                            // Dispose renderer
                            _renderer?.Dispose();

                            Close();
                        }
                    }
                    break;
            }
        }

        private void UpdateConsoleDisplay()
        {
            // Use StringBuilder to reduce string allocations
            var history = _console.History;
            if (history.Count == 0)
            {
                ConsoleHistoryText.Text = string.Empty;
            }
            else if (history.Count == 1)
            {
                ConsoleHistoryText.Text = history[0];
            }
            else
            {
                var sb = new System.Text.StringBuilder(history.Count * 50); // Estimate capacity
                for (int i = 0; i < history.Count; i++)
                {
                    if (i > 0) sb.Append('\n');
                    sb.Append(history[i]);
                }
                ConsoleHistoryText.Text = sb.ToString();
            }

            // Auto-scroll to bottom
            ConsoleScrollViewer.UpdateLayout();
            ConsoleScrollViewer.ChangeView(null, ConsoleScrollViewer.ScrollableHeight, null);
        }
    }
}
