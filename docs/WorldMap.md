# GORE Engine - World Map System

## Mode 7 Rendering

The world map uses a Mode 7-style pseudo-3D effect similar to Final Fantasy VI, Super Mario Kart, and other SNES classics.

### Features

- **Mode 7 Perspective**: Scanline-based rendering with depth perspective
- **360° Rotation**: Smooth camera rotation using Q/E or Left/Right arrows
- **Infinite Scrolling**: Wrapping world map for seamless exploration
- **Fog Effect**: Distance-based atmospheric fog
- **Real-time Movement**: WASD/Arrow keys for forward/backward movement
- **HUD Overlay**: Location name, coordinates, party leader, and step counter

### Controls

- **W / Up Arrow**: Move forward
- **S / Down Arrow**: Move backward
- **A / Left Arrow**: Rotate left (strafe)
- **D / Right Arrow**: Rotate right (strafe)
- **Q**: Rotate left
- **E**: Rotate right
- **ESC**: Return to main menu

### File Structure

```
Assets/Maps/
├── WorldMap.json          - World map definition (locations, size, metadata)
├── TerrainTypes.json      - Terrain type definitions (colors, walkability, encounters)
└── WorldMapTexture.png    - Optional: Pre-rendered world map texture (256x256 or larger)
```

### WorldMap.json Format

```json
{
  "name": "World of Balance",
  "width": 256,
  "height": 256,
  "backgroundMusic": "Assets/Music/Exploration.mp3",
  "layers": [
    {
      "name": "Terrain",
      "tiles": []
    }
  ],
  "locations": [
    {
      "name": "Starting Town",
      "x": 128,
      "y": 128,
      "type": "town",
      "visited": true
    }
  ]
}
```

### TerrainTypes.json Format

```json
[
  {
    "id": 0,
    "name": "Ocean",
    "color": "#1E4D8B",
    "isWalkable": false,
    "encounterRate": 0
  }
]
```

## Creating Custom World Maps

### Option 1: Texture-Based (Recommended)

1. Create a 256x256 (or larger power-of-2) PNG image representing your world map
2. Use different colors for different terrain types
3. Save as `Assets/Maps/WorldMapTexture.png`
4. Update `WorldMap.json` with location markers

### Option 2: Tile-Based

1. Define your terrain types in `TerrainTypes.json`
2. Create a 2D tile array in `WorldMap.json`'s layers section
3. The renderer will use the fallback mode with procedural colors

### Option 3: Future Map Editor

A visual map editor is planned for future releases to make map creation easier.

## Technical Details

### Mode 7 Algorithm

The renderer uses per-scanline transformation:

```
For each scanline y from horizon to bottom:
    distance = (cameraHeight * cameraDistance) / (y - horizonY)
    scale = distance / cameraDistance
    
    For each pixel x:
        screenX = (x - screenWidth/2) * scale
        worldX = cameraX + (screenX * cos(angle) - distance * sin(angle))
        worldY = cameraY + (screenX * sin(angle) + distance * cos(angle))
        
        Sample texture at (worldX, worldY)
        Apply fog based on distance
```

### Performance

- Uses Win2D for hardware-accelerated rendering
- Renders at ~60 FPS on modern hardware
- Optimized per-pixel drawing with fog calculations

## Future Enhancements

- [ ] World map editor tool
- [ ] Multiple layers (terrain, objects, overlays)
- [ ] Sprite rendering for towns/dungeons/vehicles
- [ ] Parallax scrolling for clouds
- [ ] Day/night cycle
- [ ] Weather effects
- [ ] Minimap display
- [ ] Airship/vehicle system
