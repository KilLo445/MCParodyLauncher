using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
    enum MC3Status
    {
        ready,
        checkUpdate,
        update,
        failed,
        unzip,
        downloading
    }
    enum MC4Status
    {
        ready,
        checkUpdate,
        update,
        failed,
        unzip,
        downloading
    }

    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionFile;
        private string installerExe;
        private string updater;
        private string gamesPath;
        private string mc2;
        private string mc3;
        private string mc3ver;
        private string mc3zip;
        private string mc3dir;
        private string mc4;
        private string mc4ver;
        private string mc4zip;
        private string mc4dir;

        private LauncherStatus _status;
        private MC3Status _statusmc3;
        private MC4Status _statusmc4;

        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.ready:
                        UpdateText.Text = "";
                        break;
                    case LauncherStatus.checkUpdate:
                        UpdateText.Text = "Checking for updates.";
                        break;
                    case LauncherStatus.failed:
                        UpdateText.Text = "Update failed!";
                        break;
                    case LauncherStatus.updateFound:
                        UpdateText.Text = "Update found!";
                        break;
                    case LauncherStatus.checkFailed:
                        UpdateText.Text = "Update check failed!";
                        break;
                }
            }

        }
        internal MC3Status StatusMC3
        {
            get => _statusmc3;
            set
            {
                _statusmc3 = value;
                switch (_statusmc3)
                {
                    case MC3Status.ready:
                        PlayMC3.Content = "Minecraft 3";
                        break;
                    case MC3Status.failed:
                        PlayMC3.Content = "Error";
                        break;
                    case MC3Status.downloading:
                        PlayMC3.Content = "Downloading";
                        break;
                    case MC3Status.unzip:
                        PlayMC3.Content = "Extracting";
                        break;
                    case MC3Status.update:
                        PlayMC3.Content = "Updating";
                        break;
                }
            }
        }
        internal MC4Status StatusMC4
        {
            get => _statusmc4;
            set
            {
                _statusmc4 = value;
                switch (_statusmc4)
                {
                    case MC4Status.ready:
                        PlayMC4.Content = "Minecraft 4";
                        break;
                    case MC4Status.failed:
                        PlayMC4.Content = "Error";
                        break;
                    case MC4Status.downloading:
                        PlayMC4.Content = "Downloading";
                        break;
                    case MC4Status.unzip:
                        PlayMC4.Content = "Extracting";
                        break;
                    case MC4Status.update:
                        PlayMC4.Content = "Updating";
                        break;
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "version.txt");
            installerExe = Path.Combine(rootPath, "installer", "MCParodyLauncherSetup.exe");
            updater = Path.Combine(rootPath, "updater.exe");
            gamesPath = Path.Combine(rootPath, "games");
            mc2 = Path.Combine(rootPath, "games", "Minecraft 2", "Minecraft2.exe");
            mc3 = Path.Combine(rootPath, "games", "Minecraft 3", "Game.exe");
            mc3ver = Path.Combine(rootPath, "games", "Minecraft 3", "version.txt");
            mc3zip = Path.Combine(rootPath, "mc3.zip");
            mc3dir = Path.Combine(rootPath, "games", "Minecraft 3");
            mc4 = Path.Combine(rootPath, "games", "Minecraft 4", "Minecraft4.exe");
            mc4ver = Path.Combine(rootPath, "games", "Minecraft 4", "version.txt");
            mc4zip = Path.Combine(rootPath, "mc4.zip");
            mc4dir = Path.Combine(rootPath, "games", "Minecraft 4");

            if (File.Exists(installerExe))
            {
                File.Delete(installerExe);
            }
            if (Directory.Exists("installer"))
            {
                Directory.Delete("installer");
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (StatusMC3 == MC3Status.downloading)
            {
                MessageBoxResult mc3DLCancelConfirmation = System.Windows.MessageBox.Show("Minecraft 3 is currently downloading, if you close the launcher the download will fail, do you want to continue?", "Minecraft 3", System.Windows.MessageBoxButton.YesNo);
                if (mc3DLCancelConfirmation == MessageBoxResult.Yes)
                {

                }
                else
                {
                    e.Cancel = true;
                }
            }
            if (StatusMC4 == MC4Status.downloading)
            {
                MessageBoxResult mc4DLCancelConfirmation = System.Windows.MessageBox.Show("Minecraft 4 is currently downloading, if you close the launcher the download will fail, do you want to continue?", "Minecraft 4", System.Windows.MessageBoxButton.YesNo);
                if (mc4DLCancelConfirmation == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.Delete(mc4zip);
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show($"Error deleting zip: {ex}");
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DLProgress.Value = e.ProgressPercentage;
        }

        private void CheckForUpdates()
        {
            Status = LauncherStatus.checkUpdate;
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1rxN417kyFzZoGmRN1arAx9prpX2pAZPY"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallUpdate(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherStatus.ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.checkFailed;
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
            Status = LauncherStatus.updateFound;

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
                        webClient.DownloadFile(new Uri("https://github.com/KilLo445/minecraft-parody-launcher-updater/releases/download/main/updater.exe"), updater);
                        Process.Start(updater);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }
        private void CheckForUpdatesMC3()
        {
            StatusMC3 = MC3Status.checkUpdate;
            if (File.Exists(mc3ver))
            {
                Version localVersionMC3 = new Version(File.ReadAllText(mc3ver));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersionMC3 = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1O0wu065VA7rEYrkYkXmOwsWAjWXhvtrS"));

                    if (onlineVersionMC3.IsDifferentThan(localVersionMC3))
                    {
                        InstallUpdateMC3(true, onlineVersionMC3);
                    }
                    else
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(mc3);
                        startInfo.WorkingDirectory = Path.Combine(rootPath, "games", "Minecraft 3");
                        Process.Start(startInfo);

                        Close();
                    }
                }
                catch (Exception ex)
                {
                    StatusMC3 = MC3Status.failed;
                    MessageBox.Show($"Error checking for updates: {ex}");
                }
            }
            else
            {
                InstallUpdateMC3(false, Version.zero);
            }
        }
        private void InstallUpdateMC3(bool isUpdate, Version _onlineVersionMC2)
        {
            try
            {
                MessageBoxResult messageBoxResultMC3Update = System.Windows.MessageBox.Show("An update for Minecraft 3 has been found! Would you like to download it?", "Minecraft 3", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResultMC3Update == MessageBoxResult.Yes)
                {
                    StatusMC3 = MC3Status.update;

                    try
                    {
                        Directory.Delete(mc3dir, true);

                        Directory.CreateDirectory(mc3dir);

                        if (File.Exists(mc3zip))
                        {
                            try
                            {
                                File.Delete(mc3zip);
                            }
                            catch (Exception ex)
                            {
                                SystemSounds.Exclamation.Play();
                                StatusMC3 = MC3Status.failed;
                                MessageBox.Show($"Error updating Minecraft 3: {ex}");
                            }
                        }
                        DLProgress.Visibility = Visibility.Visible;
                        PlayMC2.Visibility = Visibility.Hidden;
                        PlayMC2_Disabled.Visibility = Visibility.Visible;
                        try
                        {
                            WebClient webClient = new WebClient();

                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC3CompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/13smtOPH8EnJK34i1BBLaKnDs7NlmsNsv?alt=media&key=AIzaSyBJx33vWmxvAXAdgwnIZLGIz7xWzyHZffQ"), mc3zip);
                        }
                        catch (Exception ex)
                        {
                            SystemSounds.Exclamation.Play();
                            StatusMC3 = MC3Status.failed;
                            MessageBox.Show($"Error updating Minecraft 3: {ex}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show($"Error updating Minecraft 3: {ex}");
                    }
                }
                else
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(mc3);
                    startInfo.WorkingDirectory = Path.Combine(rootPath, "games", "Minecraft 3");
                    Process.Start(startInfo);

                    Close();
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                StatusMC3 = MC3Status.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Changelog.Source = new Uri($@"{Environment.CurrentDirectory}\changelog.html");
            CheckForUpdates();
        }

        private void PlayMC2_Click(object sender, RoutedEventArgs e)
        {
            Minecraft2 mc2window = new Minecraft2();
            mc2window.Show();
            this.Close();
        }
        private void PlayMC3_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(mc3))
            {
                CheckForUpdatesMC3();
            }
            else
            {
                if (StatusMC3 != MC3Status.downloading)
                {
                    MessageBoxResult mc3SpaceBox = System.Windows.MessageBox.Show("Minecraft 3 requires 318 MB of storage, do you want to continue?", "Minecraft 3", System.Windows.MessageBoxButton.YesNo);
                    if (mc3SpaceBox == MessageBoxResult.Yes)
                    {
                        Directory.CreateDirectory("games");
                        Directory.CreateDirectory(mc3dir);

                        if (File.Exists(mc3zip))
                        {
                            try
                            {
                                File.Delete(mc3zip);
                            }
                            catch (Exception ex)
                            {
                                SystemSounds.Exclamation.Play();
                                StatusMC3 = MC3Status.failed;
                                MessageBox.Show($"Error deleting zip: {ex}");
                            }
                        }
                        DLProgress.Visibility = Visibility.Visible;
                        PlayMC2.Visibility = Visibility.Hidden;
                        PlayMC2_Disabled.Visibility = Visibility.Visible;
                        try
                        {
                            WebClient webClient = new WebClient();

                            StatusMC3 = MC3Status.downloading;

                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC3CompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/13smtOPH8EnJK34i1BBLaKnDs7NlmsNsv?alt=media&key=AIzaSyBJx33vWmxvAXAdgwnIZLGIz7xWzyHZffQ"), mc3zip);
                        }
                        catch (Exception ex)
                        {
                            SystemSounds.Exclamation.Play();
                            StatusMC3 = MC3Status.failed;
                            MessageBox.Show($"Error downloading Minecraft 3: {ex}");
                        }
                    }
                }
            }
        }
        private void DownloadMC3CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            StatusMC3 = MC3Status.unzip;

            try
            {
                ZipFile.ExtractToDirectory(mc3zip, mc3dir);
                File.Delete(mc3zip);

                StatusMC3 = MC3Status.ready;
                DLProgress.Visibility = Visibility.Hidden;
                PlayMC2.Visibility = Visibility.Visible;
                PlayMC2_Disabled.Visibility = Visibility.Hidden;
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Download complete!", "Minecraft 3");
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                StatusMC3 = MC3Status.failed;
                MessageBox.Show($"Error installing Minecraft 3: {ex}");
            }
        }
        private void UpdateMC3CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            StatusMC3 = MC3Status.unzip;

            try
            {
                ZipFile.ExtractToDirectory(mc3zip, mc3dir);
                File.Delete(mc3zip);

                StatusMC3 = MC3Status.ready;
                DLProgress.Visibility = Visibility.Hidden;
                PlayMC2.Visibility = Visibility.Visible;
                PlayMC2_Disabled.Visibility = Visibility.Hidden;
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Update complete!", "Minecraft 3");
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                StatusMC3 = MC3Status.failed;
                MessageBox.Show($"Error updating Minecraft 3: {ex}");
            }
        }

        private void PlayMC4_Click(object sender, RoutedEventArgs e)
        {

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

        private void PlayMC3_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (File.Exists(mc3))
            {
                MessageBoxResult delMC3Box = System.Windows.MessageBox.Show("Are you sure you want to delete Minecraft 3?", "Minecraft 3", System.Windows.MessageBoxButton.YesNo);
                if (delMC3Box == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.Delete(mc3dir, true);
                        SystemSounds.Exclamation.Play();
                        if (Directory.GetFiles(gamesPath).Length == 0
                                 && Directory.GetDirectories(gamesPath).Length == 0)
                        {
                            Directory.Delete(gamesPath);
                        }
                        MessageBox.Show("Minecraft 3 has been successfully deleted!", "Minecraft 3");
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show($"Error deleting Minecraft 3: {ex}");
                    }
                }
                else
                {

                }
            }
            else
            {

            }
        }
    }
}