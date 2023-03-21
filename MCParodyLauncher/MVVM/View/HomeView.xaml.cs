using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;

namespace MCParodyLauncher.MVVM.View
{
    public partial class HomeView : UserControl
    {
        private static bool Navigate;

        public HomeView()
        {
            InitializeComponent();

            Changelog.Source = new Uri($@"{Environment.CurrentDirectory}\changelog.html");
        }

        private void OpenWebButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("https://killoofficial.wixsite.com/decentgames/launcher");
        }

        private void OpenGitHubButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("https://github.com/KilLo445/MCParodyLauncher");
        }
    }
}
