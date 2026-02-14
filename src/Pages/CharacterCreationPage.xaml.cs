using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GORE.Services;
using GORE.Models;

namespace GORE.Pages
{
    public sealed partial class CharacterCreationPage : BaseCharacterCreationPage
    {
        public CharacterCreationPage()
        {
            this.InitializeComponent();
            LoadGameConfiguration();
        }

        private async void LoadGameConfiguration()
        {
            config = await ConfigurationService.LoadConfigurationAsync();
        }

        protected override void UpdateSelectionCursor()
        {
            cursorConfirm.Visibility = selection == 0 ? Visibility.Visible : Visibility.Collapsed;
            cursorCancel.Visibility = selection == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnCharacterNameChanged(string name)
        {
            txtHeroName.Text = string.IsNullOrWhiteSpace(name) ? "Hero" : name;
        }

        protected override void OnConfirm(string heroName)
        {
            MusicManager.PlayMusic(MusicTrack.Exploration);
            this.Frame.Navigate(typeof(BaseGamePage), heroName);
        }

        protected override void OnCancel()
        {
            this.Frame.Navigate(typeof(MainMenuPage));
        }
    }
}
