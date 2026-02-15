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

        // Keyboard navigation
        private int _selectedMenuIndex = 0;
        private int _selectedTargetIndex = 0;
        private List<Button> _currentMenuButtons = new();

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

            // Setup party display (battlefield sprites)
            foreach (var character in _party)
            {
                var charCard = CreateCharacterCard(character);
                PartyPanel.Children.Add(charCard);
            }

            // Setup party status list (bottom right box - FF6 style)
            SetupPartyStatusList();

            // Setup enemy status list (bottom left box)
            SetupEnemyStatusList();

            // No battle text - removed event log
            await Task.Delay(1000);

            StartPlayerTurn();
        }

        private void SetupPartyStatusList()
        {
            PartyStatusList.Children.Clear();

            foreach (var character in _party)
            {
                var statusRow = new Grid
                {
                    Margin = new Thickness(0, 4, 0, 4),
                    Tag = character
                };

                statusRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                statusRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                statusRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                // Character name
                var nameText = new TextBlock
                {
                    Text = character.Name,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 18,
                    FontFamily = new FontFamily("Consolas"),
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(nameText, 0);
                statusRow.Children.Add(nameText);

                // HP display (Current/Max)
                var hpText = new TextBlock
                {
                    Text = $"{character.CurrentHP,3}/{character.MaxHP,3}",
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 16,
                    FontFamily = new FontFamily("Consolas"),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = "hp"
                };
                Grid.SetColumn(hpText, 1);
                statusRow.Children.Add(hpText);

                // MP display
                var mpText = new TextBlock
                {
                    Text = $"{character.CurrentMP,3}",
                    Foreground = new SolidColorBrush(Colors.Cyan),
                    FontSize = 16,
                    FontFamily = new FontFamily("Consolas"),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = "mp"
                };
                Grid.SetColumn(mpText, 2);
                statusRow.Children.Add(mpText);

                PartyStatusList.Children.Add(statusRow);
            }
        }

        private void SetupEnemyStatusList()
        {
            EnemyStatusList.Children.Clear();

            foreach (var enemy in _enemies)
            {
                var enemyRow = new TextBlock
                {
                    Text = enemy.Name,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 18,
                    FontFamily = new FontFamily("Consolas"),
                    Margin = new Thickness(0, 2, 0, 2),
                    Tag = enemy
                };

                EnemyStatusList.Children.Add(enemyRow);
            }
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
                Spacing = 4,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Enemy sprite (no HP bar to avoid clipping)
            var sprite = new Border
            {
                Width = 120,
                Height = 120,
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), // Transparent
                Tag = "sprite"
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

            // Container for damage text overlays
            var damageTextContainer = new Canvas
            {
                Width = 120,
                Height = 40,
                Tag = "damageContainer"
            };
            mainStack.Children.Add(damageTextContainer);

            var wrapper = new Border
            {
                Child = mainStack,
                Tag = enemy
            };

            return wrapper;
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

            // Update active character display
            ActiveCharacterName.Text = currentChar.Name;

            // Switch from enemy list to action menu
            EnemyList.Visibility = Visibility.Collapsed;
            ActionMenu.Visibility = Visibility.Visible;
            TargetSelection.Visibility = Visibility.Collapsed;
            StatusText.Visibility = Visibility.Collapsed;

            // Setup keyboard navigation for action menu
            _selectedMenuIndex = 0;
            _currentMenuButtons = new List<Button> { AttackButton };
            UpdateMenuHighlight();
        }

        private void UpdateMenuHighlight()
        {
            for (int i = 0; i < _currentMenuButtons.Count; i++)
            {
                if (i == _selectedMenuIndex)
                {
                    // Highlight selected
                    _currentMenuButtons[i].Background = new SolidColorBrush(Color.FromArgb(255, 65, 105, 225));
                    _currentMenuButtons[i].BorderBrush = new SolidColorBrush(Colors.Yellow);
                    _currentMenuButtons[i].BorderThickness = new Thickness(3);
                }
                else
                {
                    // Normal state
                    _currentMenuButtons[i].Background = new SolidColorBrush(Color.FromArgb(255, 26, 26, 62));
                    _currentMenuButtons[i].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 65, 105, 225));
                    _currentMenuButtons[i].BorderThickness = new Thickness(2);
                }
            }
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTargetSelection();
        }

        private void ShowTargetSelection()
        {
            ActionMenu.Visibility = Visibility.Collapsed;
            TargetSelection.Visibility = Visibility.Visible;
            PartyStatusList.Visibility = Visibility.Collapsed; // Hide party status during targeting
            _selectingTarget = true;

            // Clear previous target buttons
            TargetButtonPanel.Children.Clear();
            _currentMenuButtons.Clear();

            // Add target buttons for alive enemies
            int index = 0;
            foreach (var enemy in _enemies.Where(e => e.IsAlive))
            {
                var button = new Button
                {
                    Content = $"{index + 1}. {enemy.Name}",
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
                _currentMenuButtons.Add(button);
                index++;
            }

            // Setup keyboard navigation for target selection
            _selectedTargetIndex = 0;
            if (_currentMenuButtons.Count > 0)
            {
                UpdateTargetHighlight();
            }
        }

        private void UpdateTargetHighlight()
        {
            for (int i = 0; i < _currentMenuButtons.Count; i++)
            {
                if (i == _selectedTargetIndex)
                {
                    // Highlight selected with arrow
                    var enemy = _currentMenuButtons[i].Tag as Enemy;
                    _currentMenuButtons[i].Content = $"▶ {i + 1}. {enemy?.Name}";
                    _currentMenuButtons[i].Background = new SolidColorBrush(Color.FromArgb(255, 65, 105, 225));
                    _currentMenuButtons[i].BorderBrush = new SolidColorBrush(Colors.Yellow);
                    _currentMenuButtons[i].BorderThickness = new Thickness(3);
                    _currentMenuButtons[i].Foreground = new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    // Normal state
                    var enemy = _currentMenuButtons[i].Tag as Enemy;
                    _currentMenuButtons[i].Content = $"{i + 1}. {enemy?.Name}";
                    _currentMenuButtons[i].Background = new SolidColorBrush(Color.FromArgb(255, 26, 26, 62));
                    _currentMenuButtons[i].BorderBrush = new SolidColorBrush(Color.FromArgb(255, 65, 105, 225));
                    _currentMenuButtons[i].BorderThickness = new Thickness(2);
                    _currentMenuButtons[i].Foreground = new SolidColorBrush(Colors.White);
                }
            }
        }

        private async void TargetButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var target = button?.Tag as Enemy;
            if (target == null) return;

            _selectingTarget = false;
            TargetSelection.Visibility = Visibility.Collapsed;
            PartyStatusList.Visibility = Visibility.Visible; // Restore party status

            var attacker = _party.Where(c => c.IsAlive).FirstOrDefault();
            if (attacker != null)
            {
                // Calculate damage before showing
                int damage = attacker.CalculateDamage(target.Defense);
                var random = new Random();
                int variance = random.Next(-2, 3);
                damage = Math.Max(1, damage + variance);

                // Show damage text on enemy
                await ShowDamageText(target, damage);

                // Execute attack (no battle log)
                _battleSystem.ExecuteAttack(attacker, target);
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

        private async Task ShowDamageText(Enemy target, int damage)
        {
            // Find the enemy card
            foreach (var card in EnemyPanel.Children.OfType<Border>())
            {
                if (card.Tag == target)
                {
                    var mainStack = card.Child as StackPanel;
                    if (mainStack != null)
                    {
                        var damageContainer = mainStack.Children.OfType<Canvas>().FirstOrDefault(c => c.Tag?.ToString() == "damageContainer");
                        if (damageContainer != null)
                        {
                            // Create damage text (FF6 style: "X HITS" or just damage number)
                            var damageText = new TextBlock
                            {
                                Text = $"{damage}",
                                FontSize = 28,
                                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                                FontFamily = new FontFamily("Consolas"),
                                Foreground = new SolidColorBrush(Colors.White),
                                // Add shadow effect
                                //Stroke = new SolidColorBrush(Colors.Black),
                                //StrokeThickness = 2
                            };

                            Canvas.SetLeft(damageText, 40);
                            Canvas.SetTop(damageText, 0);

                            damageContainer.Children.Add(damageText);

                            // Animate: float up and fade out
                            await AnimateDamageText(damageText, damageContainer);
                        }
                    }
                    break;
                }
            }
        }

        private async Task AnimateDamageText(TextBlock damageText, Canvas container)
        {
            // Float up and fade out over 1 second
            for (int i = 0; i < 20; i++)
            {
                var currentTop = Canvas.GetTop(damageText);
                Canvas.SetTop(damageText, currentTop - 2); // Move up

                damageText.Opacity = 1.0 - (i / 20.0); // Fade out

                await Task.Delay(50);
            }

            // Remove the text
            container.Children.Remove(damageText);
        }

        private async Task ExecuteEnemyPhase()
        {
            // Show enemy list during their turn
            ActionMenu.Visibility = Visibility.Collapsed;
            EnemyList.Visibility = Visibility.Visible;

            await Task.Delay(500);

            var messages = _battleSystem.ExecuteEnemyTurn();

            // Show damage to party members
            foreach (var msg in messages)
            {
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

        private async void UpdateDisplay()
        {
            // Update enemy cards (no HP bar updates - removed to prevent clipping)
            foreach (var card in EnemyPanel.Children.OfType<Border>())
            {
                var enemy = card.Tag as Enemy;
                if (enemy != null)
                {
                    // Fade out defeated enemies over 2 seconds
                    if (!enemy.IsAlive && card.Opacity > 0.3)
                    {
                        _ = FadeOutEnemyAsync(card);
                    }
                }
            }

            // Update enemy status list (left box)
            foreach (var enemyText in EnemyStatusList.Children.OfType<TextBlock>())
            {
                var enemy = enemyText.Tag as Enemy;
                if (enemy != null && !enemy.IsAlive)
                {
                    enemyText.Foreground = new SolidColorBrush(Colors.Gray);
                    enemyText.Opacity = 0.5;
                }
            }

            // Update party cards (battlefield)
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

            // Update party status list (bottom right box)
            foreach (var statusRow in PartyStatusList.Children.OfType<Grid>())
            {
                var character = statusRow.Tag as Character;
                if (character != null)
                {
                    var hpText = statusRow.Children.OfType<TextBlock>().FirstOrDefault(t => t.Tag?.ToString() == "hp");
                    if (hpText != null)
                    {
                        hpText.Text = $"{character.CurrentHP,3}/{character.MaxHP,3}";
                    }

                    var mpText = statusRow.Children.OfType<TextBlock>().FirstOrDefault(t => t.Tag?.ToString() == "mp");
                    if (mpText != null)
                    {
                        mpText.Text = $"{character.CurrentMP,3}";
                    }
                }
            }
        }

        private async Task FadeOutEnemyAsync(Border enemyCard)
        {
            // Smooth 2-second fade animation
            double startOpacity = enemyCard.Opacity;
            int steps = 40; // 40 steps * 50ms = 2 seconds

            for (int i = 0; i < steps; i++)
            {
                double progress = (double)i / steps;
                enemyCard.Opacity = startOpacity * (1.0 - progress);
                await Task.Delay(50);
            }

            enemyCard.Opacity = 0;

            // Optionally add defeated marker
            var mainStack = enemyCard.Child as StackPanel;
            if (mainStack != null)
            {
                var sprite = mainStack.Children.OfType<Border>().FirstOrDefault(b => b.Tag?.ToString() == "sprite");
                if (sprite != null)
                {
                    sprite.Child = new TextBlock
                    {
                        Text = "✕",
                        FontSize = 80,
                        Foreground = new SolidColorBrush(Colors.Red),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Opacity = 0.7
                    };
                }
            }
        }

        private async void EndBattle()
        {
            _battleEnded = true;
            _result = _battleSystem.GetBattleResult();

            ActionMenu.Visibility = Visibility.Collapsed;
            TargetSelection.Visibility = Visibility.Collapsed;
            EnemyList.Visibility = Visibility.Collapsed;

            await Task.Delay(1000);

            if (_result.Victory)
            {
                // Show victory status
                StatusText.Text = $"VICTORY!\n{_result.TotalExp} EXP | {_result.TotalGold} Gold\n\nPress ENTER";
            }
            else
            {
                StatusText.Text = "DEFEAT...\n\nPress ENTER";
            }

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
            else if (_selectingTarget)
            {
                // Target selection navigation
                if (e.Key == Windows.System.VirtualKey.Up)
                {
                    _selectedTargetIndex--;
                    if (_selectedTargetIndex < 0)
                        _selectedTargetIndex = _currentMenuButtons.Count - 1;
                    UpdateTargetHighlight();
                    e.Handled = true;
                }
                else if (e.Key == Windows.System.VirtualKey.Down)
                {
                    _selectedTargetIndex++;
                    if (_selectedTargetIndex >= _currentMenuButtons.Count)
                        _selectedTargetIndex = 0;
                    UpdateTargetHighlight();
                    e.Handled = true;
                }
                else if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
                {
                    // Confirm target selection
                    if (_selectedTargetIndex >= 0 && _selectedTargetIndex < _currentMenuButtons.Count)
                    {
                        var selectedButton = _currentMenuButtons[_selectedTargetIndex];
                        TargetButton_Click(selectedButton, null);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Windows.System.VirtualKey.Escape || e.Key == Windows.System.VirtualKey.X)
                {
                    // Cancel target selection
                    _selectingTarget = false;
                    TargetSelection.Visibility = Visibility.Collapsed;
                    PartyStatusList.Visibility = Visibility.Visible; // Restore party status
                    ActionMenu.Visibility = Visibility.Visible;
                    _selectedMenuIndex = 0;
                    UpdateMenuHighlight();
                    e.Handled = true;
                }
                // Number keys 1-9 for quick selection
                else if (e.Key >= Windows.System.VirtualKey.Number1 && e.Key <= Windows.System.VirtualKey.Number9)
                {
                    int targetIndex = (int)e.Key - (int)Windows.System.VirtualKey.Number1;
                    if (targetIndex < _currentMenuButtons.Count)
                    {
                        var selectedButton = _currentMenuButtons[targetIndex];
                        TargetButton_Click(selectedButton, null);
                    }
                    e.Handled = true;
                }
            }
            else if (ActionMenu.Visibility == Visibility.Visible && !_battleEnded)
            {
                // Action menu navigation
                if (e.Key == Windows.System.VirtualKey.Up)
                {
                    _selectedMenuIndex--;
                    if (_selectedMenuIndex < 0)
                        _selectedMenuIndex = _currentMenuButtons.Count - 1;
                    UpdateMenuHighlight();
                    e.Handled = true;
                }
                else if (e.Key == Windows.System.VirtualKey.Down)
                {
                    _selectedMenuIndex++;
                    if (_selectedMenuIndex >= _currentMenuButtons.Count)
                        _selectedMenuIndex = 0;
                    UpdateMenuHighlight();
                    e.Handled = true;
                }
                else if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
                {
                    // Confirm menu selection
                    if (_selectedMenuIndex == 0) // Attack
                    {
                        AttackButton_Click(null, null);
                    }
                    e.Handled = true;
                }
            }
        }
    }
}
