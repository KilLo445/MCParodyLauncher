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
using Microsoft.Toolkit.Uwp.Notifications;
using WinForms = System.Windows.Forms;
using System.Threading.Tasks;
using Wsh = IWshRuntimeLibrary;

namespace MCParodyLauncher.MVVM.View
{
    enum MC2RStatus
    {
        ready,
        noInstall,
        checkUpdate,
        update,
        failed,
        unzip,
        downloading
    }
    public partial class Minecraft2View : UserControl
    {
        private string mc2rlink = "https://www.dropbox.com/s/753i22zdihth5fi/mc2r.zip?dl=1";
        private string verLink = "https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC2R/version.txt";
        private string sizeLink = "https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC2R/size.txt";

        // Paths
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string gamesPath;
        private string mc2rdir;

        // Files
        private string mc2r;
        private string mc2rver;
        private string mc2rzip;

        // Settings
        string mc2rInstalled;
        public static bool downloadActive = false;

        private MC2RStatus _status;

        internal MC2RStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case MC2RStatus.ready:
                        PlayMC2.Content = "Play";
                        downloadActive = false;
                        break;
                    case MC2RStatus.noInstall:
                        PlayMC2.Content = "Download";
                        downloadActive = false;
                        break;
                    case MC2RStatus.failed:
                        PlayMC2.Content = "Error";
                        downloadActive = false;
                        break;
                    case MC2RStatus.downloading:
                        PlayMC2.Content = "Downloading";
                        downloadActive = true;
                        break;
                    case MC2RStatus.unzip:
                        PlayMC2.Content = "Installing";
                        downloadActive = true;
                        break;
                    case MC2RStatus.update:
                        PlayMC2.Content = "Updating";
                        downloadActive = true;
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

            mc2rzip = Path.Combine(mcplTempPath, "mc2r.zip");

            CheckInst();
        }

        private void CheckInst()
        {
            RegistryKey keyMC2R = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r", true);
            Object obMC2RInstalled = keyMC2R.GetValue("Installed", null);

            if (obMC2RInstalled != null)
            {
                string MC2RInstalled = (obMC2RInstalled as String);

                if (MC2RInstalled != "0")
                {
                    Object obMC2RPath = keyMC2R.GetValue("InstallPath");
                    if (obMC2RPath != null)
                    {
                        mc2rdir = (obMC2RPath as String);
                        keyMC2R.Close();
                    }

                    keyMC2R.Close();
                    Status = MC2RStatus.ready;
                }
                else
                {
                    keyMC2R.Close();
                    Status = MC2RStatus.noInstall;
                }
            }
            else
            {
                keyMC2R.Close();
                Status = MC2RStatus.noInstall;
            }
        }

        private void CreateTemp()
        {
            Directory.CreateDirectory(mcplTempPath);
        }

        private void PlayMC2R_Click(object sender, RoutedEventArgs e)
        {
            if (Status == MC2RStatus.downloading)
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
                    mc2r = Path.Combine(mc2rdir, "Minecraft2Remake.exe");
                }

