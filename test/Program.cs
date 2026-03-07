using System;
using System.Threading.Tasks;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Input;

namespace GORETest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            bool firstLine = true;
            var engine = await GORE.Engine.GOREEngine.CreateAndInitializeAsync(
                msg => {
                    if (firstLine)
                    {
                        firstLine = false;
                        var prevBg = Console.BackgroundColor;
                        var prevFg = Console.ForegroundColor;
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                        int width = Console.WindowWidth;
                        string centered = msg.PadLeft((width + msg.Length) / 2).PadRight(width);
                        Console.WriteLine(centered);
                        Console.BackgroundColor = prevBg;
                        Console.ForegroundColor = prevFg;
                    }
                    else
                    {
                        Console.WriteLine(msg);
                    }
                },
                err => Console.Error.WriteLine(err)
            );


            // Load E1M1 map
            await engine.MapSystem.LoadMapByNameAsync("e1m1");

            // Verify all textures exist before starting
            var missingTextures = engine.MapSystem.VerifyTextures(engine.MapSystem.CurrentMap);
            if (missingTextures.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: The following textures are missing:");
                foreach (var tex in missingTextures)
                    Console.WriteLine(tex);
                Console.ResetColor();
                Console.WriteLine("Game cannot start due to missing textures. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            // OpenGL window using Silk.NET.Windowing
            var options = WindowOptions.Default;
            options.Size = new Silk.NET.Maths.Vector2D<int>(1920, 1200);
            options.Title = "GORE Engine";
            options.API = GraphicsAPI.Default;
            options.WindowState = WindowState.Fullscreen;

            IWindow window = Window.Create(options);
            GL gl = null;
            IInputContext input = null;

            window.Load += () =>
            {
                try
                {
                    gl = GL.GetApi(window);
                    input = window.CreateInput();
                    // Escape key handler to close window
                    if (input != null && input.Keyboards.Count > 0)
                    {
                        var keyboard = input.Keyboards[0];
                        keyboard.KeyDown += (kb, key, modifiers) =>
                        {
                            if (key == Silk.NET.Input.Key.Escape)
                                window.Close();
                        };
                    }
                    // Get the actual OpenGLRenderer instance from RendererSystem
                    var oglRendererField = engine.RendererSystem.GetType().GetField("_renderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var oglRenderer = oglRendererField?.GetValue(engine.RendererSystem) as GORE.Engine.Renderers.OpenGLRenderer;
                    if (oglRenderer != null)
                    {
                        // Set GL context
                        var glField = oglRenderer.GetType().GetField("_gl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (glField != null) glField.SetValue(oglRenderer, gl);

                        // Set map
                        oglRenderer.CurrentMap = engine.MapSystem.CurrentMap;

                        // Set keyboard
                        var keyboard = input.Keyboards.Count > 0 ? input.Keyboards[0] : null;
                        oglRenderer.SetKeyboard(keyboard);

                        // Initialize resources (important!)
                        // This will throw an exception if any texture fails to load
                        oglRenderer.InitializeResources(null, window.Size.X, window.Size.Y);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nFATAL ERROR during initialization: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to exit.");
                    Console.ReadKey();
                    window.Close();
                    Environment.Exit(1);
                }
            };

            window.Render += delta =>
            {
                // Render the map (pass delta for frame timing)
                engine.RendererSystem.Render(null, window.Size.X, window.Size.Y, (float)delta);
                // Update FPS counter in HUD
                engine.HudSystem.UpdateFPS((float)delta);
            };

            window.Run();
        }
    }
}
