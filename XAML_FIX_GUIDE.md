# 🔧 GORE Engine File Structure Fix

## ❌ **Current Problem:**

The XAML files in `GORE-Engine/src/Pages/` are **Mystic Chronicles-specific**, not generic GORE pages!

### **Wrong Structure:**
```
GORE-Engine/src/Pages/
├── MainMenuPage.xaml          ❌ Mystic Chronicles specific
├── MainMenuPage.xaml.cs       ✅ GORE generic (correct)
├── CharacterCreationPage.xaml ❌ Mystic Chronicles specific  
└── CharacterCreationPage.xaml.cs ✅ GORE generic (correct)
```

**The `.xaml` files have:**
- `x:Class="MysticChronicles.MainMenuPage"` ← Wrong! Should be `GORE.Pages.MainMenuPage`
- Mystic Chronicles specific UI layout
- Missing GORE generic controls (txtGameTitle, txtVersion, etc.)

**The `.xaml.cs` files have:**
- `namespace GORE.Pages` ← Correct!
- References to GORE generic controls that don't exist in the XAML

---

## ✅ **Solution:**

### **Option 1: Copy the Correct GORE XAML Files**

I created the correct GORE generic XAML files earlier. You need to:

1. **Close the XAML files in Visual Studio:**
   - `GORE-Engine/src/Pages/MainMenuPage.xaml`
   - `GORE-Engine/src/Pages/CharacterCreationPage.xaml`

2. **Replace them with the correct content:**

**MainMenuPage.xaml** (GORE generic):
```xaml
<gore:BaseMainMenuPage
    x:Class="GORE.Pages.MainMenuPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:canvas="using:Microsoft.Graphics.Canvas.UI.Xaml"
    xmlns:gore="using:GORE.Pages">

    <Grid Background="#FF0000AA">
        <canvas:CanvasControl x:Name="animationCanvas" Draw="AnimationCanvas_Draw"/>

        <Grid>
            <TextBlock x:Name="txtGameTitle"
                       Text="GORE Engine Game"
                       Foreground="White"
                       FontSize="72"
                       FontWeight="Bold"
                       FontFamily="Segoe UI"
                       HorizontalAlignment="Center"
                       VerticalAlignment="Top"
                       Margin="0,100,0,0"/>

            <Border BorderBrush="White"
                    BorderThickness="4"
                    Background="#FF0000AA"
                    Width="400"
                    HorizontalAlignment="Center"
                    VerticalAlignment="Center"
                    Padding="40">
                <StackPanel>
                    <Grid Margin="0,20">
                        <Image x:Name="cursorNewGame"
                               Source="/Assets/Cursor.png"
                               Width="32"
                               Height="32"
                               HorizontalAlignment="Left"
                               Visibility="Visible"/>
                        <TextBlock Text="New Game"
                                   FontSize="36"
                                   FontFamily="Segoe UI"
                                   Foreground="White"
                                   Margin="60,0,0,0"
                                   HorizontalAlignment="Left"/>
                    </Grid>

                    <Grid Margin="0,20">
                        <Image x:Name="cursorLoadGame"
                               Source="/Assets/Cursor.png"
                               Width="32"
                               Height="32"
                               HorizontalAlignment="Left"
                               Visibility="Collapsed"/>
                        <TextBlock Text="Load Game"
                                   FontSize="36"
                                   FontFamily="Segoe UI"
                                   Foreground="White"
                                   Margin="60,0,0,0"
                                   HorizontalAlignment="Left"/>
                    </Grid>

                    <Grid Margin="0,20">
                        <Image x:Name="cursorExit"
                               Source="/Assets/Cursor.png"
                               Width="32"
                               Height="32"
                               HorizontalAlignment="Left"
                               Visibility="Collapsed"/>
                        <TextBlock Text="Exit"
                                   FontSize="36"
                                   FontFamily="Segoe UI"
                                   Foreground="White"
                                   Margin="60,0,0,0"
                                   HorizontalAlignment="Left"/>
                    </Grid>
                </StackPanel>
            </Border>

            <StackPanel VerticalAlignment="Bottom"
                        HorizontalAlignment="Right"
                        Margin="0,0,20,20">
                <TextBlock x:Name="txtVersion"
                           Text="v1.0.0"
                           Foreground="#88FFFFFF"
                           FontSize="14"
                           FontFamily="Segoe UI"
                           HorizontalAlignment="Right"/>
                <TextBlock x:Name="txtDeveloper"
                           Text="Powered by GORE Engine"
                           Foreground="#88FFFFFF"
                           FontSize="12"
                           FontFamily="Segoe UI"
                           HorizontalAlignment="Right"/>
            </StackPanel>
        </Grid>
    </Grid>
</gore:BaseMainMenuPage>
```

