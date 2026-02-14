# GORE Engine

A WinUI 3-based RPG game engine for creating retro-style games.

## Solution Structure

- **src/GORE.csproj** - The GORE Engine library
- **test/GORETest/** - A test WinUI 3 application demonstrating GORE Engine usage

## Building

### Prerequisites
- Visual Studio 2022 (17.8 or later)
- .NET 9 SDK
- Windows App SDK

### Build Instructions

1. Open `GORE.sln` in Visual Studio
2. Restore NuGet packages
3. Build the solution (Ctrl+Shift+B)

### Running the Test App

1. Set `GORETest` as the startup project (right-click on GORETest in Solution Explorer)
2. Press F5 to run

## GORE Engine Features

### Core Components

- **GOREEngine**: Main engine initialization and window management
- **BaseGamePage**: Complete RPG framework with battle system, exploration, and menu handling
- **BaseCharacterCreationPage**: Character creation functionality
- **BaseMainMenuPage**: Main menu framework
- **BasePage**: Base page class for all GORE pages

### Key Services

- **ConfigurationService**: Loads game configuration from `game.json`
- **MusicManager**: Background music management
- **BattleSystem**: Turn-based battle system
- **InputManager**: Input handling abstraction
- **SaveGameManager**: Save/load game state

### Models

- **GameConfiguration**: Game settings and metadata
- **Character**: Player/NPC character data
- **Enemy**: Enemy data
- **Map**: Tile-based map system
- **SaveData**: Save game data structure

## Using GORE Engine

### Minimal Setup

```csharp
// In your App.xaml.cs OnLaunched method:
protected override async void OnLaunched(LaunchActivatedEventArgs args)
{
    m_window = new MainWindow();
    
    // Initialize GORE Engine
    await GORE.Engine.GOREEngine.StartAsync(m_window);
    
    m_window.Activate();
}
```

### Creating Game Pages

Inherit from the base page classes and implement the required abstract methods:

```csharp
public class MyGamePage : BaseGamePage
{
    protected override void DrawExplorationMode(CanvasDrawingSession session)
    {
        // Draw exploration view
    }

    protected override void DrawBattleMode(CanvasDrawingSession session)
    {
        // Draw battle view
    }

    // Implement other required methods...
}
```

### Configuration

Create a `game.json` file in your project root:

```json
{
  "game": {
    "title": "My Game",
    "version": "1.0.0",
    "developer": "Your Name",
    "description": "Game description"
  },
  "music": {
    "title": "music/title.mp3",
    "exploration": "music/exploration.mp3",
    "battle": "music/battle.mp3"
  }
}
```

## Current Status

The GORE Engine has been migrated from UWP to WinUI 3 for .NET 9. Some features require additional implementation:

- ✅ Window management and fullscreen mode
- ✅ Basic page navigation
- ✅ Configuration loading
- ⚠️ Input handling (needs WinUI 3-specific implementation)
- ⚠️ Cursor hiding (placeholder implementation)
- ⚠️ Game timer (needs DispatcherQueue implementation)

See the inline comments in the code for implementation notes.

## License

[Add your license here]

## Contributing

[Add contribution guidelines here]
