using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MCParodyLauncher
{
    public partial class LaunchMC4 : Window
    {
        private string mc4dir;
        private string mc4odir;

        private string mc4;
        private string mc4o;

        public LaunchMC4()
        {
            InitializeComponent();
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

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (rbtnPlayMC4.IsChecked == true)
            {
                PlayMC4();
            }
            if (rbtnPlayMC4O.IsChecked == true)
            {
                PlayMC4O();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PlayMC4()
        {
            using (RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4"))
            {
                if (keyMC4 != null)
                {
                    Object obMC4Path = keyMC4.GetValue("InstallPath");
                    if (obMC4Path != null)
                    {
                        mc4dir = (obMC4Path as String);
                        mc4 = Path.Combine(mc4dir, "Minecraft4.exe");
                        keyMC4.Close();
                        try
                        {
                            Process.Start(mc4);
                            Close();
                        }
                        catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    }
                }
            }
        }

        private void PlayMC4O()
        {
            using (RegistryKey keyMC4O = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4"))
            {
                if (keyMC4O != null)
                {
                    Object obMC4OPath = keyMC4O.GetValue("InstallPathOtherside");
                    if (obMC4OPath != null)
                    {
                        mc4odir = (obMC4OPath as String);
                        mc4o = Path.Combine(mc4odir, "Minecraft4Otherside.exe");
                        keyMC4O.Close();
                        try
                        {
                            Process.Start(mc4o);
                            Close();
                        }
                        catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    }
                }
            }
        }
    }
}
