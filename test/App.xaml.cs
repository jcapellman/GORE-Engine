using Microsoft.UI.Xaml;

namespace GORETest
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Initialize and start GORE Engine (window, splash screen, and main menu are all automatic)
            await GORE.Engine.GOREEngine.StartAsync();
        }
    }
}
