# GORE Engine Splash Screen

The GORE Engine now includes an automatic splash screen that displays when your game starts.

## Features

- **Random Logo Selection**: Five logo variants (Logo.png, LogoAlt.png, LogoAlt2.png, LogoAlt3.png, LogoAlt4.png) are randomly selected each time the app launches
- **Smooth Animations**: Fade in (1.5s) → Display (2s) → Fade out (1.5s)
- **Automatic Integration**: Works automatically when you call `GOREEngine.StartAsync()`

## Usage

The GORE Engine splash screen is always displayed when starting the engine:

```csharp
protected override async void OnLaunched(LaunchActivatedEventArgs args)
{
    m_window = new MainWindow();

    // GORE Engine splash screen shown automatically
    await GORE.Engine.GOREEngine.StartAsync(m_window);
}
```

## Customization

### Replace Logo Images

Replace these files in the GORE Engine `src/Assets` folder with your own branding:
- `Logo.png` - First logo variant
- `LogoAlt.png` - Second logo variant  
- `LogoAlt2.png` - Third logo variant
- `LogoAlt3.png` - Fourth logo variant
- `LogoAlt4.png` - Fifth logo variant

Recommended size: 800x600 pixels (or any aspect ratio that works for your branding)

### Adjust Timing

Edit `src/UI/SplashScreen.xaml.cs` and modify the constants:
- `FadeInDuration` - How long the fade-in takes (milliseconds)
- `DisplayDuration` - How long the logo stays fully visible (milliseconds)
- `FadeOutDuration` - How long the fade-out takes (milliseconds)

## Technical Details

The splash screen is implemented as a WinUI 3 Window that:
1. Displays first when the engine starts
2. Uses opacity transitions for smooth fade effects
3. Automatically closes and activates the main window when complete
4. Assets are embedded in the GORE library and copied to the output directory
