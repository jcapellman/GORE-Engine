using System;

namespace GORE.Engine
{
    public class ConfigSystem
    {
        private GameConfig _config;
        private readonly GORE.Engine.Systems.ResourceLoader _resourceLoader;

        public ConfigSystem(GORE.Engine.Systems.ResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        public void Load()
        {
            _config = new GameConfig("config.json", _resourceLoader);
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

        public bool ShowFps => GetValue(ConfigDefaults.KeyShowFps, ConfigDefaults.DefaultShowFps);
        public float MouseSensitivity => GetValue(ConfigDefaults.KeyMouseSensitivity, ConfigDefaults.DefaultMouseSensitivity);

        // Typed convenience properties
        public int RenderWidth => GetValue(ConfigDefaults.KeyRenderWidth, ConfigDefaults.DefaultRenderWidth);
        public int RenderHeight => GetValue(ConfigDefaults.KeyRenderHeight, ConfigDefaults.DefaultRenderHeight);
        public float Fov => GetValue(ConfigDefaults.KeyFov, ConfigDefaults.DefaultFov);
        public int MaxFps => GetValue(ConfigDefaults.KeyMaxFps, ConfigDefaults.DefaultMaxFps);

        public event Action ConfigValuesUpdated;
        public event Action<int,int> RenderResolutionChanged;

        public void ValidateAndClamp()
        {
            if (_config == null) return;

            // Validate and clamp critical config values to safe ranges
            ValidateConfigValue(ConfigDefaults.KeyRenderWidth, ConfigDefaults.MinRenderWidth, ConfigDefaults.MaxRenderWidth, ConfigDefaults.DefaultRenderWidth);
            ValidateConfigValue(ConfigDefaults.KeyRenderHeight, ConfigDefaults.MinRenderHeight, ConfigDefaults.MaxRenderHeight, ConfigDefaults.DefaultRenderHeight);
            ValidateConfigValue(ConfigDefaults.KeyFov, ConfigDefaults.MinFov, ConfigDefaults.MaxFov, ConfigDefaults.DefaultFov);
            ValidateConfigValue(ConfigDefaults.KeyMouseSensitivity, ConfigDefaults.MinMouseSensitivity, ConfigDefaults.MaxMouseSensitivity, ConfigDefaults.DefaultMouseSensitivity);
            ValidateConfigValue(ConfigDefaults.KeyMaxFps, ConfigDefaults.MinMaxFps, ConfigDefaults.MaxMaxFps, ConfigDefaults.DefaultMaxFps);
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

            var renderWidthVar = _config.Get(ConfigDefaults.KeyRenderWidth);
            var renderHeightVar = _config.Get(ConfigDefaults.KeyRenderHeight);
            var showFpsVar = _config.Get(ConfigDefaults.KeyShowFps);
            var mouseSensitivityVar = _config.Get(ConfigDefaults.KeyMouseSensitivity);

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