using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI;
using Windows.UI;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Numerics;

namespace GORE.Engine
{
    public class Renderer3D
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly RaycastEngine _raycastEngine;

        // Win2D resources
        private CanvasRenderTarget _renderTarget;
        private readonly Dictionary<int, CanvasBitmap> _textures = new();

        public Renderer3D(int width, int height, RaycastEngine raycastEngine)
        {
            _screenWidth = width;
            _screenHeight = height;
            _raycastEngine = raycastEngine;
        }

        public void InitializeResources(CanvasDevice device)
        {
            _renderTarget = new CanvasRenderTarget(device, _screenWidth, _screenHeight, 96);
        }

        public CanvasRenderTarget GetRenderTarget() => _renderTarget;

        public async Task LoadTextureAsync(int textureId, string texturePath, CanvasDevice device)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var fullPath = Path.Combine(baseDirectory, texturePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Texture file not found: {fullPath}");
            }

            try
            {
                var fileStream = File.OpenRead(fullPath);
                var canvasBitmap = await CanvasBitmap.LoadAsync(device, fileStream.AsRandomAccessStream());

                _textures[textureId] = canvasBitmap;
                System.Diagnostics.Debug.WriteLine($"✓ Loaded texture {textureId}: {texturePath} ({canvasBitmap.SizeInPixels.Width}x{canvasBitmap.SizeInPixels.Height})");
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load texture {textureId} from {texturePath}: {ex.Message}", ex);
            }
        }

        public void Render(CanvasDrawingSession drawingSession, float canvasWidth, float canvasHeight)
        {
            if (_renderTarget == null)
                return;

            using (var ds = _renderTarget.CreateDrawingSession())
            {
                // Clear screen with ceiling and floor colors
                ds.Clear(Color.FromArgb(255, 64, 64, 64)); // Dark gray ceiling

                // Draw floor
                ds.FillRectangle(0, _screenHeight / 2, _screenWidth, _screenHeight / 2, 
                    Color.FromArgb(255, 32, 32, 32));

                // Raycast for each vertical stripe
                for (int x = 0; x < _screenWidth; x++)
                {
                    var hit = _raycastEngine.CastRay(x, _screenWidth);

                    // Skip if no wall was hit
                    if (hit.WallType == 0)
                        continue;

                    // Calculate line height
                    int lineHeight = (int)(_screenHeight / hit.Distance);

                    // Calculate lowest and highest pixel to fill in current stripe
                    int drawStart = Math.Max(0, -lineHeight / 2 + _screenHeight / 2);
                    int drawEnd = Math.Min(_screenHeight - 1, lineHeight / 2 + _screenHeight / 2);

                    // Draw textured wall - throw error if texture is missing
                    if (!_textures.ContainsKey(hit.WallType))
                    {
                        throw new InvalidOperationException($"Missing texture for wall type {hit.WallType}. All textures must be loaded before rendering.");
                    }

                    DrawTexturedWallWin2D(ds, x, drawStart, drawEnd, hit);
                }
            }

            // Scale the render target to fill the entire canvas
            var destRect = new Windows.Foundation.Rect(0, 0, canvasWidth, canvasHeight);
            drawingSession.DrawImage(_renderTarget, destRect);
        }

                        private void DrawTexturedWallWin2D(CanvasDrawingSession ds, int screenX, int drawStart, int drawEnd, RaycastHit hit)
                        {
                            var texture = _textures[hit.WallType];

                            // Calculate wall X coordinate (0.0 to 1.0)
                            float wallX = hit.WallX;

                            // X coordinate on the texture
                            int texX = (int)(wallX * texture.SizeInPixels.Width);
                            if (hit.Side == 0 && hit.RayDirX > 0) texX = (int)texture.SizeInPixels.Width - texX - 1;
                            if (hit.Side == 1 && hit.RayDirY < 0) texX = (int)texture.SizeInPixels.Width - texX - 1;

                            // Clamp texture X
                            texX = Math.Clamp(texX, 0, (int)texture.SizeInPixels.Width - 1);

                            int wallHeight = drawEnd - drawStart;

                            // Source rectangle from texture (vertical stripe)
                            var sourceRect = new Windows.Foundation.Rect(
                                texX,
                                0,
                                1,
                                texture.SizeInPixels.Height
                            );

                            // Destination rectangle on screen
                            var destRect = new Windows.Foundation.Rect(
                                screenX,
                                drawStart,
                                1,
                                wallHeight
                            );

                            // Apply shading for side walls using a tint effect
                            if (hit.Side == 1)
                            {
                                using (var tintEffect = new TintEffect
                                {
                                    Source = texture,
                                    Color = Color.FromArgb(255, 128, 128, 128) // 50% brightness
                                })
                                {
                                    ds.DrawImage(tintEffect, destRect, sourceRect);
                                }
                            }
                            else
                            {
                                ds.DrawImage(texture, destRect, sourceRect);
                            }
                        }

                        public void Dispose()
                        {
                            _renderTarget?.Dispose();
                            foreach (var texture in _textures.Values)
                            {
                                texture?.Dispose();
                            }
                            _textures.Clear();
                        }
                    }
                }
