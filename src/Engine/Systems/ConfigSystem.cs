using System;

namespace GORE.Engine
{
    public class ConfigSystem
    {
        private GameConfig _config;

        public void Load()
        {
            _config = new GameConfig();
            _config.LoadConfig();
        }

        public GameConfig GetConfig()
        {
            if (_config == null) throw new InvalidOperationException("Config not loaded. Call Load() before accessing configuration.");
            return _config;
        }

        public T GetValue<T>(string name, T defaultValue)
        {
            if (_config == null) throw new InvalidOperationException("Config not loaded. Call Load() before accessing configuration.");
            return _config.GetValue(name, defaultValue);
        }

        public bool ShowFps => GetValue("r_showfps", ConfigDefaults.DefaultShowFps);
        public float MouseSensitivity => GetValue("m_sensitivity", ConfigDefaults.DefaultMouseSensitivity);

        // Typed convenience properties
        public int RenderWidth => GetValue("r_width", ConfigDefaults.DefaultRenderWidth);
        public int RenderHeight => GetValue("r_height", ConfigDefaults.DefaultRenderHeight);
        public float Fov => GetValue("r_fov", ConfigDefaults.DefaultFov);
        public int MaxFps => GetValue("r_maxfps", ConfigDefaults.DefaultMaxFps);

        public event Action ConfigValuesUpdated;
        public event Action<int,int> RenderResolutionChanged;

        public void ValidateAndClamp()
        {
            if (_config == null) return;

            // Validate and clamp critical config values to safe ranges
            ValidateConfigValue("r_width", ConfigDefaults.MinRenderWidth, ConfigDefaults.MaxRenderWidth, ConfigDefaults.DefaultRenderWidth);
            ValidateConfigValue("r_height", ConfigDefaults.MinRenderHeight, ConfigDefaults.MaxRenderHeight, ConfigDefaults.DefaultRenderHeight);
            ValidateConfigValue("r_fov", ConfigDefaults.MinFov, ConfigDefaults.MaxFov, ConfigDefaults.DefaultFov);
            ValidateConfigValue("m_sensitivity", ConfigDefaults.MinMouseSensitivity, ConfigDefaults.MaxMouseSensitivity, ConfigDefaults.DefaultMouseSensitivity);
            ValidateConfigValue("r_maxfps", ConfigDefaults.MinMaxFps, ConfigDefaults.MaxMaxFps, ConfigDefaults.DefaultMaxFps);
        }

        private void ValidateConfigValue<T>(string name, T min, T max, T defaultValue) where T : IComparable
        {
            var variable = _config.Get(name);
            if (variable == null) return;

            // Use object comparisons with IComparable
            var currentValueObj = variable.GetValue<object>();
            if (currentValueObj is IComparable comparable)
            {
                if (comparable.CompareTo(min) < 0)
                {
                    variable.SetValue(min, silent: true);
                }
                else if (comparable.CompareTo(max) > 0)
                {
                    variable.SetValue(max, silent: true);
                }
            }
        }

        public void SubscribeToChanges(Action<ConfigVariable> onRenderResolutionChanged, Action<ConfigVariable> onConfigChanged)
        {
            if (_config == null) return;

            var renderWidthVar = _config.Get("r_width");
            var renderHeightVar = _config.Get("r_height");
            var showFpsVar = _config.Get("r_showfps");
            var mouseSensitivityVar = _config.Get("m_sensitivity");

            if (renderWidthVar != null && onRenderResolutionChanged != null)
                renderWidthVar.OnChanged += onRenderResolutionChanged;
            if (renderHeightVar != null && onRenderResolutionChanged != null)
                renderHeightVar.OnChanged += onRenderResolutionChanged;

            // Also expose a typed event for resolution changes
            if (renderWidthVar != null && renderHeightVar != null)
            {
                renderWidthVar.OnChanged += _ => RenderResolutionChanged?.Invoke(RenderWidth, RenderHeight);
                renderHeightVar.OnChanged += _ => RenderResolutionChanged?.Invoke(RenderWidth, RenderHeight);
            }

            if (showFpsVar != null && onConfigChanged != null)
                showFpsVar.OnChanged += v => { onConfigChanged(v); ConfigValuesUpdated?.Invoke(); };
            if (mouseSensitivityVar != null && onConfigChanged != null)
                mouseSensitivityVar.OnChanged += v => { onConfigChanged(v); ConfigValuesUpdated?.Invoke(); };
        }

        public void Save()
        {
            _config?.SaveConfig();
        }
    }
}