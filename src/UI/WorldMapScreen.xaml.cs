using GORE.GameEngine;
using GORE.Models;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

namespace GORE.UI
{
    public sealed partial class WorldMapScreen : Window
    {
        private readonly Window _mainWindow;
        private TileMapRenderer tileMapRenderer;
        private WorldMap worldMap;
        private List<WorldMapTerrain> terrainTypes;
        private Character player;
        private List<Character> party;
        private DispatcherQueueTimer gameLoopTimer;
        private Random random = new Random();

        private bool pendingMoveUp;
        private bool pendingMoveDown;
        private bool pendingMoveLeft;
        private bool pendingMoveRight;
        private int steps;
        private int moveDelay = 0;
        private const int MoveDelayFrames = 8;

        public WorldMapScreen(Window mainWindow, Character playerCharacter)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            player = playerCharacter;

            // Create a party (for now just the player, can expand later)
            party = new List<Character> { playerCharacter };

            // Add two more party members for demo
            var warrior = new Character("Warrior") { Level = 1, MaxHP = 80, CurrentHP = 80, Attack = 15, Defense = 12, MaxMP = 10, CurrentMP = 10 };
            var mage = new Character("Mage") { Level = 1, MaxHP = 50, CurrentHP = 50, Attack = 8, Defense = 5, Magic = 20, MaxMP = 50, CurrentMP = 50 };
            party.Add(warrior);
            party.Add(mage);

            ExtendsContentIntoTitleBar = true;
            ScreenHelper.EnterFullScreenMode(this);

            _ = LoadEnemyDataAsync();
            _ = LoadWorldMapAsync();
            _ = LoadTerrainTypesAsync();

            SetupGameLoop();
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            RootGrid.Focus(FocusState.Programmatic);
            System.Diagnostics.Debug.WriteLine("WorldMapScreen loaded and focused");
        }

        private async Task LoadWorldMapAsync()
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var mapPath = Path.Combine(baseDirectory, "Assets", "Maps", "WorldMap.json");

                System.Diagnostics.Debug.WriteLine($"=== LOADING WORLD MAP ===");
                System.Diagnostics.Debug.WriteLine($"Base Directory: {baseDirectory}");
                System.Diagnostics.Debug.WriteLine($"Map Path: {mapPath}");
                System.Diagnostics.Debug.WriteLine($"File Exists: {File.Exists(mapPath)}");

