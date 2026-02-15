using GORE.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using System.Linq;

namespace GORE.UI
{
    public sealed partial class BattleResultScreen : Window
    {
        private readonly Window _mainWindow;
        private readonly BattleResult _result;
        private readonly List<Character> _party;

        public BattleResultScreen(Window mainWindow, BattleResult result, List<Character> party)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _result = result;
            _party = party;

            ExtendsContentIntoTitleBar = true;
            ScreenHelper.EnterFullScreenMode(this);

            DisplayResults();
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            RootGrid.Focus(FocusState.Programmatic);
        }

        private void DisplayResults()
        {
            // Display defeated enemies
            DefeatedEnemiesText.Text = string.Join(", ", _result.DefeatedEnemies);

            // Display rewards
            GoldText.Text = $"{_result.TotalGold} Gold";
            ExpText.Text = $"{_result.TotalExp} Experience";

            // Display character experience
            foreach (var kvp in _result.CharacterExpGained)
            {
                var text = new TextBlock
                {
                    Text = $"{kvp.Key}: +{kvp.Value} EXP",
                    FontSize = 16,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.LightCyan)
                };
                CharacterExpPanel.Children.Add(text);
            }

            // Display level ups
            if (_result.LevelUps.Count > 0)
            {
                LevelUpPanel.Visibility = Visibility.Visible;
                LevelUpsList.ItemsSource = _result.LevelUps;
            }
        }

        private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Return to world map
                var worldMapScreen = new WorldMapScreen(_mainWindow, _party.First());
                worldMapScreen.Activate();
                Close();
            }
        }
    }
}
