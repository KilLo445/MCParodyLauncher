using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace MCParodyLauncher
{
    public partial class LauncherUpdate : Window
    {
        private string rootPath;
        private string updater;

        string updateVer;

        public LauncherUpdate()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            updater = Path.Combine(rootPath, "updater.exe");

            WebClient webClient = new WebClient();
            updateVer = webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Launcher/version.txt");
            UpdateVersion.Text = $"Update: v{updateVer}";

            if (MainWindow.usNotifications == true)
            {
                new ToastContentBuilder()
                .AddText("Update Available")
                .AddText("An update for Minecraft Parody Launcher has been found!")
                .Show();
            }
        }

        private void DragWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(updater))
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(new Uri("https://github.com/KilLo445/MCParodyLauncher-Updater/releases/download/main/updater.exe"), updater);
            }

            Process.Start(updater);
            Application.Current.Shutdown();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
