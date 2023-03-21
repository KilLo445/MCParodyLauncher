using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Media;
using System.Windows.Input;
using Microsoft.Win32;

namespace MCParodyLauncher
{
    public partial class MainWindow : Window
    {
        string launcherVersion = "1.1.0";

        // Paths and Files
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string versionFile;
        private string updater;

        // Bools
        bool updateAvailable;
        public static bool offlineMode;

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");
            versionFile = Path.Combine(rootPath, "version.txt");
            updater = Path.Combine(rootPath, "updater.exe");

            VersionText.Text = $"Launcher v{launcherVersion}";

            OfflineMode();
            CheckForUpdates();

            DelTemp();

            CreateReg();
        }

        private void CreateReg()
        {
            RegistryKey key1 = Registry.CurrentUser.OpenSubKey(@"Software", true);
            key1.CreateSubKey("decentgames");
            RegistryKey key2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames", true);
            key2.CreateSubKey("MinecraftParodyLauncher");
            RegistryKey key3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher", true);
            key3.CreateSubKey("games");
            RegistryKey key4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games", true);
            key4.CreateSubKey("mc2");
            key4.CreateSubKey("mc2r");
            key4.CreateSubKey("mc3");
            key4.CreateSubKey("mc4");

            key1.Close();
            key2.Close();
            key3.Close();
            key4.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            DelTemp();
        }

        private void DelTemp()
        {
            if (Directory.Exists(mcplTempPath))
            {
                Directory.Delete(mcplTempPath, true);
            }
        }

        public void OfflineMode()
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                SystemSounds.Exclamation.Play();
                MessageBoxResult offlineModeA = MessageBox.Show("Are you sure you want to launch Minecraft Parody Launcher in Offline Mode?", "Offline Mode", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (offlineModeA == MessageBoxResult.Yes)
                {
                    offlineMode = true;
                    this.Title = "Minecraft Parody Launcher (Offline Mode)";
                }
            }
        }

        private void DumpVersion()
        {
            File.WriteAllText(versionFile, launcherVersion);
        }

        private void CheckForUpdates()
        {
            DumpVersion();

            if (offlineMode == true)
            {
                return;
            }

            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Launcher/version.txt"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        updateAvailable = true;
                        InstallUpdate(true, onlineVersion);
                    }
                    else
                    {
                        updateAvailable = false;
                    }
                }
                catch
                {
                    SystemSounds.Exclamation.Play();
                    MessageBoxResult offlineModeB = MessageBox.Show("Error checking for updates, Would you like to launch Minecraft Parody Launcher in offline mode?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (offlineModeB == MessageBoxResult.Yes)
                    {
                        offlineMode = true;
                        this.Title = "Minecraft Parody Launcher (Offline Mode)";
                        return;
                    }
                    if (offlineModeB == MessageBoxResult.No)
                    {
                        MessageBox.Show("Minecraft Parody Launcher will now close.");
                        Application.Current.Shutdown();
                    }
                }
            }
            else
            {
                updateAvailable = true;
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
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void CloseButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void VersionText_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (offlineMode == true)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Unable to check for updates in offine mode.", "Offline Mode");
                return;
            }

            MessageBoxResult checkUpdateLMB = MessageBox.Show("Do you want to check for updates?", "Launcher Update", MessageBoxButton.YesNo);
            if (checkUpdateLMB == MessageBoxResult.Yes)
            {
                CheckForUpdates();

                if (updateAvailable == false)
                {
                    MessageBox.Show("No update is available.", "Launcher Update");
                }
            }
        }

        private void VersionText_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (offlineMode == true)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Unable to check for updates in offine mode.", "Offline Mode");
                return;
            }
            
            MessageBoxResult checkUpdateRMB = MessageBox.Show("Do you want launch the updater and check for updates?", "Launcher Update", MessageBoxButton.YesNo);
            if (checkUpdateRMB == MessageBoxResult.Yes)
            {
                Process.Start(updater);
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