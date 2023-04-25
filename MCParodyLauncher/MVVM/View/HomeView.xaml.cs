using System.Diagnostics;
using System.Windows.Controls;
using System.IO;

namespace MCParodyLauncher.MVVM.View
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            string rootPath = Directory.GetCurrentDirectory();
            string changelogFile = Path.Combine(rootPath, "changelog.txt");
            string changelogContent;
            if (File.Exists(changelogFile)) { changelogContent = File.ReadAllText(changelogFile); }
            else { changelogContent = "Changelog file not found"; }
            Changelog.Text = changelogContent;
        }

        private void OpenWebButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://decentgamestudio.itch.io/mc") { UseShellExecute = true });
        }

        private void OpenGitHubButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/KilLo445/MCParodyLauncher") { UseShellExecute = true });
        }
    }
}