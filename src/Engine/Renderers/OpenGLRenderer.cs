using Silk.NET.OpenGL;
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using GORE.Engine.Systems;
using System.Linq;

namespace GORE.Engine.Renderers
{
    // Minimal OpenGL 3D renderer for ANGLE/WinUI3 integration
    public class OpenGLRenderer : IRenderer, IDisposable
    {
        // Returns the camera's forward direction as a unit vector
        private Vector3 GetCameraForward()
        {
            float y = MathF.Sin(_pitch);
            float x = MathF.Sin(_yaw) * MathF.Cos(_pitch);
            float z = MathF.Cos(_yaw) * MathF.Cos(_pitch);
            return Vector3.Normalize(new Vector3(x, y, z));
        }
        private readonly System.Collections.Generic.Dictionary<int, uint> _wallTextures = new();
        public GORE.Engine.MapData CurrentMap { get; set; }
        private Silk.NET.Input.IKeyboard _keyboard;
        public void SetKeyboard(Silk.NET.Input.IKeyboard keyboard) => _keyboard = keyboard;
        private GL _gl;
        private uint _vao, _vbo, _ebo, _shaderProgram;
        private int _width, _height;
        private bool _initialized;

        // Cube vertices (position, texcoord)
        private readonly float[] _vertices = {
            // positions        // texcoords
            -1, -1, -1,  0, 0,
             1, -1, -1,  1, 0,
             1,  1, -1,  1, 1,
            -1,  1, -1,  0, 1,
            -1, -1,  1,  0, 0,
             1, -1,  1,  1, 0,
             1,  1,  1,  1, 1,
            -1,  1,  1,  0, 1,
        };
        private readonly uint[] _indices = {
            0,1,2, 2,3,0, // back
            4,5,6, 6,7,4, // front
            0,4,7, 7,3,0, // left
            1,5,6, 6,2,1, // right
            3,2,6, 6,7,3, // top
            0,1,5, 5,4,0  // bottom
        };

        public OpenGLRenderer(ResourceLoader resourceLoader) { }

        public void Initialize(int width, int height, RaycastEngine raycastEngine)
        {
            _width = width;
            _height = height;
            // _gl = ... (get Silk.NET GL context from ANGLE/EGL)
            // For demo, assume _gl is valid and context is current
            // You must set _gl from your context provider before calling Render
        }

