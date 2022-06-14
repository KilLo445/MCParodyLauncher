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
        private string tempPath;
        private string mcplTemp;
        private string updateVer;
        private string updater;

        public LauncherUpdate()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTemp = Path.Combine(tempPath, "MCParodyLauncher");
            updateVer = Path.Combine(mcplTemp, "UpdateVersion.txt"); 
            updater = Path.Combine(rootPath, "updater.exe");

            GetVersion();
        }

        private void GetVersion()
        {
            Directory.CreateDirectory(mcplTemp);

            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Launcher/version.txt"), updateVer);

            updateVer = File.ReadAllText(updateVer);

            UpdateVersion.Text = $"Update: v{updateVer}";
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
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

        private void Window_ContentRendered(object sender, EventArgs e)
        {

        }
    }
}
