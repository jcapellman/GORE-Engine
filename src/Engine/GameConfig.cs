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

        public IReadOnlyDictionary<string, ConfigVariable> Variables => _variables;

        public GameConfig(string configFileName = "config.json")
        {
            var baseDirectory = AppContext.BaseDirectory;
            _configPath = Path.Combine(baseDirectory, configFileName);
            
            RegisterDefaultVariables();
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
                    System.Diagnostics.Debug.WriteLine($"Config file not found: {_configPath}");
                    SaveConfig(); // Create default config
                    return;
                }

                var json = File.ReadAllText(_configPath);
                var configData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (configData == null)
                    return;

                int loadedCount = 0;
                foreach (var kvp in configData)
                {
                    var variable = Get(kvp.Key);
                    if (variable == null || !variable.Flags.HasFlag(ConfigVariableFlags.Archive))
                        continue;

                    try
                    {
                        object value = kvp.Value.ValueKind switch
                        {
                            JsonValueKind.String => kvp.Value.GetString(),
                            JsonValueKind.Number => kvp.Value.GetDouble(),
                            JsonValueKind.True or JsonValueKind.False => kvp.Value.GetBoolean(),
                            _ => null
                        };

                        if (value != null)
                        {
                            variable.SetValue(value, silent: true);
                            loadedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load config variable {kvp.Key}: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✓ Loaded {loadedCount} config variables from {_configPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Failed to load config: {ex.Message}");
            }
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
