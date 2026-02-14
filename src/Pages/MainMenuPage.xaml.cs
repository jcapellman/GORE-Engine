using Microsoft.UI.Xaml;

namespace GORE.Pages
{
    public sealed partial class MainMenuPage : BaseMainMenuPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        protected override void UpdateSelectionCursor()
        {
            // TODO: Implement selection cursor update
        }

        protected override void OnNewGame()
        {
            // TODO: Navigate to character creation
        }

        protected override void OnLoadGame()
        {
            // TODO: Navigate to load game screen
        }

        protected override void OnExitGame()
        {
            Application.Current.Exit();
        }
    }
}
