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
using Windows.Storage.Streams;

namespace GORE.Engine
{
    public class Renderer3D
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly RaycastEngine _raycastEngine;
        private readonly WriteableBitmap _bitmap;
        private byte[] _pixels;
        private IBuffer _pixelBuffer;

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
            _pixelBuffer = _bitmap.PixelBuffer;
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

                // Skip if no wall was hit (WallType = 0 means empty space or max distance reached)
                if (hit.WallType == 0)
                    continue;

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

            // Clamp texture X
            texX = Math.Clamp(texX, 0, texture.Width - 1);

            // How much to increase the texture coordinate per screen pixel
            float step = 1.0f * texture.Height / (drawEnd - drawStart);
            float texPos = (drawStart - _screenHeight / 2 + (drawEnd - drawStart) / 2) * step;

            // Pre-calculate shading factor
            bool applyShading = hit.Side == 1;
            byte[] texPixels = texture.Pixels;
            int texWidth = texture.Width;
            int texHeightMask = texture.Height - 1;

            // Draw the vertical texture stripe - optimized direct pixel access
            for (int y = drawStart; y <= drawEnd; y++)
            {
                int texY = (int)texPos & texHeightMask;
                texPos += step;

                // Direct texture pixel access - eliminates method call overhead
                int texIndex = (texY * texWidth + texX) * 4;

                byte b = texPixels[texIndex];
                byte g = texPixels[texIndex + 1];
                byte r = texPixels[texIndex + 2];
                byte a = texPixels[texIndex + 3];

                // Apply shading inline (avoid Color object creation)
                if (applyShading)
                {
                    r >>= 1; // Equivalent to r / 2 but faster
                    g >>= 1;
                    b >>= 1;
                }

                // Direct pixel write
                int pixelIndex = (y * _screenWidth + screenX) * 4;
                _pixels[pixelIndex] = b;
                _pixels[pixelIndex + 1] = g;
                _pixels[pixelIndex + 2] = r;
                _pixels[pixelIndex + 3] = a;
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
            // Optimized: Fill ceiling in one pass
            int halfHeight = _screenHeight / 2;
            int pixelsPerRow = _screenWidth * 4;

            for (int y = 0; y < halfHeight; y++)
            {
                int rowStart = y * _screenWidth * 4;
                for (int x = 0; x < _screenWidth; x++)
                {
                    int index = rowStart + (x * 4);
                    _pixels[index] = color.B;
                    _pixels[index + 1] = color.G;
                    _pixels[index + 2] = color.R;
                    _pixels[index + 3] = color.A;
                }
            }
        }

        private void DrawFloor(Color color)
        {
            // Optimized: Fill floor in one pass
            int halfHeight = _screenHeight / 2;

            for (int y = halfHeight; y < _screenHeight; y++)
            {
                int rowStart = y * _screenWidth * 4;
                for (int x = 0; x < _screenWidth; x++)
                {
                    int index = rowStart + (x * 4);
                    _pixels[index] = color.B;
                    _pixels[index + 1] = color.G;
                    _pixels[index + 2] = color.R;
                    _pixels[index + 3] = color.A;
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
            // Optimized: Direct buffer write without stream allocation
            _pixels.AsBuffer().CopyTo(_pixelBuffer);
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
