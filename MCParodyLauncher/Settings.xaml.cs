using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

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
                cbDev.Visibility = Visibility.Visible;
            }

            GetCurrentSettings();
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

            // Offline Mode
            Object obOfflineMode = key.GetValue("OfflineMode", null); string offlinemode = (obOfflineMode as String);
            if (offlinemode == null) { offlinemode = "0"; key.SetValue("OfflineMode", "0"); }
            if (offlinemode == "0") { cbOffline.IsChecked = false; }
            if (offlinemode == "1") { cbOffline.IsChecked = true; }

            // Dev Mode
            if (File.Exists(Path.Combine(rootPath, "devmode.txt"))) { cbDev.IsChecked = true; }

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

        private void cbDev_Checked(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(Path.Combine(rootPath, "devmode.txt"), "");
        }

        private void cbDev_Unchecked(object sender, RoutedEventArgs e)
        {
            File.Delete(Path.Combine(rootPath, "devmode.txt"));
        }
    }
}
