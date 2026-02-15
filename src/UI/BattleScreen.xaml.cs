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
        private BattleResult _result;
        private bool _battleEnded = false;
        private bool _selectingTarget = false;

        public BattleScreen(Window mainWindow, List<Character> party, List<Enemy> enemies)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _party = party;
            _enemies = enemies;
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
            _battleSystem.StartBattle(_party, _enemies);

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

        private Border CreateEnemyCard(Enemy enemy)
        {
            var border = new Border
            {
                Width = 150,
                Height = 200,
                BorderBrush = new SolidColorBrush(Colors.DarkRed),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromArgb(180, 40, 0, 0)),
                Tag = enemy
            };

            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Enemy image placeholder
            var image = new Border
            {
                Width = 100,
                Height = 100,
                Background = new SolidColorBrush(Colors.DarkGray),
                CornerRadius = new CornerRadius(50),
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Try to load enemy texture
            if (!string.IsNullOrEmpty(enemy.Texture))
            {
                _ = LoadEnemyImageAsync(image, enemy.Texture);
            }

            var nameText = new TextBlock
            {
                Text = enemy.Name,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var hpText = new TextBlock
            {
                Text = $"HP: {enemy.CurrentHP}/{enemy.MaxHP}",
                Foreground = new SolidColorBrush(Colors.LightGreen),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = "hp"
            };

            stack.Children.Add(image);
            stack.Children.Add(nameText);
            stack.Children.Add(hpText);
            border.Child = stack;

            return border;
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
            var border = new Border
            {
                Width = 150,
                Height = 180,
                BorderBrush = new SolidColorBrush(Colors.DarkBlue),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 40)),
                Tag = character
            };

            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            };

            var nameText = new TextBlock
            {
                Text = character.Name,
                Foreground = new SolidColorBrush(Colors.Cyan),
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var levelText = new TextBlock
            {
                Text = $"Lv {character.Level}",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var hpText = new TextBlock
            {
                Text = $"HP: {character.CurrentHP}/{character.MaxHP}",
                Foreground = new SolidColorBrush(Colors.LightGreen),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = "hp"
            };

            var mpText = new TextBlock
            {
                Text = $"MP: {character.CurrentMP}/{character.MaxMP}",
                Foreground = new SolidColorBrush(Colors.LightBlue),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            stack.Children.Add(nameText);
            stack.Children.Add(levelText);
            stack.Children.Add(hpText);
            stack.Children.Add(mpText);
            border.Child = stack;

            return border;
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
            _selectingTarget = true;

            // Clear previous target buttons
            var existingButtons = TargetSelection.Children.OfType<Button>().ToList();
            foreach (var btn in existingButtons)
            {
                TargetSelection.Children.Remove(btn);
            }

            // Add target buttons for alive enemies
            int index = 0;
            foreach (var enemy in _enemies.Where(e => e.IsAlive))
            {
                var button = new Button
                {
                    Content = $"{index + 1}. {enemy.Name}",
                    Width = 150,
                    Height = 40,
                    FontSize = 14,
                    Tag = enemy
                };
                button.Click += TargetButton_Click;
                TargetSelection.Children.Add(button);
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
            foreach (Border card in EnemyPanel.Children)
            {
                var enemy = card.Tag as Enemy;
                if (enemy != null)
                {
                    var stack = card.Child as StackPanel;
                    var hpText = stack?.Children.OfType<TextBlock>().FirstOrDefault(t => t.Tag?.ToString() == "hp");
                    if (hpText != null)
                    {
                        hpText.Text = $"HP: {enemy.CurrentHP}/{enemy.MaxHP}";
                    }

                    if (!enemy.IsAlive)
                    {
                        card.Opacity = 0.3;
                    }
                }
            }

            // Update party cards
            foreach (Border card in PartyPanel.Children)
            {
                var character = card.Tag as Character;
                if (character != null)
                {
                    var stack = card.Child as StackPanel;
                    var hpText = stack?.Children.OfType<TextBlock>().FirstOrDefault(t => t.Tag?.ToString() == "hp");
                    if (hpText != null)
                    {
                        hpText.Text = $"HP: {character.CurrentHP}/{character.MaxHP}";
                    }

                    if (!character.IsAlive)
                    {
                        card.Opacity = 0.3;
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
                ActionMenu.Visibility = Visibility.Visible;
            }
        }
    }
}
