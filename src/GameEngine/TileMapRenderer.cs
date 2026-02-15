using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using GORE.Models;

namespace GORE.GameEngine
{
    public class TileMapRenderer
    {
        private readonly int screenWidth;
        private readonly int screenHeight;
        private readonly int tileSize = 64; // Larger tiles for FF1 SNES look (was 16)
        private readonly int mapWidth;
        private readonly int mapHeight;

        public Vector2 PlayerPosition { get; set; }
        public int PlayerDirection { get; set; }

        public List<WorldMapLocation> Locations { get; set; }

        private readonly Color[] terrainColors = new Color[]
        {
            Color.FromArgb(255, 30, 77, 139),   // 0: Water (fallback)
            Color.FromArgb(255, 94, 170, 60),   // 1: Grass (fallback)
            Color.FromArgb(255, 212, 165, 116), // 2: Sand (fallback)
        };

        private int[,] mapData;
        private Dictionary<int, CanvasBitmap> tileTextures = new Dictionary<int, CanvasBitmap>();
        private Dictionary<string, CanvasBitmap> locationTextures = new Dictionary<string, CanvasBitmap>();
        private Dictionary<int, TileEffect> tileEffects = new Dictionary<int, TileEffect>();

        // Animation state
        private float waterAnimationTime = 0f;

        public TileMapRenderer(int screenWidth, int screenHeight, int mapWidth, int mapHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.mapWidth = Math.Max(mapWidth, 64);
            this.mapHeight = Math.Max(mapHeight, 64);

            PlayerPosition = new Vector2(this.mapWidth / 2, this.mapHeight / 2);
            PlayerDirection = 0;
            Locations = new List<WorldMapLocation>();

            System.Diagnostics.Debug.WriteLine($"TileMapRenderer created: Screen {screenWidth}x{screenHeight}, Map {this.mapWidth}x{this.mapHeight}");
        }

