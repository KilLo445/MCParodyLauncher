﻿using System;
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

            if (Keyboard.IsKeyDown(Key.LeftShift) || File.Exists(Path.Combine(rootPath, "devmode.txt")) || MainWindow.devMode == true)
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

            MessageBoxResult restartNow = System.Windows.MessageBox.Show("Any changes made will only apply after you restart Minecraft Parody Launcher. Would you like to restart now?", "Restart Required", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (restartNow == MessageBoxResult.Yes)
            {
                System.Windows.Forms.Application.Restart();
                Application.Current.Shutdown();
            }
            else { this.Close(); }
        }

        private void GetCurrentSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            if (key == null) { RegistryKey key2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher", true); key2.CreateSubKey("settings"); key2.Close(); }

            // Notifications
            Object obNotifications = key.GetValue("Notifications", null); string notifications = (obNotifications as String);
            if (notifications == null) { notifications = "1"; key.SetValue("Notifications", "1"); }
            if (notifications == "0") { cbNotifications.IsChecked = false; }
            if (notifications == "1") { cbNotifications.IsChecked = true; }

            // Display download stats
            Object obDLStats = key.GetValue("DownloadStats", null); string downloadstats = (obDLStats as String);
            if (downloadstats == null) { downloadstats = "1"; key.SetValue("DownloadStats", "1"); }
            if (downloadstats == "0") { cbDLStats.IsChecked = false; }
            if (downloadstats == "1") { cbDLStats.IsChecked = true; }

            // Hide launcher in-game
            Object obHideLauncher = key.GetValue("HideLauncher", null); string hidelauncher = (obHideLauncher as String);
            if (hidelauncher == null) { hidelauncher = "1"; key.SetValue("HideLauncher", "1"); }
            if (hidelauncher == "0") { cbHide.IsChecked = true; }
            if (hidelauncher == "1") { cbHide.IsChecked = false; }

            // Offline Mode
            //Object obOfflineMode = key.GetValue("OfflineMode", null); string offlinemode = (obOfflineMode as String);
            //if (offlinemode == null) { offlinemode = "0"; key.SetValue("OfflineMode", "0"); }
            //if (offlinemode == "0") { cbOffline.IsChecked = false; }
            //if (offlinemode == "1") { cbOffline.IsChecked = true; }

            // Dev Mode
            Object obDevMode = key.GetValue("Developer", null); string devmode = (obDevMode as String);
            if (devmode == null) { devmode = "0"; key.SetValue("Developer", "0"); }
            if (devmode == "0") { cbDev.IsChecked = false; }
            if (devmode == "1") { cbDev.IsChecked = true; }

            key.Close();
        }

        // CHECKBOX ACTIONS

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

        // Display download stats

        private void cbDLStats_Checked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("DownloadStats", "1");
            key.Close();
        }

        private void cbDLStats_Unchecked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("DownloadStats", "0");
            key.Close();
        }

        // Hide in-game

        private void cbHide_Checked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("HideLauncher", "0");
            key.Close();
        }

        private void cbHide_Unchecked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\settings", true);
            key.SetValue("HideLauncher", "1");
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
