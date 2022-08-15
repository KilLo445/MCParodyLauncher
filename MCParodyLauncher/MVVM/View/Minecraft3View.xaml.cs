using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Media;
using System.Windows.Controls;

namespace MCParodyLauncher.MVVM.View
{
    enum MC3Status
    {
        ready,
        noInstall,
        checkUpdate,
        update,
        failed,
        unzip,
        downloading
    }
    public partial class Minecraft3View : UserControl
    {
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string gamesPath;
        private string mc3;
        private string mc3ver;
        private string mc3zip;
        private string mc3dir;

        private MC3Status _statusmc3;

        internal MC3Status StatusMC3
        {
            get => _statusmc3;
            set
            {
                _statusmc3 = value;
                switch (_statusmc3)
                {
                    case MC3Status.ready:
                        PlayMC3.Content = "Play";
                        break;
                    case MC3Status.noInstall:
                        PlayMC3.Content = "Download";
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
        public Minecraft3View()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");
            gamesPath = Path.Combine(rootPath, "games");
            mc3 = Path.Combine(rootPath, "games", "Minecraft 3", "Game.exe");
            mc3ver = Path.Combine(rootPath, "games", "Minecraft 3", "version.txt");
            mc3zip = Path.Combine(mcplTempPath, "mc3.zip");
            mc3dir = Path.Combine(rootPath, "games", "Minecraft 3");

            CheckInst();
        }
        private void CreateTemp()
        {
            Directory.CreateDirectory(mcplTempPath);
        }
        private void DelTemp()
        {
            if (Directory.Exists(mcplTempPath))
            {
                Directory.Delete(mcplTempPath, true);
            }
        }
        private void CheckInst()
        {
            if (File.Exists(mc3))
            {
                StatusMC3 = MC3Status.ready;
            }
            else
            {
                StatusMC3 = MC3Status.noInstall;
            }
        }
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DLProgress.Value = e.ProgressPercentage;
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
                    Version onlineVersionMC3 = new Version(webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC3/version.txt"));

                    if (onlineVersionMC3.IsDifferentThan(localVersionMC3))
                    {
                        InstallUpdateMC3(true, onlineVersionMC3);
                    }
                    else
                    {
                        StatusMC3 = MC3Status.ready;
                        StartMC3();
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

                    CreateTemp();

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
                        try
                        {
                            WebClient webClient = new WebClient();

                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC3CompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri("https://www.dropbox.com/s/k6kqkmgndyed9kg/mc3.zip?dl=1"), mc3zip);
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
                    StartMC3();
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                StatusMC3 = MC3Status.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void PlayMC3_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.offlineMode == true)
            {
                if (File.Exists(mc3))
                {
                    StartMC3();
                    return;
                }
                else
                {
                    MessageBox.Show("Please launch Minecraft Parody Launcher in online mode to install Minecraft 3.");
                    return;
                }
            }

            if (File.Exists(mc3) && StatusMC3 == MC3Status.ready)
            {
                CheckForUpdatesMC3();
            }
            else
            {
                if (StatusMC3 != MC3Status.downloading)
                {
                    WebClient webClient = new WebClient();
                    string mc3Size = webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC3/size.txt");

                    MessageBoxResult mc3SpaceBox = System.Windows.MessageBox.Show($"Minecraft 3 requires {mc3Size}Do you want to continue?", "Minecraft 3", System.Windows.MessageBoxButton.YesNo);
                    if (mc3SpaceBox == MessageBoxResult.Yes)
                    {
                        DownloadWarning();
                        CreateTemp();
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
                        try
                        {
                            StatusMC3 = MC3Status.downloading;

                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC3CompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri("https://www.dropbox.com/s/k6kqkmgndyed9kg/mc3.zip?dl=1"), mc3zip);
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
                        StatusMC3 = MC3Status.noInstall;
                        MessageBox.Show("Minecraft 3 has been successfully deleted!", "Minecraft 3");
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show($"Error deleting Minecraft 3: {ex}");
                    }
                }
            }
        }
        private void DownloadWarning()
        {
            MessageBox.Show("Please do not switch windows or close the launcher until your download finishes, it may cause issues if you do so.");
        }
        private void StartMC3()
        {
            Process.Start(mc3);
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

        private void MC3Logo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://roggzerz.itch.io/minecraft-3");
        }
    }
}
