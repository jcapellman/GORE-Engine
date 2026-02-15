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
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace GORE.UI
{
    public sealed partial class WorldMapScreen : Window
    {
        private readonly Window _mainWindow;
        private TileMapRenderer tileMapRenderer;
        private WorldMap worldMap;
        private Character player;
        private DispatcherQueueTimer gameLoopTimer;

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

            ExtendsContentIntoTitleBar = true;
            ScreenHelper.EnterFullScreenMode(this);

            _ = LoadWorldMapAsync();

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

                if (File.Exists(mapPath))
                {
                    var json = await File.ReadAllTextAsync(mapPath);
                    worldMap = JsonSerializer.Deserialize<WorldMap>(json);
                    System.Diagnostics.Debug.WriteLine($"✓ Loaded world map: {worldMap.Name}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✗ World map not found, using default");
                    CreateDefaultWorldMap();
                }

                UpdateLocationDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error loading world map: {ex.Message}");
                CreateDefaultWorldMap();
            }
        }

        private void CreateDefaultWorldMap()
        {
            worldMap = new WorldMap
            {
                Name = "World of Balance",
                Width = 256,
                Height = 256
            };
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

            MapCanvas.Invalidate();
        }

        private void CheckForEncounter()
        {
            if (steps % 30 == 0)
            {
                var random = new Random();
                if (random.Next(100) < 10)
                {
                    gameLoopTimer?.Stop();
                    System.Diagnostics.Debug.WriteLine("Battle encounter!");
                }
            }
        }

        private void UpdateLocationDisplay()
        {
            if (tileMapRenderer != null)
            {
                var pos = tileMapRenderer.PlayerPosition;
                CoordinatesText.Text = $"X: {(int)pos.X}, Y: {(int)pos.Y}";

                int terrain = tileMapRenderer.GetTerrainAt((int)pos.X, (int)pos.Y);
                string[] terrainNames = { "Ocean", "Grass", "Forest", "Mountain", "Desert", "Snow" };
                if (terrain >= 0 && terrain < terrainNames.Length)
                {
                    // Check if we're at a location from the JSON
                    string locationName = terrainNames[terrain];
                    if (worldMap != null && worldMap.Locations != null)
                    {
                        foreach (var loc in worldMap.Locations)
                        {
                            if (Math.Abs(loc.X - (int)pos.X) <= 1 && Math.Abs(loc.Y - (int)pos.Y) <= 1)
                            {
                                locationName = loc.Name;
                                break;
                            }
                        }
                    }
                    LocationText.Text = locationName;
                }
            }

            if (player != null)
            {
                PartyLeaderText.Text = player.Name;
            }

            StepsText.Text = $"Steps: {steps}";

            var inputs = new List<string>();
            if (pendingMoveUp) inputs.Add("↑UP");
            if (pendingMoveDown) inputs.Add("↓DOWN");
            if (pendingMoveLeft) inputs.Add("←LEFT");
            if (pendingMoveRight) inputs.Add("→RIGHT");
            InputDebugText.Text = inputs.Count > 0 ? string.Join(" ", inputs) : "---";
        }

        private void MapCanvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        private async Task CreateResourcesAsync(CanvasControl sender)
        {
            tileMapRenderer = new TileMapRenderer(
                (int)sender.ActualWidth,
                (int)sender.ActualHeight,
                worldMap?.Width ?? 256,
                worldMap?.Height ?? 256
            );

            // Pass locations to renderer
            if (worldMap?.Locations != null)
            {
                tileMapRenderer.Locations = worldMap.Locations;
                System.Diagnostics.Debug.WriteLine($"✓ Loaded {worldMap.Locations.Count} locations to renderer");
            }

            System.Diagnostics.Debug.WriteLine($"✓ TileMap renderer initialized with map: {worldMap?.Name ?? "Default"}");
        }

        private void MapCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            tileMapRenderer?.Render(args.DrawingSession);
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
