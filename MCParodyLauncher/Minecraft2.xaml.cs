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
    enum MC2Status
    {
        ready,
        checkUpdate,
        failed,
        unzip,
        downloading,
        update
    }
    enum MC2RStatus
    {
        ready,
        checkUpdate,
        failed,
        unzip,
        downloading,
        update
    }

    public partial class Minecraft2 : Window
    {
        private string rootPath;
        private string gamesPath;
        private string mc2ver;
        private string mc2rver;
        private string mc2zip;
        private string mc2;
        private string mc2dir;
        private string mc2dl;
        private string mc2r;
        private string mc2rzip;
        private string mc2rdir;

        private MC2Status _status;
        private MC2RStatus _statusr;

        internal MC2Status Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case MC2Status.ready:
                        MC2.Content = "Original";
                        break;
                    case MC2Status.failed:
                        MC2.Content = "Error";
                        break;
                    case MC2Status.downloading:
                        MC2.Content = "Downloading";
                        break;
                    case MC2Status.unzip:
                        MC2.Content = "Extracting";
                        break;
                    case MC2Status.update:
                        MC2.Content = "Updating";
                        break;
                }
            }
        }
        internal MC2RStatus StatusR
        {
            get => _statusr;
            set
            {
                _statusr = value;
                switch (_statusr)
                {
                    case MC2RStatus.ready:
                        MC2Remake.Content = "Remake";
                        break;
                    case MC2RStatus.failed:
                        MC2Remake.Content = "Error";
                        break;
                    case MC2RStatus.downloading:
                        MC2Remake.Content = "Downloading";
                        break;
                    case MC2RStatus.unzip:
                        MC2Remake.Content = "Extracting";
                        break;
                    case MC2RStatus.update:
                        MC2Remake.Content = "Updating";
                        break;
                }
            }
        }

        public Minecraft2()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            gamesPath = Path.Combine(rootPath, "games");
            mc2ver = Path.Combine(rootPath, "games", "Minecraft 2", "version.txt");
            mc2rver = Path.Combine(rootPath, "games", "Minecraft 2 Remake", "version.txt");
            mc2zip = Path.Combine(rootPath, "mc2.zip");
            mc2 = Path.Combine(rootPath, "games", "Minecraft 2", "Minecraft2.exe");
            mc2dir = Path.Combine(rootPath, "games", "Minecraft 2");
            mc2r = Path.Combine(rootPath, "games", "Minecraft 2 Remake", "Minecraft2Remake.exe");
            mc2rzip = Path.Combine(rootPath, "mc2r.zip");
            mc2rdir = Path.Combine(rootPath, "games", "Minecraft 2 Remake");
        }

        private void CheckForUpdatesMC2()
        {
            Status = MC2Status.checkUpdate;
            if (File.Exists(mc2ver))
            {
                Version localVersionMC2 = new Version(File.ReadAllText(mc2ver));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersionMC2 = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1jBtlhtYR2VLteceOpwT6HIe6ls-LeDUl"));

                    if (onlineVersionMC2.IsDifferentThan(localVersionMC2))
                    {
                        InstallUpdateMC2(true, onlineVersionMC2);
                    }
                    else
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(mc2);
                        startInfo.WorkingDirectory = Path.Combine(rootPath, "games", "Minecraft 2");
                        Process.Start(startInfo);

                        Close();
                    }
                }
                catch (Exception ex)
                {
                    Status = MC2Status.failed;
                    MessageBox.Show($"Error checking for updates: {ex}");
                }
            }
            else
            {
                InstallUpdateMC2(false, Version.zero);
            }
        }
        private void InstallUpdateMC2(bool isUpdate, Version _onlineVersionMC2)
        {
            try
            {
                MessageBoxResult messageBoxResultMC2Update = System.Windows.MessageBox.Show("An update for Minecraft 2 has been found! Would you like to download it?", "Minecraft 2", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResultMC2Update == MessageBoxResult.Yes)
                {
                    Status = MC2Status.update;

                    try
                    {
                        Directory.Delete(mc2dir, true);

                        Directory.CreateDirectory(mc2dir);

                        if (File.Exists(mc2zip))
                        {
                            try
                            {
                                File.Delete(mc2zip);
                            }
                            catch (Exception ex)
                            {
                                Status = MC2Status.failed;
                                MessageBox.Show($"Error updating Minecraft 2: {ex}");
                            }
                        }
                        DLProgress.Visibility = Visibility.Visible;
                        GoBack.Visibility = Visibility.Hidden;
                        MC2Remake.Visibility = Visibility.Hidden;
                        MC2RemakeDisabled.Visibility = Visibility.Visible;
                        try
                        {
                            WebClient webClient = new WebClient();

                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC2CompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1S6PRHnKPSIbfbmVw4qVRIaDPWL4aZ2x6?alt=media&key=AIzaSyBJx33vWmxvAXAdgwnIZLGIz7xWzyHZffQ"), mc2zip);
                        }
                        catch (Exception ex)
                        {
                            Status = MC2Status.failed;
                            MessageBox.Show($"Error updating Minecraft 2: {ex}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show($"Error updating Minecraft 2: {ex}");
                    }
                }
                else
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(mc2);
                    startInfo.WorkingDirectory = Path.Combine(rootPath, "games", "Minecraft 2");
                    Process.Start(startInfo);

                    Close();
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                Status = MC2Status.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }
        private void CheckForUpdatesMC2R()
        {
            StatusR = MC2RStatus.checkUpdate;
            if (File.Exists(mc2rver))
            {
                Version localVersionMC2R = new Version(File.ReadAllText(mc2rver));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersionMC2R = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=11tSuijamSPTM9E8XoI5Lfv9apPJa0PU1"));

                    if (onlineVersionMC2R.IsDifferentThan(localVersionMC2R))
                    {
                        InstallUpdateMC2R(true, onlineVersionMC2R);
                    }
                    else
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(mc2r);
                        startInfo.WorkingDirectory = Path.Combine(rootPath, "games", "Minecraft 2 Remake");
                        Process.Start(startInfo);

                        Close();
                    }
                }
                catch (Exception ex)
                {
                    StatusR = MC2RStatus.failed;
                    MessageBox.Show($"Error checking for updates: {ex}");
                }
            }
            else
            {
                InstallUpdateMC2R(false, Version.zero);
            }
        }
        private void InstallUpdateMC2R(bool isUpdate, Version _onlineVersionMC2R)
        {
            try
            {
                MessageBoxResult messageBoxResultMC2RUpdate = System.Windows.MessageBox.Show("An update for Minecraft 2 Remake has been found! Would you like to download it?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResultMC2RUpdate == MessageBoxResult.Yes)
                {
                    StatusR = MC2RStatus.update;

                    try
                    {
                        Directory.Delete(mc2rdir, true);

                        Directory.CreateDirectory(mc2rdir);

                        if (File.Exists(mc2rzip))
                        {
                            try
                            {
                                File.Delete(mc2rzip);
                            }
                            catch (Exception ex)
                            {
                                StatusR = MC2RStatus.failed;
                                MessageBox.Show($"Error updating Minecraft 2 Remake: {ex}");
                            }
                        }
                        DLProgress.Visibility = Visibility.Visible;
                        GoBack.Visibility = Visibility.Hidden;
                        MC2.Visibility = Visibility.Hidden;
                        MC2_Disabled.Visibility = Visibility.Visible;
                        try
                        {
                            WebClient webClient = new WebClient();

                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC2RCompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1RJ7NWSTpIxs-vtbR_UfPkTG73tvatEoX?alt=media&key=AIzaSyBJx33vWmxvAXAdgwnIZLGIz7xWzyHZffQ"), mc2rzip);
                        }
                        catch (Exception ex)
                        {
                            StatusR = MC2RStatus.failed;
                            MessageBox.Show($"Error updating Minecraft 2 Remake: {ex}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show($"Error updating Minecraft 2 Remake: {ex}");
                    }
                }
                else
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(mc2r);
                    startInfo.WorkingDirectory = Path.Combine(rootPath, "games", "Minecraft 2 Remake");
                    Process.Start(startInfo);

                    Close();
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                Status = MC2Status.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Status == MC2Status.downloading)
            {
                MessageBoxResult cancelMC2DL = System.Windows.MessageBox.Show("Minecraft 2 is currently downloading, are you sure to want to cancel it?", "Minecraft 2", System.Windows.MessageBoxButton.YesNo);
                if (cancelMC2DL == MessageBoxResult.Yes)
                {

                }
                else
                {
                    e.Cancel = true;
                }
            }
            if (StatusR == MC2RStatus.downloading)
            {
                MessageBoxResult cancelMC2RDL = System.Windows.MessageBox.Show("Minecraft 2 Remake is currently downloading, are you sure to want to cancel it?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                if (cancelMC2RDL == MessageBoxResult.Yes)
                {

                }
                else
                {
                    e.Cancel = true;
                }
            }
            if (Status == MC2Status.update)
            {
                MessageBoxResult cancelMCUpdate = System.Windows.MessageBox.Show("Minecraft 2 is currently updating, are you sure to want to cancel it?", "Minecraft 2", System.Windows.MessageBoxButton.YesNo);
                if (cancelMCUpdate == MessageBoxResult.Yes)
                {

                }
                else
                {
                    e.Cancel = true;
                }
            }
            if (StatusR == MC2RStatus.update)
            {
                MessageBoxResult cancelMC2RUpdate = System.Windows.MessageBox.Show("Minecraft 2 Remake is currently updating, are you sure to want to cancel it?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                if (cancelMC2RUpdate == MessageBoxResult.Yes)
                {

                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void MC2_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(mc2) && Status == MC2Status.ready)
            {
                CheckForUpdatesMC2();
            }
            else
            {
                if (Status != MC2Status.downloading)
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Minecraft 2 requires 675 MB of storage, do you want to continue?", "Minecraft 2", System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        Directory.CreateDirectory("games");
                        Directory.CreateDirectory(mc2dir);

                        if (File.Exists(mc2zip))
                        {
                            try
                            {
                                File.Delete(mc2zip);
                            }
                            catch (Exception ex)
                            {
                                Status = MC2Status.failed;
                                MessageBox.Show($"Error deleting zip: {ex}");
                            }
                        }
                        DLProgress.Visibility = Visibility.Visible;
                        GoBack.Visibility = Visibility.Hidden;
                        MC2Remake.Visibility = Visibility.Hidden;
                        MC2RemakeDisabled.Visibility = Visibility.Visible;
                        try
                        {
                            WebClient webClient = new WebClient();

                            Status = MC2Status.downloading;

                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC2CompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1S6PRHnKPSIbfbmVw4qVRIaDPWL4aZ2x6?alt=media&key=AIzaSyBJx33vWmxvAXAdgwnIZLGIz7xWzyHZffQ"), mc2zip);
                        }
                        catch (Exception ex)
                        {
                            Status = MC2Status.failed;
                            MessageBox.Show($"Error downloading Minecraft 2: {ex}");
                        }
                    }
                }
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DLProgress.Value = e.ProgressPercentage;
        }


        private void MC2Remake_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(mc2r))
            {
                CheckForUpdatesMC2R();
            }
            else
            {
                if (StatusR != MC2RStatus.downloading)
                {
                    MessageBoxResult messageBoxResult2 = System.Windows.MessageBox.Show("Minecraft 2 Remake requires 425 MB of storage, do you want to continue?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult2 == MessageBoxResult.Yes)
                    {
                        Directory.CreateDirectory("games");
                        Directory.CreateDirectory(mc2rdir);

                        if (File.Exists(mc2rzip))
                        {
                            try
                            {
                                File.Delete(mc2rzip);
                            }
                            catch (Exception ex)
                            {
                                StatusR = MC2RStatus.failed;
                                MessageBox.Show($"Error deleting zip: {ex}");
                            }
                        }
                        DLProgress.Visibility = Visibility.Visible;
                        GoBack.Visibility = Visibility.Hidden;
                        MC2.Visibility = Visibility.Hidden;
                        MC2_Disabled.Visibility = Visibility.Visible;
                        try
                        {
                            WebClient webClient = new WebClient();

                            StatusR = MC2RStatus.downloading;

                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC2RCompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1RJ7NWSTpIxs-vtbR_UfPkTG73tvatEoX?alt=media&key=AIzaSyBJx33vWmxvAXAdgwnIZLGIz7xWzyHZffQ"), mc2rzip);
                        }
                        catch (Exception ex)
                        {
                            StatusR = MC2RStatus.failed;
                            MessageBox.Show($"Error downloading Minecraft 2 Remake: {ex}");
                        }
                    }
                }
            }
        }

        private void DownloadMC2CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            Status = MC2Status.unzip;

            try
            {
                ZipFile.ExtractToDirectory(mc2zip, mc2dir);
                File.Delete(mc2zip);
                
                Status = MC2Status.ready;
                DLProgress.Visibility = Visibility.Hidden;
                MC2Remake.Visibility = Visibility.Visible;
                MC2RemakeDisabled.Visibility = Visibility.Hidden;
                GoBack.Visibility = Visibility.Visible;
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Download complete!", "Minecraft 2");
            }
            catch (Exception ex)
            {
                Status = MC2Status.failed;
                MessageBox.Show($"Error installing Minecraft 2: {ex}");
            }
        }
        private void DownloadMC2RCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            StatusR = MC2RStatus.unzip;

            try
            {
                ZipFile.ExtractToDirectory(mc2rzip, mc2rdir);
                File.Delete(mc2rzip);

                StatusR = MC2RStatus.ready;
                DLProgress.Visibility = Visibility.Hidden;
                MC2.Visibility = Visibility.Visible;
                MC2_Disabled.Visibility = Visibility.Hidden;
                GoBack.Visibility = Visibility.Visible;
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Download complete!", "Minecraft 2 Remake");
            }
            catch (Exception ex)
            {
                StatusR = MC2RStatus.failed;
                MessageBox.Show($"Error installing Minecraft 2 Remake: {ex}");
            }
        }
        private void UpdateMC2CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            Status = MC2Status.unzip;

            try
            {
                ZipFile.ExtractToDirectory(mc2zip, mc2dir);
                File.Delete(mc2zip);

                Status = MC2Status.ready;
                DLProgress.Visibility = Visibility.Hidden;
                MC2Remake.Visibility = Visibility.Visible;
                MC2RemakeDisabled.Visibility = Visibility.Hidden;
                GoBack.Visibility = Visibility.Visible;
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Update complete!", "Minecraft 2");
            }
            catch (Exception ex)
            {
                Status = MC2Status.failed;
                MessageBox.Show($"Error Updating Minecraft 2: {ex}");
            }
        }
        private void UpdateMC2RCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            StatusR = MC2RStatus.unzip;

            try
            {
                ZipFile.ExtractToDirectory(mc2rzip, mc2rdir);
                File.Delete(mc2rzip);

                StatusR = MC2RStatus.ready;
                DLProgress.Visibility = Visibility.Hidden;
                MC2.Visibility = Visibility.Visible;
                MC2_Disabled.Visibility = Visibility.Hidden;
                GoBack.Visibility = Visibility.Visible;
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Update complete!", "Minecraft 2 Remake");
            }
            catch (Exception ex)
            {
                StatusR = MC2RStatus.failed;
                MessageBox.Show($"Error Updating Minecraft 2 Remake: {ex}");
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mwindow = new MainWindow();
            mwindow.Show();
            this.Close();
        }

        private void MC2_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (File.Exists(mc2))
            {
                MessageBoxResult delMC2Box = System.Windows.MessageBox.Show("Are you sure you want to delete Minecraft 2?", "Minecraft 2", System.Windows.MessageBoxButton.YesNo);
                if (delMC2Box == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.Delete(mc2dir, true);
                        SystemSounds.Exclamation.Play();
                        if (Directory.GetFiles(gamesPath).Length == 0
                                 && Directory.GetDirectories(gamesPath).Length == 0)
                        {
                            Directory.Delete(gamesPath);
                        }
                        MessageBox.Show("Minecraft 2 has been successfully deleted!", "Minecraft 2");
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show($"Error deleting Minecraft 2: {ex}");
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

        private void MC2Remake_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (File.Exists(mc2r))
            {
                MessageBoxResult delMC3Box = System.Windows.MessageBox.Show("Are you sure you want to delete Minecraft 2 Remake?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                if (delMC3Box == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.Delete(mc2rdir, true);
                        SystemSounds.Exclamation.Play();
                        if (Directory.GetFiles(gamesPath).Length == 0
                                 && Directory.GetDirectories(gamesPath).Length == 0)
                        {
                            Directory.Delete(gamesPath);
                        }
                        MessageBox.Show("Minecraft 2 Remake has been successfully deleted!", "Minecraft 2 Remake");
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show($"Error deleting Minecraft 2 Remake: {ex}");
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