**CharacterCreationPage.xaml** (GORE generic):
```xaml
<gore:BaseCharacterCreationPage
    x:Class="GORE.Pages.CharacterCreationPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:gore="using:GORE.Pages">

    <Grid Background="#FF0000AA">
        <Grid>
            <TextBlock Text="Create Your Hero"
                       Foreground="White"
                       FontSize="48"
                       FontWeight="Bold"
                       FontFamily="Segoe UI"
                       HorizontalAlignment="Center"
                       VerticalAlignment="Top"
                       Margin="0,80,0,0"/>

            <Border BorderBrush="White"
                    BorderThickness="4"
                    Background="#FF0000AA"
                    Width="600"
                    HorizontalAlignment="Center"
                    VerticalAlignment="Center"
                    Padding="50">
                <StackPanel>
                    <TextBlock Text="Enter your hero's name:"
                               Foreground="White"
                               FontSize="24"
                               FontFamily="Segoe UI"
                               Margin="0,0,0,20"/>

                    <Border BorderBrush="White"
                            BorderThickness="2"
                            Background="#FF000066"
                            Padding="20"
                            Margin="0,0,0,40">
                        <TextBlock x:Name="txtHeroName"
                                   Text="Hero"
                                   Foreground="Yellow"
                                   FontSize="32"
                                   FontWeight="Bold"
                                   FontFamily="Segoe UI"
                                   HorizontalAlignment="Center"/>
                    </Border>

                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>

                        <StackPanel Grid.Column="0"
                                   HorizontalAlignment="Center"
                                   Margin="0,20,0,0">
                            <Image x:Name="cursorConfirm"
                                   Source="/Assets/Cursor.png"
                                   Width="32"
                                   Height="32"
                                   Visibility="Visible"
                                   Margin="0,0,0,10"/>
                            <Border BorderBrush="White"
                                    BorderThickness="2"
                                    Background="#FF000066"
                                    Padding="30,15">
                                <TextBlock Text="Confirm"
                                           Foreground="White"
                                           FontSize="24"
                                           FontFamily="Segoe UI"/>
                            </Border>
                        </StackPanel>

                        <StackPanel Grid.Column="1"
                                   HorizontalAlignment="Center"
                                   Margin="0,20,0,0">
                            <Image x:Name="cursorCancel"
                                   Source="/Assets/Cursor.png"
                                   Width="32"
                                   Height="32"
                                   Visibility="Collapsed"
                                   Margin="0,0,0,10"/>
                            <Border BorderBrush="White"
                                    BorderThickness="2"
                                    Background="#FF000066"
                                    Padding="30,15">
                                <TextBlock Text="Cancel"
                                           Foreground="White"
                                           FontSize="24"
                                           FontFamily="Segoe UI"/>
                            </Border>
                        </StackPanel>
                    </Grid>
                </StackPanel>
            </Border>
        </Grid>
    </Grid>
</gore:BaseCharacterCreationPage>
```

---

### **Option 2: Move Mystic Chronicles XAML to Correct Location**

The current XAML files should be in Mystic Chronicles, not GORE Engine!

1. Copy `GORE-Engine/src/Pages/MainMenuPage.xaml` → `Mystic-Chronicles/src/MainMenuPage.xaml`
2. Copy `GORE-Engine/src/Pages/CharacterCreationPage.xaml` → `Mystic-Chronicles/src/CharacterCreationPage.xaml`
3. Replace GORE Engine XAML files with the generic versions above

---

## 🎯 **Quick Fix (Easiest):**

**In Visual Studio:**

1. Open `GORE-Engine/src/Pages/MainMenuPage.xaml`
2. **Select All** (Ctrl+A)
3. **Paste** the MainMenuPage.xaml content from above
4. **Save**

5. Open `GORE-Engine/src/Pages/CharacterCreationPage.xaml`
6. **Select All** (Ctrl+A)
7. **Paste** the CharacterCreationPage.xaml content from above
8. **Save**

9. **Build → Rebuild Solution**

---

## ✅ **After Fix, You Should Have:**

```
GORE-Engine/src/Pages/
├── BasePage.cs                      ✅ Base page class
├── BaseMainMenuPage.cs              ✅ Base menu logic
├── MainMenuPage.xaml                ✅ GORE generic UI
├── MainMenuPage.xaml.cs             ✅ GORE generic code-behind
├── BaseCharacterCreationPage.cs     ✅ Base char creation logic
├── CharacterCreationPage.xaml       ✅ GORE generic UI
└── CharacterCreationPage.xaml.cs    ✅ GORE generic code-behind
```

All with `namespace GORE.Pages` and `x:Class="GORE.Pages.*"` ✅

---

**Follow the Quick Fix steps above and the errors should be gone!** 🚀
