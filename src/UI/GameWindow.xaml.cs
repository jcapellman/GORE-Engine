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
        private GOREEngineInstance _engine;
        private bool _isGameLoopRunning;
        private bool _consoleVisible;
        private bool _criticalInitError;
        private System.Text.StringBuilder _initLog;
        private float _lastDeltaTime = 0.016f;
        private const float MAX_DELTA_TIME = 0.05f;
        private float _mouseSensitivity = 0.002f;
        private Vector2 _cachedRightVector;
        private Microsoft.UI.Dispatching.DispatcherQueueHandler _updateHudAction;
        private int _health = 100;
        private int _ammo = 50;
        private int _frameCount = 0;
        private bool _resourcesInitialized;
        private bool _isLoadingTextures;
        private Dictionary<int, string> _pendingTextureMapping;
        private Stopwatch _frameTimer;

        public GameWindow()
        {
            InitializeComponent();
            _initLog = new System.Text.StringBuilder();

            

            _updateHudAction = () =>
            {
                if (_engine != null && _engine.HudSystem != null)
                {
                    HealthText.Text = _engine.HudSystem.HealthText;
                    AmmoText.Text = _engine.HudSystem.AmmoText;
                    FpsText.Visibility = _engine.HudSystem.ShowFps ? Visibility.Visible : Visibility.Collapsed;
                    if (_engine.HudSystem.ShowFps)
                    {
                        FpsText.Text = _engine.HudSystem.FpsText;
                    }
                }
            };

            RootGrid.PointerPressed += RootGrid_PointerPressed;

            // Start engine initialization
            _ = InitializeEngineAsync();
        }

        private void RootGrid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_criticalInitError)
            {
                // Exit when user clicks after a fatal init error
                Close();
            }
        }

        private async System.Threading.Tasks.Task InitializeEngineAsync()
        {
            ExtendsContentIntoTitleBar = true;
            ScreenHelper.EnterFullScreenMode(this);
            InitScreen.Visibility = Visibility.Visible;
            await System.Threading.Tasks.Task.Delay(100);

            _initLog = new System.Text.StringBuilder();
            void log(string msg) { _initLog.AppendLine(msg); LogInit(msg); }
            void logError(string msg) { _initLog.AppendLine(msg); LogInit(msg); }

            _engine = await GOREEngine.CreateAndInitializeAsync(log, logError);
            if (_engine == null || _engine.CriticalInitError)
            {
                _criticalInitError = true;
                LogInit("");
                LogInit("FATAL: Cannot start without valid game systems");
                LogInit("");
                LogInit("Press any key or click to exit");
                return;
            }

            // Setup config change handlers
            _engine.ConfigSystem.SubscribeToChanges(v => OnRenderResolutionChanged(v), v => OnConfigChanged(v));
            _engine.ConfigSystem.ConfigValuesUpdated += () => UpdateCachedConfigValues();
            _engine.ConfigSystem.RenderResolutionChanged += (w, h) =>
            {
                try
                {
                    _engine.RendererSystem?.Dispose();
                    _engine.RendererSystem = new RendererSystem();
                    _engine.RendererSystem.Initialize(w, h, _engine.RaycastEngine);
                    if (ViewportCanvas?.Device != null)
                    {
                        _engine.RendererSystem.InitializeResources(ViewportCanvas.Device, (int)ViewportCanvas.Size.Width, (int)ViewportCanvas.Size.Height);
                        _resourcesInitialized = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to recreate renderer on resolution change: {ex.Message}");
                }
            };
            UpdateCachedConfigValues();

            // Setup frame timer
            _frameTimer = Stopwatch.StartNew();
            _isGameLoopRunning = true;

            LogInit("");
            LogInit("Initialization complete!");
            LogInit("");
            LogInit("Press ~ to open console");
            LogInit("Type 'help' for available commands");
            LogInit("");

            await System.Threading.Tasks.Task.Delay(2000);
            InitScreen.Visibility = Visibility.Collapsed;
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
            if (_engine != null && _engine.GameConsole != null)
            {
                _engine.GameConsole.AddToHistory(message);
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
            var console = _engine.GameConsole;
            // Register game-specific console commands
            console.RegisterGameCommands(_engine.RaycastEngine,
                health => _health = health,
                ammo => _ammo = ammo);

            // Register map loading command
            console.RegisterCommand("map", "Load a map (usage: map <mapname>)", async args =>
            {
                if (args.Length == 0)
                {
                    console.AddToHistory("Usage: map <mapname>");
                    console.AddToHistory("Example: map e1m1");
                    return;
                }

                var mapName = args[0];
                await LoadMapAsync(mapName);
            });

            // Register map listing command
            console.RegisterCommand("maps", "List available maps", args =>
            {
                try
                {
                    var baseDirectory = AppContext.BaseDirectory;
                    var mapsDirectory = System.IO.Path.Combine(baseDirectory, "gt1", "maps");

                    if (!Directory.Exists(mapsDirectory))
                    {
                        console.AddToHistory("Maps directory not found");
                        return;
                    }

                    var mapFiles = Directory.GetFiles(mapsDirectory, "*.map");

                    if (mapFiles.Length == 0)
                    {
                        console.AddToHistory("No maps found");
                        return;
                    }

                    console.AddToHistory("Available maps:");
                    foreach (var mapFile in mapFiles.OrderBy(f => f))
                    {
                        var mapName = Path.GetFileNameWithoutExtension(mapFile);
                        console.AddToHistory($"  {mapName}");
                    }
                    console.AddToHistory($"Total: {mapFiles.Length} map(s)");
                }
                catch (Exception ex)
                {
                    console.AddToHistory($"Error listing maps: {ex.Message}");
                }
            });

            // Register weapon commands
            console.RegisterCommand("weapon", "Switch weapon (usage: weapon <0-7>)", args =>
            {
                var ws = _engine.WeaponSystem;
                if (args.Length == 0)
                {
                    var current = ws.CurrentWeapon;
                    console.AddToHistory($"Current weapon: [{ws.CurrentWeaponIndex}] {current.Name}");
                    if (current.InfiniteAmmo)
                    {
                        console.AddToHistory($"  Ammo: Infinite");
                    }
                    else if (current.MagazineSize > 0)
                    {
                        console.AddToHistory($"  Magazine: {current.MagazineAmmo}/{current.MagazineSize}");
                        console.AddToHistory($"  Reserve: {current.CurrentAmmo}/{current.MaxAmmo}");
                    }
                    else
                    {
                        console.AddToHistory($"  Ammo: {current.CurrentAmmo}/{current.MaxAmmo}");
                    }
                    console.AddToHistory($"  Damage: {current.DamagePerRound}");
                    console.AddToHistory($"  Fire rate: {current.FireRate:F2}s");
                    console.AddToHistory($"  Ammo type: {current.AmmoType}");
                    console.AddToHistory("Usage: weapon <0-7>");
                    return;
                }

                if (int.TryParse(args[0], out int weaponIndex))
                {
                    if (ws.SwitchToWeapon(weaponIndex))
                    {
                        var weapon = ws.CurrentWeapon;
                        console.AddToHistory($"Switched to: [{weaponIndex}] {weapon.Name}");
                    }
                    else
                    {
                        console.AddToHistory($"Invalid weapon index: {weaponIndex} (valid: 0-7)");
                    }
                }
                else
                {
                    console.AddToHistory($"Invalid number: {args[0]}");
                }
            });

            console.RegisterCommand("weapons", "List all weapons", args =>
            {
                var ws = _engine.WeaponSystem;
                console.AddToHistory("Available weapons:");
                for (int i = 0; i < 8; i++)
                {
                    var weapon = ws.GetWeapon(i);
                    if (weapon == null) continue;

                    var marker = (i == ws.CurrentWeaponIndex) ? ">" : " ";

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

                    console.AddToHistory($"{marker} [{i}] {weapon.Name} - {ammoStr} - {weapon.DamagePerRound}dmg - {weapon.FireRate:F2}s");
                }
            });

            console.RegisterCommand("giveammo", "Give ammo to weapon (usage: giveammo <weapon> <amount>)", args =>
            {
                var ws = _engine.WeaponSystem;
                if (args.Length < 2)
                {
                    console.AddToHistory("Usage: giveammo <weapon> <amount>");
                    console.AddToHistory("Example: giveammo 1 50");
                    return;
                }

                if (int.TryParse(args[0], out int weaponIndex) && int.TryParse(args[1], out int amount))
                {
                    var weapon = ws.GetWeapon(weaponIndex);
                    if (weapon != null)
                    {
                        weapon.AddAmmo(amount);
                        console.AddToHistory($"Added {amount} ammo to {weapon.Name}");
                        console.AddToHistory($"  Ammo: {weapon.CurrentAmmo}/{weapon.MaxAmmo}");
                    }
                    else
                    {
                        console.AddToHistory($"Invalid weapon: {weaponIndex}");
                    }
                }
                else
                {
                    console.AddToHistory("Invalid arguments");
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
            return _engine.ConfigSystem.GetValue(name, defaultValue);
        }

        private void UpdateCachedConfigValues()
        {
            // ConfigSystem is guaranteed to be non-null after initialization
            _engine.HudSystem.SetShowFps(_engine.ConfigSystem.ShowFps);
            _mouseSensitivity = _engine.ConfigSystem.MouseSensitivity;
        }

        private void OnConfigChanged(ConfigVariable variable)
        {
            UpdateCachedConfigValues();
        }

        private void OnRenderResolutionChanged(ConfigVariable variable)
        {
            // Resolution change will require renderer recreation
            _engine.GameConsole?.AddToHistory($"{variable.Name} changed to {variable}");
            _engine.GameConsole?.AddToHistory("Resolution changes will take effect on map reload");
        }

        private void InitializeConsole()
        {
            var console = _engine.GameConsole;
            console.SetConfig(_engine.ConfigSystem.GetConfig());
            console.OnHistoryChanged += UpdateConsoleDisplay;
            console.OnHistoryChanged += UpdateConsoleDisplay;
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
                _engine.GameConsole.AddToHistory($"Loading map: {mapName}...");
                MapData mapData;
                try
                {
                    mapData = await _engine.MapSystem.LoadMapByNameAsync(mapName);
                    _engine.GameConsole.AddToHistory($"✓ Map '{mapData.Name}' loaded successfully");
                }
                catch (FileNotFoundException)
                {
                    _engine.GameConsole.AddToHistory($"✗ Map file not found: {mapName}.map");
                    _engine.GameConsole.AddToHistory($"  Searched in: gt1/maps/");
                    return;
                }
                catch (Exception ex)
                {
                    _engine.GameConsole.AddToHistory($"✗ Failed to load map: {ex.Message}");
                    return;
                }

                // Verify all texture files exist BEFORE loading
                _engine.GameConsole.AddToHistory("Verifying textures...");
                var missingTextures = _engine.MapSystem.VerifyTextures(mapData);
                if (missingTextures.Count > 0)
                {
                    _engine.GameConsole.AddToHistory("");
                    _engine.GameConsole.AddToHistory("✗ ERROR: Missing required texture files:");
                    foreach (var missing in missingTextures)
                    {
                        _engine.GameConsole.AddToHistory(missing);
                    }
                    _engine.GameConsole.AddToHistory("");
                    _engine.GameConsole.AddToHistory("Map load aborted");
                    return;
                }
                _engine.GameConsole.AddToHistory($"  All {mapData.TextureMapping.Count} textures verified");

                // Stop the game loop temporarily
                var wasRunning = _isGameLoopRunning;
                _isGameLoopRunning = false;

                // Dispose old renderer system
                _engine.RendererSystem?.Dispose();

                // Update raycasting engine with new map
                _engine.RaycastEngine = new RaycastEngine(mapData.Grid, _engine.EventSystem);
                _engine.RaycastEngine.PlayerPosition = mapData.PlayerStart;

                // Update renderer system with resolution from config
                int renderWidth = _engine.ConfigSystem.RenderWidth;
                int renderHeight = _engine.ConfigSystem.RenderHeight;
                _engine.RendererSystem = new RendererSystem();
                _engine.RendererSystem.Initialize(renderWidth, renderHeight, _engine.RaycastEngine);

                // Reset initialization flags
                _resourcesInitialized = false;
                _isLoadingTextures = false;

                // Initialize Win2D resources if canvas is ready
                if (ViewportCanvas.Device != null)
                {
                    _engine.RendererSystem.InitializeResources(ViewportCanvas.Device, (int)ViewportCanvas.Size.Width, (int)ViewportCanvas.Size.Height);
                    _resourcesInitialized = true;

                    // Load textures for the new map
                    _engine.GameConsole.AddToHistory("Loading textures...");
                    _isLoadingTextures = true; // Set loading flag
                    foreach (var texMapping in mapData.TextureMapping)
                    {
                        try
                        {
                            await _engine.RendererSystem.LoadTextureAsync(texMapping.Key, texMapping.Value, ViewportCanvas.Device);
                        }
                        catch (Exception ex)
                        {
                            _engine.GameConsole.AddToHistory($"  ✗ FATAL: Failed to load texture {texMapping.Value}: {ex.Message}");
                            _engine.GameConsole.AddToHistory("Map load aborted");
                            _isLoadingTextures = false;
                            return;
                        }
                    }
                    _isLoadingTextures = false;
                    _engine.GameConsole.AddToHistory($"  ✓ Loaded {mapData.TextureMapping.Count} textures");
                }
                else
                {
                    // Store for later loading
                    _pendingTextureMapping = mapData.TextureMapping;
                }

                // Re-register console commands with the new engine instance
                _engine.GameConsole.RegisterGameCommands(_engine.RaycastEngine,
                    health => _health = health,
                    ammo => _ammo = ammo);

                // Restart the game loop if it was running
                if (wasRunning)
                {
                    _isGameLoopRunning = true;
                }

                _engine.GameConsole.AddToHistory($"Map loaded: {mapData.Width}x{mapData.Height}");
                _engine.GameConsole.AddToHistory($"Player spawned at ({mapData.PlayerStart.X:F2}, {mapData.PlayerStart.Y:F2})");
            }
            catch (Exception ex)
            {
                _engine.GameConsole.AddToHistory($"✗ Unexpected error loading map: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Map load error: {ex}");
            }
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            RootGrid.Focus(FocusState.Programmatic);
        }

        private void ViewportCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            if (!_isGameLoopRunning || _engine.RendererSystem == null || _engine.RaycastEngine == null)
                return;

            // Initialize Win2D resources on first update if needed
            if (!_resourcesInitialized && _engine.RendererSystem.GetRenderTarget() == null)
            {
                try
                {
                    // Initialize render target to match the canvas size to avoid scaling artifacts
                    _engine.RendererSystem.InitializeResources(sender.Device, (int)sender.Size.Width, (int)sender.Size.Height);
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

            var input = _engine.InputSystem;
            var raycast = _engine.RaycastEngine;
            if (input.MoveForward) { movementInput += raycast.PlayerDirection; needMove = true; }
            if (input.MoveBackward) { movementInput -= raycast.PlayerDirection; needMove = true; }
            if (input.StrafeLeft || input.StrafeRight)
            {
                _cachedRightVector = new Vector2(raycast.PlayerDirection.Y, -raycast.PlayerDirection.X);
                if (input.StrafeLeft) { movementInput -= _cachedRightVector; needMove = true; }
                if (input.StrafeRight) { movementInput += _cachedRightVector; needMove = true; }
            }

            if (needMove)
            {
                movementInput = Vector2.Normalize(movementInput);
                raycast.MovePlayer(movementInput, deltaTime);
            }

            if (input.TurnLeft) raycast.RotatePlayer(2.0f * deltaTime);
            if (input.TurnRight) raycast.RotatePlayer(-2.0f * deltaTime);

            // Update doors
            raycast?.UpdateDoors(deltaTime);

            // Handle continuous firing
            var weaponSystem = _engine.WeaponSystem;
            var hudSystem = _engine.HudSystem;
            if (input.FireTriggerHeld)
            {
                if (weaponSystem?.TryFire() == true)
                {
                    // TODO: Add projectile/hitscan logic here
                }
            }

            // Auto-reload when magazine is empty and reserve ammo available
            if (weaponSystem?.CurrentWeapon?.NeedsReload() == true)
            {
                weaponSystem.Reload();
            }

            // Update weapon system
            weaponSystem?.Update(deltaTime);

            // Update FPS counter
            hudSystem.UpdateFPS(deltaTime);

            // Update HUD (less frequently to save performance)
            if (_frameCount % 5 == 0) // Update HUD every 5 frames
            {
                // Health
                hudSystem.UpdateHealth(_health);
                // Ammo
                string ammoStr;
                if (weaponSystem != null)
                {
                    var weapon = weaponSystem.CurrentWeapon;
                    if (weapon.InfiniteAmmo)
                        ammoStr = "∞";
                    else if (weapon.MagazineSize > 0)
                        ammoStr = weapon.MagazineAmmo.ToString();
                    else
                        ammoStr = weapon.CurrentAmmo.ToString();
                }
                else
                {
                    ammoStr = _ammo.ToString();
                }
                hudSystem.UpdateAmmo(ammoStr);

                if (hudSystem.NeedsUpdate)
                {
                    hudSystem.ResetNeedsUpdate();
                    DispatcherQueue.TryEnqueue(_updateHudAction);
                }
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
                        await _engine.RendererSystem.LoadTextureAsync(texMapping.Key, texMapping.Value, device);
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
                await _engine.WeaponSystem.LoadWeaponSpritesAsync(device);
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
            if (_engine.RendererSystem == null || _engine.RendererSystem.GetRenderTarget() == null || _isLoadingTextures)
            {
                // Draw loading screen
                args.DrawingSession.Clear(Windows.UI.Color.FromArgb(255, 0, 0, 0));
                return;
            }

            try
            {
                // Render the 3D scene, scaled to fill the canvas
                _engine.RendererSystem.Render(args.DrawingSession, (float)sender.Size.Width, (float)sender.Size.Height);

                // Render weapon sprite on top
                _engine.WeaponSystem?.Render(args.DrawingSession, (float)sender.Size.Width, (float)sender.Size.Height);
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
            _engine.InputSystem.HandleKey(e.Key, true);
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
            _engine.InputSystem.HandleKey(e.Key, false);
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
            var console = _engine.GameConsole;
            if (e.Key == VirtualKey.Enter)
            {
                var input = ConsoleInput.Text;
                ConsoleInput.Text = string.Empty;
                console.ExecuteCommand(input);
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Up)
            {
                ConsoleInput.Text = console.GetPreviousCommand();
                ConsoleInput.SelectionStart = ConsoleInput.Text.Length;
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Down)
            {
                ConsoleInput.Text = console.GetNextCommand();
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
            _engine.InputSystem?.HandleKey(key, isPressed);

            // Only handle one-shot actions on key press
            if (!isPressed) return;

            switch (key)
            {
                case VirtualKey.Space:
                    // Try to interact with door
                    bool doorFound = _engine.RaycastEngine?.TryInteractWithDoor() == true;
                    if (doorFound)
                    {
                        System.Diagnostics.Debug.WriteLine("Door interaction triggered!");
                        // Sound is now handled by RaycastEngine event
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No door found near player at ({_engine.RaycastEngine?.PlayerPosition.X:F2}, {_engine.RaycastEngine?.PlayerPosition.Y:F2})");
                    }
                    break;

                case VirtualKey.R:
                    _engine.WeaponSystem?.Reload();
                    break;

                case VirtualKey.Escape:
                    if (_consoleVisible)
                    {
                        ToggleConsole();
                    }
                    else
                    {
                        _isGameLoopRunning = false;
                        _engine.ConfigSystem?.Save();
                        _engine.RendererSystem?.Dispose();
                        _engine.WeaponSystem?.Dispose();
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
            var history = _engine.GameConsole.History;
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