                if (File.Exists(mc2r))
                {
                    StartMC2R();
                    return;
                }
                else
                {
                    MessageBox.Show("Please launch Minecraft Parody Launcher in online mode to install Minecraft 2 Remake.");
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
            WebClient webClient = new WebClient();
            string mc2rSize = webClient.DownloadString(sizeLink);

            MessageBoxResult mc2rInstallConfirm = System.Windows.MessageBox.Show($"Minecraft 2 Remake requires {mc2rSize}Do you want to continue?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
            if (mc2rInstallConfirm == MessageBoxResult.Yes)
            {
                RegistryKey keyGames = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games", true);
                keyGames.CreateSubKey("mc2r");
                RegistryKey keyMC2R = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r", true);
                keyGames.Close();

                MessageBoxResult mc2rInstallLocationMB = System.Windows.MessageBox.Show($"Would you like to install Minecraft 2 Remake at the default path?\n\n{rootPath}\\games", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                if (mc2rInstallLocationMB == MessageBoxResult.Yes)
                {
                    keyMC2R.SetValue("InstallPath", $"{rootPath}\\games\\Minecraft 2 Remake");
                    keyMC2R.Close();
                    mc2rdir = Path.Combine(rootPath, "games", "Minecraft 2 Remake");
                    DownloadMC2R();
                }
                if (mc2rInstallLocationMB == MessageBoxResult.No)
                {
                    MessageBox.Show("Please select where you would like to install Minecraft 2 Remake, a folder called \"Minecraft 2 Remake\" will be created.", "Minecraft 2 Remake", MessageBoxButton.OK, MessageBoxImage.Information);
                    WinForms.FolderBrowserDialog mc2rFolderDialog = new WinForms.FolderBrowserDialog();
                    mc2rFolderDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
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
            }
        }

        private void DownloadMC2R()
        {
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
                    Status = MC2RStatus.failed;
                    MessageBox.Show($"Error deleting zip: {ex}");
                }
            }

            DLProgress.Visibility = Visibility.Visible;

            try
            {
                Status = MC2RStatus.downloading;

                DLProgress.IsIndeterminate = false;
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC2RCompletedCallback);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri(mc2rlink), mc2rzip);
            }
            catch (Exception ex)
            {
                Status = MC2RStatus.failed;
                MessageBox.Show($"Error downloading Minecraft 2 Remake: {ex}");
            }
        }

        private void DownloadMC2RCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            RegistryKey keyMC2R = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r", true);
            keyMC2R.SetValue("Installed", "1");
            keyMC2R.Close();
            ExtractZipAsync(mc2rzip, mc2rdir);
        }

        private void CheckForUpdatesMC2R()
        {
            Status = MC2RStatus.checkUpdate;

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
                Status = MC2RStatus.failed;
                MessageBox.Show($"Error: {ex}");
            }

            if (File.Exists(mc2rver))
            {
                Version localVersionMC2R = new Version(File.ReadAllText(mc2rver));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersionMC2R = new Version(webClient.DownloadString(verLink));

                    if (onlineVersionMC2R.IsDifferentThan(localVersionMC2R))
                    {
                        InstallUpdateMC2R(true, onlineVersionMC2R);
                    }
                    else
                    {
                        Status = MC2RStatus.ready;
                        StartMC2R();
                    }
                }
                catch (Exception ex)
                {
                    Status = MC2RStatus.failed;
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
                    Status = MC2RStatus.update;

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
                                Status = MC2RStatus.failed;
                                MessageBox.Show($"Error deleting zip: {ex}");
                            }
                        }

                        DLProgress.Visibility = Visibility.Visible;

                        try
                        {
                            Status = MC2RStatus.downloading;

                            DLProgress.IsIndeterminate = false;
                            WebClient webClient = new WebClient();
                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC2RCompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri(mc2rlink), mc2rzip);
                        }
                        catch (Exception ex)
                        {
                            Status = MC2RStatus.failed;
                            MessageBox.Show($"Error updating Minecraft 2 Remake: {ex}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Status = MC2RStatus.failed;
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
                Status = MC2RStatus.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void UpdateMC2RCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            ExtractZipAsync(mc2rzip, mc2rdir);
        }

        private async Task ExtractZipAsync(string zipfile, string output)
        {
            try
            {
                Status = MC2RStatus.unzip;
                DLProgress.IsIndeterminate = true;
                await Task.Run(() => ZipFile.ExtractToDirectory(zipfile, output));
                File.Delete(mc2rzip);
                Status = MC2RStatus.ready;
                DLProgress.Visibility = Visibility.Hidden;
                if (MainWindow.usNotifications == true)
                {
                    new ToastContentBuilder()
                    .AddText("Download complete!")
                    .AddText("Minecraft 2 Remake has finished downloading.")
                    .Show();
                }
                return;
            }
            catch (Exception ex)
            {
                Status = MC2RStatus.failed;
                MessageBox.Show($"Error Updating Minecraft 2 Remake: {ex}");
                return;
            }
        }

        private void DesktopShortcut_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r"))
            {
                if (keyMC2 != null)
                {
                    Object obMC2Install = keyMC2.GetValue("Installed");
                    mc2rInstalled = (obMC2Install as String);

                    if (mc2rInstalled == "1")
                    {
                        object shDesktop = (object)"Desktop";
                        Wsh.WshShell shell = new Wsh.WshShell();
                        string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Minecraft 2 Remake.lnk";
                        Wsh.IWshShortcut shortcut = (Wsh.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                        shortcut.TargetPath = mc2rdir + "\\Minecraft2Remake.exe";
                        shortcut.Save();

                        keyMC2.Close();
                    }
                    else
                    {
                        MessageBox.Show("Minecraft 2 Remake does not seem to be installed.");
                        keyMC2.Close();
                    }
                }
            }
        }

        private void FileLocation_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r"))
            {
                if (keyMC2 != null)
                {
                    Object obMC2Install = keyMC2.GetValue("Installed");
                    mc2rInstalled = (obMC2Install as String);

                    if (mc2rInstalled == "1")
                    {
                        Object obMC2Path = keyMC2.GetValue("InstallPath");
                        if (obMC2Path != null)
                        {
                            mc2rdir = (obMC2Path as String);
                            keyMC2.Close();

                            try
                            {
                                Process.Start(new ProcessStartInfo { FileName = mc2rdir, UseShellExecute = true });
                            }
                            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Minecraft 2 Remake does not seem to be installed.");
                        keyMC2.Close();
                    }
                }
            }
        }

        private void SelectLocation_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r", true))
            {
                if (keyMC2 != null)
                {
                    MessageBox.Show("Please select the folder that containes \"Minecraft2Remake.exe\".", "Minecraft 2 Remake", MessageBoxButton.OK, MessageBoxImage.Information);
                    WinForms.FolderBrowserDialog selectInstallDialog = new WinForms.FolderBrowserDialog();
                    selectInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    selectInstallDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc2rResult = selectInstallDialog.ShowDialog();

                    if (mc2rResult == WinForms.DialogResult.OK)
                    {
                        string _mc2rResult = Path.Combine(selectInstallDialog.SelectedPath, "Minecraft2Remake.exe");
                        if (!File.Exists(_mc2rResult))
                        {
                            SystemSounds.Exclamation.Play();
                            MessageBox.Show("Please select the location with Minecraft2Remake.exe");
                            return;
                        }

                        mc2rdir = Path.Combine(selectInstallDialog.SelectedPath);
                        keyMC2.SetValue("InstallPath", mc2rdir);
                        keyMC2.SetValue("Installed", "1");
                        keyMC2.Close();
                        Status = MC2RStatus.ready;
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
            using (RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r", true))
            {
                if (keyMC2 != null)
                {
                    Object obMC2Install = keyMC2.GetValue("Installed");
                    mc2rInstalled = (obMC2Install as String);

                    if (mc2rInstalled != "1")
                    {
                        MessageBox.Show("Minecraft 2 Remake does not seem to be installed.");
                        keyMC2.Close();
                        return;
                    }

                    MessageBox.Show("Please select where you would like to move Minecraft 2 Remake to, a folder called \"Minecraft 2 Remake\" will be created.", "Minecraft 2 Remake", MessageBoxButton.OK, MessageBoxImage.Information);
                    WinForms.FolderBrowserDialog moveInstallDialog = new WinForms.FolderBrowserDialog();
                    moveInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    moveInstallDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc2rResult = moveInstallDialog.ShowDialog();

                    if (mc2rResult == WinForms.DialogResult.OK)
                    {
                        string dirOld = mc2rdir;
                        mc2rdir = Path.Combine(moveInstallDialog.SelectedPath, "Minecraft 2 Remake");
                        keyMC2.SetValue("InstallPath", mc2rdir);
                        keyMC2.Close();

                        string dirOldChar = dirOld.Substring(0, 1);
                        string mc2rdirChar = mc2rdir.Substring(0, 1);
                        bool sameVolume = dirOldChar.Equals(mc2rdirChar);

                        if (sameVolume == true)
                        {
                            try
                            {
                                Directory.Move(dirOld, mc2rdir);
                                MessageBox.Show("Install has been moved!", "Minecraft 2 Remake", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error Moving Minecraft 2 Remake: {ex}");
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                Directory.CreateDirectory(mc2rdir);

                                foreach (string dirPath in Directory.GetDirectories(dirOld, "*", SearchOption.AllDirectories))
                                {
                                    Directory.CreateDirectory(dirPath.Replace(dirOld, mc2rdir));
                                }
                                foreach (string newPath in Directory.GetFiles(dirOld, "*.*", SearchOption.AllDirectories))
                                {
                                    File.Copy(newPath, newPath.Replace(dirOld, mc2rdir), true);
                                }
                                Directory.Delete(dirOld, true );
                                MessageBox.Show("Install has been moved!", "Minecraft 2 Remake", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error Moving Minecraft 2 Remake: {ex}");
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
            using (RegistryKey keyMC2 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc2r", true))
            {
                if (keyMC2 != null)
                {
                    Object obMC2Install = keyMC2.GetValue("Installed");
                    mc2rInstalled = (obMC2Install as String);

                    if (mc2rInstalled != "1")
                    {
                        MessageBox.Show("Minecraft 2 Remake does not seem to be installed.");
                        keyMC2.Close();
                        return;
                    }

                    MessageBoxResult delMC2Box = System.Windows.MessageBox.Show("Are you sure you want to uninstall Minecraft 2 Remake?", "Minecraft 2 Remake", System.Windows.MessageBoxButton.YesNo);
                    if (delMC2Box == MessageBoxResult.Yes)
                    {
                        Object obMC2Path = keyMC2.GetValue("InstallPath");
                        if (obMC2Path != null)
                        {
                            mc2rdir = (obMC2Path as String);

                            try
                            {
                                Directory.Delete(mc2rdir, true);
                                keyMC2.SetValue("Installed", "0");
                                keyMC2.Close();
                                Status = MC2RStatus.noInstall;
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show("Minecraft 2 Remake has been successfully uninstalled!", "Minecraft 2 Remake");
                            }
                            catch (Exception ex)
                            {
                                keyMC2.Close();
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show($"Error uninstalling Minecraft 2 Remake: {ex}");
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
            Process.Start(new ProcessStartInfo("https://killo445.itch.io/minecraft-2-remake") { UseShellExecute = true });
        }
    }
}
