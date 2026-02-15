using Microsoft.UI.Xaml;

namespace GORETest
{
    public partial class App : Application
    {
        private Window m_window;

        public App()
        {
            InitializeComponent();

            // Show cursor when app exits
            this.UnhandledException += (sender, e) =>
            {
                GORE.Engine.GOREEngine.ShowCursor();
            };
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();

            // Initialize and start GORE Engine (splash screen is shown automatically)
            await GORE.Engine.GOREEngine.StartAsync(m_window);
        }
    }
}
