using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MCParodyLauncherUpdater
{
    enum UpdaterStatus
    {
        checking,
        noUpdate,
        updateFound,
        waiting,
        downloading,
        extracting,
        done
    }
    public partial class MainWindow : Window
    {
        ///////////////////////////////////////////////////////////////////
        /// Ignore my spagetti code, this hasnt been updated in a while.///
        ///////////////////////////////////////////////////////////////////

        string updaterVersion = "2.5.6";
        string rootPath;

        private string tempPath;
        private string mcplTempPath;
        private string updaterTemp;
        private string launcherVersionFile;
        private string installerZip;
        private string installerDir;
        private string installer;

        bool mcplRunning = false;

        private UpdaterStatus _status;

        internal UpdaterStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case UpdaterStatus.checking:
                        DLProgress.IsIndeterminate = true;
                        UpdaterStatusText.Text = "Checking for updates";
                        break;
                    case UpdaterStatus.noUpdate:
                        DLProgress.IsIndeterminate = false;
                        UpdaterStatusText.Text = "No update available";
                        break;
                    case UpdaterStatus.updateFound:
                        DLProgress.IsIndeterminate = false;
                        UpdaterStatusText.Text = "Update Found";
                        break;
                    case UpdaterStatus.waiting:
                        DLProgress.IsIndeterminate = true;
                        UpdaterStatusText.Text = "Waiting for MC Parody Launcher to exit";
                        break;
                    case UpdaterStatus.downloading:
                        DLProgress.IsIndeterminate = false;
                        UpdaterStatusText.Text = "Downloading";
                        break;
                    case UpdaterStatus.extracting:
                        DLProgress.IsIndeterminate = true;
                        UpdaterStatusText.Text = "Extracting";
                        break;
                }
            }

        }

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");
            updaterTemp = Path.Combine(mcplTempPath, "updater");
            launcherVersionFile = Path.Combine(rootPath, "version.txt");
            installerZip = Path.Combine(mcplTempPath, "installer.zip");
            installerDir = Path.Combine(mcplTempPath, "installer");
            installer = Path.Combine(mcplTempPath, "installer", "MCParodyLauncherSetup.exe");
        }

        private void DelInstaller()
        {
            try
            {
                if (File.Exists(installer)) { File.Delete(installer); }
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void CreateTemp()
        {
            try
            {
                Directory.CreateDirectory(mcplTempPath);
                Directory.CreateDirectory(installerDir);
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                Status = UpdaterStatus.checking;
                await Task.Delay(1500);
                if (File.Exists(launcherVersionFile))
                {
                    Version localVersion = new Version(File.ReadAllText(launcherVersionFile));
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Launcher/version.txt"));

                    if (onlineVersion.IsDifferentThan(localVersion)) { _ = InstallUpdateAsync(true, onlineVersion); }
                    else { NoUpdate(); }

                }
                else { _ = InstallUpdateAsync(false, Version.zero); }
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }
        private async Task InstallUpdateAsync(bool isUpdate, Version _onlineVersion)
        {
            try
            {
                Status = UpdaterStatus.waiting;
                mcplRunning = true;
                while (mcplRunning == true)
                {
                    if (Process.GetProcessesByName("MCParodyLauncher").Length > 0) { mcplRunning = true; }
                    else { mcplRunning = false; }
                    await Task.Delay(500);
                }

                Status = UpdaterStatus.downloading;
                DelInstaller();
                CreateTemp();
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompletedCallback);
                webClient.DownloadFileAsync(new Uri("https://github.com/KilLo445/MCParodyLauncher/releases/download/main/MinecraftParodyLauncher.zip"), installerZip);
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                Status = UpdaterStatus.checking;
                CheckForUpdatesAsync();
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e) { DLProgress.Value = e.ProgressPercentage; }

        private void DownloadCompletedCallback(object sender, AsyncCompletedEventArgs e) { ExtractZipAsync(installerZip, installerDir); }

        private async Task ExtractZipAsync(string zipfile, string output)
        {
            Status = UpdaterStatus.extracting;
            try
            {
                await Task.Run(() => ZipFile.ExtractToDirectory(zipfile, output));
                File.Delete(installerZip);
                Status = UpdaterStatus.done;
                await Task.Delay(1500);
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = installer,
                        Arguments = "/SILENT /NOCANCEL"
                    }
                };
                process.Start();
                Application.Current.Shutdown();
                return;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void NoUpdate()
        {
            Status = UpdaterStatus.noUpdate;
            CloseButton.Visibility = Visibility.Visible;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { Application.Current.Shutdown(); }

        private void ErrorMSG(Exception exception) { MessageBox.Show($"{exception}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

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
