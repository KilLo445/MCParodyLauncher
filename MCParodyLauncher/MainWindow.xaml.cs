using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Media;

namespace MCParodyLauncher
{
    enum LauncherStatus
    {
        ready,
        checkUpdate,
        updateFound,
        failed,
        checkFailed
    }

    public partial class MainWindow : Window
    {
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string versionFile;
        private string installerPath;
        private string installerExe;
        private string updater;

        private LauncherStatus _status;

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");
            versionFile = Path.Combine(rootPath, "version.txt");
            installerPath = Path.Combine(mcplTempPath, "installer");
            installerExe = Path.Combine(mcplTempPath, "installer", "MCParodyLauncherSetup.exe");
            updater = Path.Combine(rootPath, "updater.exe");

            MoveOldTemp();
            DelTemp();
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
        }
        private void MoveOldTemp()
        {
            if (File.Exists("mc2.zip"))
            {
                File.Move("mc2.zip", mcplTempPath);
            }
            if (File.Exists("mc2r.zip"))
            {
                File.Move("mc2r.zip", mcplTempPath);
            }
            if (File.Exists("mc3.zip"))
            {
                File.Move("mc3.zip", mcplTempPath);
            }
        }

        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Launcher/version.txt"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallUpdate(true, onlineVersion);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking for updates: {ex}");
                }
            }
            else
            {
                InstallUpdate(false, Version.zero);
            }
        }

        private void InstallUpdate(bool isUpdate, Version _onlineVersion)
        {

            try
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("An update for the launcher has been found! Would you like to download it?", "Launcher Update", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (File.Exists(updater))
                    {
                        Process.Start(updater);
                        Close();
                    }
                    else
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(new Uri("https://github.com/KilLo445/MCParodyLauncher-Updater/releases/download/main/updater.exe"), updater);
                        Process.Start(updater);
                        Close();
                    }
                }
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

        private void CloseButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}