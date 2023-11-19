using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.IO;
using System.Windows.Input;
using System.Media;
using System.Windows;
using System.Net;
using Microsoft.Win32;

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

        private void cmResetSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult resetSettings = System.Windows.MessageBox.Show("Are you sure you want to reset all settings to default?", "Reset Settings", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resetSettings == MessageBoxResult.Yes)
            {
                try
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\", true);
                    key.DeleteSubKey("settings");
                    key.Close();
                    MessageBox.Show("Settings have successfully been reset to default.", "Settings Reset", MessageBoxButton.OK, MessageBoxImage.Information);
                    MessageBox.Show("Changes will only apply after you restart Minecraft Parody Launcher.", "Restart Required", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void cmUpdate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MainWindow.offlineMode == true)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Unable to check for updates in offine mode.", "Offline Mode");
                return;
            }

            CheckForUpdateHV();

            if (MainWindow.updateAvailable == false)
            {
                MessageBox.Show("No update is available.", "Launcher Update");
                return;
            }
        }

        private void cmAbout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Show();
        }

        private void CheckForUpdateHV()
        {
            string rootPath = Directory.GetCurrentDirectory();
            string versionFile = Path.Combine(rootPath, "version.txt");
            File.WriteAllText(versionFile, MainWindow.launcherVersion);

            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Launcher/version.txt"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        MainWindow.updateAvailable = true;
                        InstallUpdate(true, onlineVersion);
                    }
                    else
                    {
                        MainWindow.updateAvailable = false;
                    }
                }
                catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            else
            {
                MainWindow.updateAvailable = true;
                InstallUpdate(false, Version.zero);
            }
        }

        private void InstallUpdate(bool isUpdate, Version _onlineVersion)
        {
            try
            {
                LauncherUpdate updateWindow = new LauncherUpdate();
                updateWindow.Show();
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
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