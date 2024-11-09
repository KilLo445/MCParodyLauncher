using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
            this.Close();
        }

        private async void PlayMC4()
        {
            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (Process.GetProcessesByName("Minecraft4").Length > 0)
                {
                    MessageBox.Show("Minecraft 4 is already running.", "Minecraft 4", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            using (RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4"))
            {
                if (keyMC4 != null)
                {
                    Object obMC4Path = keyMC4.GetValue("InstallPath");
                    if (obMC4Path != null)
                    {
                        mc4dir = (obMC4Path as String);
                        mc4 = Path.Combine(mc4dir + "\\mc4\\", "Minecraft4.exe");
                        keyMC4.Close();
                        try
                        {
                            this.Hide();
                            Process.Start(mc4);
                            LaunchingGame launchWindow = new LaunchingGame("Minecraft 4");
                            launchWindow.Show();
                            await Task.Delay(500);
                            if (MainWindow.usHideWindow == true)
                            {
                                Application.Current.MainWindow.Hide();
                                bool gameRunning = true;
                                while (gameRunning == true)
                                {
                                    await Task.Delay(50);
                                    if (Process.GetProcessesByName(LaunchingGame.mc4Proc).Length > 0)
                                    {
                                        gameRunning = true;
                                    }
                                    else
                                    {
                                        gameRunning = false;
                                    }
                                }
                                await Task.Delay(100);
                                Application.Current.MainWindow.Show();
                                Application.Current.MainWindow.Activate();
                            }
                            this.Close();
                        }
                        catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    }
                }
            }
        }

        private async void PlayMC4O()
        {
            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (Process.GetProcessesByName("Minecraft4Otherside").Length > 0)
                {
                    MessageBox.Show("Minecraft 4 Otherside is already running.", "Minecraft 4 Otherside", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            using (RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4"))
            {
                if (keyMC4 != null)
                {
                    Object obMC4Path = keyMC4.GetValue("InstallPath");
                    if (obMC4Path != null)
                    {
                        mc4odir = (obMC4Path as String);
                        mc4o = Path.Combine(mc4odir + "\\mc4o\\", "Minecraft4Otherside.exe");
                        keyMC4.Close();
                        try
                        {
                            this.Hide();
                            Process.Start(mc4o);
                            LaunchingGame launchWindow = new LaunchingGame("Minecraft 4 Otherside");
                            launchWindow.Show();
                            await Task.Delay(500);
                            if (MainWindow.usHideWindow == true)
                            {
                                Application.Current.MainWindow.Hide();
                                bool gameRunning = true;
                                while (gameRunning == true)
                                {
                                    await Task.Delay(50);
                                    if (Process.GetProcessesByName(LaunchingGame.mc4oProc).Length > 0)
                                    {
                                        gameRunning = true;
                                    }
                                    else
                                    {
                                        gameRunning = false;
                                    }
                                }
                                await Task.Delay(100);
                                Application.Current.MainWindow.Show();
                                Application.Current.MainWindow.Activate();
                            }
                            this.Close();
                        }
                        catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    }
                }
            }
        }
    }
}
