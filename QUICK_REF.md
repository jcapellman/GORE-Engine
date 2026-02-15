# 🎮 GORE Wolf3D - Quick Reference

## 🚀 Run the Game
```bash
dotnet run --project test/GORETest.csproj
```

## 🗺️ Map Files

**Location**: `test/gt1/maps/level1.map`

```
# Comments start with #
# 0 = empty, 1-9 = textured walls
1,1,1,1,1,1
1,0,0,0,0,1
1,0,2,2,0,1
1,0,0,0,0,1
1,1,1,1,1,1
```

## 🎨 Textures

**Config**: `test/gt1/maps/textures.cfg`
```
1,gt1/textures/amberrock.jpg
2,gt1/textures/amberstone.jpg
3,gt1/textures/lightstone.jpg
4,gt1/textures/yellowrock.jpg
```

**Location**: `test/gt1/textures/`

## 📁 Project Structure
```
GORE-Engine/
├── src/Engine/
│   ├── RaycastEngine.cs    # Raycasting
│   ├── Renderer3D.cs       # Textures
│   └── MapLoader.cs        # Map loading
├── test/gt1/
│   ├── maps/
│   │   ├── level1.map      # Level data
│   │   └── textures.cfg    # Texture mapping
│   └── textures/           # Texture images
└── docs/
    ├── TEXTURE_GUIDE.md    # Full texture guide
    └── UPDATE_SUMMARY.md   # Recent changes
```

## 🎯 Key Features

✅ External map files (.map)  
✅ Textured walls (JPG/PNG)  
✅ Fallback system  
✅ 60 FPS rendering  
✅ Side wall shading  

## 🔧 Quick Edits

### Change Map
`src/UI/GameWindow.xaml.cs` line 39:
```csharp
var mapPath = Path.Combine(baseDirectory, "gt1", "maps", "level1.map");
```

### Add Texture
1. Copy image to `test/gt1/textures/`
2. Add line to `test/gt1/maps/textures.cfg`
3. Use ID in map file

### Change Resolution
`src/UI/GameWindow.xaml.cs` line 57:
```csharp
_renderer = new Renderer3D(1024, 768, _raycastEngine);
```

---
**Status**: ✅ Build Successful | 🎮 Ready to Play
