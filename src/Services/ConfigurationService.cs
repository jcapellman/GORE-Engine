using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using GORE.Models;
#nullable enable
namespace GORE.Services
{
    /// <summary>
    /// Service for loading and managing game configuration from game.json
    /// </summary>
    public static class ConfigurationService
    {
        private static GameConfiguration? _config;

        /// <summary>
        /// Loads game configuration from Assets/game.json
        /// </summary>
        public static async Task<GameConfiguration> LoadConfigurationAsync()
        {
            if (_config != null)
                return _config;

            try
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appx:///Assets/game.json"));
                
                var json = await FileIO.ReadTextAsync(file);
                
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                
                _config = JsonSerializer.Deserialize<GameConfiguration>(json, options);
                
                _config ??= GetDefaultConfiguration();
                
                return _config;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load game.json: {ex.Message}");
                
                // Fall back to default configuration
                _config = GetDefaultConfiguration();
                return _config;
            }
        }

        /// <summary>
        /// Gets the currently loaded configuration (must call LoadConfigurationAsync first)
        /// </summary>
        public static GameConfiguration GetConfiguration()
        {
            return _config ?? GetDefaultConfiguration();
        }

        /// <summary>
        /// Reloads the configuration from disk
        /// </summary>
        public static async Task ReloadConfigurationAsync()
        {
            _config = null;
            await LoadConfigurationAsync();
        }

        private static GameConfiguration GetDefaultConfiguration()
        {
            return new GameConfiguration
            {
                Game = new GameInfo
                {
                    Title = "Untitled RPG",
                    Version = "1.0.0",
                    Developer = "GORE Engine"
                },
                Assets = new AssetPaths
                {
                    MainMenuBackground = "Assets/MainMenu.png",
                    Logo = "Assets/Logo.png",
                    Cursor = "Assets/Cursor.png",
                    Font = "Segoe UI"
                },
                Music = new MusicPaths(),
                BattleBackgrounds = new System.Collections.Generic.List<string>(),
                Sprites = new SpritePaths(),
                Gameplay = new GameplaySettings
                {
                    StartingLevel = 1,
                    StartingHP = 100,
                    StartingMP = 50,
                    EncounterRate = 10,
                    ExpMultiplier = 1.0f
                },
                UI = new UISettings
                {
                    Theme = "FF6Blue",
                    BorderColor = "#FFFFFF",
                    BackgroundColor = "#0000AA",
                    HpBarColors = new HPBarColors
                    {
                        High = "#00FF00",
                        Medium = "#FFFF00",
                        Low = "#FF0000"
                    }
                }
            };
        }
    }
}
