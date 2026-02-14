using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using GORE.Models;
using GORE.Services;

namespace GORE.Pages
{
    /// <summary>
    /// Base character creation page - fully driven by game.json configuration.
    /// Games just provide configuration, no code needed!
    /// </summary>
    public abstract class BaseCharacterCreationPage : BasePage
    {
        // Character name
        protected string characterName = "Hero";
        
        // Menu state
        protected int selection = 0;
        protected const int OptionCount = 2; // Confirm, Cancel
        
        // Configuration
        protected GameConfiguration config;

        protected BaseCharacterCreationPage()
        {
            LoadConfiguration();
        }

        private async void LoadConfiguration()
        {
            config = await ConfigurationService.LoadConfigurationAsync();
            
            // Set default character name from config
            if (config?.Game?.Title != null)
            {
                characterName = "Hero"; // Default, can be overridden in config
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Note: WinUI 3 doesn't have CoreWindow.KeyDown, input handling needs to be implemented differently
            // This is a placeholder - implement keyboard input via Window.KeyDown or control-level events
            UpdateSelectionCursor();
            OnCharacterNameChanged(characterName);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            // Note: WinUI 3 doesn't have CoreWindow, cleanup event handlers differently if needed
        }

        // Note: Input handling has been commented out as it needs to be reimplemented for WinUI 3
        // WinUI 3 doesn't have CoreWindow - use Window.KeyDown or control-level KeyboardAccelerators instead
        /*
        protected virtual void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            args.Handled = true;

            if (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.A)
            {
                selection = 0; // Confirm
                UpdateSelectionCursor();
            }
            else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.D)
            {
                selection = 1; // Cancel
                UpdateSelectionCursor();
            }
            else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.Space)
            {
                ExecuteSelection();
            }
            else if (args.VirtualKey == VirtualKey.Back)
            {
                // Handle backspace for name input
                if (characterName.Length > 0)
                {
                    characterName = characterName.Substring(0, characterName.Length - 1);
                    OnCharacterNameChanged(characterName);
                }
            }
            else if (args.VirtualKey == VirtualKey.Escape)
            {
                OnCancel();
            }
        }
        */

        /*
        protected virtual void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            // Handle character input for name
            char character = (char)args.KeyCode;

            // Only accept letters, spaces, and common characters
            if (char.IsLetterOrDigit(character) || character == ' ')
            {
                if (characterName.Length < 20) // Max name length
                {
                    characterName += character;
                    OnCharacterNameChanged(characterName);
                }
            }
        }
        */

        protected virtual void ExecuteSelection()
        {
            if (selection == 0)
            {
                // Confirm - validate and start game
                if (!string.IsNullOrWhiteSpace(characterName))
                {
                    OnConfirm(characterName);
                }
            }
            else
            {
                // Cancel - return to main menu
                OnCancel();
            }
        }

        // Abstract methods for UI updates (XAML controls)
        protected abstract void UpdateSelectionCursor();
        protected abstract void OnCharacterNameChanged(string name);

        // Action hooks
        protected abstract void OnConfirm(string heroName);
        protected abstract void OnCancel();
    }
}
