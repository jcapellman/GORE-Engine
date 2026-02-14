using System.Collections.Generic;

namespace GORE.Models
{
    /// <summary>
    /// Main configuration model for GORE Engine games.
    /// Loaded from game.json in the Assets folder.
    /// </summary>
    public class GameConfiguration
    {
        public GameInfo Game { get; set; } = new GameInfo();
        public AssetPaths Assets { get; set; } = new AssetPaths();
        public MusicPaths Music { get; set; } = new MusicPaths();
        public List<string> BattleBackgrounds { get; set; } = new List<string>();
        public SpritePaths Sprites { get; set; } = new SpritePaths();
        public GameplaySettings Gameplay { get; set; } = new GameplaySettings();
        public UISettings UI { get; set; } = new UISettings();
    }

    public class GameInfo
    {
        public string Title { get; set; } = "Untitled RPG";
        public string Version { get; set; } = "1.0.0";
        public string Developer { get; set; } = "GORE Engine";
    }

    public class AssetPaths
    {
        public string MainMenuBackground { get; set; } = "Assets/MainMenu.png";
        public string Logo { get; set; } = "Assets/Logo.png";
        public string Cursor { get; set; } = "Assets/Cursor.png";
        public string Font { get; set; } = "Segoe UI";
    }

    public class MusicPaths
    {
        public string MainMenu { get; set; } = "";
        public string Exploration { get; set; } = "";
        public string Battle { get; set; } = "";
        public string Victory { get; set; } = "";
        public string GameOver { get; set; } = "";
    }

    public class SpritePaths
    {
        public string Heroes { get; set; } = "Assets/Sprites/Heroes/";
        public string Enemies { get; set; } = "Assets/Sprites/Enemies/";
    }

    public class GameplaySettings
    {
        public int StartingLevel { get; set; } = 1;
        public int StartingHP { get; set; } = 100;
        public int StartingMP { get; set; } = 50;
        public int EncounterRate { get; set; } = 10;
        public float ExpMultiplier { get; set; } = 1.0f;
    }

    public class UISettings
    {
        public string MainMenuBackground { get; set; } = "Assets/UI/MainMenu.png";
        public string Cursor { get; set; } = "Assets/UI/Cursor.png";
        public string Font { get; set; } = "Segoe UI";
        public int FontSize { get; set; } = 32;
        public string Theme { get; set; } = "FF6Blue";
        public string BorderColor { get; set; } = "#FFFFFF";
        public string BackgroundColor { get; set; } = "#0000AA";
        public HPBarColors HpBarColors { get; set; } = new HPBarColors();
    }

    public class HPBarColors
    {
        public string High { get; set; } = "#00FF00";
        public string Medium { get; set; } = "#FFFF00";
        public string Low { get; set; } = "#FF0000";
    }
}
