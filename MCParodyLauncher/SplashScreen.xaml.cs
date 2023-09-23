using System.Threading.Tasks;
using System.Windows;

namespace MCParodyLauncher
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            if (MainWindow.offlineMode == true)
            {
                Status.Text = "Launching in offline mode...";

                if (MainWindow.devMode == true) { Status.Text = "Launching in developer mode..."; }
            }

            Splash();
        }

        private async Task Splash()
        {
            await Task.Delay(2000);
            this.Close();
        }
    }
}
