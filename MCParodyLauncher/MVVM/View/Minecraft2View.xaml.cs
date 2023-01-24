using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Media;
using System.Windows.Controls;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;
using System.Threading.Tasks;

namespace MCParodyLauncher.MVVM.View
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
    public partial class Minecraft2View : UserControl
    {
        // Paths
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string gamesPath;
        private string mc2dir;
        private string mc2rdir;

        // Files
        private string mc2;
        private string mc2ver;
        private string mc2zip;
        private string mc2r;
        private string mc2rver;
        private string mc2rzip;

        // Settings
        string mc2Installed;
        string mc2rInstalled;

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
                        PlayMC2.Content = "Original";
                        break;
                    case MC2Status.failed:
                        PlayMC2.Content = "Error";
                        break;
                    case MC2Status.downloading:
                        PlayMC2.Content = "Downloading";
                        break;
                    case MC2Status.unzip:
                        PlayMC2.Content = "Installing";
                        break;
                    case MC2Status.update:
                        PlayMC2.Content = "Updating";
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
                        PlayMC2R.Content = "Remake";
                        break;
                    case MC2RStatus.failed:
                        PlayMC2R.Content = "Error";
                        break;
                    case MC2RStatus.downloading:
                        PlayMC2R.Content = "Downloading";
                        break;
                    case MC2RStatus.unzip:
                        PlayMC2R.Content = "Installing";
                        break;
                    case MC2RStatus.update:
                        PlayMC2R.Content = "Updating";
                        break;
                }
            }
        }

        public Minecraft2View()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");

            mc2zip = Path.Combine(mcplTempPath, "mc2.zip");
            mc2rzip = Path.Combine(mcplTempPath, "mc2r.zip");
        }

        private void CreateTemp()
        {
            Directory.CreateDirectory(mcplTempPath);
        }

        private void DownloadWarning()
        {
            MessageBox.Show("Please do not switch game tabs or close the launcher until your download finishes, it may cause issues if you do so.");
        }

        private async Task ExtractZipAsyncMC2(string zipfile, string output)
        {
            try
            {
                Status = MC2Status.unzip;
                DLProgress.IsIndeterminate = true;
                await Task.Run(() => ZipFile.ExtractToDirectory(zipfile, output));
                File.Delete(zipfile);
                Status = MC2Status.ready;
                DLProgress.Visibility = Visibility.Hidden;
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Download complete!", "Minecraft 2");
                return;
            }
            catch (Exception ex)
            {
                Status = MC2Status.failed;
                MessageBox.Show($"Error Updating Minecraft 2: {ex}");
                return;
            }
        }

        private async Task ExtractZipAsyncMC2R(string zipfile, string output)
        {
            try
            {
                StatusR = MC2RStatus.unzip;
                DLProgress.IsIndeterminate = true;
                await Task.Run(() => ZipFile.ExtractToDirectory(zipfile, output));
                File.Delete(zipfile);
                StatusR = MC2RStatus.ready;
                DLProgress.Visibility = Visibility.Hidden;
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Download complete!", "Minecraft 2 Remake");
                return;
            }
            catch (Exception ex)
            {
                StatusR = MC2RStatus.failed;
                MessageBox.Show($"Error Updating Minecraft 2 Remake: {ex}");
                return;
            }
        }

        // Minecarft 2

        private void PlayMC2_Click(object sender, RoutedEventArgs e)
        {
            if (Status == MC2Status.downloading)
            {
                return;
            }

            RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2", true);
            Object obMC2Installed = keyMC2.GetValue("Installed", null);

            if (MainWindow.offlineMode == true)
            {
                Object obMC2Path = keyMC2.GetValue("InstallPath");
                if (obMC2Path != null)
                {
                    mc2dir = (obMC2Path as String);
                    mc2 = Path.Combine(mc2dir, "Minecraft2.exe");
                }

                if (File.Exists(mc2))
                {
                    StartMC2();
                    return;
                }
                else
                {
                    MessageBox.Show("Please launch Minecraft Parody Launcher in online mode to install Minecraft 2.");
                    return;
                }
            }

            keyMC2.Close();
            if (obMC2Installed != null)
            {
                mc2Installed = (obMC2Installed as String);

                if (mc2Installed == "1")
                {
                    CheckForUpdatesMC2();
                }
                else
                {
                    InstallMC2();
                }
            }
            else
            {
                InstallMC2();
            }
        }

        private void StartMC2()
        {
            using (RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2"))
            {
                if (keyMC2 != null)
                {
                    Object obMC2Path = keyMC2.GetValue("InstallPath");
                    if (obMC2Path != null)
                    {
                        mc2dir = (obMC2Path as String);
                        mc2 = Path.Combine(mc2dir, "Minecraft2.exe");
                        keyMC2.Close();
                        try
                        {
                            Process.Start(mc2);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error launching Minecraft 2: {ex}");
                        }
                    }
                }
            }
        }

        private void InstallMC2()
        {
            if (StatusR == MC2RStatus.downloading)
            {
                MessageBox.Show("Please wait until your download finishes before starting another one.");
                return;
            }

            MessageBox.Show("Minecraft 2 has not been updated in a long time, and it will never be updated again, I highly recommend downloading Minecraft 2 Remake, it takes up less space on your computer.", "Minecraft 2");

            WebClient webClient = new WebClient();
            string mc2Size = webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC2/size.txt");

            MessageBoxResult mc2InstallConfirm = System.Windows.MessageBox.Show($"Minecraft 2 requires {mc2Size}Do you want to continue?", "Minecraft 2", System.Windows.MessageBoxButton.YesNo);
            if (mc2InstallConfirm == MessageBoxResult.Yes)
            {
                RegistryKey keyGames = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games", true);
                keyGames.CreateSubKey("mc2");
                RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2", true);
                keyGames.Close();

                MessageBoxResult mc2InstallLocationMB = System.Windows.MessageBox.Show($"Would you like to install Minecraft 2 at {rootPath}\\games", "Minecraft 2", System.Windows.MessageBoxButton.YesNo);
                if (mc2InstallLocationMB == MessageBoxResult.Yes)
                {
                    keyMC2.SetValue("InstallPath", $"{rootPath}\\games\\Minecraft 2");
                    keyMC2.Close();
                    mc2dir = Path.Combine(rootPath, "games", "Minecraft 2");
                    DownloadMC2();
                }
                if (mc2InstallLocationMB == MessageBoxResult.No)
                {
                    WinForms.FolderBrowserDialog mc2FolderDialog = new WinForms.FolderBrowserDialog();
                    mc2FolderDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    mc2FolderDialog.Description = "Please select where you would like to install Minecraft 2, a folder called \"Minecraft 2\" will be created.";
                    mc2FolderDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc2Result = mc2FolderDialog.ShowDialog();

                    if (mc2Result == WinForms.DialogResult.OK)
                    {
                        mc2dir = Path.Combine(mc2FolderDialog.SelectedPath, "Minecraft 2");
                        keyMC2.SetValue("InstallPath", mc2dir);
                        keyMC2.Close();
                        DownloadMC2();
                    }
                }

                keyMC2.Close();
            }
        }

        private void DownloadMC2()
        {
            DownloadWarning();
            CreateTemp();
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

            try
            {
                Status = MC2Status.downloading;

                DLProgress.IsIndeterminate = false;
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC2CompletedCallback);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri("https://www.dropbox.com/s/6ea5or46qxbuamz/mc2.zip?dl=1"), mc2zip);
            }
            catch (Exception ex)
            {
                Status = MC2Status.failed;
                MessageBox.Show($"Error downloading Minecraft 2: {ex}");
            }
        }

        private void DownloadMC2CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2", true);
            keyMC2.SetValue("Installed", "1");
            keyMC2.Close();
            ExtractZipAsyncMC2(mc2zip, mc2dir);
        }

        private void CheckForUpdatesMC2()
        {
            Status = MC2Status.checkUpdate;

            try
            {
                using (RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2"))
                {
                    if (keyMC2 != null)
                    {
                        Object obMC2Path = keyMC2.GetValue("InstallPath");
                        if (obMC2Path != null)
                        {
                            mc2dir = (obMC2Path as String);
                            mc2ver = Path.Combine(mc2dir, "version.txt");
                            keyMC2.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                Status = MC2Status.failed;
                MessageBox.Show($"Error: {ex}");
            }

            if (File.Exists(mc2ver))
            {
                Version localVersionMC2 = new Version(File.ReadAllText(mc2ver));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersionMC2 = new Version(webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC2/version.txt"));

                    if (onlineVersionMC2.IsDifferentThan(localVersionMC2))
                    {
                        InstallUpdateMC2(true, onlineVersionMC2);
                    }
                    else
                    {
                        Status = MC2Status.ready;
                        StartMC2();
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

                    CreateTemp();

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
                                MessageBox.Show($"Error deleting zip: {ex}");
                            }
                        }

                        DLProgress.Visibility = Visibility.Visible;

                        try
                        {
                            Status = MC2Status.downloading;

                            DLProgress.IsIndeterminate = false;
                            WebClient webClient = new WebClient();
                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC2CompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri("https://www.dropbox.com/s/6ea5or46qxbuamz/mc2.zip?dl=1"), mc2zip);
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
                    StartMC2();
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                Status = MC2Status.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void UpdateMC2CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            ExtractZipAsyncMC2(mc2zip, mc2dir);
        }

        private void PlayMC2_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            using (RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2", true))
            {
                if (keyMC2 != null)
                {
                    Object obMC2Install = keyMC2.GetValue("Installed");
                    mc2Installed = (obMC2Install as String);

                    if (mc2Installed != "1")
                    {
                        keyMC2.Close();
                        return;
                    }

                    MessageBoxResult delMC2Box = System.Windows.MessageBox.Show("Are you sure you want to delete Minecraft 2?", "Minecraft 2", System.Windows.MessageBoxButton.YesNo);
                    if (delMC2Box == MessageBoxResult.Yes)
                    {
                        Object obMC2Path = keyMC2.GetValue("InstallPath");
                        if (obMC2Path != null)
                        {
                            mc2dir = (obMC2Path as String);

                            try
                            {
                                Directory.Delete(mc2dir, true);
                                keyMC2.SetValue("Installed", "0");
                                keyMC2.Close();
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show("Minecraft 2 has been successfully deleted!", "Minecraft 2");
                            }
                            catch (Exception ex)
                            {
                                keyMC2.Close();
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show($"Error deleting Minecraft 2: {ex}");
                            }
                        }
                    }
                }
            }
        }

        // Minecraft 2 Remake

        private void PlayMC2R_Click(object sender, RoutedEventArgs e)
        {
            if (StatusR == MC2RStatus.downloading)
            {
                return;
            }

            RegistryKey keyMC2R = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r", true);
            Object obMC2RInstalled = keyMC2R.GetValue("Installed", null);

            if (MainWindow.offlineMode == true)
            {
                Object obMC2RPath = keyMC2R.GetValue("InstallPath");
                if (obMC2RPath != null)
                {
                    mc2rdir = (obMC2RPath as String);
                    mc2r = Path.Combine(mc2dir, "Minecraft2Remake.exe");
                }

                if (File.Exists(mc2))
                {
                    StartMC2();
                    return;
                }
                else
                {
                    MessageBox.Show("Please launch Minecraft Parody Launcher in online mode to install Minecraft 2.");
                    return;
                }
            }

            keyMC2R.Close();
            if (obMC2RInstalled != null)
            {
                mc2rInstalled = (obMC2RInstalled as String);

                if (mc2rInstalled == "1")
                {
                    CheckForUpdatesMC2R();
                }
                else
                {
                    InstallMC2R();
                }
            }
            else
            {
                InstallMC2R();
            }
        }

        private void StartMC2R()
        {
            using (RegistryKey keyMC2R = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r"))
            {
                if (keyMC2R != null)
                {
                    Object obMC2RPath = keyMC2R.GetValue("InstallPath");
                    if (obMC2RPath != null)
                    {
                        mc2rdir = (obMC2RPath as String);
                        mc2r = Path.Combine(mc2rdir, "Minecraft2Remake.exe");
                        keyMC2R.Close();
                        try
                        {
                            Process.Start(mc2r);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error launching Minecraft 2 Remake: {ex}");
                        }
                    }
                }
            }
        }

        private void InstallMC2R()
        {
            if (Status == MC2Status.downloading)
            {
                MessageBox.Show("Please wait until your download finishes before starting another one.");
                return;
            }

            WebClient webClient = new WebClient();
            string mc2rSize = webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC2R/size.txt");

            MessageBoxResult mc2rInstallConfirm = System.Windows.MessageBox.Show($"Minecraft 2 Remake requires {mc2rSize}Do you want to continue?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
            if (mc2rInstallConfirm == MessageBoxResult.Yes)
            {
                RegistryKey keyGames = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games", true);
                keyGames.CreateSubKey("mc2r");
                RegistryKey keyMC2R = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r", true);
                keyGames.Close();

                MessageBoxResult mc2rInstallLocationMB = System.Windows.MessageBox.Show($"Would you like to install Minecraft 2 Remake at {rootPath}\\games", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                if (mc2rInstallLocationMB == MessageBoxResult.Yes)
                {
                    keyMC2R.SetValue("InstallPath", $"{rootPath}\\games\\Minecraft 2 Remake");
                    keyMC2R.Close();
                    mc2rdir = Path.Combine(rootPath, "games", "Minecraft 2 Remake");
                    DownloadMC2R();
                }
                if (mc2rInstallLocationMB == MessageBoxResult.No)
                {
                    WinForms.FolderBrowserDialog mc2rFolderDialog = new WinForms.FolderBrowserDialog();
                    mc2rFolderDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    mc2rFolderDialog.Description = "Please select where you would like to install Minecraft 2 Remake, a folder called \"Minecraft 2 Remake\" will be created.";
                    mc2rFolderDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc2rResult = mc2rFolderDialog.ShowDialog();

                    if (mc2rResult == WinForms.DialogResult.OK)
                    {
                        mc2rdir = Path.Combine(mc2rFolderDialog.SelectedPath, "Minecraft 2 Remake");
                        keyMC2R.SetValue("InstallPath", mc2rdir);
                        keyMC2R.Close();
                        DownloadMC2R();
                    }
                }

                keyMC2R.Close();
            }
        }

        private void DownloadMC2R()
        {
            DownloadWarning();
            CreateTemp();
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

            try
            {
                StatusR = MC2RStatus.downloading;

                DLProgress.IsIndeterminate = false;
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC2RCompletedCallback);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri("https://www.dropbox.com/s/753i22zdihth5fi/mc2r.zip?dl=1"), mc2rzip);
            }
            catch (Exception ex)
            {
                StatusR = MC2RStatus.failed;
                MessageBox.Show($"Error downloading Minecraft 2 Remake: {ex}");
            }
        }

        private void DownloadMC2RCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            RegistryKey keyMC2R = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r", true);
            keyMC2R.SetValue("Installed", "1");
            keyMC2R.Close();
            ExtractZipAsyncMC2R(mc2rzip, mc2rdir);
        }

        private void CheckForUpdatesMC2R()
        {
            StatusR = MC2RStatus.checkUpdate;

            try
            {
                using (RegistryKey keyMC2R = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r"))
                {
                    if (keyMC2R != null)
                    {
                        Object obMC2RPath = keyMC2R.GetValue("InstallPath");
                        if (obMC2RPath != null)
                        {
                            mc2rdir = (obMC2RPath as String);
                            mc2rver = Path.Combine(mc2rdir, "version.txt");
                            keyMC2R.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                StatusR = MC2RStatus.failed;
                MessageBox.Show($"Error: {ex}");
            }

            if (File.Exists(mc2rver))
            {
                Version localVersionMC2R = new Version(File.ReadAllText(mc2rver));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersionMC2R = new Version(webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC2R/version.txt"));

                    if (onlineVersionMC2R.IsDifferentThan(localVersionMC2R))
                    {
                        InstallUpdateMC2R(true, onlineVersionMC2R);
                    }
                    else
                    {
                        StatusR = MC2RStatus.ready;
                        StartMC2R();
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

                    CreateTemp();

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
                                MessageBox.Show($"Error deleting zip: {ex}");
                            }
                        }

                        DLProgress.Visibility = Visibility.Visible;

                        try
                        {
                            StatusR = MC2RStatus.downloading;

                            DLProgress.IsIndeterminate = false;
                            WebClient webClient = new WebClient();
                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC2RCompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri("https://www.dropbox.com/s/753i22zdihth5fi/mc2r.zip?dl=1"), mc2zip);
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
                    StartMC2R();
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                StatusR = MC2RStatus.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void UpdateMC2RCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            ExtractZipAsyncMC2R(mc2rzip, mc2rdir);
        }

        private void PlayMC2R_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            using (RegistryKey keyMC2R = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r", true))
            {
                if (keyMC2R != null)
                {
                    Object obMC2RInstall = keyMC2R.GetValue("Installed");
                    mc2rInstalled = (obMC2RInstall as String);

                    if (mc2rInstalled != "1")
                    {
                        keyMC2R.Close();
                        return;
                    }

                    MessageBoxResult delMC2RBox = System.Windows.MessageBox.Show("Are you sure you want to delete Minecraft 2 Remake?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                    if (delMC2RBox == MessageBoxResult.Yes)
                    {
                        Object obMC2RPath = keyMC2R.GetValue("InstallPath");
                        if (obMC2RPath != null)
                        {
                            mc2rdir = (obMC2RPath as String);

                            try
                            {
                                Directory.Delete(mc2rdir, true);
                                keyMC2R.SetValue("Installed", "0");
                                keyMC2R.Close();
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show("Minecraft 2 Remake has been successfully deleted!", "Minecraft 2 Remake");
                            }
                            catch (Exception ex)
                            {
                                keyMC2R.Close();
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show($"Error deleting Minecraft 2 Remake: {ex}");
                            }
                        }
                    }
                }
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DLProgress.Value = e.ProgressPercentage;
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

        private void MC2Logo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://killo445.itch.io/minecraft-2");
        }
    }
}
