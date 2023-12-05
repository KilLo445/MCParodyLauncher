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
            try
            {
                if (File.Exists(updater))
                {
                    Process.Start(updater);
                    Application.Current.Shutdown();
                }
                else { MessageBox.Show("updater.exe not found.\n\nReinstalling the program might fix the issue.", "File not found", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Show();
            Application.Current.MainWindow.Activate();
            this.Close();
        }

        private void ChangelogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Changelog changelogWindow = new();
                changelogWindow.Show();
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}
