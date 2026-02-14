# GORE Engine - Complete Implementation Guide

## 🎯 Overview

This document provides **step-by-step instructions** for completing the GORE Engine implementation and integrating it into Mystic Chronicles.

---

## 📦 **What's Already Done**

✅ GORE.Core project structure created  
✅ BasePage.cs (fullscreen/cursor management)  
✅ GameConfiguration.cs (all config models)  
✅ ConfigurationService.cs (JSON loading)  
✅ GORE.Core.csproj (NuGet package metadata)  
✅ Documentation and templates  

---

## 🚀 **Phase 2: Copy Core Components**

### Step 1: Copy Model Files

**From:** `C:\Users\jcape\source\repos\Mystic-Chronicles\src\Models\`  
**To:** `C:\Users\jcape\source\repos\GORE-Engine\src\GORE.Core\Models\`

Files to copy:
- `Character.cs`
- `Enemy.cs`
- `Tile.cs` (if exists)
- `SaveData.cs`

**Modifications needed:**
```csharp
// Change namespace from:
namespace MysticChronicles.Models

// To:
namespace GORE.Core.Models
```

### Step 2: Copy GameEngine Files

**From:** `C:\Users\jcape\source\repos\Mystic-Chronicles\src\GameEngine\`  
**To:** `C:\Users\jcape\source\repos\GORE-Engine\src\GORE.Core\GameEngine\`

Files to copy:
- `BattleSystem.cs`
- `Map.cs`
- `InputManager.cs`
- `GameState.cs`

**Modifications needed:**
```csharp
// Change namespace from:
namespace MysticChronicles.GameEngine

// To:
namespace GORE.Core.GameEngine

// Update using statements:
using GORE.Core.Models;
```

### Step 3: Copy Service Files

**From:** `C:\Users\jcape\source\repos\Mystic-Chronicles\src\Services\`  
**To:** `C:\Users\jcape\source\repos\GORE-Engine\src\GORE.Core\Services\`

Files to copy:
- `MusicManager.cs`
- `SaveGameManager.cs`

**Modifications needed:**
```csharp
// Change namespace
namespace GORE.Core.Services

// Update using statements
using GORE.Core.Models;
using GORE.Core.GameEngine;
```

---

## 🔧 **Phase 3: Create GameEngine.cs**

Create new file: `C:\Users\jcape\source\repos\GORE-Engine\src\GORE.Core\Engine\GameEngine.cs`

```csharp
using System;
using System.Threading.Tasks;
using GORE.Core.Models;
using GORE.Core.Services;
using GORE.Core.GameEngine;

namespace GORE.Core.Engine
{
    /// <summary>
    /// Main GORE Engine orchestrator. Initialize this in your App.xaml.cs
    /// </summary>
    public class GameEngine
    {
        private static GameEngine? _instance;
        public static GameEngine Instance => _instance ??= new GameEngine();

        public GameConfiguration Config { get; private set; } = new GameConfiguration();
        public MusicManager Music { get; private set; } = null!;

        private GameEngine() { }

        /// <summary>
        /// Initialize the GORE Engine. Call this in App.xaml.cs OnLaunched
        /// </summary>
        public async Task InitializeAsync()
        {
            // Load game configuration from Assets/game.json
            Config = await ConfigurationService.LoadConfigurationAsync();

            // Initialize music manager with config paths
            Music = new MusicManager();

            // Enter fullscreen and hide cursor globally
            EnterFullScreenMode();
            HideCursor();
        }

        /// <summary>
        /// Create a new hero character based on configuration settings
        /// </summary>
        public Character CreateHero(string name)
        {
            return new Character
            {
                Name = name,
                Level = Config.Gameplay.StartingLevel,
                MaxHP = Config.Gameplay.StartingHP,
                CurrentHP = Config.Gameplay.StartingHP,
                MaxMP = Config.Gameplay.StartingMP,
                CurrentMP = Config.Gameplay.StartingMP,
                Attack = 20,
                Defense = 15,
                Magic = 18,
                Speed = 12,
                X = 5,
                Y = 5
            };
        }

        private void EnterFullScreenMode()
        {
            var view = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            view.TryEnterFullScreenMode();
        }