                if (File.Exists(mapPath))
                {
                    var json = await File.ReadAllTextAsync(mapPath);
                    System.Diagnostics.Debug.WriteLine($"JSON Content Length: {json.Length}");
                    System.Diagnostics.Debug.WriteLine($"JSON First 100 chars: {json.Substring(0, Math.Min(100, json.Length))}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    worldMap = JsonSerializer.Deserialize<WorldMap>(json, options);

                    System.Diagnostics.Debug.WriteLine($"✓ Loaded world map: {worldMap?.Name}");
                    System.Diagnostics.Debug.WriteLine($"  Width: {worldMap?.Width}, Height: {worldMap?.Height}");
                    System.Diagnostics.Debug.WriteLine($"  Locations: {worldMap?.Locations?.Count ?? 0}");

                    if (worldMap?.Locations != null)
                    {
                        foreach (var loc in worldMap.Locations)
                        {
                            System.Diagnostics.Debug.WriteLine($"    - {loc.Name} at ({loc.X}, {loc.Y}) [{loc.Type}]");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✗ World map not found at: {mapPath}");

                    // List what files ARE in Assets/Maps
                    var mapsDir = Path.Combine(baseDirectory, "Assets", "Maps");
                    if (Directory.Exists(mapsDir))
                    {
                        System.Diagnostics.Debug.WriteLine($"Files in {mapsDir}:");
                        foreach (var file in Directory.GetFiles(mapsDir))
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {Path.GetFileName(file)}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Directory doesn't exist: {mapsDir}");
                    }

                    CreateDefaultWorldMap();
                }

                UpdateLocationDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error loading world map: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                CreateDefaultWorldMap();
            }
        }

        private async Task LoadTerrainTypesAsync()
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var terrainPath = Path.Combine(baseDirectory, "Assets", "Maps", "TerrainTypes.json");

                System.Diagnostics.Debug.WriteLine($"=== LOADING TERRAIN TYPES ===");
                System.Diagnostics.Debug.WriteLine($"Terrain Path: {terrainPath}");
                System.Diagnostics.Debug.WriteLine($"File Exists: {File.Exists(terrainPath)}");

                if (File.Exists(terrainPath))
                {
                    var json = await File.ReadAllTextAsync(terrainPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    terrainTypes = JsonSerializer.Deserialize<List<WorldMapTerrain>>(json, options);

                    System.Diagnostics.Debug.WriteLine($"✓ Loaded {terrainTypes?.Count ?? 0} terrain types");

                    if (terrainTypes != null)
                    {
                        foreach (var terrain in terrainTypes)
                        {
                            System.Diagnostics.Debug.WriteLine($"    - {terrain.Name} (ID: {terrain.Id}, Texture: {terrain.Texture})");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✗ TerrainTypes.json not found at: {terrainPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error loading terrain types: {ex.Message}");
            }
        }

        private async Task LoadEnemyDataAsync()
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var enemyPath = Path.Combine(baseDirectory, "Assets", "Data", "Enemies.json");

                System.Diagnostics.Debug.WriteLine($"=== LOADING ENEMY DATA ===");
                System.Diagnostics.Debug.WriteLine($"Enemy Path: {enemyPath}");

                if (File.Exists(enemyPath))
                {
                    var json = await File.ReadAllTextAsync(enemyPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var enemies = JsonSerializer.Deserialize<List<EnemyData>>(json, options);
                    EnemyDatabase.Load(enemies);

                    System.Diagnostics.Debug.WriteLine($"✓ Loaded {enemies?.Count ?? 0} enemy types");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Enemies.json not found at: {enemyPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error loading enemy data: {ex.Message}");
            }
        }

        private void CreateDefaultWorldMap()
        {
            throw new InvalidOperationException(
                "WorldMap.json is required but was not found or failed to load!\n" +
                "Ensure WorldMap.json exists in test/Assets/Maps/ and is copied to output directory.");
        }

        private void SetupGameLoop()
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            gameLoopTimer = dispatcherQueue.CreateTimer();
            gameLoopTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameLoopTimer.Tick += GameLoop_Tick;
            gameLoopTimer.Start();
        }

        private void GameLoop_Tick(object sender, object e)
        {
            if (tileMapRenderer == null) return;

            moveDelay++;

            if (moveDelay >= MoveDelayFrames)
            {
                bool moved = false;

                if (pendingMoveUp && tileMapRenderer.MovePlayer(0, -1))
                {
                    moved = true;
                }
                else if (pendingMoveDown && tileMapRenderer.MovePlayer(0, 1))
                {
                    moved = true;
                }
                else if (pendingMoveLeft && tileMapRenderer.MovePlayer(-1, 0))
                {
                    moved = true;
                }
                else if (pendingMoveRight && tileMapRenderer.MovePlayer(1, 0))
                {
                    moved = true;
                }

                if (moved)
                {
                    steps++;
                    moveDelay = 0;
                    UpdateLocationDisplay();
                    CheckForEncounter();
                }
            }

            // Update animations (water effects, etc.)
            tileMapRenderer.Update(0.016f); // ~60 FPS

            MapCanvas.Invalidate();
            MinimapCanvas?.Invalidate(); // Update minimap for player position flash
        }

        private void CheckForEncounter()
        {
            if (tileMapRenderer == null || terrainTypes == null) return;

            // Get current terrain
            var playerPos = tileMapRenderer.GetPlayerPosition();
            int terrainId = tileMapRenderer.GetTerrainAt((int)playerPos.X, (int)playerPos.Y);
            var terrain = terrainTypes.FirstOrDefault(t => t.Id == terrainId);

            if (terrain == null || terrain.EncounterRate == 0) return;

            // Check for random encounter
            if (random.Next(100) < terrain.EncounterRate)
            {
                System.Diagnostics.Debug.WriteLine($"Battle encounter on {terrain.Name}!");
                TriggerBattle(terrain.EncounterZone);
            }
        }

        private void TriggerBattle(int encounterZone)
        {
            gameLoopTimer?.Stop();

            // Get possible enemies for this zone
            var possibleEnemies = EnemyDatabase.GetEnemiesForZone(encounterZone);
            if (possibleEnemies.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No enemies for this zone!");
                gameLoopTimer?.Start();
                return;
            }

            // Create 1-3 random enemies
            int enemyCount = random.Next(1, 4);
            var enemies = new List<Enemy>();

            for (int i = 0; i < enemyCount; i++)
            {
                var enemyData = possibleEnemies[random.Next(possibleEnemies.Count)];
                var enemy = EnemyDatabase.CreateEnemyInstance(enemyData);
                enemies.Add(enemy);
            }

            System.Diagnostics.Debug.WriteLine($"Starting battle with {enemies.Count} enemies: {string.Join(", ", enemies.Select(e => e.Name))}");

            // Get battle background based on current terrain
            string battleBackground = null;
            var playerPos = tileMapRenderer.GetPlayerPosition();
            int terrainId = tileMapRenderer.GetTerrainAt((int)playerPos.X, (int)playerPos.Y);
            var terrain = terrainTypes?.FirstOrDefault(t => t.Id == terrainId);

            System.Diagnostics.Debug.WriteLine($"=== BATTLE TRIGGER ===");
            System.Diagnostics.Debug.WriteLine($"Player position: ({(int)playerPos.X}, {(int)playerPos.Y})");
            System.Diagnostics.Debug.WriteLine($"Terrain ID: {terrainId}");
            System.Diagnostics.Debug.WriteLine($"Terrain name: {terrain?.Name ?? "Unknown"}");

            if (terrain != null && !string.IsNullOrEmpty(terrain.BattleBackground))
            {
                battleBackground = terrain.BattleBackground;
                System.Diagnostics.Debug.WriteLine($"✓ Using battle background: {battleBackground}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"✗ No battle background configured for this terrain");
            }

            // Open battle screen
            var battleScreen = new BattleScreen(_mainWindow, party, enemies, battleBackground);
            battleScreen.Activate();
            Close();
        }

        private void UpdateLocationDisplay()
        {
            // Location display removed - pure gameplay view
        }

        private void MapCanvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        private async Task CreateResourcesAsync(CanvasControl sender)
        {
            // Wait for WorldMap and TerrainTypes to be loaded
            int attempts = 0;
            while ((worldMap == null || terrainTypes == null) && attempts < 20)
            {
                await Task.Delay(100);
                attempts++;
            }

            System.Diagnostics.Debug.WriteLine($"CreateResourcesAsync: WorldMap={worldMap != null}, TerrainTypes={terrainTypes != null}");

            tileMapRenderer = new TileMapRenderer(
                (int)sender.ActualWidth,
                (int)sender.ActualHeight,
                worldMap?.Width ?? 64,
                worldMap?.Height ?? 64
            );

            // Load tile data from JSON - REQUIRED!
            if (worldMap?.Layers != null && worldMap.Layers.Count > 0 && worldMap.Layers[0].Tiles != null)
            {
                tileMapRenderer.LoadMapData(worldMap.Layers[0].Tiles);
            }
            else
            {
                throw new InvalidOperationException(
                    "WorldMap.json must contain a 'Terrain' layer with tile data!\n" +
                    "Add a 'tiles' array to the first layer in WorldMap.json");
            }

            // Load tile textures
            System.Diagnostics.Debug.WriteLine($"About to load tile textures. TerrainTypes count: {terrainTypes?.Count ?? 0}");
            if (terrainTypes != null && terrainTypes.Count > 0)
            {
                await tileMapRenderer.LoadTileTexturesAsync(sender, terrainTypes);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("✗ TerrainTypes is null or empty, cannot load textures!");
            }

            // Pass locations to renderer and load location textures
            if (worldMap?.Locations != null)
            {
                tileMapRenderer.Locations = worldMap.Locations;
                System.Diagnostics.Debug.WriteLine($"✓ Loaded {worldMap.Locations.Count} locations to renderer");

                // Load location textures
                await tileMapRenderer.LoadLocationTexturesAsync(sender, worldMap.Locations);
            }

            System.Diagnostics.Debug.WriteLine($"✓ TileMap renderer initialized with map: {worldMap?.Name ?? "Default"}");
        }

        private void MapCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            tileMapRenderer?.Render(args.DrawingSession);
        }

        private void MinimapCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (tileMapRenderer == null || worldMap == null) return;

            var session = args.DrawingSession;

            // Calculate scale to fit entire map in 128x128 minimap
            float mapWidth = worldMap.Width;
            float mapHeight = worldMap.Height;
            float minimapSize = 128f;
            float scale = Math.Min(minimapSize / mapWidth, minimapSize / mapHeight);

            // Draw simplified map (each pixel = one tile)
            for (int y = 0; y < worldMap.Height; y++)
            {
                for (int x = 0; x < worldMap.Width; x++)
                {
                    int terrain = tileMapRenderer.GetTerrainAt(x, y);
                    Color color;

                    // Simplified colors for minimap
                    switch (terrain)
                    {
                        case 0: // Water
                            color = Color.FromArgb(255, 30, 77, 139);
                            break;
                        case 1: // Grass
                            color = Color.FromArgb(255, 94, 170, 60);
                            break;
                        case 2: // Sand
                            color = Color.FromArgb(255, 212, 165, 116);
                            break;
                        default:
                            color = Color.FromArgb(255, 128, 128, 128);
                            break;
                    }

                    float px = x * scale;
                    float py = y * scale;
                    session.FillRectangle(px, py, scale, scale, color);
                }
            }

            // Draw location markers on minimap
            if (worldMap.Locations != null)
            {
                foreach (var loc in worldMap.Locations)
                {
                    float locX = loc.X * scale;
                    float locY = loc.Y * scale;
                    int locWidth = (loc.Width > 0 ? loc.Width : 1);
                    int locHeight = (loc.Height > 0 ? loc.Height : 1);
                    float markerWidth = locWidth * scale;
                    float markerHeight = locHeight * scale;

                    Color markerColor = loc.Type switch
                    {
                        "town" => Color.FromArgb(255, 255, 255, 0),
                        "dungeon" => Color.FromArgb(255, 255, 0, 0),
                        "castle" => Color.FromArgb(255, 100, 100, 255),
                        _ => Color.FromArgb(255, 255, 255, 255)
                    };

                    // Draw rectangle marker for multi-tile locations
                    session.FillRectangle(locX, locY, Math.Max(markerWidth, 2), Math.Max(markerHeight, 2), markerColor);
                }
            }

            // Draw player position as a flashing white dot
            var playerPos = tileMapRenderer.PlayerPosition;
            float playerX = playerPos.X * scale;
            float playerY = playerPos.Y * scale;

            // Flash effect based on time
            bool flash = (DateTime.Now.Millisecond / 500) % 2 == 0;
            if (flash)
            {
                session.FillCircle(playerX, playerY, 2, Color.FromArgb(255, 255, 255, 255));
            }

            // Border around minimap
            session.DrawRectangle(0, 0, minimapSize, minimapSize, Color.FromArgb(255, 255, 255, 255), 1);
        }

        private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"KeyDown: {e.Key}");

            switch (e.Key)
            {
                case Windows.System.VirtualKey.W:
                case Windows.System.VirtualKey.Up:
                    if (!pendingMoveUp)
                    {
                        pendingMoveUp = true;
                        System.Diagnostics.Debug.WriteLine(">>> MOVE UP");
                    }
                    e.Handled = true;
                    break;

                case Windows.System.VirtualKey.S:
                case Windows.System.VirtualKey.Down:
                    if (!pendingMoveDown)
                    {
                        pendingMoveDown = true;
                        System.Diagnostics.Debug.WriteLine(">>> MOVE DOWN");
                    }
                    e.Handled = true;
                    break;

                case Windows.System.VirtualKey.A:
                case Windows.System.VirtualKey.Left:
                    if (!pendingMoveLeft)
                    {
                        pendingMoveLeft = true;
                        System.Diagnostics.Debug.WriteLine(">>> MOVE LEFT");
                    }
                    e.Handled = true;
                    break;

                case Windows.System.VirtualKey.D:
                case Windows.System.VirtualKey.Right:
                    if (!pendingMoveRight)
                    {
                        pendingMoveRight = true;
                        System.Diagnostics.Debug.WriteLine(">>> MOVE RIGHT");
                    }
                    e.Handled = true;
                    break;
                    e.Handled = true;
                    break;

                case Windows.System.VirtualKey.Escape:
                    gameLoopTimer?.Stop();
                    Close();
                    _mainWindow?.Activate();
                    e.Handled = true;
                    break;
            }

            RootGrid.Focus(FocusState.Programmatic);
        }

        private void RootGrid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"KeyUp: {e.Key}");

            switch (e.Key)
            {
                case Windows.System.VirtualKey.W:
                case Windows.System.VirtualKey.Up:
                    pendingMoveUp = false;
                    e.Handled = true;
                    break;

                case Windows.System.VirtualKey.S:
                case Windows.System.VirtualKey.Down:
                    pendingMoveDown = false;
                    e.Handled = true;
                    break;

                case Windows.System.VirtualKey.A:
                case Windows.System.VirtualKey.Left:
                    pendingMoveLeft = false;
                    e.Handled = true;
                    break;

                case Windows.System.VirtualKey.D:
                case Windows.System.VirtualKey.Right:
                    pendingMoveRight = false;
                    e.Handled = true;
                    break;
            }

            RootGrid.Focus(FocusState.Programmatic);
        }
    }
}
