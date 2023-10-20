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
using Wsh = IWshRuntimeLibrary;
using Microsoft.Toolkit.Uwp.Notifications;

namespace MCParodyLauncher.MVVM.View
{
    enum MC5Status
    {
        ready,
        noInstall,
        checkUpdate,
        update,
        failed,
        unzip,
        downloading
    }

    public partial class Minecraft5View : UserControl
    {
        public static string GameName = "Minecraft 5";

        private string mc5link = "https://www.dropbox.com/s/6b06sm6ttwuljqs/mc5.zip?dl=1";
        private string verLink = "https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Games/MC5/version.txt";
        private string sizeLink = "https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Games/MC5/size.txt";

        // Paths
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string gamesPath;
        private string mc5dir;

        // Files
        private string mc5;
        private string mc5ver;
        private string mc5zip;

        // Settings
        string mc5Installed;
        public static bool downloadActive = false;

        private MC5Status _status;

        private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        internal MC5Status Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case MC5Status.ready:
                        PlayMC5.Content = "Play";
                        downloadActive = false;
                        break;
                    case MC5Status.noInstall:
                        PlayMC5.Content = "Download";
                        downloadActive = false;
                        break;
                    case MC5Status.failed:
                        PlayMC5.Content = "Error";
                        downloadActive = false;
                        break;
                    case MC5Status.downloading:
                        PlayMC5.Content = "Downloading";
                        downloadActive = true;
                        break;
                    case MC5Status.unzip:
                        PlayMC5.Content = "Installing";
                        downloadActive = true;
                        break;
                    case MC5Status.update:
                        PlayMC5.Content = "Updating";
                        downloadActive = true;
                        break;
                }
            }
        }
        public Minecraft5View()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");

            mc5zip = Path.Combine(mcplTempPath, "mc5.zip");

            CheckInst();
        }

        private void CheckInst()
        {
            RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5", true);
            Object obMC5Installed = keyMC5.GetValue("Installed", null);

            if (obMC5Installed != null)
            {
                string MC5Installed = (obMC5Installed as String);

                if (MC5Installed != "0")
                {
                    Object obMC5Path = keyMC5.GetValue("InstallPath");
                    if (obMC5Path != null)
                    {
                        mc5dir = (obMC5Path as String);
                        keyMC5.Close();
                    }

                    keyMC5.Close();
                    Status = MC5Status.ready;
                }
                else
                {
                    keyMC5.Close();
                    Status = MC5Status.noInstall;
                }
            }
            else
            {
                keyMC5.Close();
                Status = MC5Status.noInstall;
            }
        }

        private void CreateTemp()
        {
            Directory.CreateDirectory(mcplTempPath);
        }

        private void PlayMC5_Click(object sender, RoutedEventArgs e)
        {
            if (Status == MC5Status.downloading)
            {
                return;
            }

            RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5", true);
            Object obMC5Installed = keyMC5.GetValue("Installed", null);

            if (MainWindow.offlineMode == true)
            {
                Object obMC5Path = keyMC5.GetValue("InstallPath");
                if (obMC5Path != null)
                {
                    mc5dir = (obMC5Path as String);
                    mc5 = Path.Combine(mc5dir, "Minecraft4.exe");
                    if (File.Exists(mc5))
                    {
                        StartMC5();
                        return;
                    }

                }
                else
                {
                    MessageBox.Show("Please launch Minecraft Parody Launcher in online mode to install Minecraft 5.");
                    return;
                }
            }

            keyMC5.Close();
            if (obMC5Installed != null)
            {
                mc5Installed = (obMC5Installed as String);

                if (mc5Installed == "1")
                {
                    CheckForUpdatesMC5();
                }
                else
                {
                    InstallMC5();
                }
            }
            else
            {
                InstallMC5();
            }
        }

        private void StartMC5()
        {
            using (RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5"))
            {
                if (keyMC5 != null)
                {
                    Object obMC5Path = keyMC5.GetValue("InstallPath");
                    if (obMC5Path != null)
                    {
                        mc5dir = (obMC5Path as String);
                        mc5 = Path.Combine(mc5dir, "MC5.exe");
                        keyMC5.Close();
                        try
                        {
                            Process.Start(mc5);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error launching Minecraft 5: {ex}");
                        }
                    }
                }
            }
        }

        private async void InstallMC5()
        {
            InstallGame.installConfirmed = false;
            InstallGame.installCanceled = false;

            WebClient webClient = new WebClient();
            string mc5Size = webClient.DownloadString(sizeLink);

            MessageBoxResult mc5InstallConfirm = System.Windows.MessageBox.Show($"Minecraft 5 requires {mc5Size}Do you want to continue?", "Minecraft 5", System.Windows.MessageBoxButton.YesNo);
            if (mc5InstallConfirm == MessageBoxResult.Yes)
            {
                RegistryKey keyGames = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games", true);
                keyGames.CreateSubKey("mc5");
                RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5", true);
                keyGames.Close();

                InstallGame installWindow = new InstallGame("Minecraft 5");
                installWindow.Show();
                PlayMC5.IsEnabled = false;
                downloadActive = true;
                while (InstallGame.installConfirmed == false)
                {
                    if (InstallGame.installCanceled == true) { PlayMC5.IsEnabled = true; downloadActive = false; return; }
                    await Task.Delay(100);
                }
                PlayMC5.IsEnabled = true;
                downloadActive = false;

                mc5dir = Path.Combine(InstallGame.InstallPath, "Minecraft 5");
                keyMC5.SetValue("InstallPath", mc5dir);
                keyMC5.Close();

                DownloadMC5();
            }
        }

        private void DownloadMC5()
        {
            CreateTemp();
            Directory.CreateDirectory(mc5dir);

            if (File.Exists(mc5zip))
            {
                try
                {
                    File.Delete(mc5zip);
                }
                catch (Exception ex)
                {
                    Status = MC5Status.failed;
                    MessageBox.Show($"Error deleting zip: {ex}");
                }
            }

            DLProgress.Visibility = Visibility.Visible;

            try
            {
                Status = MC5Status.downloading;

                DLProgress.IsIndeterminate = false;
                WebClient webClient = new WebClient();
                lblProgress.Visibility = Visibility.Visible;
                stopwatch.Start();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC5CompletedCallback);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri(mc5link), mc5zip);
            }
            catch (Exception ex)
            {
                Status = MC5Status.failed;
                MessageBox.Show($"Error downloading Minecraft 5: {ex}");
            }
        }

        private void DownloadMC5CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            stopwatch.Reset();
            lblProgress.Visibility = Visibility.Hidden;
            RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5", true);
            keyMC5.SetValue("Installed", "1");
            keyMC5.Close();
            ExtractZipAsync(mc5zip, mc5dir);
        }

        private void CheckForUpdatesMC5()
        {
            Status = MC5Status.checkUpdate;

            try
            {
                using (RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5"))
                {
                    if (keyMC5 != null)
                    {
                        Object obMC5Path = keyMC5.GetValue("InstallPath");
                        if (obMC5Path != null)
                        {
                            mc5dir = (obMC5Path as String);
                            mc5ver = Path.Combine(mc5dir, "version.txt");
                            keyMC5.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                Status = MC5Status.failed;
                MessageBox.Show($"Error: {ex}");
            }

            if (File.Exists(mc5ver))
            {
                Version localVersionMC5 = new Version(File.ReadAllText(mc5ver));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersionMC5 = new Version(webClient.DownloadString(verLink));

                    if (onlineVersionMC5.IsDifferentThan(localVersionMC5))
                    {
                        InstallUpdateMC5(true, onlineVersionMC5);
                    }
                    else
                    {
                        Status = MC5Status.ready;
                        StartMC5();
                    }
                }
                catch (Exception ex)
                {
                    Status = MC5Status.failed;
                    MessageBox.Show($"Error checking for updates: {ex}");
                }
            }
            else
            {
                InstallUpdateMC5(false, Version.zero);
            }
        }

        private void InstallUpdateMC5(bool isUpdate, Version _onlineVersionMC5)
        {
            try
            {
                MessageBoxResult messageBoxResultMC5Update = System.Windows.MessageBox.Show("An update for Minecraft 5 has been found! Would you like to download it?", "Minecraft 5", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResultMC5Update == MessageBoxResult.Yes)
                {
                    Status = MC5Status.update;

                    CreateTemp();

                    try
                    {
                        Directory.Delete(mc5dir, true);

                        Directory.CreateDirectory(mc5dir);

                        if (File.Exists(mc5zip))
                        {
                            try
                            {
                                File.Delete(mc5zip);
                            }
                            catch (Exception ex)
                            {
                                Status = MC5Status.failed;
                                MessageBox.Show($"Error deleting zip: {ex}");
                            }
                        }

                        DLProgress.Visibility = Visibility.Visible;

                        try
                        {
                            Status = MC5Status.downloading;

                            DLProgress.IsIndeterminate = false;
                            WebClient webClient = new WebClient();
                            lblProgress.Visibility = Visibility.Visible;
                            stopwatch.Start();
                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC5CompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri(mc5link), mc5zip);
                        }
                        catch (Exception ex)
                        {
                            Status = MC5Status.failed;
                            MessageBox.Show($"Error updating Minecraft 5: {ex}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        Status = MC5Status.failed;
                        MessageBox.Show($"Error updating Minecraft 5: {ex}");
                    }
                }
                else
                {
                    StartMC5();
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                Status = MC5Status.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void UpdateMC5CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            stopwatch.Reset();
            lblProgress.Visibility = Visibility.Hidden;
            ExtractZipAsync(mc5zip, mc5dir);
        }

        private async Task ExtractZipAsync(string zipfile, string output)
        {
            try
            {
                Status = MC5Status.unzip;
                DLProgress.IsIndeterminate = true;
                await Task.Run(() => ZipFile.ExtractToDirectory(zipfile, output));
                File.Delete(mc5zip);
                Status = MC5Status.ready;
                DLProgress.Visibility = Visibility.Hidden;
                if (MainWindow.usNotifications == true)
                {
                    new ToastContentBuilder()
                    .AddText("Download complete!")
                    .AddText("Minecraft 5 has finished downloading.")
                    .Show();
                }
                return;
            }
            catch (Exception ex)
            {
                Status = MC5Status.failed;
                MessageBox.Show($"Error Updating Minecraft 5: {ex}");
                return;
            }
        }

        private void DesktopShortcut_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5"))
            {
                if (keyMC5 != null)
                {
                    Object obMC5Install = keyMC5.GetValue("Installed");
                    mc5Installed = (obMC5Install as String);

                    if (mc5Installed == "1")
                    {
                        object shDesktop = (object)"Desktop";
                        Wsh.WshShell shell = new Wsh.WshShell();
                        string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Minecraft 5.lnk";
                        Wsh.IWshShortcut shortcut = (Wsh.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                        shortcut.TargetPath = mc5dir + "\\MC5.exe";
                        shortcut.IconLocation = mc5dir + "\\www\\icon\\icon.ico";
                        shortcut.Save();

                        keyMC5.Close();
                    }
                    else
                    {
                        MessageBox.Show("Minecraft 5 does not seem to be installed.");
                        keyMC5.Close();
                    }
                }
            }
        }

        private void FileLocation_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5"))
            {
                if (keyMC5 != null)
                {
                    Object obMC5Install = keyMC5.GetValue("Installed");
                    mc5Installed = (obMC5Install as String);

                    if (mc5Installed == "1")
                    {
                        Object obMC5Path = keyMC5.GetValue("InstallPath");
                        if (obMC5Path != null)
                        {
                            mc5dir = (obMC5Path as String);
                            keyMC5.Close();

                            try
                            {
                                Process.Start(new ProcessStartInfo { FileName = mc5dir, UseShellExecute = true });
                            }
                            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Minecraft 5 does not seem to be installed.");
                        keyMC5.Close();
                    }
                }
            }
        }

        private void SelectLocation_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5", true))
            {
                if (keyMC5 != null)
                {
                    MessageBox.Show("Please select the folder that containes \"MC5.exe\".", "Minecraft 5", MessageBoxButton.OK, MessageBoxImage.Information);
                    WinForms.FolderBrowserDialog selectInstallDialog = new WinForms.FolderBrowserDialog();
                    selectInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    selectInstallDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc5Result = selectInstallDialog.ShowDialog();

                    if (mc5Result == WinForms.DialogResult.OK)
                    {
                        string _mc5Result = Path.Combine(selectInstallDialog.SelectedPath, "MC5.exe");
                        if (!File.Exists(_mc5Result))
                        {
                            SystemSounds.Exclamation.Play();
                            MessageBox.Show("Please select the location with MC5.exe");
                            return;
                        }

                        mc5dir = Path.Combine(selectInstallDialog.SelectedPath);
                        keyMC5.SetValue("InstallPath", mc5dir);
                        keyMC5.SetValue("Installed", "1");
                        keyMC5.Close();
                        Status = MC5Status.ready;
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private void MoveInstall_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5", true))
            {
                if (keyMC5 != null)
                {
                    Object obMC5Install = keyMC5.GetValue("Installed");
                    mc5Installed = (obMC5Install as String);

                    if (mc5Installed != "1")
                    {
                        MessageBox.Show("Minecraft 5 does not seem to be installed.");
                        keyMC5.Close();
                        return;
                    }

                    MessageBox.Show("Please select where you would like to move Minecraft 5 to, a folder called \"Minecraft 5\" will be created.", "Minecraft 5", MessageBoxButton.OK, MessageBoxImage.Information);
                    WinForms.FolderBrowserDialog moveInstallDialog = new WinForms.FolderBrowserDialog();
                    moveInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    moveInstallDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc5Result = moveInstallDialog.ShowDialog();

                    if (mc5Result == WinForms.DialogResult.OK)
                    {
                        string dirOld = mc5dir;
                        mc5dir = Path.Combine(moveInstallDialog.SelectedPath, "Minecraft 5");
                        keyMC5.SetValue("InstallPath", mc5dir);
                        keyMC5.Close();

                        string dirOldChar = dirOld.Substring(0, 1);
                        string mc5dirChar = mc5dir.Substring(0, 1);
                        bool sameVolume = dirOldChar.Equals(mc5dirChar);

                        if (sameVolume == true)
                        {
                            try
                            {
                                Directory.Move(dirOld, mc5dir);
                                MessageBox.Show("Install has been moved!", "Minecraft 5", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error Moving Minecraft 5: {ex}");
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                Directory.CreateDirectory(mc5dir);

                                foreach (string dirPath in Directory.GetDirectories(dirOld, "*", SearchOption.AllDirectories))
                                {
                                    Directory.CreateDirectory(dirPath.Replace(dirOld, mc5dir));
                                }
                                foreach (string newPath in Directory.GetFiles(dirOld, "*.*", SearchOption.AllDirectories))
                                {
                                    File.Copy(newPath, newPath.Replace(dirOld, mc5dir), true);
                                }
                                Directory.Delete(dirOld, true);
                                MessageBox.Show("Install has been moved!", "Minecraft 5", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error Moving Minecraft 5: {ex}");
                                return;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC5 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc5", true))
            {
                if (keyMC5 != null)
                {
                    Object obMC5Install = keyMC5.GetValue("Installed");
                    mc5Installed = (obMC5Install as String);

                    if (mc5Installed != "1")
                    {
                        MessageBox.Show("Minecraft 5 does not seem to be installed.");
                        keyMC5.Close();
                        return;
                    }

                    MessageBoxResult delMC5Box = System.Windows.MessageBox.Show("Are you sure you want to uninstall Minecraft 5?", "Minecraft 5", System.Windows.MessageBoxButton.YesNo);
                    if (delMC5Box == MessageBoxResult.Yes)
                    {
                        Object obMC5Path = keyMC5.GetValue("InstallPath");
                        if (obMC5Path != null)
                        {
                            mc5dir = (obMC5Path as String);

                            try
                            {
                                Directory.Delete(mc5dir, true);
                                keyMC5.SetValue("Installed", "0");
                                keyMC5.Close();
                                Status = MC5Status.noInstall;
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show("Minecraft 5 has been successfully uninstalled!", "Minecraft 5");
                            }
                            catch (Exception ex)
                            {
                                keyMC5.Close();
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show($"Error uninstalling Minecraft 5: {ex}");
                            }
                        }
                    }
                }
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string downloadProgress = e.ProgressPercentage + "%";
            string downloadSpeed = string.Format("{0} MB/s", (e.BytesReceived / 1024.0 / 1024.0 / stopwatch.Elapsed.TotalSeconds).ToString("0.00"));
            string downloadedMBs = Math.Round(e.BytesReceived / 1024.0 / 1024.0) + " MB";
            string totalMBs = Math.Round(e.TotalBytesToReceive / 1024.0 / 1024.0) + " MB";

            string progress = $"{downloadedMBs} / {totalMBs} ({downloadProgress}) ({downloadSpeed})";

            lblProgress.Content = progress;

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

        private void MC5Logo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://decentgamestudio.itch.io/minecraft-5") { UseShellExecute = true });
        }
    }
}