        private void HideCursor()
        {
            Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = null;
        }
    }
}
```

---

## 📦 **Phase 4: Build NuGet Package**

### Option A: Visual Studio

1. Open `GORE.Core.csproj` in Visual Studio
2. Right-click project → Properties
3. Verify NuGet metadata (already set in .csproj)
4. Build → Build Solution (Release mode)
5. Output: `bin\Release\GORE.Core.1.0.0.nupkg`

### Option B: Command Line

```powershell
cd C:\Users\jcape\source\repos\GORE-Engine\src\GORE.Core
dotnet build -c Release
dotnet pack -c Release
```

### Test Locally

Add local NuGet source:
```powershell
nuget sources add -name "GORE Local" -source "C:\Users\jcape\source\repos\GORE-Engine\src\GORE.Core\bin\Release"
```

---

## 🔄 **Phase 5: Update Mystic Chronicles**

### Step 1: Create game.json

Create: `C:\Users\jcape\source\repos\Mystic-Chronicles\src\Assets\game.json`

Copy content from: `docs/game.json.template`

### Step 2: Update MysticChronicles.csproj

Add package reference:
```xml
<ItemGroup>
  <PackageReference Include="GORE.Core" Version="1.0.0" />
  <PackageReference Include="Win2D.uwp" Version="1.28.0" />
</ItemGroup>

<ItemGroup>
  <Content Include="Assets\game.json" />
</ItemGroup>
```

### Step 3: Update App.xaml.cs

```csharp
using GORE.Core.Engine;

protected override async void OnLaunched(LaunchActivatedEventArgs e)
{
    // Initialize GORE Engine FIRST
    await GameEngine.Instance.InitializeAsync();

    Frame rootFrame = Window.Current.Content as Frame;

    if (rootFrame == null)
    {
        rootFrame = new Frame();
        Window.Current.Content = rootFrame;
    }

    if (rootFrame.Content == null)
    {
        rootFrame.Navigate(typeof(MainMenuPage), e.Arguments);
    }

    // Don't call fullscreen/hide cursor - engine handles it
    Window.Current.Activate();
}
```

### Step 4: Update Page Files

#### MainMenuPage.xaml
```xml
<!-- OLD -->
<local:BasePage ...>

<!-- NEW -->
<gore:BasePage 
    xmlns:gore="using:GORE.Core.Pages"
    ...>
```

#### MainMenuPage.xaml.cs
```csharp
// OLD
using MysticChronicles;
public sealed partial class MainMenuPage : BasePage

// NEW
using GORE.Core.Pages;
public sealed partial class MainMenuPage : BasePage
```

**Repeat for all pages:**
- GamePage.xaml / GamePage.xaml.cs
- CharacterCreationPage.xaml / CharacterCreationPage.xaml.cs

### Step 5: Update Using Statements

Find and replace across all .cs files:
```csharp
// OLD
using MysticChronicles.Models;
using MysticChronicles.GameEngine;
using MysticChronicles.Services;

// NEW
using GORE.Core.Models;
using GORE.Core.GameEngine;
using GORE.Core.Services;
```

### Step 6: Remove Duplicate Files

Delete from Mystic Chronicles (now in GORE.Core):
- ❌ `src/BasePage.cs`
- ❌ `src/Models/Character.cs`
- ❌ `src/Models/Enemy.cs`
- ❌ `src/Models/SaveData.cs`
- ❌ `src/GameEngine/BattleSystem.cs`
- ❌ `src/GameEngine/Map.cs`
- ❌ `src/GameEngine/InputManager.cs`
- ❌ `src/GameEngine/GameState.cs`
- ❌ `src/Services/MusicManager.cs`
- ❌ `src/Services/SaveGameManager.cs`

### Step 7: Build and Test

```powershell
cd C:\Users\jcape\source\repos\Mystic-Chronicles
dotnet build
```

Fix any compilation errors (usually just missing using statements)

---

## ✅ **Verification Checklist**

### GORE Engine:
- [ ] All files copied from Mystic Chronicles
- [ ] Namespaces updated to GORE.Core.*
- [ ] Project builds successfully
- [ ] NuGet package created

### Mystic Chronicles:
- [ ] game.json created and configured
- [ ] GORE.Core package reference added
- [ ] All pages inherit from GORE.Core.Pages.BasePage
- [ ] All using statements updated
- [ ] Duplicate files removed
- [ ] Project builds successfully
- [ ] Game runs correctly

---

## 🎮 **Success Criteria**

When complete, you should be able to:
1. Build GORE.Core and generate NuGet package
2. Reference GORE.Core in Mystic Chronicles
3. Run Mystic Chronicles with zero code changes (just config)
4. Create NEW RPG games by just changing game.json

---

## 🆘 **Troubleshooting**

### "Type 'BasePage' not found"
- Ensure GORE.Core package is installed
- Check using statement: `using GORE.Core.Pages;`
- Verify XAML namespace: `xmlns:gore="using:GORE.Core.Pages"`

### "Configuration not loading"
- Check game.json exists in Assets folder
- Verify Build Action = Content
- Ensure JSON is valid (use JSONLint.com)

### "Music not playing"
- Verify music file paths in game.json
- Check files exist in Assets/Music/
- Ensure files are marked as Content

---

## 📞 **Need Help?**

Open an issue on GitHub with:
- Error message
- File/line number
- What you were trying to do

---

✅ **Follow these steps in order and you'll have GORE Engine fully operational!** 🚀
