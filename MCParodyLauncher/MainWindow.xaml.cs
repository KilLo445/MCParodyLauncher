using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Media;

namespace MCParodyLauncher
{
    public partial class MainWindow : Window
    {
        string launcherVersion = "0.3.8";

        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string versionFile;
        private string updater;
        bool updateAvailable;

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");
            versionFile = Path.Combine(rootPath, "version.txt");
            updater = Path.Combine(rootPath, "updater.exe");

            VersionText.Text = $"Launcher v{launcherVersion}";

            DelTemp();
            DumpVersion();
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
            if (Directory.Exists("installer"))
            {
                Directory.Delete("installer", true);
            }
            if (File.Exists("mc2.zip"))
            {
                File.Delete("mc2.zip");
            }
            if (File.Exists("mc2r.zip"))
            {
                File.Delete("mc2r.zip");
            }
            if (File.Exists("mc3.zip"))
            {
                File.Delete("mc3.zip");
            }
        }
        private void DumpVersion()
        {
            if (!File.Exists(versionFile))
            {
                File.Create(versionFile);
            }
            File.WriteAllText(versionFile, launcherVersion);
        }
        private void CheckForUpdates()
        {
            DumpVersion();

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
                    MessageBox.Show("Error checking for updates, are you connected to the internet?", "Error");
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

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
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