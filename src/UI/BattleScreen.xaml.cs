using GORE.GameEngine;
using GORE.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace GORE.UI
{
    public sealed partial class BattleScreen : Window
    {
        private readonly Window _mainWindow;
        private readonly BattleSystem _battleSystem;
        private readonly List<Character> _party;
        private readonly List<Enemy> _enemies;
        private readonly string _battleBackground;
        private BattleResult _result;
        private bool _battleEnded = false;
        private bool _selectingTarget = false;

        public BattleScreen(Window mainWindow, List<Character> party, List<Enemy> enemies, string battleBackground = null)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _party = party;
            _enemies = enemies;
            _battleBackground = battleBackground;
            _battleSystem = new BattleSystem();

            ExtendsContentIntoTitleBar = true;
            ScreenHelper.EnterFullScreenMode(this);

            SetupBattle();
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            RootGrid.Focus(FocusState.Programmatic);
        }

        private async void SetupBattle()
        {
            // Set default background
            RootGrid.Background = new SolidColorBrush(Color.FromArgb(255, 26, 0, 51)); // #1a0033

            _battleSystem.StartBattle(_party, _enemies);

            // Load battle background
            if (!string.IsNullOrEmpty(_battleBackground))
            {
                await LoadBattleBackgroundAsync(_battleBackground);
            }

            // Setup enemy display
            foreach (var enemy in _enemies)
            {
                var enemyCard = CreateEnemyCard(enemy);
                EnemyPanel.Children.Add(enemyCard);
            }

            // Setup party display
            foreach (var character in _party)
            {
                var charCard = CreateCharacterCard(character);
                PartyPanel.Children.Add(charCard);
            }

            // Add initial battle text
            await AddBattleText("Battle Start!");
            await Task.Delay(500);
            await AddBattleText($"Encountered {_enemies.Count} enemies!");
            await Task.Delay(1000);

            StartPlayerTurn();
        }

        private async Task LoadBattleBackgroundAsync(string backgroundPath)
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var fullPath = Path.Combine(baseDirectory, backgroundPath);

                System.Diagnostics.Debug.WriteLine($"=== LOADING BATTLE BACKGROUND ===");
                System.Diagnostics.Debug.WriteLine($"Background path: {backgroundPath}");
                System.Diagnostics.Debug.WriteLine($"Full path: {fullPath}");
                System.Diagnostics.Debug.WriteLine($"File exists: {File.Exists(fullPath)}");

                if (File.Exists(fullPath))
                {
                    var bitmap = new BitmapImage(new Uri(fullPath));
                    var imageBrush = new ImageBrush
                    {
                        ImageSource = bitmap,
                        Stretch = Stretch.UniformToFill
                    };
                    RootGrid.Background = imageBrush;
                    System.Diagnostics.Debug.WriteLine("✓ Battle background loaded successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Battle background not found: {fullPath}");

                    // List files in the Backgrounds directory
                    var backgroundsDir = Path.Combine(baseDirectory, "Assets", "Backgrounds");
                    if (Directory.Exists(backgroundsDir))
                    {
                        System.Diagnostics.Debug.WriteLine($"Files in {backgroundsDir}:");
                        foreach (var file in Directory.GetFiles(backgroundsDir))
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {Path.GetFileName(file)}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Backgrounds directory doesn't exist: {backgroundsDir}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error loading battle background: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private Border CreateEnemyCard(Enemy enemy)
        {
            var mainStack = new StackPanel
            {
                Spacing = 8
            };

            // Enemy sprite
            var sprite = new Border
            {
                Width = 120,
                Height = 120,
                Background = new SolidColorBrush(Color.FromArgb(100, 80, 0, 0)),
                BorderBrush = new SolidColorBrush(Colors.DarkRed),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(4),
                Tag = enemy
            };

            // Try to load enemy texture
            if (!string.IsNullOrEmpty(enemy.Texture))
            {
                _ = LoadEnemyImageAsync(sprite, enemy.Texture);
            }
            else
            {
                // Placeholder icon
                var placeholderText = new TextBlock
                {
                    Text = "👾",
                    FontSize = 60,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                sprite.Child = placeholderText;
            }

            mainStack.Children.Add(sprite);

            // Enemy name
            var nameText = new TextBlock
            {
                Text = enemy.Name,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainStack.Children.Add(nameText);

            // HP bar container
            var hpContainer = new StackPanel
            {
                Spacing = 4
            };

            var hpLabel = new TextBlock
            {
                Text = "HP",
                Foreground = new SolidColorBrush(Colors.LightGray),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            hpContainer.Children.Add(hpLabel);

            // HP bar background
            var hpBarBg = new Border
            {
                Width = 100,
                Height = 12,
                Background = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40)),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2)
            };

            // HP bar fill
            var hpBarFill = new Border
            {
                Height = 12,
                HorizontalAlignment = HorizontalAlignment.Left,
                CornerRadius = new CornerRadius(2),
                Tag = "hpbar"
            };

            UpdateEnemyHPBar(hpBarFill, enemy);

            var hpGrid = new Grid();
            hpGrid.Children.Add(hpBarBg);
            hpGrid.Children.Add(hpBarFill);
            hpContainer.Children.Add(hpGrid);

            // HP text
            var hpText = new TextBlock
            {
                Text = $"{enemy.CurrentHP}/{enemy.MaxHP}",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = "hptext"
            };
            hpContainer.Children.Add(hpText);

            mainStack.Children.Add(hpContainer);

            var wrapper = new Border
            {
                Child = mainStack,
                Tag = enemy
            };

            return wrapper;
        }

        private void UpdateEnemyHPBar(Border hpBar, Enemy enemy)
        {
            double hpPercent = (double)enemy.CurrentHP / enemy.MaxHP;
            hpBar.Width = 100 * hpPercent;

            // Color based on HP percentage
            Color barColor;
            if (hpPercent > 0.6)
                barColor = Color.FromArgb(255, 0, 200, 0); // Green
            else if (hpPercent > 0.3)
                barColor = Color.FromArgb(255, 255, 200, 0); // Yellow
            else
                barColor = Color.FromArgb(255, 200, 0, 0); // Red

            hpBar.Background = new SolidColorBrush(barColor);
        }

        private async Task LoadEnemyImageAsync(Border imageBorder, string texturePath)
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var fullPath = Path.Combine(baseDirectory, texturePath);

                if (File.Exists(fullPath))
                {
                    var bitmap = new BitmapImage(new Uri(fullPath));
                    imageBorder.Background = new ImageBrush
                    {
                        ImageSource = bitmap,
                        Stretch = Stretch.UniformToFill
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading enemy image: {ex.Message}");
            }
        }

        private Border CreateCharacterCard(Character character)
        {
            var mainBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 20, 20, 60)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 65, 105, 225)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12, 8, 12, 8),
                Tag = character
            };

            var stack = new StackPanel
            {
                Spacing = 6
            };

            // Name and Level
            var nameStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var nameText = new TextBlock
            {
                Text = character.Name,
                Foreground = new SolidColorBrush(Colors.Cyan),
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                FontFamily = new FontFamily("Consolas")
            };

            var levelText = new TextBlock
            {
                Text = $"Lv{character.Level}",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 14,
                FontFamily = new FontFamily("Consolas"),
                VerticalAlignment = VerticalAlignment.Center
            };

            nameStack.Children.Add(nameText);
            nameStack.Children.Add(levelText);
            stack.Children.Add(nameStack);

            // HP Bar
            var hpStack = new StackPanel
            {
                Spacing = 3
            };

            var hpLabelStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5
            };

            var hpLabel = new TextBlock
            {
                Text = "HP",
                Foreground = new SolidColorBrush(Colors.LightGray),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                Width = 25
            };

            var hpText = new TextBlock
            {
                Text = $"{character.CurrentHP,3}/{character.MaxHP,3}",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                Tag = "hptext"
            };

            hpLabelStack.Children.Add(hpLabel);
            hpLabelStack.Children.Add(hpText);
            hpStack.Children.Add(hpLabelStack);

            // HP Bar
            var hpBarBg = new Border
            {
                Width = 150,
                Height = 14,
                Background = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40)),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2)
            };

            var hpBarFill = new Border
            {
                Height = 14,
                HorizontalAlignment = HorizontalAlignment.Left,
                CornerRadius = new CornerRadius(2),
                Tag = "hpbar"
            };

            UpdateCharacterHPBar(hpBarFill, character);

            var hpGrid = new Grid();
            hpGrid.Children.Add(hpBarBg);
            hpGrid.Children.Add(hpBarFill);
            hpStack.Children.Add(hpGrid);
            stack.Children.Add(hpStack);

            // MP Bar
            var mpStack = new StackPanel
            {
                Spacing = 3
            };

            var mpLabelStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5
            };

            var mpLabel = new TextBlock
            {
                Text = "MP",
                Foreground = new SolidColorBrush(Colors.LightGray),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                Width = 25
            };

            var mpText = new TextBlock
            {
                Text = $"{character.CurrentMP,3}/{character.MaxMP,3}",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                Tag = "mptext"
            };

            mpLabelStack.Children.Add(mpLabel);
            mpLabelStack.Children.Add(mpText);
            mpStack.Children.Add(mpLabelStack);

            // MP Bar
            var mpBarBg = new Border
            {
                Width = 150,
                Height = 14,
                Background = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40)),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2)
            };

            var mpBarFill = new Border
            {
                Height = 14,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(Color.FromArgb(255, 50, 150, 255)),
                CornerRadius = new CornerRadius(2),
                Tag = "mpbar"
            };

            UpdateCharacterMPBar(mpBarFill, character);

            var mpGrid = new Grid();
            mpGrid.Children.Add(mpBarBg);
            mpGrid.Children.Add(mpBarFill);
            mpStack.Children.Add(mpGrid);
            stack.Children.Add(mpStack);

            mainBorder.Child = stack;
            return mainBorder;
        }

        private void UpdateCharacterHPBar(Border hpBar, Character character)
        {
            double hpPercent = (double)character.CurrentHP / character.MaxHP;
            hpBar.Width = 150 * hpPercent;

            // Color based on HP percentage
            Color barColor;
            if (hpPercent > 0.6)
                barColor = Color.FromArgb(255, 0, 200, 0); // Green
            else if (hpPercent > 0.3)
                barColor = Color.FromArgb(255, 255, 200, 0); // Yellow
            else
                barColor = Color.FromArgb(255, 200, 0, 0); // Red

            hpBar.Background = new SolidColorBrush(barColor);
        }

        private void UpdateCharacterMPBar(Border mpBar, Character character)
        {
            double mpPercent = (double)character.CurrentMP / character.MaxMP;
            mpBar.Width = 150 * mpPercent;
        }

        private void StartPlayerTurn()
        {
            if (_battleSystem.IsBattleOver())
            {
                EndBattle();
                return;
            }

            var currentChar = _party.Where(c => c.IsAlive).FirstOrDefault();
            if (currentChar == null)
            {
                EndBattle();
                return;
            }

            _ = AddBattleText($"\n{currentChar.Name}'s turn!");
            
            ActionMenu.Visibility = Visibility.Visible;
            TargetSelection.Visibility = Visibility.Collapsed;
            StatusText.Visibility = Visibility.Collapsed;
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTargetSelection();
        }

        private void ShowTargetSelection()
        {
            ActionMenu.Visibility = Visibility.Collapsed;
            TargetSelection.Visibility = Visibility.Visible;
            BattleLogScroll.Visibility = Visibility.Collapsed;
            _selectingTarget = true;

            // Clear previous target buttons
            TargetButtonPanel.Children.Clear();

            // Add target buttons for alive enemies
            int index = 0;
            foreach (var enemy in _enemies.Where(e => e.IsAlive))
            {
                var button = new Button
                {
                    Content = $"▶ {enemy.Name}",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Height = 45,
                    FontSize = 18,
                    FontFamily = new FontFamily("Consolas"),
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Tag = enemy,
                    Background = new SolidColorBrush(Color.FromArgb(255, 26, 26, 62)),
                    Foreground = new SolidColorBrush(Colors.White),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(255, 65, 105, 225)),
                    BorderThickness = new Thickness(2)
                };
                button.Click += TargetButton_Click;
                TargetButtonPanel.Children.Add(button);
                index++;
            }
        }

        private async void TargetButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var target = button?.Tag as Enemy;
            if (target == null) return;

            _selectingTarget = false;
            TargetSelection.Visibility = Visibility.Collapsed;
            BattleLogScroll.Visibility = Visibility.Visible;

            var attacker = _party.Where(c => c.IsAlive).FirstOrDefault();
            if (attacker != null)
            {
                string message = _battleSystem.ExecuteAttack(attacker, target);
                await AddBattleText(message);
                await Task.Delay(800);

                UpdateDisplay();

                if (_battleSystem.IsBattleOver())
                {
                    EndBattle();
                    return;
                }

                // Enemy turn
                await ExecuteEnemyPhase();
            }
        }

        private async Task ExecuteEnemyPhase()
        {
            await AddBattleText("\n--- Enemy Turn ---");
            await Task.Delay(500);

            var messages = _battleSystem.ExecuteEnemyTurn();
            foreach (var msg in messages)
            {
                await AddBattleText(msg);
                await Task.Delay(800);
            }

            UpdateDisplay();

            if (_battleSystem.IsBattleOver())
            {
                EndBattle();
                return;
            }

            await Task.Delay(500);
            StartPlayerTurn();
        }

        private void UpdateDisplay()
        {
            // Update enemy cards
            foreach (var card in EnemyPanel.Children.OfType<Border>())
            {
                var enemy = card.Tag as Enemy;
                if (enemy != null)
                {
                    var mainStack = card.Child as StackPanel;
                    if (mainStack != null)
                    {
                        // Update HP bar
                        var hpContainer = mainStack.Children.OfType<StackPanel>().LastOrDefault();
                        if (hpContainer != null)
                        {
                            var hpGrid = hpContainer.Children.OfType<Grid>().FirstOrDefault();
                            if (hpGrid != null)
                            {
                                var hpBar = hpGrid.Children.OfType<Border>().FirstOrDefault(b => b.Tag?.ToString() == "hpbar");
                                if (hpBar != null)
                                {
                                    UpdateEnemyHPBar(hpBar, enemy);
                                }
                            }

                            // Update HP text
                            var hpText = hpContainer.Children.OfType<TextBlock>().FirstOrDefault(t => t.Tag?.ToString() == "hptext");
                            if (hpText != null)
                            {
                                hpText.Text = $"{enemy.CurrentHP}/{enemy.MaxHP}";
                            }
                        }

                        // Fade out defeated enemies
                        if (!enemy.IsAlive)
                        {
                            card.Opacity = 0.3;

                            // Add defeated overlay
                            var sprite = mainStack.Children.OfType<Border>().FirstOrDefault();
                            if (sprite != null && sprite.Child == null)
                            {
                                var defeatedText = new TextBlock
                                {
                                    Text = "✕",
                                    FontSize = 80,
                                    Foreground = new SolidColorBrush(Colors.Red),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center
                                };
                                var grid = new Grid();
                                grid.Children.Add(defeatedText);
                                sprite.Child = grid;
                            }
                        }
                    }
                }
            }

            // Update party cards
            foreach (var card in PartyPanel.Children.OfType<Border>())
            {
                var character = card.Tag as Character;
                if (character != null)
                {
                    var stack = card.Child as StackPanel;
                    if (stack != null)
                    {
                        // Update HP bar
                        var hpStack = stack.Children.OfType<StackPanel>().Skip(1).FirstOrDefault();
                        if (hpStack != null)
                        {
                            var hpGrid = hpStack.Children.OfType<Grid>().FirstOrDefault();
                            if (hpGrid != null)
                            {
                                var hpBar = hpGrid.Children.OfType<Border>().FirstOrDefault(b => b.Tag?.ToString() == "hpbar");
                                if (hpBar != null)
                                {
                                    UpdateCharacterHPBar(hpBar, character);
                                }
                            }

                            // Update HP text
                            var hpLabelStack = hpStack.Children.OfType<StackPanel>().FirstOrDefault();
                            if (hpLabelStack != null)
                            {
                                var hpText = hpLabelStack.Children.OfType<TextBlock>().FirstOrDefault(t => t.Tag?.ToString() == "hptext");
                                if (hpText != null)
                                {
                                    hpText.Text = $"{character.CurrentHP,3}/{character.MaxHP,3}";
                                }
                            }
                        }

                        // Update MP bar
                        var mpStack = stack.Children.OfType<StackPanel>().LastOrDefault();
                        if (mpStack != null)
                        {
                            var mpGrid = mpStack.Children.OfType<Grid>().FirstOrDefault();
                            if (mpGrid != null)
                            {
                                var mpBar = mpGrid.Children.OfType<Border>().FirstOrDefault(b => b.Tag?.ToString() == "mpbar");
                                if (mpBar != null)
                                {
                                    UpdateCharacterMPBar(mpBar, character);
                                }
                            }

                            // Update MP text
                            var mpLabelStack = mpStack.Children.OfType<StackPanel>().FirstOrDefault();
                            if (mpLabelStack != null)
                            {
                                var mpText = mpLabelStack.Children.OfType<TextBlock>().FirstOrDefault(t => t.Tag?.ToString() == "mptext");
                                if (mpText != null)
                                {
                                    mpText.Text = $"{character.CurrentMP,3}/{character.MaxMP,3}";
                                }
                            }
                        }
                    }

                    // Fade out fallen characters
                    if (!character.IsAlive)
                    {
                        card.Opacity = 0.4;
                        card.Background = new SolidColorBrush(Color.FromArgb(220, 60, 20, 20));
                    }
                }
            }
        }

        private async Task AddBattleText(string text)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (!string.IsNullOrEmpty(BattleLogText.Text))
                {
                    BattleLogText.Text += "\n";
                }
                BattleLogText.Text += text;

                BattleLogScroll.ChangeView(null, BattleLogScroll.ScrollableHeight, null);
            });

            await Task.Delay(50); // Small delay to ensure UI updates
        }

        private async void EndBattle()
        {
            _battleEnded = true;
            _result = _battleSystem.GetBattleResult();

            ActionMenu.Visibility = Visibility.Collapsed;
            TargetSelection.Visibility = Visibility.Collapsed;

            await Task.Delay(1000);

            if (_result.Victory)
            {
                await AddBattleText("\n=== VICTORY! ===");
                await Task.Delay(500);
                await AddBattleText($"Gained {_result.TotalExp} EXP!");
                await AddBattleText($"Gained {_result.TotalGold} Gold!");

                foreach (var levelUp in _result.LevelUps)
                {
                    await Task.Delay(300);
                    await AddBattleText($"★ {levelUp}");
                }
            }
            else
            {
                await AddBattleText("\n=== DEFEAT ===");
                await AddBattleText("Game Over...");
            }

            await Task.Delay(1000);
            StatusText.Text = "Press ENTER to continue...";
            StatusText.Visibility = Visibility.Visible;
        }

        private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && _battleEnded)
            {
                if (_result.Victory)
                {
                    // Open battle result screen
                    var resultScreen = new BattleResultScreen(_mainWindow, _result, _party);
                    resultScreen.Activate();
                    Close();
                }
                else
                {
                    // Game over - return to main menu
                    _mainWindow.Activate();
                    Close();
                }
            }
            else if (e.Key == Windows.System.VirtualKey.Escape && _selectingTarget)
            {
                _selectingTarget = false;
                TargetSelection.Visibility = Visibility.Collapsed;
                BattleLogScroll.Visibility = Visibility.Visible;
                ActionMenu.Visibility = Visibility.Visible;
            }
        }
    }
}
