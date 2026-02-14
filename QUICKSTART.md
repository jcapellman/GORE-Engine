# GORE Engine Quick Start Guide

## 1. Open the Solution
- Open `GORE.sln` in Visual Studio 2022

## 2. Set GORETest as Startup Project
- In Solution Explorer, right-click on `GORETest`
- Select "Set as Startup Project"

## 3. Run the Test Application
- Press **F5** or click the Start button
- The application should launch in fullscreen mode

## 4. What to Expect
When you run GORETest, you should see:
- The window enters fullscreen mode (borderless)
- A black background with white text saying "GORE Engine Test"
- The cursor should be hidden (placeholder implementation)

## 5. Creating Your Own Game

### Step 1: Create a New WinUI 3 Project
```bash
dotnet new winui -n MyGame
```

### Step 2: Add GORE Engine Reference
Edit your `.csproj` file and add:
```xml
<ItemGroup>
  <ProjectReference Include="path\to\GORE.csproj" />
</ItemGroup>
```

### Step 3: Initialize GORE Engine
In `App.xaml.cs`:
```csharp
protected override async void OnLaunched(LaunchActivatedEventArgs args)
{
    m_window = new MainWindow();
    await GORE.Engine.GOREEngine.StartAsync(m_window);
    m_window.Activate();
}
```

### Step 4: Create game.json
Add a `game.json` file with your game configuration:
```json
{
  "game": {
    "title": "My Awesome Game",
    "version": "1.0.0",
    "developer": "Your Name"
  }
}
```

### Step 5: Create Game Pages
Create pages that inherit from GORE base classes:
- `BaseMainMenuPage` - For your main menu
- `BaseCharacterCreationPage` - For character creation
- `BaseGamePage` - For the main game

## 6. Testing Individual Components

### Test Configuration Loading
```csharp
var config = GORE.Engine.GOREEngine.GetConfiguration();
Console.WriteLine($"Game Title: {config?.Game?.Title}");
```

### Test Window Management
The engine automatically:
- Enters fullscreen mode
- Hides window borders
- Hides the cursor

## 7. Troubleshooting

### Build Errors
- Ensure you have .NET 9 SDK installed
- Restore NuGet packages: `dotnet restore`
- Check that Windows App SDK is properly installed

### Runtime Errors
- Verify `game.json` exists and is properly formatted
- Check that all required music files exist (they're optional but referenced in config)

### Input Not Working
The input handling has been commented out during the WinUI 3 migration. You'll need to implement:
- Window.KeyDown event handlers
- Or use KeyboardAccelerators on controls

## 8. Next Steps
- Check out the [README.md](README.md) for full documentation
- Look at the BaseGamePage implementation for examples
- Implement your own sprite rendering methods
- Add game assets (sprites, music, etc.)

## Need Help?
- Review the inline code comments for WinUI 3 migration notes
- Check the TODO items in the codebase
- The test app serves as a minimal working example
