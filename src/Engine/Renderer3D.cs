using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.Graphics.Imaging;

namespace GORE.Engine
{
    public class Renderer3D
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly RaycastEngine _raycastEngine;
        private readonly WriteableBitmap _bitmap;
        private byte[] _pixels;

        // Texture storage
        private readonly Dictionary<int, TextureData> _textures = new();

        // Fallback color palette for walls without textures
        private readonly Color[] _wallColors = new[]
        {
            Color.FromArgb(255, 100, 100, 100),  // 0 = empty
            Color.FromArgb(255, 255, 0, 0),      // 1 = red
            Color.FromArgb(255, 0, 255, 0),      // 2 = green
            Color.FromArgb(255, 0, 0, 255),      // 3 = blue
            Color.FromArgb(255, 255, 255, 0),    // 4 = yellow
            Color.FromArgb(255, 255, 0, 255),    // 5 = magenta
            Color.FromArgb(255, 0, 255, 255),    // 6 = cyan
            Color.FromArgb(255, 255, 255, 255),  // 7 = white
        };

        public Renderer3D(int width, int height, RaycastEngine raycastEngine)
        {
            _screenWidth = width;
            _screenHeight = height;
            _raycastEngine = raycastEngine;
            _bitmap = new WriteableBitmap(width, height);
            _pixels = new byte[width * height * 4]; // BGRA format
        }

        public WriteableBitmap GetBitmap() => _bitmap;

        public async Task LoadTextureAsync(int textureId, string texturePath)
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var fullPath = Path.Combine(baseDirectory, texturePath);

                if (!File.Exists(fullPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Texture not found: {fullPath}");
                    return;
                }

                var fileStream = File.OpenRead(fullPath);
                var decoder = await BitmapDecoder.CreateAsync(fileStream.AsRandomAccessStream());
                var pixelData = await decoder.GetPixelDataAsync();
                var bytes = pixelData.DetachPixelData();

                var textureData = new TextureData
                {
                    Width = (int)decoder.PixelWidth,
                    Height = (int)decoder.PixelHeight,
                    Pixels = bytes
                };

                _textures[textureId] = textureData;
                System.Diagnostics.Debug.WriteLine($"✓ Loaded texture {textureId}: {texturePath} ({textureData.Width}x{textureData.Height})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Failed to load texture {textureId}: {ex.Message}");
            }
        }

        public void Render()
        {
            // Clear screen
            ClearScreen(Color.FromArgb(255, 64, 64, 64)); // Dark gray ceiling
            DrawFloor(Color.FromArgb(255, 32, 32, 32));   // Darker floor

            // Raycast for each vertical stripe
            for (int x = 0; x < _screenWidth; x++)
            {
                var hit = _raycastEngine.CastRay(x, _screenWidth);

                // Calculate line height
                int lineHeight = (int)(_screenHeight / hit.Distance);

                // Calculate lowest and highest pixel to fill in current stripe
                int drawStart = Math.Max(0, -lineHeight / 2 + _screenHeight / 2);
                int drawEnd = Math.Min(_screenHeight - 1, lineHeight / 2 + _screenHeight / 2);

                // Draw textured wall if texture exists, otherwise use solid color
                if (_textures.ContainsKey(hit.WallType))
                {
                    DrawTexturedWall(x, drawStart, drawEnd, hit);
                }
                else
                {
                    // Fallback to solid color
                    Color wallColor = _wallColors[Math.Min(hit.WallType, _wallColors.Length - 1)];

                    // Darken color for side walls (create depth effect)
                    if (hit.Side == 1)
                    {
                        wallColor = Color.FromArgb(255,
                            (byte)(wallColor.R / 2),
                            (byte)(wallColor.G / 2),
                            (byte)(wallColor.B / 2));
                    }

                    DrawVerticalLine(x, drawStart, drawEnd, wallColor);
                }
            }

            // Update bitmap
            UpdateBitmap();
        }

        private void DrawTexturedWall(int screenX, int drawStart, int drawEnd, RaycastHit hit)
        {
            var texture = _textures[hit.WallType];

            // Calculate wall X coordinate (0.0 to 1.0)
            float wallX = hit.WallX;

            // X coordinate on the texture
            int texX = (int)(wallX * texture.Width);
            if (hit.Side == 0 && hit.RayDirX > 0) texX = texture.Width - texX - 1;
            if (hit.Side == 1 && hit.RayDirY < 0) texX = texture.Width - texX - 1;

            // How much to increase the texture coordinate per screen pixel
            float step = 1.0f * texture.Height / (drawEnd - drawStart);
            float texPos = (drawStart - _screenHeight / 2 + (drawEnd - drawStart) / 2) * step;

            // Draw the vertical texture stripe
            for (int y = drawStart; y <= drawEnd; y++)
            {
                int texY = (int)texPos & (texture.Height - 1);
                texPos += step;

                Color color = GetTexturePixel(texture, texX, texY);

                // Apply shading for side walls
                if (hit.Side == 1)
                {
                    color = Color.FromArgb(255,
                        (byte)(color.R / 2),
                        (byte)(color.G / 2),
                        (byte)(color.B / 2));
                }

                SetPixel(screenX, y, color);
            }
        }

        private Color GetTexturePixel(TextureData texture, int x, int y)
        {
            // Clamp coordinates
            x = Math.Clamp(x, 0, texture.Width - 1);
            y = Math.Clamp(y, 0, texture.Height - 1);

            int index = (y * texture.Width + x) * 4;

            // Texture is in BGRA format
            byte b = texture.Pixels[index];
            byte g = texture.Pixels[index + 1];
            byte r = texture.Pixels[index + 2];
            byte a = texture.Pixels[index + 3];

            return Color.FromArgb(a, r, g, b);
        }

        private void ClearScreen(Color color)
        {
            for (int y = 0; y < _screenHeight / 2; y++)
            {
                for (int x = 0; x < _screenWidth; x++)
                {
                    SetPixel(x, y, color);
                }
            }
        }

        private void DrawFloor(Color color)
        {
            for (int y = _screenHeight / 2; y < _screenHeight; y++)
            {
                for (int x = 0; x < _screenWidth; x++)
                {
                    SetPixel(x, y, color);
                }
            }
        }

        private void DrawVerticalLine(int x, int y1, int y2, Color color)
        {
            for (int y = y1; y <= y2; y++)
            {
                SetPixel(x, y, color);
            }
        }

        private void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= _screenWidth || y < 0 || y >= _screenHeight)
                return;

            int index = (y * _screenWidth + x) * 4;
            _pixels[index] = color.B;     // Blue
            _pixels[index + 1] = color.G; // Green
            _pixels[index + 2] = color.R; // Red
            _pixels[index + 3] = color.A; // Alpha
        }

        private void UpdateBitmap()
        {
            using (var stream = _bitmap.PixelBuffer.AsStream())
            {
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                stream.Write(_pixels, 0, _pixels.Length);
            }
            _bitmap.Invalidate();
        }
    }

    public class TextureData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Pixels { get; set; }
    }
}
