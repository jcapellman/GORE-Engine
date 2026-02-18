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
        private readonly Dictionary<int, TintEffect> _tintEffects = new();
        private readonly GORE.Engine.Systems.ResourceLoader _resourceLoader;

        public Renderer3D(int width, int height, RaycastEngine raycastEngine, GORE.Engine.Systems.ResourceLoader resourceLoader)
        {
            _screenWidth = width;
            _screenHeight = height;
            _raycastEngine = raycastEngine;
            _resourceLoader = resourceLoader;
        }

        // Initialize or recreate the render target. Accept explicit size so the render target
        // can match the output canvas resolution and avoid scaling artifacts.
        public void InitializeResources(CanvasDevice device, int width, int height)
        {
            // Dispose previous render target if any
            _renderTarget?.Dispose();
            _renderTarget = new CanvasRenderTarget(device, width, height, 96);
        }

        // Backwards-compatible overload: initialize using the configured logical screen
        // resolution (the renderer's `r_width` / `r_height` settings).
        public void InitializeResources(CanvasDevice device)
        {
            InitializeResources(device, _screenWidth, _screenHeight);
        }

        public CanvasRenderTarget GetRenderTarget() => _renderTarget;

        public async Task LoadTextureAsync(int textureId, string texturePath, CanvasDevice device)
        {
            try
            {
                var canvasBitmap = await _resourceLoader.LoadTextureAsync(texturePath, device);
                _textures[textureId] = canvasBitmap;
                _tintEffects[textureId] = new TintEffect
                {
                    Source = canvasBitmap,
                    Color = Color.FromArgb(255, 128, 128, 128)
                };
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
                // Use the actual render-target pixel size so rendering covers the full target.
                int rtWidth = (int)_renderTarget.SizeInPixels.Width;
                int rtHeight = (int)_renderTarget.SizeInPixels.Height;

                // Clear screen with ceiling and floor colors
                ds.Clear(Color.FromArgb(255, 64, 64, 64)); // Dark gray ceiling

                // Draw floor
                ds.FillRectangle(0, rtHeight / 2, rtWidth, rtHeight / 2,
                    Color.FromArgb(255, 32, 32, 32));

                // Raycast for each vertical stripe
                for (int x = 0; x < rtWidth; x++)
                {
                    var hit = _raycastEngine.CastRay(x, rtWidth);

                    // Skip if no wall was hit
                    if (hit.WallType == 0)
                        continue;

                    // Check if this is a door and get its state
                    DoorState doorState = null;
                    if (hit.IsDoor)
                    {
                        doorState = _raycastEngine.GetDoorState(hit.MapX, hit.MapY);

                        // If door is fully open, skip rendering it
                        if (doorState != null && doorState.OpenAmount >= 0.99f)
                            continue;
                    }

                    // Calculate line height using the render-target height
                    int lineHeight = (int)(rtHeight / hit.Distance);

                    // Calculate lowest and highest pixel to fill in current stripe
                    int drawStart = Math.Max(0, -lineHeight / 2 + rtHeight / 2);
                    int drawEnd = Math.Min(rtHeight - 1, lineHeight / 2 + rtHeight / 2);

                    // Draw textured wall - throw error if texture is missing
                    if (!_textures.ContainsKey(hit.WallType))
                    {
                        throw new InvalidOperationException($"Missing texture for wall type {hit.WallType}. All textures must be loaded before rendering.");
                    }

                    DrawTexturedWallWin2D(ds, x, drawStart, drawEnd, hit, doorState);
                }
            }

            // Scale the render target to fill the entire canvas.
            // Use nearest-neighbor interpolation to avoid sampling/warping artifacts
            // when the render target is scaled up (common in raycasters with 1px-wide stripes).
            var destRect = new Windows.Foundation.Rect(0, 0, canvasWidth, canvasHeight);
            // Draw the full render target to the output canvas. Provide an explicit source rect
            // (the full render target) so we can specify opacity and nearest-neighbor interpolation.
            var sourceRect = new Windows.Foundation.Rect(0, 0, _renderTarget.SizeInPixels.Width, _renderTarget.SizeInPixels.Height);
            drawingSession.DrawImage(_renderTarget, destRect, sourceRect, 1.0f, CanvasImageInterpolation.NearestNeighbor);
        }

                        private void DrawTexturedWallWin2D(CanvasDrawingSession ds, int screenX, int drawStart, int drawEnd, RaycastHit hit, DoorState doorState = null)
                        {
                            var texture = _textures[hit.WallType];

                            // Calculate wall X coordinate (0.0 to 1.0)
                            float wallX = hit.WallX;

                            // For doors, offset the texture based on how open the door is
                            // Doors slide horizontally into the wall pocket (Wolfenstein 3D style)
                            if (doorState != null)
                            {
                                // As door opens (0 to 1), we want to show less of the texture
                                // OpenAmount 0.0 = fully visible, 1.0 = fully hidden

                                // The door texture slides to the side as it opens
                                // We reduce the visible portion of the texture
                                float visiblePortion = 1.0f - doorState.OpenAmount;

                                // Only render if there's something visible
                                if (visiblePortion <= 0.01f)
                                    return;

                                // Adjust the texture coordinate to show only the visible portion
                                // This creates the sliding effect
                                wallX = wallX * visiblePortion;
                            }

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

                            // Apply shading for side walls using cached tint effect
                            if (hit.Side == 1)
                            {
                                // Use cached tint effect instead of creating new one
                                var tintEffect = _tintEffects[hit.WallType];
                                ds.DrawImage(tintEffect, destRect, sourceRect, 1.0f, CanvasImageInterpolation.NearestNeighbor);
                            }
                            else
                            {
                                ds.DrawImage(texture, destRect, sourceRect, 1.0f, CanvasImageInterpolation.NearestNeighbor);
                            }
                        }

                        public void Dispose()
                        {
                            _renderTarget?.Dispose();
                            foreach (var tintEffect in _tintEffects.Values)
                            {
                                tintEffect?.Dispose();
                            }
                            _tintEffects.Clear();
                            foreach (var texture in _textures.Values)
                            {
                                texture?.Dispose();
                            }
                            _textures.Clear();
                        }
                    }
                }
