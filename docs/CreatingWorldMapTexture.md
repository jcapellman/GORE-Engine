# Creating a World Map Texture for GORE Engine

## Quick Start

Create a 256x256 PNG image with different colored regions representing terrain types.

## Recommended Terrain Colors

Use these colors to match the `TerrainTypes.json` definitions:

### Terrain Color Palette

| Terrain   | Hex Color | RGB               | Description           |
|-----------|-----------|-------------------|-----------------------|
| Ocean     | #1E4D8B   | (30, 77, 139)     | Deep blue water       |
| Grass     | #5EAA3C   | (94, 170, 60)     | Green plains          |
| Forest    | #2D5A1E   | (45, 90, 30)      | Dark green trees      |
| Mountain  | #8B7355   | (139, 115, 85)    | Brown/gray peaks      |
| Desert    | #D4A574   | (212, 165, 116)   | Sandy tan             |
| Snow      | #E8F0F8   | (232, 240, 248)   | White/light blue      |

## Tools You Can Use

### Option 1: GIMP (Free)
1. Create new 256x256 image
2. Use pencil tool (hard edge, no anti-aliasing)
3. Paint terrain regions
4. Export as PNG to `src/Assets/Maps/WorldMapTexture.png`

### Option 2: Paint.NET (Free)
1. New image, 256x256
2. Use paint bucket and pencil tools
3. Save as PNG

### Option 3: Aseprite (Paid, Pixel Art Focused)
1. Create 256x256 canvas
2. Use pixel art tools to paint terrain
3. Export as PNG

### Option 4: Photoshop/Krita
1. 256x256 canvas
2. Paint with hard brushes (no anti-aliasing)
3. Save as PNG

## Design Tips

### Layout Suggestions
- **Center Island**: Place main continent in center
- **Ocean Borders**: Surround with ocean for wrapping effect
- **Clusters**: Group similar terrain (forest patches, mountain ranges)
- **Variety**: Mix terrain types for visual interest
- **Paths**: Create natural land bridges between continents

### Example Layout (256x256 grid)

```
┌────────────────────────────────────────┐
│ ≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈ │  ≈ Ocean
│ ≈≈≈≈≈≈≈▲▲▲▲░░░░░░░∙∙∙∙≈≈≈≈≈≈≈≈≈≈≈≈≈≈ │  ∙ Grass
│ ≈≈≈≈▲▲▲▲▲░░░░∙∙∙∙∙∙∙∙∙≈≈≈≈≈≈≈≈≈≈≈≈≈ │  ░ Forest
│ ≈≈≈▲▲▲▲░░░∙∙∙∙∙∙∙∙∙∙∙∙≈≈≈≈≈≈≈≈≈≈≈≈ │  ▲ Mountain
│ ≈≈≈▲▲░░░∙∙∙∙∙∙░░∙∙∙∙∙∙≈≈≈≈≈≈≈≈≈≈≈≈ │  ☼ Desert
│ ≈≈≈░░░∙∙∙∙∙∙∙∙░░∙∙∙∙∙∙∙≈≈≈≈≈≈≈≈≈≈≈ │  * Snow
│ ≈≈∙∙∙∙∙∙∙∙∙∙∙∙░░∙∙∙∙∙∙∙∙≈≈≈≈≈≈≈≈≈≈ │
│ ≈≈∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙≈≈≈≈≈≈≈≈≈≈ │
│ ≈≈∙∙∙∙∙░░∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙≈≈≈≈≈≈≈≈≈≈ │
│ ≈≈≈∙∙░░░░∙∙∙∙∙∙∙∙∙∙∙∙∙∙≈≈≈≈≈≈≈≈≈≈≈ │
│ ≈≈≈≈∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙≈≈≈≈☼☼☼≈≈≈≈ │
│ ≈≈≈≈≈∙∙∙∙∙∙∙∙∙∙∙∙∙∙∙≈≈≈☼☼☼☼☼☼≈≈≈ │
│ ≈≈≈≈≈≈∙∙∙∙∙∙∙∙∙∙∙∙≈≈≈≈☼☼☼☼☼☼≈≈≈≈ │
│ ≈≈≈≈≈≈≈≈∙∙∙∙∙∙∙≈≈≈≈≈≈≈☼☼☼≈≈≈≈≈≈≈ │
│ ≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈ │
└────────────────────────────────────────┘
```

### World Map Patterns

**FF6-Style World of Balance:**
- Large central continent
- Smaller islands scattered around
- Mountain ranges dividing regions
- Coastal towns and ports
- Hidden caves in mountains
- Desert regions in one area

**Island Archipelago:**
- Multiple medium-sized islands
- Lots of ocean for ship travel
- Bridge connections or shallow water paths

**Pangaea Continent:**
- One massive landmass
- Diverse terrain zones (tundra north, desert south)
- Rivers and lakes as obstacles

## File Specifications

- **Dimensions**: 256x256 pixels (or 512x512, 1024x1024 for higher detail)
- **Format**: PNG
- **Color Mode**: RGB (no alpha channel needed)
- **Location**: `src/Assets/Maps/WorldMapTexture.png`
- **Anti-aliasing**: None (use hard edges for pixel art style)

## Testing Your Map

1. Save your PNG to `src/Assets/Maps/WorldMapTexture.png`
2. Build and run GORE Engine
3. Select "New Game"
4. Your map should render with Mode 7 perspective!

## Advanced: Multiple Maps

For multiple world maps (like FF6's World of Balance vs World of Ruin):

1. Create separate textures:
   - `WorldMapBalance.png`
   - `WorldMapRuin.png`

2. Update `WorldMap.json`:
   ```json
   {
     "name": "World of Balance",
     "texture": "Assets/Maps/WorldMapBalance.png",
     ...
   }
   ```

3. Load appropriate map based on game progress

## Example Color Swatches (Copy to your image editor)

### Ocean
- Primary: #1E4D8B
- Deep: #0F2645
- Shallow: #4A7BB7

### Grass
- Primary: #5EAA3C
- Light: #7BC461
- Dark: #3D7227

### Forest
- Primary: #2D5A1E
- Dense: #1E3D14
- Edge: #4A7D32

### Mountain
- Primary: #8B7355
- Peak: #B0937A
- Shadow: #6B563F

## Resources

- [Lospec Palette List](https://lospec.com/palette-list) - Pixel art color palettes
- [Piskel](https://www.piskelapp.com/) - Free online pixel art editor
- [Tiled Map Editor](https://www.mapeditor.org/) - For future tile-based approach

## Need Help?

The Mode 7 renderer will use a procedural fallback if no texture is found, so you can test the system without creating a texture first!
