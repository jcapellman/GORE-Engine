# Wolf3D Texture System

## Overview
The GORE Wolf3D engine now supports textured walls loaded from image files!

## File Structure

```
test/gt1/
├── maps/
│   ├── level1.map        # Map grid data
│   └── textures.cfg      # Texture ID to file mapping
└── textures/
    ├── amberrock.jpg     # Texture ID 1
    ├── amberstone.jpg    # Texture ID 2
    ├── lightstone.jpg    # Texture ID 3
    └── yellowrock.jpg    # Texture ID 4
```

## Map File Format (.map)

Maps are simple CSV files with comments:

```
# GORE Wolf3D Map Format
# 0 = empty space (walkable)
# 1-9 = wall with texture ID

1,1,1,1,1,1,1,1
1,0,0,0,0,0,0,1
1,0,2,2,2,2,0,1
1,0,2,0,0,2,0,1
1,0,0,0,0,0,0,1
1,1,1,1,1,1,1,1
```

- Lines starting with `#` are comments
- `0` = empty space (player can walk)
- `1-9` = walls (textured if texture exists)

## Texture Configuration (.cfg)

Maps texture IDs to image files:

```
# Format: TextureID,TexturePath

1,gt1/textures/amberrock.jpg
2,gt1/textures/amberstone.jpg
3,gt1/textures/lightstone.jpg
4,gt1/textures/yellowrock.jpg
```

## Creating New Textures

### Texture Requirements
- **Format**: JPG or PNG
- **Recommended Size**: 64x64, 128x128, or 256x256 pixels
- **Power of 2**: Width and height should be power of 2 for best performance
- **Seamless**: Textures should tile seamlessly

### Steps to Add a Texture

1. **Add image file** to `test/gt1/textures/`
   ```
   test/gt1/textures/mywall.jpg
   ```

2. **Update textures.cfg**
   ```
   5,gt1/textures/mywall.jpg
   ```

3. **Use in map file**
   ```
   1,1,1,5,5,5,1,1
   1,0,0,0,0,0,0,1
   ```

## Creating New Maps

### 1. Create map file in `test/gt1/maps/`

Example `test/gt1/maps/level2.map`:
```
# Level 2 - The Castle

1,1,1,1,1,1,1,1,1,1
1,0,0,0,0,0,0,0,0,1
1,0,2,2,2,2,2,2,0,1
1,0,2,0,0,0,0,2,0,1
1,0,2,0,3,3,0,2,0,1
1,0,2,0,3,3,0,2,0,1
1,0,2,0,0,0,0,2,0,1
1,0,2,2,2,4,2,2,0,1
1,0,0,0,0,0,0,0,0,1
1,1,1,1,1,1,1,1,1,1
```

### 2. Load in GameWindow.xaml.cs

Change the map path:
```csharp
var mapPath = System.IO.Path.Combine(baseDirectory, "gt1", "maps", "level2.map");
```

## Texture Rendering Details

### How Textures Are Mapped

The engine:
1. **Casts a ray** for each screen column
2. **Finds wall intersection** using DDA algorithm
3. **Calculates texture X coordinate** based on wall hit position
4. **Samples texture vertically** for each pixel in the wall stripe
5. **Applies shading** (side walls are darker for depth effect)

### Shading
- **Front/back walls**: Full brightness
- **Side walls**: 50% brightness (darker)

This creates a depth effect without true lighting.

## Performance Considerations

### Texture Size
- **Smaller textures** (64x64): Faster loading, less memory, lower quality
- **Larger textures** (256x256): Slower loading, more memory, higher quality

### Recommended
- Use **128x128** for most textures
- Use **64x64** for distant walls
- Use **256x256** for important/close-up walls

### Memory Usage
Each texture uses: `Width × Height × 4 bytes`
- 64×64 = 16 KB
- 128×128 = 64 KB
- 256×256 = 256 KB

## Advanced: Fallback System

If a texture fails to load, the engine falls back to solid colors:

```csharp
private readonly Color[] _wallColors = new[]
{
    Color.FromArgb(255, 100, 100, 100),  // 0 = empty
    Color.FromArgb(255, 255, 0, 0),      // 1 = red
    Color.FromArgb(255, 0, 255, 0),      // 2 = green
    Color.FromArgb(255, 0, 0, 255),      // 3 = blue
    Color.FromArgb(255, 255, 255, 0),    // 4 = yellow
    // ...
};
```

## Troubleshooting

### Textures Don't Load
1. Check file path in `textures.cfg`
2. Verify image exists in `test/gt1/textures/`
3. Check build output directory has textures copied
4. Look for debug output in Visual Studio Output window

### Textures Look Distorted
- Ensure texture dimensions are equal (square)
- Try using power-of-2 sizes (64, 128, 256)
- Check if image is corrupted

### Poor Performance
- Reduce texture sizes
- Use JPG instead of PNG (smaller file size)
- Reduce map complexity

## Example: Creating a Dungeon Level

```
# dungeon.map
1,1,1,1,1,1,1,1,1,1,1,1
1,0,0,0,2,2,2,0,0,0,0,1
1,0,3,0,0,0,0,0,3,0,0,1
1,0,0,0,0,0,0,0,0,0,0,1
1,2,0,0,4,4,4,4,0,0,2,1
1,2,0,0,4,0,0,4,0,0,2,1
1,2,0,0,4,4,4,4,0,0,2,1
1,0,0,0,0,0,0,0,0,0,0,1
1,0,3,0,0,0,0,0,3,0,0,1
1,0,0,0,2,2,2,0,0,0,0,1
1,1,1,1,1,1,1,1,1,1,1,1
```

Where:
- `1` = Stone walls (amberrock.jpg)
- `2` = Wood panels (amberstone.jpg)
- `3` = Pillars (lightstone.jpg)
- `4` = Metal doors (yellowrock.jpg)

---

**Happy texture mapping! 🎨**