        public void InitializeResources(object device, int canvasWidth, int canvasHeight)
        {
            // You must set _gl from your context provider here if not already set
            if (_initialized) return;
            // Example: _gl = Silk.NET.OpenGL.GL.GetApi(...);
            if (_gl == null) return;

            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();
            _ebo = _gl.GenBuffer();

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            unsafe
            {
                fixed (float* v = _vertices)
                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            unsafe
            {
                fixed (uint* i = _indices)
                    _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(_indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
            }
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            _gl.BindVertexArray(0);

            _shaderProgram = CreateShaderProgram(_gl);
            _initialized = true;

            // Load all wall textures for the current map
            if (CurrentMap != null && CurrentMap.TextureMapping != null)
            {
                Console.WriteLine($"[TextureLoader] Loading {CurrentMap.TextureMapping.Count} textures for map '{CurrentMap.Name}'...");
                foreach (var kvp in CurrentMap.TextureMapping)
                {
                    // Only load if not already loaded
                    if (!_wallTextures.ContainsKey(kvp.Key))
                    {
                        try
                        {
                            // Synchronously wait for async method (safe here, only called once per texture)
                            LoadTextureAsync(kvp.Key, kvp.Value, device).GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[TextureLoader] FATAL ERROR: {ex.Message}");
                            Console.ResetColor();
                            throw new Exception($"Texture loading failed. Game cannot start.", ex);
                        }
                    }
                }
                Console.WriteLine($"[TextureLoader] All {_wallTextures.Count} textures loaded successfully.");
            }
        }

        public object GetRenderTarget() => null;

        // --- First-person raycasting state ---
        // 3D camera state
        // Camera Y will be set to playerHeight (6 units)
        private Vector3 _cameraPos = new Vector3(1.5f, 6.0f, 1.5f); // Player is 6 units tall, wall is 8 units
        private float _yaw = 0f;   // radians
        private float _pitch = 0f; // radians
        private float _moveSpeed = 20.0f; // Wolf3D-fast movement speed
        private float _rotSpeed = 1.5f; // radians/sec


        public unsafe void Render(object drawingSession, float width, float height, float dt = 1f/60f)
        {
            // Robust null checks to prevent NullReferenceException
            if (_gl == null)
            {
                Console.WriteLine("GL context is null");
                return;
            }
            if (!_initialized)
            {
                Console.WriteLine("Renderer not initialized");
                return;
            }
            if (CurrentMap == null)
            {
                Console.WriteLine("CurrentMap is null");
                return;
            }
            if (CurrentMap.Grid == null)
            {
                Console.WriteLine("CurrentMap.Grid is null");
                return;
            }
            // Ensure correct OpenGL state for opaque geometry
            _gl.Enable(Silk.NET.OpenGL.EnableCap.DepthTest);
            _gl.DepthMask(true);
            _gl.Disable(Silk.NET.OpenGL.EnableCap.Blend);
            _gl.Viewport(0, 0, (uint)width, (uint)height);
            _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f); // Black background
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            // WASD/Arrow movement for 3D camera
            if (_keyboard != null)
            {
                Vector3 forward = new Vector3((float)Math.Sin(_yaw), 0, (float)Math.Cos(_yaw));
                Vector3 right = new Vector3(-forward.Z, 0, forward.X);
                Vector3 move = Vector3.Zero;
                if (_keyboard.IsKeyPressed(Silk.NET.Input.Key.W))
                    move += forward;
                if (_keyboard.IsKeyPressed(Silk.NET.Input.Key.S))
                    move -= forward;
                if (_keyboard.IsKeyPressed(Silk.NET.Input.Key.A))
                    move -= right;
                if (_keyboard.IsKeyPressed(Silk.NET.Input.Key.D))
                    move += right;
                if (move != Vector3.Zero)
                {
                    move = Vector3.Normalize(move);
                    _cameraPos += move * _moveSpeed * dt;
                }
                // Arrow keys for looking
                if (_keyboard.IsKeyPressed(Silk.NET.Input.Key.Left))
                    _yaw += _rotSpeed * dt;
                if (_keyboard.IsKeyPressed(Silk.NET.Input.Key.Right))
                    _yaw -= _rotSpeed * dt;
                if (_keyboard.IsKeyPressed(Silk.NET.Input.Key.Up))
                    _pitch = Math.Clamp(_pitch - _rotSpeed * dt, -1.5f, 1.5f);
                if (_keyboard.IsKeyPressed(Silk.NET.Input.Key.Down))
                    _pitch = Math.Clamp(_pitch + _rotSpeed * dt, -1.5f, 1.5f);
            }

            // --- 3D map rendering ---
            if (CurrentMap != null && CurrentMap.Grid != null)
            {
                int mapW = CurrentMap.Width;
                int mapH = CurrentMap.Height;
                _gl.UseProgram(_shaderProgram);
                // Perspective projection
                float aspect = width / height;
                float fovY = MathF.PI / 3f; // 60 deg
                float near = 0.01f, far = 1000f; // Increased far plane for distant rendering
                var proj = Matrix4x4.CreatePerspectiveFieldOfView(fovY, aspect, near, far);
                // Camera view
                Vector3 camTarget = _cameraPos + GetCameraForward();
                var view = Matrix4x4.CreateLookAt(_cameraPos, camTarget, Vector3.UnitY);
                float wallSize = 8.0f;
                float playerHeight = 6.0f;
                uint[] quadIndices = { 0, 1, 2, 2, 3, 0 };
                int modelLoc = _gl.GetUniformLocation(_shaderProgram, "uModel");
                int colorLoc = _gl.GetUniformLocation(_shaderProgram, "uFlatColor");
                unsafe
                {
                    _gl.UniformMatrix4(_gl.GetUniformLocation(_shaderProgram, "uView"), 1, false, (float*)&view);
                    _gl.UniformMatrix4(_gl.GetUniformLocation(_shaderProgram, "uProj"), 1, false, (float*)&proj);
                }
                // --- Floor and Ceiling Rendering ---
                // --- Floor Rendering ---
                float[] floorVertices = {
                    0f, 0f, 0f, 0f, 0f,
                    mapW * wallSize, 0f, 0f, 1f, 0f,
                    mapW * wallSize, 0f, mapH * wallSize, 1f, 1f,
                    0f, 0f, mapH * wallSize, 0f, 1f
                };
                float[] ceilVertices = {
                    0f, 0f, 0f, 0f, 0f,
                    mapW * wallSize, 0f, 0f, 1f, 0f,
                    mapW * wallSize, 0f, mapH * wallSize, 1f, 1f,
                    0f, 0f, mapH * wallSize, 0f, 1f
                };
                // Floor (single quad)
                if (colorLoc != -1) _gl.Uniform4(colorLoc, 0.3f, 0.3f, 0.3f, 1.0f); // medium gray
                var model = Matrix4x4.CreateTranslation(0f, -0.01f, 0f);
                if (modelLoc != -1)
                {
                    unsafe { _gl.UniformMatrix4(modelLoc, 1, false, (float*)&model); }
                }
                _gl.BindVertexArray(_vao);
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
                unsafe
                {
                    fixed (float* v = floorVertices)
                        _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(floorVertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
                    fixed (uint* i = quadIndices)
                        _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(quadIndices.Length * sizeof(uint)), i, BufferUsageARB.DynamicDraw);
                }
                _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
                _gl.BindVertexArray(0);
                // Ceiling (single quad at correct height)
                if (colorLoc != -1) _gl.Uniform4(colorLoc, 0.2f, 0.2f, 0.2f, 1.0f); // dark gray
                var ceilingModel = Matrix4x4.CreateTranslation(0f, wallSize, 0f);
                if (modelLoc != -1)
                {
                    unsafe { _gl.UniformMatrix4(modelLoc, 1, false, (float*)&ceilingModel); }
                }
                _gl.BindVertexArray(_vao);
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
                unsafe
                {
                    fixed (float* v = ceilVertices)
                        _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(ceilVertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
                    fixed (uint* i = quadIndices)
                        _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(quadIndices.Length * sizeof(uint)), i, BufferUsageARB.DynamicDraw);
                }
                _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
                _gl.BindVertexArray(0);
                // Wolf3D-style: Draw all exposed faces for each wall cell
                float[][] faceVertices = new float[4][];
                // North face (toward -Z)
                faceVertices[0] = new float[] {
                    0f, 0f, 0f, 0f, 0f,
                    wallSize, 0f, 0f, 1f, 0f,
                    wallSize, wallSize, 0f, 1f, 1f,
                    0f, wallSize, 0f, 0f, 1f
                };
                // South face (toward +Z)
                faceVertices[1] = new float[] {
                    0f, 0f, wallSize, 0f, 0f,
                    wallSize, 0f, wallSize, 1f, 0f,
                    wallSize, wallSize, wallSize, 1f, 1f,
                    0f, wallSize, wallSize, 0f, 1f
                };
                // West face (toward -X)
                faceVertices[2] = new float[] {
                    0f, 0f, 0f, 0f, 0f,
                    0f, 0f, wallSize, 1f, 0f,
                    0f, wallSize, wallSize, 1f, 1f,
                    0f, wallSize, 0f, 0f, 1f
                };
                // East face (toward +X)
                faceVertices[3] = new float[] {
                    wallSize, 0f, 0f, 0f, 0f,
                    wallSize, 0f, wallSize, 1f, 0f,
                    wallSize, wallSize, wallSize, 1f, 1f,
                    wallSize, wallSize, 0f, 0f, 1f
                };
                int[] dx = { 0, 0, -1, 1 };
                int[] dy = { -1, 1, 0, 0 };
                for (int y = 0; y < mapH; y++)
                {
                    for (int x = 0; x < mapW; x++)
                    {
                        int wallType = CurrentMap.Grid[x, y];
                        if (wallType == 0) continue;
                        uint texId = 0;
                        _wallTextures.TryGetValue(wallType, out texId);
                        if (texId != 0)
                        {
                            _gl.BindTexture(TextureTarget.Texture2D, texId);
                            if (colorLoc != -1) _gl.Uniform4(colorLoc, 0f, 0f, 0f, -1f);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: No texture for wall type {wallType}, using blue fallback");
                            if (colorLoc != -1) _gl.Uniform4(colorLoc, 0.2f, 0.2f, 0.8f, 1f);
                        }
                        float wx = x * wallSize;
                        float wy = 0.0f;
                        float wz = y * wallSize;
                        // Always draw all four vertical faces (N, S, W, E)
                        for (int face = 0; face < 4; face++)
                        {
                            if (faceVertices[face] == null)
                                continue;
                            var model2 = Matrix4x4.CreateTranslation(wx, wy, wz);
                            if (modelLoc != -1)
                            {
                                unsafe { _gl.UniformMatrix4(modelLoc, 1, false, (float*)&model2); }
                            }
                            if (texId != 0)
                                _gl.BindTexture(TextureTarget.Texture2D, texId);
                            _gl.BindVertexArray(_vao);
                            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
                            unsafe
                            {
                                fixed (float* v = faceVertices[face])
                                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(faceVertices[face].Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
                                fixed (uint* i = quadIndices)
                                    _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(quadIndices.Length * sizeof(uint)), i, BufferUsageARB.DynamicDraw);
                            }
                            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
                            _gl.BindVertexArray(0);
                        }
                        if (texId != 0)
                            _gl.BindTexture(TextureTarget.Texture2D, 0);
                    }
                }
                _gl.UseProgram(0);
            }
        }

        // Satisfy IRenderer interface
        public void Render(object drawingSession, float width, float height)
        {
            Render(drawingSession, width, height, 1f / 60f);
        }

        public async Task LoadTextureAsync(int id, string path, object device)
        {
            if (_gl == null) 
                throw new InvalidOperationException("OpenGL context is not initialized");
            if (string.IsNullOrWhiteSpace(path)) 
                throw new ArgumentException("Texture path cannot be null or empty", nameof(path));
            if (_wallTextures.ContainsKey(id)) return;

            // Print the full resolved path for diagnostics
            var baseDirectory = AppContext.BaseDirectory;
            var resolvedPath = System.IO.Path.Combine(baseDirectory, path);
            Console.WriteLine($"[TextureLoader] Wall type {id}: requested '{path}', resolved '{resolvedPath}'");

            if (!System.IO.File.Exists(resolvedPath))
            {
                throw new FileNotFoundException($"Texture file not found for wall type {id}: {resolvedPath}", resolvedPath);
            }

            try
            {
                uint tex = ImageLoader.LoadTexture2D(_gl, resolvedPath);
                if (tex == 0)
                {
                    throw new Exception($"Failed to create OpenGL texture for wall type {id}: {resolvedPath}");
                }
                _wallTextures[id] = tex;
                Console.WriteLine($"[TextureLoader] Successfully loaded texture for wall type {id} as GL id {tex}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load texture for wall type {id} from '{resolvedPath}': {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            if (_gl == null) return;
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
            _gl.DeleteProgram(_shaderProgram);
        }

        // --- Helper methods for shader setup ---
        private static uint CreateShaderProgram(GL gl)
        {
            string vert = @"#version 300 es
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aTexCoord;
uniform mat4 uView;
uniform mat4 uProj;
uniform mat4 uModel;
out vec2 vTexCoord;
void main() {
    gl_Position = uProj * uView * uModel * vec4(aPos, 1.0);
    vTexCoord = aTexCoord;
}";
            string frag = @"#version 300 es
precision mediump float;
in vec2 vTexCoord;
uniform sampler2D uWallTex;
uniform vec4 uFlatColor;
out vec4 FragColor;
void main() {
    // Always reference both uniforms to avoid optimization out
    vec4 texColor = texture(uWallTex, vTexCoord);
    vec4 flatColor = uFlatColor;
    FragColor = (flatColor.a > 0.99) ? flatColor : texColor;
}";
            uint vs = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vs, vert);
            gl.CompileShader(vs);
            int vStatus = gl.GetShader(vs, Silk.NET.OpenGL.ShaderParameterName.CompileStatus);
            if (vStatus == 0)
            {
                string vLog = gl.GetShaderInfoLog(vs);
                Console.WriteLine("Vertex shader compile error:\n" + vLog);
            }
            uint fs = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fs, frag);
            gl.CompileShader(fs);
            int fStatus = gl.GetShader(fs, Silk.NET.OpenGL.ShaderParameterName.CompileStatus);
            if (fStatus == 0)
            {
                string fLog = gl.GetShaderInfoLog(fs);
                Console.WriteLine("Fragment shader compile error:\n" + fLog);
            }
            uint prog = gl.CreateProgram();
            gl.AttachShader(prog, vs);
            gl.AttachShader(prog, fs);
            gl.LinkProgram(prog);
            int linkStatus = gl.GetProgram(prog, Silk.NET.OpenGL.ProgramPropertyARB.LinkStatus);
            if (linkStatus == 0)
            {
                string pLog = gl.GetProgramInfoLog(prog);
                Console.WriteLine("Shader program link error:\n" + pLog);
            }
            gl.DeleteShader(vs);
            gl.DeleteShader(fs);
            return prog;
        }
    }
}