        public void LoadMapData(int[][] tileData)
        {
            if (tileData == null || tileData.Length == 0)
            {
                throw new InvalidOperationException("Map tile data is required! Add a 'tiles' array to the 'Terrain' layer in WorldMap.json");
            }

            System.Diagnostics.Debug.WriteLine($"Loading tile data: {tileData.Length} rows");

            int height = tileData.Length;
            int width = tileData[0].Length;

            mapData = new int[width, height];

            for (int y = 0; y < height && y < mapHeight; y++)
            {
                for (int x = 0; x < width && x < mapWidth; x++)
                {
                    if (x < tileData[y].Length)
                    {
                        mapData[x, y] = tileData[y][x];
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"✓ Loaded {width}x{height} tile map from JSON");
        }

        public async System.Threading.Tasks.Task LoadTileTexturesAsync(CanvasControl canvas, List<WorldMapTerrain> terrainTypes)
        {
            if (terrainTypes == null || terrainTypes.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("✗ No terrain types provided, will use solid colors");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"=== LOADING TILE TEXTURES ===");
            System.Diagnostics.Debug.WriteLine($"Base Directory: {AppContext.BaseDirectory}");

            foreach (var terrain in terrainTypes)
            {
                System.Diagnostics.Debug.WriteLine($"Processing terrain: {terrain.Name} (ID: {terrain.Id})");
                System.Diagnostics.Debug.WriteLine($"  Texture path from JSON: {terrain.Texture}");

                if (!string.IsNullOrEmpty(terrain.Texture))
                {
                    try
                    {
                        var baseDirectory = AppContext.BaseDirectory;
                        var texturePath = System.IO.Path.Combine(baseDirectory, terrain.Texture);

                        System.Diagnostics.Debug.WriteLine($"  Full path: {texturePath}");
                        System.Diagnostics.Debug.WriteLine($"  File exists: {System.IO.File.Exists(texturePath)}");

                        if (System.IO.File.Exists(texturePath))
                        {
                            var bitmap = await CanvasBitmap.LoadAsync(canvas, texturePath);
                            tileTextures[terrain.Id] = bitmap;

                            // Store effect data if present
                            if (terrain.Effect != null)
                            {
                                tileEffects[terrain.Id] = terrain.Effect;
                                System.Diagnostics.Debug.WriteLine($"  ✓ Registered effect: {terrain.Effect.Type} (Speed: {terrain.Effect.Speed}, Intensity: {terrain.Effect.Intensity})");
                            }

                            System.Diagnostics.Debug.WriteLine($"  ✓ SUCCESS: Loaded texture {bitmap.SizeInPixels.Width}x{bitmap.SizeInPixels.Height}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"  ✗ FAILED: File not found");

                            // List what files ARE in the Tiles directory
                            var tilesDir = System.IO.Path.Combine(baseDirectory, "Assets", "Tiles");
                            if (System.IO.Directory.Exists(tilesDir))
                            {
                                System.Diagnostics.Debug.WriteLine($"  Files in {tilesDir}:");
                                foreach (var file in System.IO.Directory.GetFiles(tilesDir))
                                {
                                    System.Diagnostics.Debug.WriteLine($"    - {System.IO.Path.GetFileName(file)}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"  Directory doesn't exist: {tilesDir}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✗ EXCEPTION: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"  Stack: {ex.StackTrace}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  No texture specified for {terrain.Name}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"=== TEXTURE LOADING COMPLETE ===");
            System.Diagnostics.Debug.WriteLine($"Loaded {tileTextures.Count}/{terrainTypes.Count} textures");
            System.Diagnostics.Debug.WriteLine($"Using textures: {tileTextures.Count > 0}");
        }

        public async System.Threading.Tasks.Task LoadLocationTexturesAsync(CanvasControl canvas, List<WorldMapLocation> locations)
        {
            if (locations == null || locations.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No locations provided for texture loading");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"=== LOADING LOCATION TEXTURES ===");

            foreach (var location in locations)
            {
                if (!string.IsNullOrEmpty(location.Texture))
                {
                    try
                    {
                        var baseDirectory = AppContext.BaseDirectory;
                        var texturePath = System.IO.Path.Combine(baseDirectory, location.Texture);

                        System.Diagnostics.Debug.WriteLine($"Loading texture for {location.Name}");
                        System.Diagnostics.Debug.WriteLine($"  Path: {texturePath}");
                        System.Diagnostics.Debug.WriteLine($"  Exists: {System.IO.File.Exists(texturePath)}");

                        if (System.IO.File.Exists(texturePath))
                        {
                            var bitmap = await CanvasBitmap.LoadAsync(canvas, texturePath);
                            locationTextures[location.Name] = bitmap;
                            System.Diagnostics.Debug.WriteLine($"  ✓ Loaded {location.Name} texture: {bitmap.SizeInPixels.Width}x{bitmap.SizeInPixels.Height}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"  ✗ File not found");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✗ Error loading {location.Name} texture: {ex.Message}");
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"✓ Loaded {locationTextures.Count} location textures");
        }

        public void Update(float deltaTime)
        {
            // Update animation time
            waterAnimationTime += deltaTime;
        }

        public void Render(CanvasDrawingSession drawSession)
        {
            if (mapData == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: mapData is null in Render");
                return;
            }

            // Calculate camera offset to center on player
            int tilesWide = screenWidth / tileSize;
            int tilesHigh = screenHeight / tileSize;

            int cameraX = (int)PlayerPosition.X - tilesWide / 2;
            int cameraY = (int)PlayerPosition.Y - tilesHigh / 2;

            // Draw tiles
            for (int y = 0; y < tilesHigh + 2; y++)
            {
                for (int x = 0; x < tilesWide + 2; x++)
                {
                    int mapX = cameraX + x;
                    int mapY = cameraY + y;

                    // Wrap around map edges - handle negative values correctly
                    if (mapWidth > 0 && mapHeight > 0)
                    {
                        mapX = ((mapX % mapWidth) + mapWidth) % mapWidth;
                        mapY = ((mapY % mapHeight) + mapHeight) % mapHeight;

                        if (mapX >= 0 && mapX < mapWidth && mapY >= 0 && mapY < mapHeight)
                        {
                            int terrainType = mapData[mapX, mapY];

                            float screenX = x * tileSize;
                            float screenY = y * tileSize;

                            // Draw texture if available, otherwise use color
                            if (tileTextures.ContainsKey(terrainType))
                            {
                                var texture = tileTextures[terrainType];

                                // Check if this tile has a water effect
                                if (tileEffects.ContainsKey(terrainType) && tileEffects[terrainType].Type == "water")
                                {
                                    var effect = tileEffects[terrainType];
                                    DrawWaterTile(drawSession, texture, screenX, screenY, effect);
                                }
                                else
                                {
                                    drawSession.DrawImage(texture, 
                                        new Windows.Foundation.Rect(screenX, screenY, tileSize, tileSize),
                                        new Windows.Foundation.Rect(0, 0, texture.SizeInPixels.Width, texture.SizeInPixels.Height));
                                }
                            }
                            else
                            {
                                // Fallback to solid color
                                Color tileColor = terrainColors[Math.Min(terrainType, terrainColors.Length - 1)];
                                drawSession.FillRectangle(screenX, screenY, tileSize, tileSize, tileColor);
                            }

                            // Draw subtle tile border for grid effect (optional)
                            // drawSession.DrawRectangle(screenX, screenY, tileSize, tileSize, Color.FromArgb(20, 0, 0, 0));
                        }
                    }
                }
            }

            // Draw player sprite (centered on screen)
            float playerScreenX = (tilesWide / 2) * tileSize;
            float playerScreenY = (tilesHigh / 2) * tileSize;

            // Draw location markers BEFORE player so player is on top
            if (Locations != null)
            {
                foreach (var location in Locations)
                {
                    int screenX = (int)((location.X - cameraX) * tileSize);
                    int screenY = (int)((location.Y - cameraY) * tileSize);

                    // Only draw if on screen
                    if (screenX >= -tileSize && screenX < screenWidth + tileSize && 
                        screenY >= -tileSize && screenY < screenHeight + tileSize)
                    {
                        DrawLocationMarker(drawSession, screenX, screenY, location);
                    }
                }
            }

            DrawPlayerSprite(drawSession, playerScreenX, playerScreenY);
        }

        private void DrawLocationMarker(CanvasDrawingSession drawSession, float x, float y, WorldMapLocation location)
        {
            // Calculate size in pixels based on tile count
            int widthInTiles = location.Width > 0 ? location.Width : 1;
            int heightInTiles = location.Height > 0 ? location.Height : 1;
            float width = widthInTiles * tileSize;
            float height = heightInTiles * tileSize;

            // Check if location has a texture
            if (locationTextures.ContainsKey(location.Name))
            {
                var texture = locationTextures[location.Name];

                // Draw texture with transparency support, scaled to cover all tiles
                drawSession.DrawImage(texture,
                    new Windows.Foundation.Rect(x, y, width, height),
                    new Windows.Foundation.Rect(0, 0, texture.SizeInPixels.Width, texture.SizeInPixels.Height));
            }
            else
            {
                // Fallback to colored square if no texture
                Color markerColor = location.Type switch
                {
                    "town" => Color.FromArgb(255, 255, 255, 0),        // Bright Yellow
                    "dungeon" => Color.FromArgb(255, 255, 0, 0),       // Bright Red
                    "castle" => Color.FromArgb(255, 100, 100, 255),    // Bright Blue
                    _ => Color.FromArgb(255, 255, 255, 255)            // White
                };

                // Draw a marker covering all tiles
                drawSession.FillRectangle(x + 2, y + 2, width - 4, height - 4, markerColor);
                drawSession.DrawRectangle(x + 2, y + 2, width - 4, height - 4, Color.FromArgb(255, 0, 0, 0), 2);

                // Add a bright center dot for extra visibility
                drawSession.FillCircle(x + width / 2, y + height / 2, 3, Color.FromArgb(255, 255, 255, 255));
            }
        }

        private void DrawPlayerSprite(CanvasDrawingSession drawSession, float x, float y)
        {
            // Simple player sprite - a colored square with direction indicator
            Color playerColor = Color.FromArgb(255, 255, 200, 100); // Orange/Gold
            
            // Draw player body
            drawSession.FillRectangle(x + 2, y + 2, tileSize - 4, tileSize - 4, playerColor);
            
            // Draw direction indicator
            Color indicatorColor = Color.FromArgb(255, 255, 100, 100); // Red
            float indicatorSize = 3;
            
            switch (PlayerDirection)
            {
                case 0: // Down
                    drawSession.FillRectangle(x + tileSize / 2 - indicatorSize / 2, 
                        y + tileSize - 4, indicatorSize, indicatorSize, indicatorColor);
                    break;
                case 1: // Left
                    drawSession.FillRectangle(x + 2, 
                        y + tileSize / 2 - indicatorSize / 2, indicatorSize, indicatorSize, indicatorColor);
                    break;
                case 2: // Right
                    drawSession.FillRectangle(x + tileSize - 4, 
                        y + tileSize / 2 - indicatorSize / 2, indicatorSize, indicatorSize, indicatorColor);
                    break;
                case 3: // Up
                    drawSession.FillRectangle(x + tileSize / 2 - indicatorSize / 2, 
                        y + 2, indicatorSize, indicatorSize, indicatorColor);
                    break;
            }
            
            // Draw outline
            drawSession.DrawRectangle(x + 2, y + 2, tileSize - 4, tileSize - 4, 
                Color.FromArgb(255, 0, 0, 0));
        }

        public bool CanMoveTo(int x, int y)
        {
            if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                return false;
                
            int terrainType = mapData[x, y];
            // Ocean (0) and Mountain (3) are not walkable
            return terrainType != 0 && terrainType != 3;
        }

        public bool MovePlayer(int dx, int dy)
        {
            int newX = (int)PlayerPosition.X + dx;
            int newY = (int)PlayerPosition.Y + dy;
            
            // Wrap around map edges
            newX = ((newX % mapWidth) + mapWidth) % mapWidth;
            newY = ((newY % mapHeight) + mapHeight) % mapHeight;
            
            if (CanMoveTo(newX, newY))
            {
                PlayerPosition = new Vector2(newX, newY);
                
                // Update direction based on movement
                if (dy > 0) PlayerDirection = 0; // Down
                else if (dx < 0) PlayerDirection = 1; // Left
                else if (dx > 0) PlayerDirection = 2; // Right
                else if (dy < 0) PlayerDirection = 3; // Up
                
                return true;
            }
            
            return false;
        }

        public int GetTerrainAt(int x, int y)
        {
            if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                return -1;
            return mapData[x, y];
        }

        public Vector2 GetPlayerPosition()
        {
            return PlayerPosition;
        }

        private void DrawWaterTile(CanvasDrawingSession drawSession, CanvasBitmap texture, float x, float y, TileEffect effect)
        {
            // Simple scrolling water effect
            float speed = effect.Speed * 10f; // Adjust speed
            float offset = (waterAnimationTime * speed) % tileSize;

            // Draw the texture with a slight offset for wave simulation
            float waveOffset = (float)Math.Sin(waterAnimationTime * effect.Speed + x * 0.1f) * effect.Intensity * 2f;

            drawSession.DrawImage(texture,
                new Windows.Foundation.Rect(x, y + waveOffset, tileSize, tileSize),
                new Windows.Foundation.Rect(0, 0, texture.SizeInPixels.Width, texture.SizeInPixels.Height));

            // Optional: Add a subtle overlay for shimmer effect
            var overlayColor = Color.FromArgb(
                (byte)(20 * effect.Intensity), // Alpha based on intensity
                255, 255, 255); // White overlay

            float shimmer = (float)Math.Sin(waterAnimationTime * effect.Speed * 2f + x * 0.05f + y * 0.05f);
            if (shimmer > 0.7f)
            {
                drawSession.FillRectangle(x, y, tileSize, tileSize, overlayColor);
            }
        }
    }
}
