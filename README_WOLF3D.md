# GORE Engine - Wolfenstein 3D Clone

A Wolfenstein 3D-style raycasting FPS engine built with WinUI 3 and .NET 9.

## Features

✅ **Raycasting 3D Engine**
- Classic Wolfenstein 3D-style raycasting
- Real-time rendering at 60 FPS
- Colored wall textures with depth shading
- Smooth player movement and rotation

✅ **Controls**
- **W/A/S/D** - Move forward/left/backward/right
- **Arrow Keys** - Look left/right
- **Space** - Shoot
- **ESC** - Exit game

✅ **Game Systems**
- Health system
- Ammo tracking
- Position tracking (debug)
- Collision detection

## Architecture

### Core Components

1. **RaycastEngine** (`src/Engine/RaycastEngine.cs`)
   - DDA (Digital Differential Analysis) raycasting algorithm
   - Player position and rotation management
   - Collision detection
   - Map representation (2D grid)

2. **Renderer3D** (`src/Engine/Renderer3D.cs`)
   - Software rendering using WriteableBitmap
   - Wall color palette system
   - Depth-based shading (darker side walls)
   - Ceiling and floor rendering

3. **GameWindow** (`src/UI/GameWindow.xaml(.cs)`)
   - Main game loop (60 FPS)
   - Input handling
   - HUD rendering
   - Player stats display

## Map Format

The game uses a 2D integer array for the map:
- `0` = Empty space
- `1-7` = Different colored walls

```csharp
int[,] worldMap = new int[,]
{
    {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
    {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
    {1,0,0,0,0,0,2,2,2,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
    // ... more rows
};
```

## Rendering System

The raycasting algorithm:
1. Cast a ray for each vertical screen column
2. Use DDA to find wall intersections
3. Calculate perpendicular distance to avoid fish-eye effect
4. Calculate wall height based on distance
5. Apply color and shading

## Performance

- Target: 60 FPS
- Resolution: 800x600 (configurable)
- Software rendering using WinUI WriteableBitmap

## Future Enhancements

Potential additions:
- [ ] Textured walls (bitmap textures instead of solid colors)
- [ ] Sprites (enemies, items, decorations)
- [ ] Doors and animated walls
- [ ] Weapons and shooting mechanics
- [ ] Sound effects and music
- [ ] Multiple levels
- [ ] Minimap
- [ ] Enemy AI
- [ ] HUD weapons display
- [ ] Particle effects (muzzle flash, blood)

## Technical Notes

### Why Software Rendering?
This implementation uses software raycasting (WriteableBitmap) instead of OpenGL for:
- Simplicity and educational value
- Better integration with WinUI
- Classic retro feel
- Cross-platform compatibility within WinUI ecosystem

### Upgrading to Hardware Acceleration
To use OpenGL/DirectX:
1. Add Silk.NET.OpenGL package (already in .csproj)
2. Create OpenGL rendering context
3. Port Renderer3D to use OpenGL vertex buffers
4. Add texture loading and shader support

## Build and Run

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project test/GORETest.csproj
```

## Project Structure

```
GORE-Engine/
├── src/
│   ├── Engine/
│   │   ├── GOREEngine.cs       # Entry point
│   │   ├── RaycastEngine.cs    # Raycasting algorithm
│   │   └── Renderer3D.cs       # 3D renderer
│   └── UI/
│       ├── GameWindow.xaml     # Main game window
│       └── GameWindow.xaml.cs  # Game loop & input
└── test/
    └── App.xaml.cs             # Application entry
```

## License

This is a reimplementation of classic Wolfenstein 3D raycasting techniques for educational purposes.

---

**Enjoy blasting through dungeons! 🔫**
