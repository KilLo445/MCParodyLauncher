using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using MCParodyLauncher.MVVM.View;
using System.Threading.Tasks;
using System.Security.Principal;
using WinForms = System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace MCParodyLauncher
{
    public partial class MainWindow : Window
    {
        public static string launcherVersion = "1.2.11";
        public static string onlineVerLink = "https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Launcher/version.txt";
        public static bool devMode = false;

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // Paths and Files
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string versionFile;

        // Bools
        public static bool updateAvailable;
        public static bool offlineMode;

        // User Settings
        public static bool usNotifications;
        bool usOfflineMode;
        public static bool usDownloadStats;
        public static bool usHideWindow;


        public MainWindow()
        {
            InitializeComponent();

            if (Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                MessageBox.Show("Minecraft Parody Launcher is already running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");
            versionFile = Path.Combine(rootPath, "version.txt");

            VersionText.Text = $"Launcher v{launcherVersion}";

            GetUserSettings();

            if (File.Exists(Path.Combine(rootPath, "devmode.txt"))) { devMode = true; }
            if (devMode == true) { rbtnDev.Visibility = Visibility.Visible; offlineMode = true; }
            if (devMode == false) { rbtnDev.Visibility = Visibility.Hidden; }

            if (usOfflineMode == true && devMode == false) { offlineMode = true; this.Title = "Minecraft Parody Launcher (Offline Mode)"; OfflineText.Visibility = Visibility.Visible; }

            SplashScreen();

            DelTemp();
            CreateReg();
            AddToTray();
        }

        private async void AddToTray()
        {
            WinForms.NotifyIcon nIcon = new();
            nIcon.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Images/Minecraft.ico")).Stream);
            nIcon.Text = "Minecraft Parody Launcher";

            nIcon.ContextMenuStrip = new();
            nIcon.ContextMenuStrip.Items.Add("Open", null, NIcon_Open);
            nIcon.ContextMenuStrip.Items.Add("Exit", null, NIcon_Exit);

            nIcon.Visible = true;
        }

        private async void NIcon_Open(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private async void NIcon_Exit(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
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


            if ((Process.GetProcessesByName("Minecraft2Remake").Length > 0) || (Process.GetProcessesByName("Game").Length > 0) || (Process.GetProcessesByName("Minecraft4").Length > 0) || (Process.GetProcessesByName("Minecraft4Otherside").Length > 0) || (Process.GetProcessesByName("MC5").Length > 0))
            {
                MessageBoxResult closeGames = MessageBox.Show("It seems like one or more games are running, would you like to close them?", "Games Running", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (closeGames == MessageBoxResult.Yes)
                {
                    try
                    {
                        foreach (var process in Process.GetProcessesByName("Minecraft2Remake"))
                        {
                            process.Kill();
                        }
                        foreach (var process in Process.GetProcessesByName("Game"))
                        {
                            process.Kill();
                        }
                        foreach (var process in Process.GetProcessesByName("Minecraft4"))
                        {
                            process.Kill();
                        }
                        foreach (var process in Process.GetProcessesByName("Minecraft4Otherside"))
                        {
                            process.Kill();
                        }
                        foreach (var process in Process.GetProcessesByName("MC5"))
                        {
                            process.Kill();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
            }
        }

        private void DelTemp()
        {
            try
            {
                if (Directory.Exists(mcplTempPath)) { Directory.Delete(mcplTempPath, true); }
                if (File.Exists(versionFile)) { File.Delete(versionFile); }
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void GetUserSettings()
        {
            RegistryKey keyMCPL = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher", true); keyMCPL.CreateSubKey("settings"); keyMCPL.Close();
            RegistryKey keySettings = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);

            // Notifications
            Object obNotifications = keySettings.GetValue("Notifications", null); string notifications = (obNotifications as String);
            if (notifications == null) { notifications = "1"; keySettings.SetValue("Notifications", "1"); }
            if (notifications == "0") { usNotifications = false; }
            if (notifications == "1") { usNotifications = true; }

            // Display download stats
            Object obDLStats = keySettings.GetValue("DownloadStats", null); string downloadstats = (obDLStats as String);
            if (downloadstats == null) { downloadstats = "1"; keySettings.SetValue("DownloadStats", "1"); }
            if (downloadstats == "0") { usDownloadStats = false; }
            if (downloadstats == "1") { usDownloadStats = true; }

            // Hide launcher in-game
            Object obHideLauncher = keySettings.GetValue("HideLauncher", null); string hidelauncher = (obHideLauncher as String);
            if (hidelauncher == null) { hidelauncher = "1"; keySettings.SetValue("HideLauncher", "1"); }
            if (hidelauncher == "0") { usHideWindow = false; }
            if (hidelauncher == "1") { usHideWindow = true; }

            // Offline Mode
            Object obOfflineMode = keySettings.GetValue("OfflineMode", null); string offlinemode = (obOfflineMode as String);
            if (offlinemode == null) { offlinemode = "0"; keySettings.SetValue("OfflineMode", "0"); }
            if (offlinemode == "0") { usOfflineMode = false; }
            if (offlinemode == "1") { usOfflineMode = true; }

            // Developer Mode
            Object obDevMode = keySettings.GetValue("Developer", null); string devmode = (obDevMode as String);
            if (devmode == null) { offlinemode = "0"; keySettings.SetValue("Developer", "0"); }
            if (devmode == "0") { devMode = false; }
            if (devmode == "1") { devMode = true; }

            // Delete old keys
            Object obSplashScreen = keySettings.GetValue("SplashScreen", null); string splashscreen = (obSplashScreen as String);
            if (splashscreen != null) { keySettings.DeleteValue("SplashScreen"); }
            Object obStartup = keySettings.GetValue("Startup", null); string startup = (obStartup as String);
            if (startup != null) { keySettings.DeleteValue("Startup"); }

            keySettings.Close();
        }

        private async Task SplashScreen()
        {
            SplashScreen splashWindow = new SplashScreen();
            splashWindow.Show();
            return;
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
    }
}
