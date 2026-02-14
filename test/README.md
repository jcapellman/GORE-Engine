# GORE Engine Test Application

This is a simple WinUI 3 test application for the GORE Engine.

## Project Structure

```
test/GORETest/
├── App.xaml              - Application definition
├── App.xaml.cs           - Application code-behind (initializes GORE Engine)
├── MainWindow.xaml       - Main window UI
├── MainWindow.xaml.cs    - Main window code-behind
├── game.json             - Game configuration for GORE Engine
├── app.manifest          - Application manifest
└── GORETest.csproj       - Project file
```

## How It Works

The test app demonstrates the minimum setup required to use the GORE Engine:

1. **App.xaml.cs**: In the `OnLaunched` method, we:
   - Create the main window
   - Initialize the GORE Engine by calling `GORE.Engine.GOREEngine.StartAsync(m_window)`
   - Activate the window

2. **game.json**: Contains the game configuration that the GORE Engine loads

3. **MainWindow**: A simple window with a test message

## Running the Test

1. Set GORETest as the startup project in Visual Studio
2. Press F5 to run
3. The window should appear in fullscreen with the cursor hidden (as configured by GORE Engine)

## What Gets Tested

- GORE Engine initialization
- Window management (fullscreen, cursor hiding)
- Configuration loading from game.json
- Basic WinUI 3 integration

## Next Steps

To create a full game:
- Add pages that inherit from `BaseGamePage`, `BaseMainMenuPage`, or `BaseCharacterCreationPage`
- Implement the abstract sprite rendering methods
- Add game assets (sprites, music, etc.)
- Configure navigation between pages
