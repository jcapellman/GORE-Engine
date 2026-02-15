using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace GORE.Engine
{
    public class Renderer3D
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly RaycastEngine _raycastEngine;
        private readonly WriteableBitmap _bitmap;
        private byte[] _pixels;

        // Color palette for walls
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

                // Choose wall color
                Color wallColor = _wallColors[Math.Min(hit.WallType, _wallColors.Length - 1)];

                // Darken color for side walls (create depth effect)
                if (hit.Side == 1)
                {
                    wallColor = Color.FromArgb(255,
                        (byte)(wallColor.R / 2),
                        (byte)(wallColor.G / 2),
                        (byte)(wallColor.B / 2));
                }

                // Draw the vertical line
                DrawVerticalLine(x, drawStart, drawEnd, wallColor);
            }

            // Update bitmap
            UpdateBitmap();
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
}
