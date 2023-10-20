using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Wsh = IWshRuntimeLibrary;

namespace MCParodyLauncher
{
    public partial class Settings : Window
    {
        string rootPath;

        public Settings()
        {
            rootPath = Directory.GetCurrentDirectory();

            InitializeComponent();

            if (Keyboard.IsKeyDown(Key.LeftShift) || File.Exists(Path.Combine(rootPath, "devmode.txt")))
            {
                cbOffline.Margin = new Thickness(30, 20, 0, 45);
                cbDev.Visibility = Visibility.Visible;
            }

            GetCurrentSettings();
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
            cbDev.Visibility = Visibility.Hidden;

            MessageBoxResult restartNow = System.Windows.MessageBox.Show("Any changes made will only apply after restart. Would you like to restart now?", "Restart Required", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (restartNow == MessageBoxResult.Yes)
            {
                var currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(currentExecutablePath);
                Application.Current.Shutdown();
            }
            else { this.Close(); }
        }

        private void GetCurrentSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            if (key == null) { RegistryKey key2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher", true); key2.CreateSubKey("settings"); key2.Close(); }

            // Splash Screen
            Object obSplashScreen = key.GetValue("SplashScreen", null); string splashscreen = (obSplashScreen as String);
            if (splashscreen == null) { splashscreen = "1"; key.SetValue("SplashScreen", "1"); }
            if (splashscreen == "0") { cbSplash.IsChecked = false; }
            if (splashscreen == "1") { cbSplash.IsChecked = true; }

            // Notifications
            Object obNotifications = key.GetValue("Notifications", null); string notifications = (obNotifications as String);
            if (notifications == null) { notifications = "1"; key.SetValue("Notifications", "1"); }
            if (notifications == "0") { cbNotifications.IsChecked = false; }
            if (notifications == "1") { cbNotifications.IsChecked = true; }

            // Start with Windows
            Object obStartup = key.GetValue("Startup", null); string startwin = (obStartup as String);
            if (startwin == null) { startwin = "0"; key.SetValue("Startup", "0"); }
            if (startwin == "0") { cbStartup.IsChecked = false; }
            if (startwin == "1") { cbStartup.IsChecked = true; }



            // Offline Mode
            Object obOfflineMode = key.GetValue("OfflineMode", null); string offlinemode = (obOfflineMode as String);
            if (offlinemode == null) { offlinemode = "0"; key.SetValue("OfflineMode", "0"); }
            if (offlinemode == "0") { cbOffline.IsChecked = false; }
            if (offlinemode == "1") { cbOffline.IsChecked = true; }

            // Dev Mode
            Object obDevMode = key.GetValue("Developer", null); string devmode = (obDevMode as String);
            if (devmode == null) { devmode = "0"; key.SetValue("Developer", "0"); }
            if (devmode == "0") { cbDev.IsChecked = false; }
            if (devmode == "1") { cbDev.IsChecked = true; }

            key.Close();
        }


        // Splash Screen

        private void cbSplash_Checked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("SplashScreen", "1");
            key.Close();
        }

        private void cbSplash_Unchecked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("SplashScreen", "0");
            key.Close();

        }

        // Notifications

        private void cbNotifications_Checked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("Notifications", "1");
            key.Close();
        }

        private void cbNotifications_Unchecked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("Notifications", "0");
            key.Close();
        }

        // Start with Windows

        private void cbStartup_Checked(object sender, RoutedEventArgs e)
        {
            if (!MainWindow.IsAdministrator())
            {
                cbStartup.IsChecked = false;
                MessageBox.Show("Administrator privlages are required to change this setting.", "Administrator privlages required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("This setting is completely pointless, honestly I just thought it would be funny to add.", "Start with Windows", MessageBoxButton.OK, MessageBoxImage.Information);
            try
            {
                Wsh.WshShell shell = new Wsh.WshShell();
                string shortcutAddress = "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\Startup" + @"\Minecraft Parody Launcher.lnk";
                Wsh.IWshShortcut shortcut = (Wsh.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.TargetPath = rootPath + "\\MCParodyLauncher.exe";
                shortcut.Save();

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
                key.SetValue("Startup", "1");
                key.Close();
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void cbStartup_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                File.Delete("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\Startup" + @"\Minecraft Parody Launcher.lnk");
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
                key.SetValue("Startup", "0");
                key.Close();
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        // Offline Mode

        private void cbOffline_Checked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("OfflineMode", "1");
            key.Close();
        }

        private void cbOffline_Unchecked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("OfflineMode", "0");
            key.Close();
        }


        // Developer Mode

        private void cbDev_Checked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("Developer", "1");
            key.Close();
        }

        private void cbDev_Unchecked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("Developer", "0");
            key.Close();
        }
    }
}
