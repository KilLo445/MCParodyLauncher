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
        failed,
        unzip,
        downloading
    }
    enum MC2RStatus
    {
        ready,
        failed,
        unzip,
        downloading
    }

    public partial class Minecraft2 : Window
    {
        private const bool V = true;
        private string rootPath;
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
                }
            }
        }

        public Minecraft2()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            mc2zip = Path.Combine(rootPath, "mc2.zip");
            mc2 = Path.Combine(rootPath, "games", "Minecraft 2", "Minecraft2.exe");
            mc2dir = Path.Combine(rootPath, "games", "Minecraft 2");
            mc2r = Path.Combine(rootPath, "games", "Minecraft 2 Remake", "Minecraft2Remake.exe");
            mc2rzip = Path.Combine(rootPath, "mc2r.zip");
            mc2rdir = Path.Combine(rootPath, "games", "Minecraft 2 Remake");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Status == MC2Status.downloading)
            {
                MessageBoxResult cancelMC2DL = System.Windows.MessageBox.Show("Minecraft 2 is currently downloading, if you close the launcher the download will fail, do you want to continue?", "Minecraft 2", System.Windows.MessageBoxButton.YesNo);
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
                MessageBoxResult cancelMC2RDL = System.Windows.MessageBox.Show("Minecraft 2 Remake is currently downloading, if you close the launcher the download will fail, do you want to continue?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                if (cancelMC2RDL == MessageBoxResult.Yes)
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
            Directory.CreateDirectory("games");
            Directory.CreateDirectory(mc2dir);

            if (File.Exists(mc2) && Status == MC2Status.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(mc2);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "games", "Minecraft 2");
                Process.Start(startInfo);

                Close();
            }
            else
            {
                if (Status != MC2Status.downloading)
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Minecraft 2 requires 675 MB of storage, do you want to continue?", "Minecraft 2", System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
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
                            webClient.DownloadFileAsync(new Uri("https://www.dropbox.com/s/sfklhomu6trkk7z/mc2.zip?dl=1"), mc2zip);
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
            Directory.CreateDirectory("games");
            Directory.CreateDirectory(mc2rdir);

            if (File.Exists(mc2r))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(mc2r);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "games", "Minecraft 2 Remake");
                Process.Start(startInfo);

                Close();
            }
            else
            {
                if (StatusR != MC2RStatus.downloading)
                {
                    MessageBoxResult messageBoxResult2 = System.Windows.MessageBox.Show("Minecraft 2 Remake requires 425 MB of storage, do you want to continue?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult2 == MessageBoxResult.Yes)
                    {
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
                            webClient.DownloadFileAsync(new Uri("https://www.dropbox.com/s/nv0d1hpdjvh9edm/mc2r.zip?dl=1"), mc2rzip);
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

        private void Window_ContentRendered(object sender, EventArgs e)
        {

        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mwindow = new MainWindow();
            mwindow.Show();
            this.Close();
        }

        private void MC2RemakeDisabled_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MC2Remake_Disabled_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MC2_Disabled_Click(object sender, RoutedEventArgs e)
        {

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
    }
}