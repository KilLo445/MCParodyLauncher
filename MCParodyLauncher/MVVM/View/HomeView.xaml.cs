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

        private void cmWeb_Click(object sender, System.Windows.RoutedEventArgs e) { Process.Start(new ProcessStartInfo("https://decentstudios.com") { UseShellExecute = true }); }

        private void cmItch_Click(object sender, System.Windows.RoutedEventArgs e) { Process.Start(new ProcessStartInfo("https://decentgamestudio.itch.io/mc") { UseShellExecute = true }); }
   
        private void cmGitHub_Click(object sender, System.Windows.RoutedEventArgs e) { Process.Start(new ProcessStartInfo("https://github.com/KilLo445/MCParodyLauncher") { UseShellExecute = true }); }

        private void cmFAQ_Click(object sender, RoutedEventArgs e) { Process.Start(new ProcessStartInfo("https://github.com/KilLo445/MCParodyLauncher/blob/master/FAQ.md") { UseShellExecute = true }); }
    
        private void mc2Uninst_Click(object sender, RoutedEventArgs e) { Process.Start(new ProcessStartInfo("https://files.decentstudios.com/decentgames/mc2/MC2_Uninstaller.zip") { UseShellExecute = true }); }


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
                    MessageBoxResult restartNow = System.Windows.MessageBox.Show("Changes will only apply after you restart Minecraft Parody Launcher, would you like to restart now?", "Restart Required", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (restartNow == MessageBoxResult.Yes)
                    {
                        System.Windows.Forms.Application.Restart();
                        Application.Current.Shutdown();
                    }
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

            CheckForUpdates();

            if (MainWindow.updateAvailable == false)
            {
                MessageBox.Show("No update is available.", "Launcher Update");
                return;
            }
        }


        private void cmUpdateChangelog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Changelog changelogWindow = new();
                changelogWindow.Show();
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void cmAbout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Show();
        }

        private void cmRestart_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.Restart();
            Application.Current.Shutdown();
        }

        private void cmExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CheckForUpdates()
        {
            try
            {
                Version localVersion = new Version(MainWindow.launcherVersion);
                WebClient webClient = new();
                Version onlineVersion = new (webClient.DownloadString(MainWindow.onlineVerLink));
                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    MainWindow.updateAvailable = true;
                    try
                    {
                        LauncherUpdate updateWindow = new();
                        updateWindow.Show();
                        return;
                    }
                    catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
                else
                {
                    MainWindow.updateAvailable = false;
                    return;
                }
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