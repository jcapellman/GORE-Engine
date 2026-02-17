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
        private RendererSystem _rendererSystem;
        private bool _isGameLoopRunning;
        private GameConsole _console;
        // config is managed via ConfigSystem
        private bool _consoleVisible;
        private Dictionary<int, string> _pendingTextureMapping;
        private bool _isLoadingTextures;
        private bool _resourcesInitialized;
        private System.Text.StringBuilder _initLog;
        
        // If a critical error occurs during initialization, set this to true
        // and wait for the user to press any key or click before exiting.
        private bool _criticalInitError;

        private InputSystem _inputSystem;

        private Stopwatch _frameTimer;
        private int _health = 100;
        private int _ammo = 50;
        private WeaponSystem _weaponSystem;
        private MapSystem _mapSystem;
        private ConfigSystem _configSystem;
        private SoundEffectSystem _sfxSystem;
        private MusicSystem _musicSystem;

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

        // Cached HUD strings to reduce allocations
        private string _cachedHealthText = "100";
        private string _cachedAmmoText = "50";
        private string _cachedFpsText = "60fps";
        private bool _hudNeedsUpdate = true;

        // Cached dispatcher action to avoid lambda allocations
        private Microsoft.UI.Dispatching.DispatcherQueueHandler _updateHudAction;

        public GameWindow()
        {
            InitializeComponent();
            _initLog = new System.Text.StringBuilder();

            

            // Initialize cached dispatcher action
            _updateHudAction = () =>
            {
                HealthText.Text = _cachedHealthText;
                AmmoText.Text = _cachedAmmoText;
                FpsText.Visibility = _showFps ? Visibility.Visible : Visibility.Collapsed;
                if (_showFps)
                {
                    FpsText.Text = _cachedFpsText;
                }

            };

            // Pointer press handler for dismissing fatal init errors
            RootGrid.PointerPressed += RootGrid_PointerPressed;

            // Initialize InputSystem and wire up weapon cycling
            _inputSystem = new InputSystem();
            // Weapon system will be initialized later; defer wiring until after subsystems created

            // Start initialization sequence
            _ = RunInitializationSequenceAsync();
        }

        private void RootGrid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_criticalInitError)
            {
                // Exit when user clicks after a fatal init error
                Close();
            }
        }

        private async System.Threading.Tasks.Task RunInitializationSequenceAsync()
        {
            try
            {
                ExtendsContentIntoTitleBar = true;
                ScreenHelper.EnterFullScreenMode(this);

                // Ensure init screen is visible
                InitScreen.Visibility = Visibility.Visible;

                await System.Threading.Tasks.Task.Delay(100); // Let UI render

                // Initialize Config
                LogInit("Initializing configuration system...");
                _configSystem = new ConfigSystem(); // Initialize ConfigSystem
                _configSystem.Load(); // Load configuration (throws on failure)
                LogInit($"  Config loaded"); // Log config loaded message
                // Validate and subscribe using ConfigSystem to handle changes
                _configSystem.ValidateAndClamp(); // Validate config values
                _configSystem.SubscribeToChanges(v => OnRenderResolutionChanged(v), v => OnConfigChanged(v)); // Subscribe to changes
                // Subscribe to config value updates for cached values to keep them updated
                _configSystem.ConfigValuesUpdated += () => UpdateCachedConfigValues(); // Update cached values on change
                // Subscribe to typed render resolution change to recreate renderer automatically
                _configSystem.RenderResolutionChanged += (w, h) =>
                {
                    // Recreate renderer with new resolution
                    try
                    {
                        _rendererSystem?.Dispose();
                        _rendererSystem = new RendererSystem();
                        _rendererSystem.Initialize(w, h, _raycastEngine);
                        if (ViewportCanvas?.Device != null)
                        {
                            _rendererSystem.InitializeResources(ViewportCanvas.Device, (int)ViewportCanvas.Size.Width, (int)ViewportCanvas.Size.Height);
                            _resourcesInitialized = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to recreate renderer on resolution change: {ex.Message}");
                    }
                };
                UpdateCachedConfigValues(); // Initial update of cached values

                // Initialize Console
                LogInit("Initializing game console...");
                InitializeConsole();
                LogInit("  Console ready");

                // Check directories
                LogInit("Checking game directories...");
                var baseDirectory = AppContext.BaseDirectory;
                CheckDirectory(Path.Combine(baseDirectory, "gt1"));
                CheckDirectory(Path.Combine(baseDirectory, "gt1", "maps"));
                CheckDirectory(Path.Combine(baseDirectory, "gt1", "textures"));
                CheckDirectory(Path.Combine(baseDirectory, "gt1", "hud"));

                // Load custom font
                LogInit("Loading custom font...");
                await LoadCustomFontAsync();

                // Load HUD icons
                LogInit("Loading HUD icons...");
                await LoadHudIconsAsync();

                // Initialize map
                LogInit("Initializing map system...");
                _mapSystem = new MapSystem();
                try
                {
                    var mapData = await _mapSystem.LoadInitialMapAsync("e1m1");
                    LogInit($"  Map: {mapData.Name}");
                    LogInit($"  Dimensions: {mapData.Width}x{mapData.Height}");
                    LogInit($"  Textures defined: {mapData.TextureMapping.Count}");
                }
                catch (Exception ex)
                {
                    LogInit($"ERROR: Failed to load map - {ex.Message}");
                    LogInit("FATAL: Cannot start without valid map");
                    _criticalInitError = true;
                    LogInit("");
                    LogInit("Press any key or click to exit");
                    return;
                }

                // Verify all texture files exist
                LogInit("Verifying texture files...");
                var missingTextures = new List<string>();
                var mapDataLocal = _mapSystem.CurrentMap; // map system exposes loaded map
                foreach (var texMapping in mapDataLocal.TextureMapping)
                {
                    var texturePath = Path.Combine(baseDirectory, texMapping.Value);
                    if (!File.Exists(texturePath))
                    {
                        missingTextures.Add($"  Texture {texMapping.Key}: {texMapping.Value}");
                    }
                    else
                    {
                        LogInit($"  Texture {texMapping.Key}: {texMapping.Value} - OK");
                    }
                }

                if (missingTextures.Count > 0)
                {
                    LogInit("");
                    LogInit("ERROR: Missing required texture files:");
                    foreach (var missing in missingTextures)
                    {
                        LogInit(missing);
                    }
                    LogInit("");
                    LogInit("FATAL: Cannot start with missing textures");
                    // Enter critical error state and wait for user to dismiss
                    _criticalInitError = true;
                    LogInit("");
                    LogInit("Press any key or click to exit");
                    return;
                }

                // Initialize raycasting engine
                LogInit("Initializing raycasting engine...");
                var map = _mapSystem.CurrentMap;
                _raycastEngine = new RaycastEngine(map.Grid);
                _raycastEngine.PlayerPosition = map.PlayerStart;
                LogInit($"  Player spawned at ({map.PlayerStart.X:F2}, {map.PlayerStart.Y:F2})");

                // Initialize renderer system
                LogInit("Initializing 3D renderer...");
                int renderWidth = _configSystem.RenderWidth;
                int renderHeight = _configSystem.RenderHeight;
                _rendererSystem = new RendererSystem();
                _rendererSystem.Initialize(renderWidth, renderHeight, _raycastEngine);
                LogInit($"  Resolution: {renderWidth}x{renderHeight}");
                LogInit("  Win2D hardware acceleration enabled");

                // Store texture mapping for later
                _pendingTextureMapping = _mapSystem.CurrentMap.TextureMapping; 

                // Initialize systems (weapons, audio, sfx)
                LogInit("Initializing subsystem managers...");
                _weaponSystem = new WeaponSystem();
                _weaponSystem.LoadWeaponsConfig();
                _sfxSystem = new SoundEffectSystem();
                _musicSystem = new MusicSystem();
                // Now wire input system to weapons
                _inputSystem.NextWeaponRequested = () => _weaponSystem?.NextWeapon();
                _inputSystem.PreviousWeaponRequested = () => _weaponSystem?.PreviousWeapon();
                LogInit("  Subsystems initialized");

                // Register console commands
                LogInit("Registering console commands...");
                RegisterConsoleCommands();
                LogInit("  Commands registered");

                // Setup frame timer
                _frameTimer = Stopwatch.StartNew();
                _isGameLoopRunning = true;

                LogInit("");
                LogInit("Initialization complete!");
                LogInit("");
                LogInit("Press ~ to open console");
                LogInit("Type 'help' for available commands");
                LogInit("");

                // Pause for effect (BUILD engine style)
                await System.Threading.Tasks.Task.Delay(2000);

                // Hide init screen and show game
                InitScreen.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LogInit("");
                LogInit($"FATAL ERROR: {ex.Message}");
                LogInit("");
                LogInit("Press any key or click to exit");
                System.Diagnostics.Debug.WriteLine($"Initialization failed: {ex}");

                // Enter critical error state and wait for user to dismiss
                _criticalInitError = true;
                return;
            }
        }

        private void LogInit(string message)
        {
            _initLog.AppendLine(message);

            // Update UI on UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                InitMessages.Text = _initLog.ToString();
            });

            // Also add to console if it's initialized
            if (_console != null)
            {
                _console.AddToHistory(message);
            }

            System.Diagnostics.Debug.WriteLine(message);
        }

        private void CheckDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                LogInit($"  {path} - OK");
            }
            else
            {
                LogInit($"  {path} - NOT FOUND");
            }
        }

        private void RegisterConsoleCommands()
        {
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

            // Register weapon commands
            _console.RegisterCommand("weapon", "Switch weapon (usage: weapon <0-7>)", args =>
            {
                if (args.Length == 0)
                {
                    var current = _weaponSystem.CurrentWeapon;
                    _console.AddToHistory($"Current weapon: [{_weaponSystem.CurrentWeaponIndex}] {current.Name}");
                    if (current.InfiniteAmmo)
                    {
                        _console.AddToHistory($"  Ammo: Infinite");
                    }
                    else if (current.MagazineSize > 0)
                    {
                        _console.AddToHistory($"  Magazine: {current.MagazineAmmo}/{current.MagazineSize}");
                        _console.AddToHistory($"  Reserve: {current.CurrentAmmo}/{current.MaxAmmo}");
                    }
                    else
                    {
                        _console.AddToHistory($"  Ammo: {current.CurrentAmmo}/{current.MaxAmmo}");
                    }
                    _console.AddToHistory($"  Damage: {current.DamagePerRound}");
                    _console.AddToHistory($"  Fire rate: {current.FireRate:F2}s");
                    _console.AddToHistory($"  Ammo type: {current.AmmoType}");
                    _console.AddToHistory("Usage: weapon <0-7>");
                    return;
                }

                if (int.TryParse(args[0], out int weaponIndex))
                {
                    if (_weaponSystem.SwitchToWeapon(weaponIndex))
                    {
                        var weapon = _weaponSystem.CurrentWeapon;
                        _console.AddToHistory($"Switched to: [{weaponIndex}] {weapon.Name}");
                    }
                    else
                    {
                        _console.AddToHistory($"Invalid weapon index: {weaponIndex} (valid: 0-7)");
                    }
                }
                else
                {
                    _console.AddToHistory($"Invalid number: {args[0]}");
                }
            });

            _console.RegisterCommand("weapons", "List all weapons", args =>
            {
                _console.AddToHistory("Available weapons:");
                for (int i = 0; i < 8; i++)
                {
                    var weapon = _weaponSystem.GetWeapon(i);
                    if (weapon == null) continue;

                    var marker = (i == _weaponSystem.CurrentWeaponIndex) ? ">" : " ";

                    string ammoStr;
                    if (weapon.InfiniteAmmo)
                    {
                        ammoStr = "∞";
                    }
                    else if (weapon.MagazineSize > 0)
                    {
                        ammoStr = $"{weapon.MagazineAmmo}/{weapon.MagazineSize} ({weapon.CurrentAmmo})";
                    }
                    else
                    {
                        ammoStr = $"{weapon.CurrentAmmo}/{weapon.MaxAmmo}";
                    }

                    _console.AddToHistory($"{marker} [{i}] {weapon.Name} - {ammoStr} - {weapon.DamagePerRound}dmg - {weapon.FireRate:F2}s");
                }
            });

            _console.RegisterCommand("giveammo", "Give ammo to weapon (usage: giveammo <weapon> <amount>)", args =>
            {
                if (args.Length < 2)
                {
                    _console.AddToHistory("Usage: giveammo <weapon> <amount>");
                    _console.AddToHistory("Example: giveammo 1 50");
                    return;
                }

                if (int.TryParse(args[0], out int weaponIndex) && int.TryParse(args[1], out int amount))
                {
                    var weapon = _weaponSystem.GetWeapon(weaponIndex);
                    if (weapon != null)
                    {
                        weapon.AddAmmo(amount);
                        _console.AddToHistory($"Added {amount} ammo to {weapon.Name}");
                        _console.AddToHistory($"  Ammo: {weapon.CurrentAmmo}/{weapon.MaxAmmo}");
                    }
                    else
                    {
                        _console.AddToHistory($"Invalid weapon: {weaponIndex}");
                    }
                }
                else
                {
                    _console.AddToHistory("Invalid arguments");
                }
            });
        }

        // Config handled by ConfigSystem; helpers removed

        /// <summary>
        /// Safely get a config value with fallback default
        /// </summary>
        private T GetConfigValue<T>(string name, T defaultValue)
        {
            // ConfigSystem is guaranteed to be non-null after initialization
            return _configSystem.GetValue(name, defaultValue);
        }

        private void UpdateCachedConfigValues()
        {
            // ConfigSystem is guaranteed to be non-null after initialization
            _showFps = _configSystem.ShowFps;
            _mouseSensitivity = _configSystem.MouseSensitivity;
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
            _console.SetConfig(_configSystem.GetConfig());
            _console.OnHistoryChanged += UpdateConsoleDisplay;
            _console.OnHistoryChanged += UpdateConsoleDisplay;
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

                    LogInit($"  Custom font loaded from: {fontPath}");
                }
                else
                {
                    LogInit($"  Font not found: {fontPath} - using default");
                }
            }
            catch (Exception ex)
            {
                LogInit($"  Failed to load custom font: {ex.Message}");
            }

            await System.Threading.Tasks.Task.CompletedTask;
        }

        private async System.Threading.Tasks.Task LoadHudIconsAsync()
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var healthIconPath = Path.Combine(baseDirectory, "gt1", "hud", "health.png");
                var ammoIconPath = Path.Combine(baseDirectory, "gt1", "hud", "ammo.png");

                var missingIcons = new List<string>();

                // Check health icon
                if (File.Exists(healthIconPath))
                {
                    var healthUri = new Uri($"file:///{healthIconPath.Replace("\\", "/")}");
                    var healthBitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(healthUri);
                    HealthIcon.Source = healthBitmap;
                    LogInit($"  Health icon loaded: gt1/hud/health.png");
                }
                else
                {
                    missingIcons.Add("  gt1/hud/health.png");
                }

                // Check ammo icon
                if (File.Exists(ammoIconPath))
                {
                    var ammoUri = new Uri($"file:///{ammoIconPath.Replace("\\", "/")}");
                    var ammoBitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(ammoUri);
                    AmmoIcon.Source = ammoBitmap;
                    LogInit($"  Ammo icon loaded: gt1/hud/ammo.png");
                }
                else
                {
                    missingIcons.Add("  gt1/hud/ammo.png");
                }

                if (missingIcons.Count > 0)
                {
                    LogInit("");
                    LogInit("WARNING: Missing HUD icon files:");
                    foreach (var missing in missingIcons)
                    {
                        LogInit(missing);
                    }
                    LogInit("  HUD will display without icons");
                }
            }
            catch (Exception ex)
            {
                LogInit($"  Failed to load HUD icons: {ex.Message}");
                LogInit("  HUD will display without icons");
            }

            await System.Threading.Tasks.Task.CompletedTask;
        }

        private async System.Threading.Tasks.Task LoadMapAsync(string mapName)
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var mapPath = System.IO.Path.Combine(baseDirectory, "gt1", "maps", $"{mapName}.map");

                _console.AddToHistory($"Loading map: {mapName}...");

                MapData mapData;
                try
                {
                    mapData = await MapLoader.LoadMapAsync(mapPath);
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

                // Verify all texture files exist BEFORE loading
                _console.AddToHistory("Verifying textures...");
                var missingTextures = new List<string>();
                foreach (var texMapping in mapData.TextureMapping)
                {
                    var texturePath = Path.Combine(baseDirectory, texMapping.Value);
                    if (!File.Exists(texturePath))
                    {
                        missingTextures.Add($"  Texture {texMapping.Key}: {texMapping.Value}");
                    }
                }

                if (missingTextures.Count > 0)
                {
                    _console.AddToHistory("");
                    _console.AddToHistory("✗ ERROR: Missing required texture files:");
                    foreach (var missing in missingTextures)
                    {
                        _console.AddToHistory(missing);
                    }
                    _console.AddToHistory("");
                    _console.AddToHistory("Map load aborted");
                    return;
                }

                _console.AddToHistory($"  All {mapData.TextureMapping.Count} textures verified");

                // Stop the game loop temporarily
                var wasRunning = _isGameLoopRunning;
                _isGameLoopRunning = false;

                // Dispose old renderer system
                _rendererSystem?.Dispose();

                // Update raycasting engine with new map
                _raycastEngine = new RaycastEngine(mapData.Grid);
                _raycastEngine.PlayerPosition = mapData.PlayerStart;

                // Update renderer system with resolution from config
                int renderWidth = _configSystem.RenderWidth;
                int renderHeight = _configSystem.RenderHeight;
                _rendererSystem = new RendererSystem();
                _rendererSystem.Initialize(renderWidth, renderHeight, _raycastEngine);

                // Reset initialization flags
                _resourcesInitialized = false;
                _isLoadingTextures = false;

                // Initialize Win2D resources if canvas is ready
                if (ViewportCanvas.Device != null)
                {
                    _rendererSystem.InitializeResources(ViewportCanvas.Device, (int)ViewportCanvas.Size.Width, (int)ViewportCanvas.Size.Height);
                    _resourcesInitialized = true;

                    // Load textures for the new map
                    _console.AddToHistory("Loading textures...");
                            _isLoadingTextures = true; // Set loading flag
                    foreach (var texMapping in mapData.TextureMapping)
                    {
                        try
                        {
                            await _rendererSystem.LoadTextureAsync(texMapping.Key, texMapping.Value, ViewportCanvas.Device);
                        }
                        catch (Exception ex)
                        {
                            _console.AddToHistory($"  ✗ FATAL: Failed to load texture {texMapping.Value}: {ex.Message}");
                            _console.AddToHistory("Map load aborted");
                            _isLoadingTextures = false;
                            return;
                        }
                    }
                    _isLoadingTextures = false;
                    _console.AddToHistory($"  ✓ Loaded {mapData.TextureMapping.Count} textures");
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
            if (!_isGameLoopRunning || _rendererSystem == null || _raycastEngine == null)
                return;

            // Initialize Win2D resources on first update if needed
            if (!_resourcesInitialized && _rendererSystem.GetRenderTarget() == null)
            {
                try
                {
                    // Initialize render target to match the canvas size to avoid scaling artifacts
                    _rendererSystem.InitializeResources(sender.Device, (int)sender.Size.Width, (int)sender.Size.Height);
                    _resourcesInitialized = true;

                    // Start loading textures asynchronously
                    if (_pendingTextureMapping != null && !_isLoadingTextures)
                    {
                        _isLoadingTextures = true;
                            _ = LoadTexturesAsync(sender.Device); // Start loading textures
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

            // Update player movement using InputSystem
            Vector2 movementInput = Vector2.Zero;
            bool needMove = false;

            if (_inputSystem.MoveForward) { movementInput += _raycastEngine.PlayerDirection; needMove = true; }
            if (_inputSystem.MoveBackward) { movementInput -= _raycastEngine.PlayerDirection; needMove = true; }
            if (_inputSystem.StrafeLeft || _inputSystem.StrafeRight)
            {
                _cachedRightVector = new Vector2(_raycastEngine.PlayerDirection.Y, -_raycastEngine.PlayerDirection.X);
                if (_inputSystem.StrafeLeft) { movementInput -= _cachedRightVector; needMove = true; }
                if (_inputSystem.StrafeRight) { movementInput += _cachedRightVector; needMove = true; }
            }

            if (needMove)
            {
                movementInput = Vector2.Normalize(movementInput);
                _raycastEngine.MovePlayer(movementInput, deltaTime);
            }

            if (_inputSystem.TurnLeft) _raycastEngine.RotatePlayer(2.0f * deltaTime);
            if (_inputSystem.TurnRight) _raycastEngine.RotatePlayer(-2.0f * deltaTime);

            // Update doors
            _raycastEngine?.UpdateDoors(deltaTime);

            // Handle continuous firing
            if (_inputSystem.FireTriggerHeld)
            {
                if (_weaponSystem?.TryFire() == true)
                {
                    _hudNeedsUpdate = true;
                    // TODO: Add projectile/hitscan logic here
                }
            }

            // Auto-reload when magazine is empty and reserve ammo available
            if (_weaponSystem?.CurrentWeapon?.NeedsReload() == true)
            {
                _weaponSystem.Reload();
            }

            // Update weapon system
            _weaponSystem?.Update(deltaTime);

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
                        await _rendererSystem.LoadTextureAsync(texMapping.Key, texMapping.Value, device);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"FATAL: Failed to load texture {texMapping.Value}: {ex.Message}");
                        _isLoadingTextures = false;
                        _isGameLoopRunning = false;

                        // Show error on init screen if still visible
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            LogInit("");
                            LogInit($"FATAL ERROR: Failed to load texture {texMapping.Value}");
                            LogInit($"  {ex.Message}");
                            LogInit("");
                            LogInit("Cannot continue - missing required texture");
                            LogInit("");
                            LogInit("Press any key or click to exit");
                            _criticalInitError = true;
                        });

                        return;
                    }
                }

                _pendingTextureMapping = null;

                // Load weapon sprites
                System.Diagnostics.Debug.WriteLine("Loading weapon sprites...");
                await _weaponSystem.LoadWeaponSpritesAsync(device);
                System.Diagnostics.Debug.WriteLine("✓ Weapon sprites loaded");

                _isLoadingTextures = false;
                System.Diagnostics.Debug.WriteLine("✓ All textures loaded");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading textures: {ex.Message}");
                _isLoadingTextures = false;
                _isGameLoopRunning = false;

                DispatcherQueue.TryEnqueue(() =>
                {
                    LogInit("");
                    LogInit($"FATAL ERROR: Texture loading failed");
                    LogInit($"  {ex.Message}");
                    LogInit("");
                    LogInit("Press any key or click to exit");
                    _criticalInitError = true;
                });

                return;
            }
        }

        private void ViewportCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
                // Don't render if resources aren't initialized or textures are still loading
            if (_rendererSystem == null || _rendererSystem.GetRenderTarget() == null || _isLoadingTextures)
            {
                // Draw loading screen
                args.DrawingSession.Clear(Windows.UI.Color.FromArgb(255, 0, 0, 0));
                return;
            }

            try
            {
                // Render the 3D scene, scaled to fill the canvas
                _rendererSystem.Render(args.DrawingSession, (float)sender.Size.Width, (float)sender.Size.Height);

                // Render weapon sprite on top
                _weaponSystem?.Render(args.DrawingSession, (float)sender.Size.Width, (float)sender.Size.Height);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Render error: {ex.Message}");
            }
        }

        private void UpdatePlayerMovement(float deltaTime)
        {
            // Now handled inline in the update loop via InputSystem
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

                    // Cache the FPS string to avoid allocations during HUD update
                    _cachedFpsText = $"{_lastFps}fps";
                    _hudNeedsUpdate = true;
                }
            }
        }

        private void UpdateHUD()
        {
            // Only update cached strings if values have changed
            bool healthChanged = false;
            bool ammoChanged = false;

            var healthStr = _health.ToString();
            if (_cachedHealthText != healthStr)
            {
                _cachedHealthText = healthStr;
                healthChanged = true;
            }

            // Show current weapon's magazine ammo (or total if no magazine system)
            string ammoStr;
            if (_weaponSystem != null)
            {
                var weapon = _weaponSystem.CurrentWeapon;
                if (weapon.InfiniteAmmo)
                {
                    ammoStr = "∞";
                }
                else if (weapon.MagazineSize > 0)
                {
                    ammoStr = weapon.MagazineAmmo.ToString();
                }
                else
                {
                    ammoStr = weapon.CurrentAmmo.ToString();
                }
            }
            else
            {
                ammoStr = _ammo.ToString();
            }

            if (_cachedAmmoText != ammoStr)
            {
                _cachedAmmoText = ammoStr;
                ammoChanged = true;
            }

            // Only marshal to UI thread if something changed
            if (healthChanged || ammoChanged || _hudNeedsUpdate)
            {
                _hudNeedsUpdate = false;
                DispatcherQueue.TryEnqueue(_updateHudAction);
            }
        }

        private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (_criticalInitError)
            {
                // Any key press after a critical init error will exit
                Close();
                return;
            }

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

            // Delegate to input system
            _inputSystem.HandleKey(e.Key, true);
            e.Handled = true;
        }

        private void RootGrid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (_criticalInitError)
            {
                Close();
                e.Handled = true;
                return;
            }
            // Don't process game input if console is open
            if (_consoleVisible)
            {
                e.Handled = false;
                return;
            }

            // Delegate to input system
            _inputSystem.HandleKey(e.Key, false);
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
            if (_criticalInitError)
            {
                Close();
                e.Handled = true;
                return;
            }
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
            // Delegate continuous/hold input to InputSystem
            _inputSystem?.HandleKey(key, isPressed);

            // Only handle one-shot actions on key press
            if (!isPressed) return;

            switch (key)
            {
                case VirtualKey.Space:
                    // Try to interact with door
                    bool doorFound = _raycastEngine?.TryInteractWithDoor() == true;
                    if (doorFound)
                    {
                        System.Diagnostics.Debug.WriteLine("Door interaction triggered!");
                        _sfxSystem?.Play("door");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No door found near player at ({_raycastEngine?.PlayerPosition.X:F2}, {_raycastEngine?.PlayerPosition.Y:F2})");
                    }
                    break;

                case VirtualKey.R:
                    _weaponSystem?.Reload();
                    break;

                case VirtualKey.Number1: _weaponSystem?.SwitchToWeapon(0); break;
                case VirtualKey.Number2: _weaponSystem?.SwitchToWeapon(1); break;
                case VirtualKey.Number3: _weaponSystem?.SwitchToWeapon(2); break;
                case VirtualKey.Number4: _weaponSystem?.SwitchToWeapon(3); break;
                case VirtualKey.Number5: _weaponSystem?.SwitchToWeapon(4); break;
                case VirtualKey.Number6: _weaponSystem?.SwitchToWeapon(5); break;
                case VirtualKey.Number7: _weaponSystem?.SwitchToWeapon(6); break;
                case VirtualKey.Number8: _weaponSystem?.SwitchToWeapon(7); break;

                case VirtualKey.Q:
                    _weaponSystem?.PreviousWeapon();
                    break;
                case VirtualKey.E:
                    _weaponSystem?.NextWeapon();
                    break;

                case VirtualKey.Escape:
                    if (_consoleVisible)
                    {
                        ToggleConsole();
                    }
                    else
                    {
                        _isGameLoopRunning = false;
                        _configSystem?.Save();
                        _rendererSystem?.Dispose();
                        _weaponSystem?.Dispose();
                        Close();
                    }
                    break;
            }
        }

        private void UpdateConsoleDisplay()
        {
            // Skip update if console is not visible (performance optimization)
            if (!_consoleVisible && ConsoleOverlay.Visibility != Visibility.Visible)
            {
                return;
            }

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
