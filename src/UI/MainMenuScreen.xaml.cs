using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using Windows.Storage.Streams;

namespace GORE.UI
{
    public sealed partial class MainMenuScreen : Window
    {
        private readonly Window _mainWindow;
        private int _selectedIndex = 0;
        private readonly TextBlock[] _menuItems;

        public MainMenuScreen(Window mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            ExtendsContentIntoTitleBar = true;

            // Enter fullscreen mode
            ScreenHelper.EnterFullScreenMode(this);

            // Initialize menu items array
            _menuItems = new[] { NewGameText, LoadGameText, SettingsText, ExitText };

            // Load configuration and set up UI
            LoadConfiguration();

            // Set initial cursor position
            UpdateCursorPosition(0);

            // Setup keyboard navigation  
            Content.PreviewKeyDown += MainMenuScreen_KeyDown;
        }

        private async void LoadConfiguration()
        {
            var config = Engine.GOREEngine.GetConfiguration();

            // Load background image
            if (!string.IsNullOrEmpty(config.UI.MainMenuBackground))
            {
                try
                {
                    var baseDirectory = AppContext.BaseDirectory;
                    var backgroundPath = Path.Combine(baseDirectory, config.UI.MainMenuBackground);

                    System.Diagnostics.Debug.WriteLine($"=== BACKGROUND LOADING ===");
                    System.Diagnostics.Debug.WriteLine($"Base Directory: {baseDirectory}");
                    System.Diagnostics.Debug.WriteLine($"Config Path: {config.UI.MainMenuBackground}");
                    System.Diagnostics.Debug.WriteLine($"Full Path: {backgroundPath}");
                    System.Diagnostics.Debug.WriteLine($"File Exists: {File.Exists(backgroundPath)}");

                    if (File.Exists(backgroundPath))
                    {
                        using (var fileStream = File.OpenRead(backgroundPath))
                        {
                            var bitmapImage = new BitmapImage();
                            await bitmapImage.SetSourceAsync(fileStream.AsRandomAccessStream());
                            BackgroundImage.Source = bitmapImage;

                            System.Diagnostics.Debug.WriteLine($"✓ Successfully loaded background image");
                            System.Diagnostics.Debug.WriteLine($"  Image size: {bitmapImage.PixelWidth}x{bitmapImage.PixelHeight}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ Background file not found: {backgroundPath}");
                        // List what IS in the Assets/UI directory
                        var uiDir = Path.Combine(baseDirectory, "Assets", "UI");
                        if (Directory.Exists(uiDir))
                        {
                            System.Diagnostics.Debug.WriteLine($"Files in {uiDir}:");
                            foreach (var file in Directory.GetFiles(uiDir))
                            {
                                System.Diagnostics.Debug.WriteLine($"  - {Path.GetFileName(file)}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Failed to load background: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }

            // Load cursor image
            if (!string.IsNullOrEmpty(config.UI.Cursor))
            {
                try
                {
                    var baseDirectory = AppContext.BaseDirectory;
                    var cursorPath = Path.Combine(baseDirectory, config.UI.Cursor);

                    System.Diagnostics.Debug.WriteLine($"=== CURSOR LOADING ===");
                    System.Diagnostics.Debug.WriteLine($"Cursor Path: {cursorPath}");
                    System.Diagnostics.Debug.WriteLine($"File Exists: {File.Exists(cursorPath)}");

                    if (File.Exists(cursorPath))
                    {
                        using (var fileStream = File.OpenRead(cursorPath))
                        {
                            var bitmapImage = new BitmapImage();
                            await bitmapImage.SetSourceAsync(fileStream.AsRandomAccessStream());
                            CursorImage.Source = bitmapImage;
                            System.Diagnostics.Debug.WriteLine($"✓ Successfully loaded cursor image");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ Cursor file not found: {cursorPath}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Failed to load cursor: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }

            // Apply font settings
            var fontSize = config.UI.FontSize;
            foreach (var menuItem in _menuItems)
            {
                menuItem.FontSize = fontSize;
            }
        }

        private void MainMenuScreen_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.W:
                    _selectedIndex = (_selectedIndex - 1 + _menuItems.Length) % _menuItems.Length;
                    UpdateCursorPosition(_selectedIndex);
                    e.Handled = true;
                    break;

                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.S:
                    _selectedIndex = (_selectedIndex + 1) % _menuItems.Length;
                    UpdateCursorPosition(_selectedIndex);
                    e.Handled = true;
                    break;

                case Windows.System.VirtualKey.Enter:
                case Windows.System.VirtualKey.Space:
                    ExecuteMenuItem(_selectedIndex);
                    e.Handled = true;
                    break;

                case Windows.System.VirtualKey.Escape:
                    Application.Current.Exit();
                    e.Handled = true;
                    break;
            }
        }

        private void MenuItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is string tagStr)
            {
                if (int.TryParse(tagStr, out int index))
                {
                    _selectedIndex = index;
                    UpdateCursorPosition(index);
                }
            }
        }

        private void UpdateCursorPosition(int index)
        {
            if (index < 0 || index >= _menuItems.Length) return;

            var targetItem = _menuItems[index];

            // Animate cursor to new position
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                To = index * (targetItem.ActualHeight + 20), // Height + margin
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(animation, CursorTransform);
            Storyboard.SetTargetProperty(animation, "Y");
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void ExecuteMenuItem(int index)
        {
            switch (index)
            {
                case 0: NewGame_Click(null, null); break;
                case 1: LoadGame_Click(null, null); break;
                case 2: Settings_Click(null, null); break;
                case 3: Exit_Click(null, null); break;
            }
        }

        private void NewGame_Click(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("New Game selected - Starting world map");

            var defaultCharacter = new Models.Character("Terra")
            {
                Level = 1,
                MaxHP = 100,
                CurrentHP = 100,
                MaxMP = 50,
                CurrentMP = 50,
                Attack = 10,
                Defense = 8,
                Magic = 12,
                Speed = 10,
                Experience = 0,
                X = 128,
                Y = 128
            };

            var worldMapScreen = new WorldMapScreen(this, defaultCharacter);
            worldMapScreen.Activate();
        }

        private void LoadGame_Click(object sender, PointerRoutedEventArgs e)
        {
            // TODO: Transition to load game screen
            System.Diagnostics.Debug.WriteLine("Load Game selected");
        }

        private void Settings_Click(object sender, PointerRoutedEventArgs e)
        {
            // TODO: Transition to settings screen
            System.Diagnostics.Debug.WriteLine("Settings selected");
        }

        private void Exit_Click(object sender, PointerRoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
