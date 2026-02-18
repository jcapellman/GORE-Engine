using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GORE.Engine
{
    /// <summary>
    /// Game configuration manager (similar to Quake 2's cvar system)
    /// </summary>
    public class GameConfig
    {
        private readonly Dictionary<string, ConfigVariable> _variables = new();
        private readonly string _configPath;
        private bool _configLoadedSuccessfully;
        private string _configStatusMessage;

        public IReadOnlyDictionary<string, ConfigVariable> Variables => _variables;
        public string ConfigPath => _configPath;
        public bool ConfigLoadedSuccessfully => _configLoadedSuccessfully;
        public string ConfigStatusMessage => _configStatusMessage;

        private readonly GORE.Engine.Systems.ResourceLoader _resourceLoader;

        public GameConfig(string configFileName = "config.json", GORE.Engine.Systems.ResourceLoader resourceLoader = null)
        {
            var baseDirectory = AppContext.BaseDirectory;
            _configPath = ResolveConfigPath(baseDirectory, configFileName);
            _resourceLoader = resourceLoader;
            RegisterDefaultVariables();
        }

        /// <summary>
        /// Resolve config file path, checking multiple locations in priority order
        /// </summary>
        private string ResolveConfigPath(string baseDirectory, string configFileName)
        {
            // Priority order for config file locations:
            // 1. Base directory (deployment location)
            // 2. test subdirectory (development fallback)
            var potentialPaths = new[]
            {
                Path.Combine(baseDirectory, configFileName),
                Path.Combine(baseDirectory, "test", configFileName)
            };

            foreach (var path in potentialPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Default to base directory if none found (will be created on save)
            return potentialPaths[0];
        }

        private void RegisterDefaultVariables()
        {
            // Video/Rendering settings
            Register("r_width", 640, "Render width", ConfigVariableFlags.Archive);
            Register("r_height", 480, "Render height", ConfigVariableFlags.Archive);
            Register("r_fullscreen", true, "Fullscreen mode", ConfigVariableFlags.Archive);
            Register("r_vsync", true, "Vertical sync", ConfigVariableFlags.Archive);
            Register("r_fov", 90.0f, "Field of view (60-120)", ConfigVariableFlags.Archive);
            
            // Performance settings
            Register("r_maxfps", 60, "Maximum frames per second", ConfigVariableFlags.Archive);
            Register("r_showfps", true, "Show FPS counter", ConfigVariableFlags.Archive);
            
            // Input settings
            Register("m_sensitivity", 0.002f, "Mouse sensitivity", ConfigVariableFlags.Archive);
            Register("m_invert", false, "Invert mouse Y axis", ConfigVariableFlags.Archive);
            
            // Gameplay settings
            Register("g_godmode", false, "God mode (invincibility)", ConfigVariableFlags.Cheat);
            Register("g_noclip", false, "No clip mode (walk through walls)", ConfigVariableFlags.Cheat);
            Register("g_infiniteammo", false, "Infinite ammo", ConfigVariableFlags.Cheat);
            
            // Audio settings
            Register("s_volume", 1.0f, "Master volume (0-1)", ConfigVariableFlags.Archive);
            Register("s_musicvolume", 0.8f, "Music volume (0-1)", ConfigVariableFlags.Archive);
            Register("s_effectsvolume", 1.0f, "Sound effects volume (0-1)", ConfigVariableFlags.Archive);
            
            // Developer settings
            Register("developer", false, "Developer mode", ConfigVariableFlags.Archive);
            Register("com_showfps", true, "Show FPS in console", ConfigVariableFlags.Archive);
            
            // Engine info (read-only)
            Register("version", "1.0.0", "Engine version", ConfigVariableFlags.ReadOnly);
            Register("build", "dev", "Build type", ConfigVariableFlags.ReadOnly);
        }

        public ConfigVariable Register(string name, object defaultValue, string description = "", ConfigVariableFlags flags = ConfigVariableFlags.None)
        {
            var variable = new ConfigVariable(name, defaultValue, description, flags);
            _variables[name.ToLower()] = variable;
            return variable;
        }

        public ConfigVariable Get(string name)
        {
            return _variables.TryGetValue(name.ToLower(), out var variable) ? variable : null;
        }

        public T GetValue<T>(string name, T defaultValue = default)
        {
            var variable = Get(name);
            return variable != null ? variable.GetValue<T>() : defaultValue;
        }

        public void Set(string name, object value, bool silent = false)
        {
            var variable = Get(name);
            variable?.SetValue(value, silent);
        }

        public void LoadConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    _configStatusMessage = $"Config not found - creating defaults at {Path.GetFileName(_configPath)}";
                    _configLoadedSuccessfully = false;
                    System.Diagnostics.Debug.WriteLine(_configStatusMessage);
                    SaveConfig(); // Create default config
                    _configLoadedSuccessfully = true;
                    _configStatusMessage = "Default config created successfully";
                    return;
                }

                Dictionary<string, JsonElement> configData = null;
                if (_resourceLoader != null)
                {
                    try
                    {
                        configData = _resourceLoader.LoadJson<Dictionary<string, JsonElement>>(_configPath);
                    }
                    catch (Exception ex)
                    {
                        _configStatusMessage = $"Invalid config JSON - regenerating defaults (Error: {ex.Message})";
                        _configLoadedSuccessfully = false;
                        System.Diagnostics.Debug.WriteLine(_configStatusMessage);

                        // Backup corrupt config
                        try
                        {
                            var backupPath = _configPath + ".corrupt.bak";
                            File.Copy(_configPath, backupPath, true);
                            System.Diagnostics.Debug.WriteLine($"  Backed up corrupt config to {Path.GetFileName(backupPath)}");
                        }
                        catch { /* Ignore backup errors */ }

                        SaveConfig(); // Regenerate defaults
                        _configLoadedSuccessfully = true;
                        _configStatusMessage = "Config regenerated from defaults";
                        return;
                    }
                }
                else
                {
                    var json = File.ReadAllText(_configPath);
                    configData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                }

                if (configData == null || configData.Count == 0)
                {
                    _configStatusMessage = "Empty config - using defaults";
                    _configLoadedSuccessfully = false;
                    System.Diagnostics.Debug.WriteLine(_configStatusMessage);
                    SaveConfig();
                    _configLoadedSuccessfully = true;
                    return;
                }

                int loadedCount = 0;
                int failedCount = 0;
                foreach (var kvp in configData)
                {
                    var variable = Get(kvp.Key);
                    if (variable == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Unknown config variable: {kvp.Key} (ignored)");
                        continue;
                    }

                    if (!variable.Flags.HasFlag(ConfigVariableFlags.Archive))
                        continue;

                    try
                    {
                        object value = kvp.Value.ValueKind switch
                        {
                            JsonValueKind.String => kvp.Value.GetString(),
                            JsonValueKind.Number => ConvertNumber(kvp.Value, variable),
                            JsonValueKind.True or JsonValueKind.False => kvp.Value.GetBoolean(),
                            _ => null
                        };

                        if (value != null)
                        {
                            variable.SetValue(value, silent: true);
                            loadedCount++;
                        }
                        else
                        {
                            failedCount++;
                            System.Diagnostics.Debug.WriteLine($"  Invalid value type for {kvp.Key} - using default");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        System.Diagnostics.Debug.WriteLine($"  Failed to load {kvp.Key}: {ex.Message} - using default");
                    }
                }

                _configLoadedSuccessfully = true;
                _configStatusMessage = failedCount > 0 
                    ? $"Loaded {loadedCount} settings ({failedCount} using defaults)" 
                    : $"Loaded {loadedCount} settings";

                System.Diagnostics.Debug.WriteLine($"✓ {_configStatusMessage} from {Path.GetFileName(_configPath)}");
            }
            catch (Exception ex)
            {
                _configStatusMessage = $"Config load failed - using defaults (Error: {ex.Message})";
                _configLoadedSuccessfully = false;
                System.Diagnostics.Debug.WriteLine($"✗ {_configStatusMessage}");
                // Continue with defaults - don't crash
            }
        }

        /// <summary>
        /// Convert JSON number to the appropriate type based on variable type
        /// </summary>
        private object ConvertNumber(JsonElement element, ConfigVariable variable)
        {
            var valueType = variable.GetValue<object>().GetType();

            if (valueType == typeof(int))
                return element.GetInt32();
            else if (valueType == typeof(float))
                return (float)element.GetDouble();
            else if (valueType == typeof(double))
                return element.GetDouble();
            else
                return element.GetDouble(); // Default to double
        }

        public void SaveConfig()
        {
            try
            {
                var configData = new Dictionary<string, object>();

                foreach (var variable in _variables.Values)
                {
                    if (variable.Flags.HasFlag(ConfigVariableFlags.Archive))
                    {
                        configData[variable.Name] = variable.GetValue<object>();
                    }
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(configData, options);
                File.WriteAllText(_configPath, json);

                System.Diagnostics.Debug.WriteLine($"✓ Saved config to {_configPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Failed to save config: {ex.Message}");
            }
        }

        public void ResetAll()
        {
            foreach (var variable in _variables.Values)
            {
                if (!variable.Flags.HasFlag(ConfigVariableFlags.ReadOnly))
                {
                    variable.Reset();
                }
            }
        }

        public List<ConfigVariable> Find(string filter)
        {
            var results = new List<ConfigVariable>();
            filter = filter.ToLower();

            foreach (var variable in _variables.Values)
            {
                if (variable.Name.Contains(filter))
                {
                    results.Add(variable);
                }
            }

            return results;
        }
    }
}
