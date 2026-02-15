# Quick Start Guide - GORE Wolf3D Clone

## 🚀 Running the Game

```bash
dotnet run --project test/GORETest.csproj
```

## 🎮 Controls

| Key | Action |
|-----|--------|
| W | Move Forward |
| S | Move Backward |
| A | Strafe Left |
| D | Strafe Right |
| ← | Turn Left |
| → | Turn Right |
| Space | Shoot |
| ESC | Exit |

## 📁 Current Project Files (Clean!)

```
src/
├── Engine/
│   ├── GOREEngine.cs       ✅ Entry point
│   ├── RaycastEngine.cs    ✅ Core raycasting
│   └── Renderer3D.cs       ✅ 3D rendering
└── UI/
    ├── GameWindow.xaml     ✅ Main window
    ├── GameWindow.xaml.cs  ✅ Game loop
    └── ScreenHelper.cs     ✅ Utilities
```

## 🎯 What Was Removed

✅ **26 old JRPG files deleted:**
- All battle system files
- All world map files  
- All character/enemy models
- All service layers
- Win2D dependencies

## ⚙️ Build Status

✅ **BUILD SUCCESSFUL** - Ready to run!

---

**Enjoy your retro FPS! 🔫**
