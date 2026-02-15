using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using Windows.UI;

namespace GORE.GameEngine
{
    public class Mode7Renderer
    {
        private readonly int screenWidth;
        private readonly int screenHeight;
        private readonly int horizonY;
        
        public Vector2 CameraPosition { get; set; }
        public float CameraAngle { get; set; }
        public float CameraHeight { get; set; } = 100f;
        public float CameraDistance { get; set; } = 200f;

        private CanvasBitmap mapTexture;
        private readonly int mapWidth;
        private readonly int mapHeight;

        public Mode7Renderer(int screenWidth, int screenHeight, int mapWidth, int mapHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.horizonY = screenHeight / 3;
            
            CameraPosition = new Vector2(mapWidth / 2f, mapHeight / 2f);
            CameraAngle = 0f;
        }

        public void SetMapTexture(CanvasBitmap texture)
        {
            mapTexture = texture;
        }

        public void Render(CanvasDrawingSession drawSession)
        {
            if (mapTexture == null)
            {
                RenderFallbackMode(drawSession);
                return;
            }

            DrawSky(drawSession);
            DrawMode7Ground(drawSession);
        }

        private void DrawSky(CanvasDrawingSession drawSession)
        {
            var skyGradient = new Color[]
            {
                Color.FromArgb(255, 135, 206, 235), // Sky blue
                Color.FromArgb(255, 100, 149, 237)  // Cornflower blue
            };

            for (int y = 0; y < horizonY; y++)
            {
                float t = (float)y / horizonY;
                byte r = (byte)(skyGradient[0].R + (skyGradient[1].R - skyGradient[0].R) * t);
                byte g = (byte)(skyGradient[0].G + (skyGradient[1].G - skyGradient[0].G) * t);
                byte b = (byte)(skyGradient[0].B + (skyGradient[1].B - skyGradient[0].B) * t);
                
                drawSession.DrawLine(0, y, screenWidth, y, Color.FromArgb(255, r, g, b));
            }
        }

        private void DrawMode7Ground(CanvasDrawingSession drawSession)
        {
            float sinAngle = MathF.Sin(CameraAngle);
            float cosAngle = MathF.Cos(CameraAngle);

            for (int y = horizonY; y < screenHeight; y++)
            {
                float distance = (CameraHeight * CameraDistance) / (y - horizonY + 1);
                
                float scale = distance / CameraDistance;
                
                for (int x = 0; x < screenWidth; x++)
                {
                    float screenX = (x - screenWidth / 2f) * scale;
                    float screenY = distance;

                    float worldX = CameraPosition.X + (screenX * cosAngle - screenY * sinAngle);
                    float worldY = CameraPosition.Y + (screenX * sinAngle + screenY * cosAngle);

                    int mapX = ((int)worldX % mapWidth + mapWidth) % mapWidth;
                    int mapY = ((int)worldY % mapHeight + mapHeight) % mapHeight;

                    if (mapX >= 0 && mapX < mapTexture.SizeInPixels.Width &&
                        mapY >= 0 && mapY < mapTexture.SizeInPixels.Height)
                    {
                        var color = mapTexture.GetPixelColors(mapX, mapY, 1, 1)[0];
                        
                        float fog = Math.Clamp(distance / 500f, 0f, 0.7f);
                        byte r = (byte)(color.R * (1 - fog) + 135 * fog);
                        byte g = (byte)(color.G * (1 - fog) + 206 * fog);
                        byte b = (byte)(color.B * (1 - fog) + 235 * fog);
                        
                        drawSession.DrawLine(x, y, x + 1, y, Color.FromArgb(255, r, g, b));
                    }
                }
            }
        }

        private void RenderFallbackMode(CanvasDrawingSession drawSession)
        {
            DrawSky(drawSession);

            var grassColor = Color.FromArgb(255, 94, 170, 60);
            var waterColor = Color.FromArgb(255, 30, 77, 139);

            float sinAngle = MathF.Sin(CameraAngle);
            float cosAngle = MathF.Cos(CameraAngle);

            for (int y = horizonY; y < screenHeight; y++)
            {
                float distance = (CameraHeight * CameraDistance) / (y - horizonY + 1);
                float scale = distance / CameraDistance;

                for (int x = 0; x < screenWidth; x++)
                {
                    float screenX = (x - screenWidth / 2f) * scale;
                    float screenY = distance;

                    float worldX = CameraPosition.X + (screenX * cosAngle - screenY * sinAngle);
                    float worldY = CameraPosition.Y + (screenX * sinAngle + screenY * cosAngle);

                    int tileX = (int)(worldX / 16) % 16;
                    int tileY = (int)(worldY / 16) % 16;

                    bool isWater = (tileX + tileY) % 5 == 0;
                    var baseColor = isWater ? waterColor : grassColor;

                    float fog = Math.Clamp(distance / 500f, 0f, 0.7f);
                    byte r = (byte)(baseColor.R * (1 - fog) + 135 * fog);
                    byte g = (byte)(baseColor.G * (1 - fog) + 206 * fog);
                    byte b = (byte)(baseColor.B * (1 - fog) + 235 * fog);

                    drawSession.DrawLine(x, y, x + 1, y, Color.FromArgb(255, r, g, b));
                }
            }
        }

        public void MoveForward(float amount)
        {
            CameraPosition += new Vector2(
                MathF.Sin(CameraAngle) * amount,
                MathF.Cos(CameraAngle) * amount
            );
            
            CameraPosition = new Vector2(
                (CameraPosition.X % mapWidth + mapWidth) % mapWidth,
                (CameraPosition.Y % mapHeight + mapHeight) % mapHeight
            );
        }

        public void Rotate(float amount)
        {
            CameraAngle += amount;
            if (CameraAngle > MathF.PI * 2) CameraAngle -= MathF.PI * 2;
            if (CameraAngle < 0) CameraAngle += MathF.PI * 2;
        }
    }
}
