using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Media;
using Microsoft.Win32;
using MCParodyLauncher.MVVM.View;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCParodyLauncher
{
    public partial class MainWindow : Window
    {
        string launcherVersion = "1.2.4";
        public static bool devMode = false;

        // Paths and Files
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string versionFile;
        private string updater;

        // Bools
        bool updateAvailable;
        public static bool offlineMode;

        // User Settings
        bool usSplashScreen;
        public static bool usNotifications;
        bool usOfflineMode;

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");
            versionFile = Path.Combine(rootPath, "version.txt");
            updater = Path.Combine(rootPath, "updater.exe");

            VersionText.Text = $"Launcher v{launcherVersion}";

            GetUserSettings();

            if (File.Exists(Path.Combine(rootPath, "devmode.txt"))) { devMode = true; }
            if (devMode == true) { rbtnDev.Visibility = Visibility.Visible; offlineMode = true; }
            if (devMode == false) { rbtnDev.Visibility = Visibility.Hidden; }

            if (usOfflineMode == true && devMode == false) { offlineMode = true; this.Title = "Minecraft Parody Launcher (Offline Mode)"; OfflineText.Visibility = Visibility.Visible; }

            if (usSplashScreen == true) { SplashScreen(); }
            else
            {
                this.Visibility = Visibility.Visible; this.ShowInTaskbar = true;
                if (offlineMode == false)
                {
                    CheckForUpdates();
                }
            }

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
            key3.CreateSubKey("settings");
            RegistryKey key4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games", true);
            key4.CreateSubKey("mc2r");
            key4.CreateSubKey("mc3");
            key4.CreateSubKey("mc4");
            key4.CreateSubKey("mc5");

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

        private void DumpVersion()
        {
            File.WriteAllText(versionFile, launcherVersion);
        }

        private void GetUserSettings()
        {
            RegistryKey keySettings = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            if (keySettings == null) { RegistryKey keySettings2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher", true); keySettings2.CreateSubKey("settings"); keySettings2.Close(); }

            // Splash Screen
            Object obSplashScreen = keySettings.GetValue("SplashScreen", null); string splashscreen = (obSplashScreen as String);
            if (splashscreen == null) { splashscreen = "1"; keySettings.SetValue("SplashScreen", "1"); }
            if (splashscreen == "0") { usSplashScreen = false; }
            if (splashscreen == "1") { usSplashScreen = true; }

            // Notifications
            Object obNotifications = keySettings.GetValue("Notifications", null); string notifications = (obNotifications as String);
            if (notifications == null) { notifications = "1"; keySettings.SetValue("Notifications", "1"); }
            if (notifications == "0") { usNotifications = false; }
            if (notifications == "1") { usNotifications = true; }

            // Offline Mode
            Object obOfflineMode = keySettings.GetValue("OfflineMode", null); string offlinemode = (obOfflineMode as String);
            if (offlinemode == null) { offlinemode = "0"; keySettings.SetValue("OfflineMode", "0"); }
            if (offlinemode == "0") { usOfflineMode = false; }
            if (offlinemode == "1") { usOfflineMode = true; }

            keySettings.Close();
        }

        private async Task SplashScreen()
        {
            SplashScreen splashWindow = new SplashScreen();
            splashWindow.Show();

            await Task.Delay(2000);

            CheckForUpdates();

            return;
        }

        private void RestartApp()
        {
            var currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(currentExecutablePath);
            Application.Current.Shutdown();
        }

        private void CheckForUpdates()
        {
            DumpVersion();

            if (offlineMode == true)
            {
                this.Visibility = Visibility.Visible;
                this.ShowInTaskbar = true;
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
                        this.Visibility = Visibility.Visible;
                        this.ShowInTaskbar = true;
                        updateAvailable = false;
                    }
                }
                catch
                {
                    SystemSounds.Exclamation.Play();
                    MessageBoxResult offlineModeB = MessageBox.Show("Error checking for updates, Would you like to launch Minecraft Parody Launcher in offline mode?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (offlineModeB == MessageBoxResult.Yes)
                    {
                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
                        key.SetValue("OfflineMode", "1");
                        key.Close();
                        RestartApp();
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
                updateWindow.Closed += (sender, args) => { this.Visibility = Visibility.Visible; this.ShowInTaskbar = true; };
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
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
            if (Minecraft2View.downloadActive == true || Minecraft3View.downloadActive == true || Minecraft4View.downloadActive == true || Minecraft5View.downloadActive == true)
            {
                MessageBox.Show("Please do not close Minecraft Parody Launcher while a download is active.", "Download Active", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                try
                {
                    Application.Current.Shutdown();
                }
                catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void MinimizeButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void cmSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Show();
        }

        private void cmAbout_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Show();
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
