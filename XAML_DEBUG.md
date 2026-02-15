# XAML Parsing Error Debug

## Error Details
```
Microsoft.UI.Xaml.Markup.XamlParseException: XAML parsing failed.
at GORE.UI.GameWindow.InitializeComponent()
```

## Possible Causes & Fixes

### 1. Color Format Issue
Some WinUI versions don't support hex color strings like `#FF0000` directly in XAML.

**Try this simpler version with named colors:**

```xml
<Window
    x:Class="GORE.UI.GameWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d">

    <Grid x:Name="RootGrid" Background="Black" KeyDown="RootGrid_KeyDown" KeyUp="RootGrid_KeyUp" Loaded="RootGrid_Loaded">
        <!-- 3D Viewport -->
        <Image x:Name="ViewportImage" Stretch="Fill"/>

        <!-- Health (Bottom Left) -->
        <StackPanel Orientation="Horizontal" 
                    HorizontalAlignment="Left" 
                    VerticalAlignment="Bottom"
                    Margin="30,0,0,30">
            <Border Background="DarkRed" 
                    BorderBrush="Red" 
                    BorderThickness="3" 
                    Padding="15,5"
                    CornerRadius="3">
                <TextBlock x:Name="HealthText" 
                          Text="100"
                          Foreground="Lime"
                          FontFamily="Consolas"
                          FontSize="48"
                          FontWeight="Black"/>
            </Border>
            <TextBlock Text="HEALTH"
                      Foreground="Gray"
                      FontFamily="Consolas"
                      FontSize="16"
                      FontWeight="Bold"
                      VerticalAlignment="Center"
                      Margin="10,0,0,0"/>
        </StackPanel>

        <!-- Ammo (Bottom Right) -->
        <StackPanel Orientation="Horizontal" 
                    HorizontalAlignment="Right" 
                    VerticalAlignment="Bottom"
                    Margin="0,0,30,30">
            <TextBlock Text="AMMO"
                      Foreground="Gray"
                      FontFamily="Consolas"
                      FontSize="16"
                      FontWeight="Bold"
                      VerticalAlignment="Center"
                      Margin="0,0,10,0"/>
            <Border Background="DarkGoldenrod" 
                    BorderBrush="Orange" 
                    BorderThickness="3" 
                    Padding="15,5"
                    CornerRadius="3">
                <TextBlock x:Name="AmmoText" 
                          Text="50"
                          Foreground="Yellow"
                          FontFamily="Consolas"
                          FontSize="48"
                          FontWeight="Black"/>
            </Border>
        </StackPanel>
    </Grid>
</Window>
```

### 2. Try Even Simpler Version

If the above still fails, try this minimal version:

```xml
<Window
    x:Class="GORE.UI.GameWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Grid x:Name="RootGrid" Background="Black" KeyDown="RootGrid_KeyDown" KeyUp="RootGrid_KeyUp" Loaded="RootGrid_Loaded">
        <Image x:Name="ViewportImage" Stretch="Fill"/>
        
        <TextBlock x:Name="HealthText" 
                   Text="100"
                   Foreground="Lime"
                   FontSize="48"
                   HorizontalAlignment="Left"
                   VerticalAlignment="Bottom"
                   Margin="30"/>
        
        <TextBlock x:Name="AmmoText" 
                   Text="50"
                   Foreground="Yellow"
                   FontSize="48"
                   HorizontalAlignment="Right"
                   VerticalAlignment="Bottom"
                   Margin="30"/>
    </Grid>
</Window>
```

### 3. Clean Build
```powershell
Remove-Item -Path "src\obj","src\bin","test\obj","test\bin" -Recurse -Force
dotnet clean
dotnet build
```

### 4. Check Runtime
Make sure you're running the test project, not the source project:
```powershell
dotnet run --project test/GORETest.csproj
```

## What to Try First

1. Replace `GameWindow.xaml` with the "simpler version with named colors" above
2. Clean and rebuild
3. Run the game
4. If it works, you can gradually add back the styling

The issue is likely the hex color format `#RRGGBB` which might not be parsing correctly at runtime in your WinUI version.
