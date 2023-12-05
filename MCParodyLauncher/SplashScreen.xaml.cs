using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace MCParodyLauncher
{
    public partial class SplashScreen : Window
    {
        int tryCount = 1;
        int failCount = 0;

        public SplashScreen()
        {
            InitializeComponent();

            if (MainWindow.offlineMode == true)
            {
                Status.Text = "Launching in offline mode...";

                if (MainWindow.devMode == true) { Status.Text = "Launching in developer mode..."; DevText.Visibility = Visibility.Visible; }
            }

            Splash();
        }

        private async Task Splash()
        {
            if (MainWindow.offlineMode == true || MainWindow.devMode == true)
            {
                await Task.Delay(1500);
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.Activate();
                this.Close();
            }
            else
            {
                failCount = 0;
                tryCount = 1;
                CheckForUpdates();
            }
        }

        private void RestartApp()
        {
            var currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(currentExecutablePath);
            Application.Current.Shutdown();
        }

        private async Task CheckForUpdates()
        {
            try
            {
                Version localVersion = new Version(MainWindow.launcherVersion);
                WebClient webClient = new();
                Version onlineVersion = new(await webClient.DownloadStringTaskAsync(MainWindow.onlineVerLink));
                await Task.Delay(500);
                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    MainWindow.updateAvailable = true;
                    try
                    {
                        LauncherUpdate updateWindow = new();
                        updateWindow.Show();
                        this.Close();
                    }
                    catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
                else
                {
                    Application.Current.MainWindow.Show();
                    Application.Current.MainWindow.Activate();
                    MainWindow.updateAvailable = false;
                    this.Close();
                }
            }
            catch
            {
                failCount = failCount + 1;
                tryCount = tryCount + 1;
                await Task.Delay(500);
                Status.Text = $"Checking for updates... (Attempt {tryCount} of 5)";
                await Task.Delay(750);
                if (failCount == 4) { UpdateFailed(); }
                else { CheckForUpdates(); }
            }
        }

        private void UpdateFailed()
        {
            Status.Text = $"Error checking for updates...";
            progressBar.IsIndeterminate = false;
            MessageBoxResult offlineMode = MessageBox.Show("Error checking for updates, would you like to launch Minecraft Parody Launcher in offline mode?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (offlineMode == MessageBoxResult.Yes)
            {
                MessageBox.Show("You can disable offline mode at any time in the settings.", "Offline Mode", MessageBoxButton.OK, MessageBoxImage.Information);
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
                key.SetValue("OfflineMode", "1");
                key.Close();
                RestartApp();
            }
            if (offlineMode == MessageBoxResult.No)
            {
                MessageBox.Show("Minecraft Parody Launcher will now close.", "Minecraft Parody Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
            }
        }

        struct Version
        {
            internal static Version zero = new Version(0, 0, 0);

            private short major;
            private short minor;
            private short subMinor;

            internal Version(short _major, short _minor, short _subMinor)
            {
                major = _major;
                minor = _minor;
                subMinor = _subMinor;
            }
            internal Version(string _version)
            {
                string[] versionStrings = _version.Split('.');
                if (versionStrings.Length != 3)
                {
                    major = 0;
                    minor = 0;
                    subMinor = 0;
                    return;
                }

                major = short.Parse(versionStrings[0]);
                minor = short.Parse(versionStrings[1]);
                subMinor = short.Parse(versionStrings[2]);
            }

            internal bool IsDifferentThan(Version _otherVersion)
            {
                if (major != _otherVersion.major)
                {
                    return true;
                }
                else
                {
                    if (minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (subMinor != _otherVersion.subMinor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return $"{major}.{minor}.{subMinor}";
            }
        }
    }
}
