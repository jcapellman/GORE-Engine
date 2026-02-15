# MANUAL FIX REQUIRED - GameWindow.xaml

## Issue
The `src/UI/GameWindow.xaml` file is empty or corrupted and needs to be recreated.

## Solution
Copy the following content into `src/UI/GameWindow.xaml`:

```xml
<Window
    x:Class="GORE.UI.GameWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d">

    <Grid x:Name="RootGrid" Background="#000000" KeyDown="RootGrid_KeyDown" KeyUp="RootGrid_KeyUp" Loaded="RootGrid_Loaded">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- 3D Viewport -->
        <Border Grid.Row="0" BorderBrush="DarkGray" BorderThickness="2">
            <Image x:Name="ViewportImage" Stretch="Fill"/>
        </Border>

        <!-- HUD - Quake 3 Style -->
        <Grid Grid.Row="1" Background="#000000" Padding="30,10">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- Health -->
            <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="10">
                <Border Background="#330000" 
                        BorderBrush="#FF0000" 
                        BorderThickness="3" 
                        Padding="15,5"
                        CornerRadius="3">
                    <TextBlock x:Name="HealthText" 
                              Text="100"
                              Foreground="#00FF00"
                              FontFamily="Consolas"
                              FontSize="48"
                              FontWeight="Black"
                              VerticalAlignment="Center"/>
                </Border>
                <TextBlock Text="HEALTH"
                          Foreground="#888888"
                          FontFamily="Consolas"
                          FontSize="16"
                          FontWeight="Bold"
                          VerticalAlignment="Center"/>
            </StackPanel>

            <!-- Ammo -->
            <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="10">
                <TextBlock Text="AMMO"
                          Foreground="#888888"
                          FontFamily="Consolas"
                          FontSize="16"
                          FontWeight="Bold"
                          VerticalAlignment="Center"/>
                <Border Background="#331100" 
                        BorderBrush="#FFAA00" 
                        BorderThickness="3" 
                        Padding="15,5"
                        CornerRadius="3">
                    <TextBlock x:Name="AmmoText" 
                              Text="50"
                              Foreground="#FFFF00"
                              FontFamily="Consolas"
                              FontSize="48"
                              FontWeight="Black"
                              VerticalAlignment="Center"/>
                </Border>
            </StackPanel>
        </Grid>
    </Grid>
</Window>
```

## What Changed

### HUD Style - Quake 3 Arena Inspired
- ✅ **Removed** control instructions text
- ✅ **Removed** position debug display (X/Y coordinates)
- ✅ **Large numbers** - Health and ammo in 48pt font
- ✅ **Bordered boxes** - Red border for health, orange for ammo
- ✅ **Color coded** - Green text for health, yellow for ammo
- ✅ **Labels** - "HEALTH" and "AMMO" in gray, 16pt
- ✅ **Clean layout** - Health on left, ammo on right, nothing in center

### Code-Behind Updates
The `GameWindow.xaml.cs` file has been updated to:
- ✅ Load custom font from `gt1/font.ttf`
- ✅ Apply font to HUD elements
- ✅ Update HUD to show just numbers (no "HEALTH:" prefix)
- ✅ Simplified HUD update method

### Project Configuration
The `test/GORETest.csproj` now includes:
```xml
<Content Include="gt1\font.ttf">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</Content>
```

## After Fixing

1. **Save** the XAML content above to `src/UI/GameWindow.xaml`
2. **Build** the project: `dotnet build`
3. **Run** the game: `dotnet run --project test/GORETest.csproj`

The HUD will display large numbers for health/ammo in a Quake 3-style interface!
