using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.IO;
using System.Windows.Input;

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
            if (!OpenWebButton.ContextMenu.IsOpen)
            {
                e.Handled = true;

                var mouseRightClickEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                {
                    RoutedEvent = Mouse.MouseUpEvent,
                    Source = sender,
                };
                InputManager.Current.ProcessInput(mouseRightClickEvent);
            }
        }

        private void cmItch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://decentgamestudio.itch.io/mc") { UseShellExecute = true });
        }

        private void cmGitHub_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/KilLo445/MCParodyLauncher") { UseShellExecute = true });
        }

        private void OpenMoreButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!OpenMoreButton.ContextMenu.IsOpen)
            {
                e.Handled = true;

                var mouseRightClickEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                {
                    RoutedEvent = Mouse.MouseUpEvent,
                    Source = sender,
                };
                InputManager.Current.ProcessInput(mouseRightClickEvent);
            }
        }

        private void cmSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Show();
        }

        private void cmAbout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Show();
        }
    }
}