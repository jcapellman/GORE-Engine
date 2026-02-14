# 🔧 GORE Engine Namespace Fix - Manual Steps

## ✅ Current Status:

You correctly want to use the **simple `GORE.*` namespace** instead of `GORE.Core.*`!

## 📁 Current Structure (CORRECT):

```
GORE-Engine/
└── src/
    ├── Pages/           ← namespace GORE.Pages ✅
    ├── Models/          ← namespace GORE.Models ✅
    ├── Engine/          ← namespace GORE.Engine ✅
    ├── Services/        ← namespace GORE.Services ✅
    └── GameEngine/      ← namespace GORE.GameEngine ✅
```

---

## 🔧 Files to Fix Manually:

### **1. Close and Reopen These Files in Visual Studio:**

These files are currently open and can't be edited:
- `GORE-Engine/src/Pages/MainMenuPage.xaml.cs`
- `GORE-Engine/src/Pages/CharacterCreationPage.xaml.cs`

### **2. Replace MainMenuPage.xaml.cs Content:**

**File:** `C:\Users\jcape\source\repos\GORE-Engine\src\Pages\MainMenuPage.xaml.cs`

**Replace entire contents with:**
```csharp
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI.Xaml;
using GORE.Services;
using GORE.Models;

namespace GORE.Pages
{
    public sealed partial class MainMenuPage : BaseMainMenuPage
    {
        public MainMenuPage()
        {
            this.InitializeComponent();
            LoadGameConfiguration();
        }

        private async void LoadGameConfiguration()
        {
            config = await ConfigurationService.LoadConfigurationAsync();

            if (config != null)
            {
                txtGameTitle.Text = config.Game.Title;
                txtVersion.Text = $"v{config.Game.Version}";
                txtDeveloper.Text = $"by {config.Game.Developer}";
            }
        }

        protected override void UpdateMenuCursor()
        {
            cursorNewGame.Visibility = menuSelection == 0 ? Visibility.Visible : Visibility.Collapsed;
            cursorLoadGame.Visibility = menuSelection == 1 ? Visibility.Visible : Visibility.Collapsed;
            cursorExit.Visibility = menuSelection == 2 ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnAnimationFrame()
        {
            animationCanvas?.Invalidate();
        }

        protected override void OnNewGame()
        {
            this.Frame.Navigate(typeof(CharacterCreationPage));
        }

        protected override async void OnLoadGame()
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;

            bool saveExists = await SaveGameManager.SaveExists();

            if (saveExists)
            {
                var saveData = await SaveGameManager.LoadGame();
                if (saveData != null)
                {
                    await ShowDialogAsync("Load Game", "Implement game-specific navigation");
                }
                else
                {
                    await ShowDialogAsync("Load Failed", "Failed to load saved game.");
                }
            }
            else
            {
                await ShowDialogAsync("No Save Data", "No saved game found.");
            }
            
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        protected override void OnExit()
        {
            Application.Current.Exit();
        }

        private void AnimationCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var session = args.DrawingSession;
            float width = (float)sender.ActualWidth;
            float height = (float)sender.ActualHeight;

            DrawCloudLayer(session, width, height, cloudOffset1, 0.3f, 25);
            DrawCloudLayer(session, width, height, cloudOffset2, 0.5f, 40);
            DrawMistLayer(session, width, height, mistOffset);
        }

        private async System.Threading.Tasks.Task ShowDialogAsync(string title, string content)
        {
            isDialogOpen = true;
            
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK"
            };
            
            await dialog.ShowAsync();
            isDialogOpen = false;
        }
    }
}
```

### **3. Replace CharacterCreationPage.xaml.cs Content:**

**File:** `C:\Users\jcape\source\repos\GORE-Engine\src\Pages\CharacterCreationPage.xaml.cs`

**Replace entire contents with:**
```csharp
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GORE.Services;
using GORE.Models;

namespace GORE.Pages
{
    public sealed partial class CharacterCreationPage : BaseCharacterCreationPage
    {
        public CharacterCreationPage()
        {
            this.InitializeComponent();
            LoadGameConfiguration();
        }

        private async void LoadGameConfiguration()
        {
            config = await ConfigurationService.LoadConfigurationAsync();
        }

        protected override void UpdateSelectionCursor()
        {
            cursorConfirm.Visibility = selection == 0 ? Visibility.Visible : Visibility.Collapsed;
            cursorCancel.Visibility = selection == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnCharacterNameChanged(string name)
        {
            txtHeroName.Text = string.IsNullOrWhiteSpace(name) ? "Hero" : name;
        }

        protected override void OnConfirm(string heroName)
        {
            MusicManager.PlayMusic(MusicTrack.Exploration);
            
            // Games will navigate to their own GamePage
            // For now, navigate to Mystic Chronicles GamePage
            this.Frame.Navigate(typeof(MysticChronicles.GamePage), heroName);
        }

        protected override void OnCancel()
        {
            this.Frame.Navigate(typeof(MainMenuPage));
        }
    }
}
```

---

## ✅ Verification:

After making these changes:

1. **Clean Solution** (Build → Clean Solution)
2. **Rebuild Solution** (Build → Rebuild Solution)
3. **Check for errors** - should compile successfully!

---

## 🎯 Expected Result:

**GORE Engine Structure:**
```
namespace GORE.Pages
namespace GORE.Models
namespace GORE.Engine
namespace GORE.Services
namespace GORE.GameEngine
```

**Simple, clean, no `GORE.Core`!** ✅

---

## 📝 Already Fixed:

✅ `GamePage.xaml.cs` in Mystic Chronicles - now uses `GORE.*`
✅ `MainMenuPage.xaml.cs` in Mystic Chronicles - now uses `GORE.*`
✅ `ULTIMATE_App.xaml.cs` - now uses `GORE.Engine`
✅ GORE.Core folder doesn't exist (good!)

---

**Once you make these two file changes, everything should build!** 🎉
