using System;
using System.IO;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;

namespace GORE.Tools
{
    /// <summary>
    /// Helper class to generate a procedural world map texture for testing.
    /// In production, you would create this with an actual map editor.
    /// </summary>
    public static class WorldMapTextureGenerator
    {
        public static void GenerateDefaultTexture(string outputPath, int width = 256, int height = 256)
        {
            // This is a placeholder - actual implementation would need a CanvasDevice
            // For now, the Mode7Renderer will use its fallback rendering
            System.Diagnostics.Debug.WriteLine("WorldMapTextureGenerator: Use an external tool to create WorldMapTexture.png");
        }

        public static Color GetTerrainColor(int x, int y, int seed = 42)
        {
            var random = new Random(seed + x * 1000 + y);
            int terrainType = random.Next(100);

            if (terrainType < 40)
                return Color.FromArgb(255, 94, 170, 60);    // Grass
            else if (terrainType < 60)
                return Color.FromArgb(255, 45, 90, 30);     // Forest
            else if (terrainType < 75)
                return Color.FromArgb(255, 30, 77, 139);    // Water
            else if (terrainType < 85)
                return Color.FromArgb(255, 139, 115, 85);   // Mountain
            else if (terrainType < 95)
                return Color.FromArgb(255, 212, 165, 116);  // Desert
            else
                return Color.FromArgb(255, 232, 240, 248);  // Snow
        }
    }
}
