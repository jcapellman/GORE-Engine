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
        private readonly int tileSize = 16; // 16x16 pixel tiles like FF1
        private readonly int mapWidth;
        private readonly int mapHeight;

        public Vector2 PlayerPosition { get; set; }
        public int PlayerDirection { get; set; } // 0=Down, 1=Left, 2=Right, 3=Up

        public List<WorldMapLocation> Locations { get; set; }
        
        private readonly Color[] terrainColors = new Color[]
        {
            Color.FromArgb(255, 30, 77, 139),   // 0: Ocean
            Color.FromArgb(255, 94, 170, 60),   // 1: Grass
            Color.FromArgb(255, 45, 90, 30),    // 2: Forest
            Color.FromArgb(255, 139, 115, 85),  // 3: Mountain
            Color.FromArgb(255, 212, 165, 116), // 4: Desert
            Color.FromArgb(255, 232, 240, 248), // 5: Snow
        };

        private int[,] mapData;

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
                            Color tileColor = terrainColors[Math.Min(terrainType, terrainColors.Length - 1)];

                            float screenX = x * tileSize;
                            float screenY = y * tileSize;

                            drawSession.FillRectangle(screenX, screenY, tileSize, tileSize, tileColor);

                            // Draw tile border for grid effect
                            drawSession.DrawRectangle(screenX, screenY, tileSize, tileSize, 
                                Color.FromArgb(40, 0, 0, 0));
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
            Color markerColor = location.Type switch
            {
                "town" => Color.FromArgb(255, 255, 255, 0),        // Bright Yellow
                "dungeon" => Color.FromArgb(255, 255, 0, 0),       // Bright Red
                "castle" => Color.FromArgb(255, 100, 100, 255),    // Bright Blue
                _ => Color.FromArgb(255, 255, 255, 255)            // White
            };

            // Draw a larger, more visible marker
            drawSession.FillRectangle(x + 2, y + 2, tileSize - 4, tileSize - 4, markerColor);
            drawSession.DrawRectangle(x + 2, y + 2, tileSize - 4, tileSize - 4, Color.FromArgb(255, 0, 0, 0), 2);

            // Add a bright center dot for extra visibility
            drawSession.FillCircle(x + tileSize / 2, y + tileSize / 2, 3, Color.FromArgb(255, 255, 255, 255));
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
    }
}
