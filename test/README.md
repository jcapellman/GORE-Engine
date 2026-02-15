# GORE Engine Test Application

A minimal WinUI 3 test application demonstrating GORE Engine integration.

## Project Structure

```
test/
├── App.xaml              - Application definition with resource dictionary
├── App.xaml.cs           - Application entry point (starts GORE Engine)
├── game.json             - Game configuration
├── app.manifest          - Application manifest
└── GORETest.csproj       - Project file
```

## How It Works

The test app demonstrates the **absolute minimum** setup required to use the GORE Engine:

1. **App.xaml.cs**: Single line in `OnLaunched`:
   ```csharp
   await GORE.Engine.GOREEngine.StartAsync();
   ```

   The GORE Engine handles everything automatically:
   - Window creation
   - Fullscreen mode
   - Cursor management
   - Splash screen display
   - Main menu transition
   - Configuration loading

2. **game.json**: Contains the game configuration that the GORE Engine loads

## Running the Test

1. Set `GORETest` as the startup project in Visual Studio
2. Press F5 to run
3. You'll see:
   - GORE Engine splash screen (3 seconds)
   - Automatic transition to main menu
   - Fullscreen mode with hidden cursor

## What Gets Tested

- ✅ GORE Engine initialization
- ✅ Automatic window management
- ✅ Splash screen display
- ✅ Main menu screen
- ✅ Configuration loading from game.json
- ✅ WinUI 3 integration

## Creating Your Own Game

To create a game using the GORE Engine:

1. Create a new WinUI 3 application
2. Add a reference to the GORE Engine project
3. Create a `game.json` configuration file
4. In `App.xaml.cs`, call `await GORE.Engine.GOREEngine.StartAsync();`
5. Customize the main menu and add game screens

That's it! The GORE Engine is a "batteries included" framework.
