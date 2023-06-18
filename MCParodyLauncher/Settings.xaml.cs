using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace MCParodyLauncher
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

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
            MessageBox.Show("Any changes made will only apply after restart.", "Minecraft Parody Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
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

            // Offline Mode
            Object obOfflineMode = key.GetValue("OfflineMode", null); string offlinemode = (obOfflineMode as String);
            if (offlinemode == null) { offlinemode = "0"; key.SetValue("OfflineMode", "0"); }
            if (offlinemode == "0") { cbOffline.IsChecked = false; }
            if (offlinemode == "1") { cbOffline.IsChecked = true; }

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
    }
}
