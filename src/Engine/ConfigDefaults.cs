namespace GORE.Engine
{
    // Central place for default config values and safe ranges
    public static class ConfigDefaults
    {
        // Config keys
        public const string KeyRenderWidth = "r_width";
        public const string KeyRenderHeight = "r_height";
        public const string KeyFov = "r_fov";
        public const string KeyMouseSensitivity = "m_sensitivity";
        public const string KeyMaxFps = "r_maxfps";
        public const string KeyShowFps = "r_showfps";

        // Resolution defaults
        public const int DefaultRenderWidth = 640;
        public const int DefaultRenderHeight = 480;
        public const int MinRenderWidth = 320;
        public const int MaxRenderWidth = 7680;
        public const int MinRenderHeight = 240;
        public const int MaxRenderHeight = 4320;

        // Field of view
        public const float DefaultFov = 90.0f;
        public const float MinFov = 60.0f;
        public const float MaxFov = 120.0f;

        // Mouse sensitivity
        public const float DefaultMouseSensitivity = 0.002f;
        public const float MinMouseSensitivity = 0.0001f;
        public const float MaxMouseSensitivity = 0.1f;

        // FPS
        public const int DefaultMaxFps = 60;
        public const int MinMaxFps = 30;
        public const int MaxMaxFps = 300;

        // Show FPS default
        public const bool DefaultShowFps = true;
    }
}
