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
    enum MC4Status
    {
        ready,
        noInstall,
        checkUpdate,
        update,
        failed,
        unzip,
        downloading
    }
    public partial class Minecraft4View : UserControl
    {
        private string mc4link = "https://www.dropbox.com/s/dq5vl127q7gza4r/mc4.zip?dl=1";

        // Paths
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string gamesPath;
        private string mc4dir;

        // Files
        private string mc4;
        private string mc4ver;
        private string mc4zip;

        // Settings
        string mc4Installed;
        public static bool downloadActive = false;

        private MC4Status _status;

        internal MC4Status Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case MC4Status.ready:
                        PlayMC4.Content = "Play";
                        downloadActive = false;
                        break;
                    case MC4Status.noInstall:
                        PlayMC4.Content = "Download";
                        downloadActive = false;
                        break;
                    case MC4Status.failed:
                        PlayMC4.Content = "Error";
                        downloadActive = false;
                        break;
                    case MC4Status.downloading:
                        PlayMC4.Content = "Downloading";
                        downloadActive = true;
                        break;
                    case MC4Status.unzip:
                        PlayMC4.Content = "Installing";
                        downloadActive = true;
                        break;
                    case MC4Status.update:
                        PlayMC4.Content = "Updating";
                        downloadActive = true;
                        break;
                }
            }
        }

        public Minecraft4View()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");

            mc4zip = Path.Combine(mcplTempPath, "mc4.zip");

            CheckInst();
        }

        private void CheckInst()
        {
            RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true);
            Object obMC4Installed = keyMC4.GetValue("Installed", null);

            if (obMC4Installed != null)
            {
                string MC4Installed = (obMC4Installed as String);

                if (MC4Installed != "0")
                {
                    Object obMC4Path = keyMC4.GetValue("InstallPath");
                    if (obMC4Path != null)
                    {
                        mc4dir = (obMC4Path as String);
                        keyMC4.Close();
                    }

                    keyMC4.Close();
                    Status = MC4Status.ready;
                }
                else
                {
                    keyMC4.Close();
                    Status = MC4Status.noInstall;
                }
            }
            else
            {
                keyMC4.Close();
                Status = MC4Status.noInstall;
            }
        }

        private void CreateTemp()
        {
            Directory.CreateDirectory(mcplTempPath);
        }

        private void PlayMC4_Click(object sender, RoutedEventArgs e)
        {
            if (Status == MC4Status.downloading)
            {
                return;
            }

            RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true);
            Object obMC4Installed = keyMC4.GetValue("Installed", null);

            if (MainWindow.offlineMode == true)
            {
                Object obMC4Path = keyMC4.GetValue("InstallPath");
                if (obMC4Path != null)
                {
                    mc4dir = (obMC4Path as String);
                    mc4 = Path.Combine(mc4dir, "Game.exe");
                }

                if (File.Exists(mc4))
                {
                    StartMC4();
                    return;
                }
                else
                {
                    MessageBox.Show("Please launch Minecraft Parody Launcher in online mode to install Minecraft 4.");
                    return;
                }
            }

            keyMC4.Close();
            if (obMC4Installed != null)
            {
                mc4Installed = (obMC4Installed as String);

                if (mc4Installed == "1")
                {
                    CheckForUpdatesMC4();
                }
                else
                {
                    InstallMC4();
                }
            }
            else
            {
                InstallMC4();
            }
        }

        private void StartMC4()
        {
            using (RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4"))
            {
                if (keyMC4 != null)
                {
                    Object obMC4Path = keyMC4.GetValue("InstallPath");
                    if (obMC4Path != null)
                    {
                        mc4dir = (obMC4Path as String);
                        mc4 = Path.Combine(mc4dir, "Minecraft4.exe");
                        keyMC4.Close();
                        try
                        {
                            Process.Start(mc4);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error launching Minecraft 4: {ex}");
                        }
                    }
                }
            }
        }

        private void InstallMC4()
        {
            WebClient webClient = new WebClient();
            string mc4Size = webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC4/size.txt");

            MessageBoxResult mc4InstallConfirm = System.Windows.MessageBox.Show($"Minecraft 4 requires {mc4Size}Do you want to continue?", "Minecraft 4", System.Windows.MessageBoxButton.YesNo);
            if (mc4InstallConfirm == MessageBoxResult.Yes)
            {
                RegistryKey keyGames = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games", true);
                keyGames.CreateSubKey("mc4");
                RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true);
                keyGames.Close();

                MessageBoxResult mc4InstallLocationMB = System.Windows.MessageBox.Show($"Would you like to install Minecraft 4 at {rootPath}\\games", "Minecraft 4", System.Windows.MessageBoxButton.YesNo);
                if (mc4InstallLocationMB == MessageBoxResult.Yes)
                {
                    keyMC4.SetValue("InstallPath", $"{rootPath}\\games\\Minecraft 4");
                    keyMC4.Close();
                    mc4dir = Path.Combine(rootPath, "games", "Minecraft 4");
                    DownloadMC4();
                }
                if (mc4InstallLocationMB == MessageBoxResult.No)
                {
                    WinForms.FolderBrowserDialog mc4FolderDialog = new WinForms.FolderBrowserDialog();
                    mc4FolderDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    mc4FolderDialog.Description = "Please select where you would like to install Minecraft 4, a folder called \"Minecraft 4\" will be created.";
                    mc4FolderDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc4Result = mc4FolderDialog.ShowDialog();

                    if (mc4Result == WinForms.DialogResult.OK)
                    {
                        mc4dir = Path.Combine(mc4FolderDialog.SelectedPath, "Minecraft 4");
                        keyMC4.SetValue("InstallPath", mc4dir);
                        keyMC4.Close();
                        DownloadMC4();
                    }
                }

                keyMC4.Close();
            }
        }

        private void DownloadMC4()
        {
            CreateTemp();
            Directory.CreateDirectory(mc4dir);

            if (File.Exists(mc4zip))
            {
                try
                {
                    File.Delete(mc4zip);
                }
                catch (Exception ex)
                {
                    Status = MC4Status.failed;
                    MessageBox.Show($"Error deleting zip: {ex}");
                }
            }

            DLProgress.Visibility = Visibility.Visible;

            try
            {
                Status = MC4Status.downloading;

                DLProgress.IsIndeterminate = false;
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC4CompletedCallback);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri(mc4link), mc4zip);
            }
            catch (Exception ex)
            {
                Status = MC4Status.failed;
                MessageBox.Show($"Error downloading Minecraft 4: {ex}");
            }
        }

        private void DownloadMC4CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true);
            keyMC4.SetValue("Installed", "1");
            keyMC4.Close();
            ExtractZipAsync(mc4zip, mc4dir);
        }

        private void CheckForUpdatesMC4()
        {
            Status = MC4Status.checkUpdate;

            try
            {
                using (RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4"))
                {
                    if (keyMC4 != null)
                    {
                        Object obMC4Path = keyMC4.GetValue("InstallPath");
                        if (obMC4Path != null)
                        {
                            mc4dir = (obMC4Path as String);
                            mc4ver = Path.Combine(mc4dir, "version.txt");
                            keyMC4.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                Status = MC4Status.failed;
                MessageBox.Show($"Error: {ex}");
            }

            if (File.Exists(mc4ver))
            {
                Version localVersionMC4 = new Version(File.ReadAllText(mc4ver));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersionMC4 = new Version(webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC4/version.txt"));

                    if (onlineVersionMC4.IsDifferentThan(localVersionMC4))
                    {
                        InstallUpdateMC4(true, onlineVersionMC4);
                    }
                    else
                    {
                        Status = MC4Status.ready;
                        StartMC4();
                    }
                }
                catch (Exception ex)
                {
                    Status = MC4Status.failed;
                    MessageBox.Show($"Error checking for updates: {ex}");
                }
            }
            else
            {
                InstallUpdateMC4(false, Version.zero);
            }
        }

        private void InstallUpdateMC4(bool isUpdate, Version _onlineVersionMC4)
        {
            try
            {
                MessageBoxResult messageBoxResultMC4Update = System.Windows.MessageBox.Show("An update for Minecraft 4 has been found! Would you like to download it?", "Minecraft 4", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResultMC4Update == MessageBoxResult.Yes)
                {
                    Status = MC4Status.update;

                    CreateTemp();

                    try
                    {
                        Directory.Delete(mc4dir, true);

                        Directory.CreateDirectory(mc4dir);

                        if (File.Exists(mc4zip))
                        {
                            try
                            {
                                File.Delete(mc4zip);
                            }
                            catch (Exception ex)
                            {
                                Status = MC4Status.failed;
                                MessageBox.Show($"Error deleting zip: {ex}");
                            }
                        }

                        DLProgress.Visibility = Visibility.Visible;

                        try
                        {
                            Status = MC4Status.downloading;

                            DLProgress.IsIndeterminate = false;
                            WebClient webClient = new WebClient();
                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC4CompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri(mc4link), mc4zip);
                        }
                        catch (Exception ex)
                        {
                            Status = MC4Status.failed;
                            MessageBox.Show($"Error updating Minecraft 4: {ex}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show($"Error updating Minecraft 4: {ex}");
                    }
                }
                else
                {
                    StartMC4();
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                Status = MC4Status.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void UpdateMC4CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            ExtractZipAsync(mc4zip, mc4dir);
        }

        private async Task ExtractZipAsync(string zipfile, string output)
        {
            try
            {
                Status = MC4Status.unzip;
                DLProgress.IsIndeterminate = true;
                await Task.Run(() => ZipFile.ExtractToDirectory(zipfile, output));
                File.Delete(mc4zip);
                Status = MC4Status.ready;
                DLProgress.Visibility = Visibility.Hidden;
                new ToastContentBuilder()
                .AddText("Download complete!")
                .AddText("Minecraft 4 has finished downloading.")
                .Show();
                return;
            }
            catch (Exception ex)
            {
                Status = MC4Status.failed;
                MessageBox.Show($"Error Updating Minecraft 4: {ex}");
                return;
            }
        }

        private void DesktopShortcut_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4"))
            {
                if (keyMC4 != null)
                {
                    Object obMC4Install = keyMC4.GetValue("Installed");
                    mc4Installed = (obMC4Install as String);

                    if (mc4Installed == "1")
                    {
                        object shDesktop = (object)"Desktop";
                        Wsh.WshShell shell = new Wsh.WshShell();
                        string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Minecraft 4.lnk";
                        Wsh.IWshShortcut shortcut = (Wsh.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                        shortcut.TargetPath = mc4dir + "\\Minecraft4.exe";
                        shortcut.Save();

                        keyMC4.Close();
                    }
                    else
                    {
                        MessageBox.Show("Minecraft 4 does not seem to be installed.");
                        keyMC4.Close();
                    }
                }
            }
        }

        private void MoveInstall_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true))
            {
                if (keyMC4 != null)
                {
                    Object obMC4Install = keyMC4.GetValue("Installed");
                    mc4Installed = (obMC4Install as String);

                    if (mc4Installed != "1")
                    {
                        MessageBox.Show("Minecraft 4 does not seem to be installed.");
                        keyMC4.Close();
                        return;
                    }

                    WinForms.FolderBrowserDialog moveInstallDialog = new WinForms.FolderBrowserDialog();
                    moveInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    moveInstallDialog.Description = "Please select where you would like to move Minecraft 4 to, a folder called \"Minecraft 4\" will be created.";
                    moveInstallDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc4Result = moveInstallDialog.ShowDialog();

                    if (mc4Result == WinForms.DialogResult.OK)
                    {
                        string dirOld = mc4dir;
                        mc4dir = Path.Combine(moveInstallDialog.SelectedPath, "Minecraft 4");
                        keyMC4.SetValue("InstallPath", mc4dir);
                        keyMC4.Close();

                        string dirOldChar = dirOld.Substring(0, 1);
                        string mc4dirChar = mc4dir.Substring(0, 1);
                        bool sameVolume = dirOldChar.Equals(mc4dirChar);

                        if (sameVolume == true)
                        {
                            try
                            {
                                Directory.Move(dirOld, mc4dir);
                                MessageBox.Show("Install has been moved!", "Minecraft 4", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error Moving Minecraft 4: {ex}");
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                Directory.CreateDirectory(mc4dir);

                                foreach (string dirPath in Directory.GetDirectories(dirOld, "*", SearchOption.AllDirectories))
                                {
                                    Directory.CreateDirectory(dirPath.Replace(dirOld, mc4dir));
                                }
                                foreach (string newPath in Directory.GetFiles(dirOld, "*.*", SearchOption.AllDirectories))
                                {
                                    File.Copy(newPath, newPath.Replace(dirOld, mc4dir), true);
                                }
                                Directory.Delete(dirOld, true);
                                MessageBox.Show("Install has been moved!", "Minecraft 4", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error Moving Minecraft 4: {ex}");
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

        private void FileLocation_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4"))
            {
                if (keyMC4 != null)
                {
                    Object obMC4Install = keyMC4.GetValue("Installed");
                    mc4Installed = (obMC4Install as String);

                    if (mc4Installed == "1")
                    {
                        Object obMC4Path = keyMC4.GetValue("InstallPath");
                        if (obMC4Path != null)
                        {
                            mc4dir = (obMC4Path as String);
                            keyMC4.Close();

                            Process.Start(mc4dir);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Minecraft 4 does not seem to be installed.");
                        keyMC4.Close();
                    }
                }
            }
        }

        private void SelectLocation_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true))
            {
                if (keyMC4 != null)
                {
                    MessageBox.Show("Please select the folder that containes \"Minecraft4.exe\".", "Minecraft 4", MessageBoxButton.OK, MessageBoxImage.Information);
                    WinForms.FolderBrowserDialog selectInstallDialog = new WinForms.FolderBrowserDialog();
                    selectInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    selectInstallDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc4Result = selectInstallDialog.ShowDialog();

                    if (mc4Result == WinForms.DialogResult.OK)
                    {
                        string _mc4Result = Path.Combine(selectInstallDialog.SelectedPath, "Minecraft4.exe");
                        if (!File.Exists(_mc4Result))
                        {
                            SystemSounds.Exclamation.Play();
                            MessageBox.Show("Please select the location with Minecraft4.exe");
                            return;
                        }

                        mc4dir = Path.Combine(selectInstallDialog.SelectedPath);
                        keyMC4.SetValue("InstallPath", mc4dir);
                        keyMC4.SetValue("Installed", "1");
                        keyMC4.Close();
                        Status = MC4Status.ready;
                        return;
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
            using (RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true))
            {
                if (keyMC4 != null)
                {
                    Object obMC4Install = keyMC4.GetValue("Installed");
                    mc4Installed = (obMC4Install as String);

                    if (mc4Installed != "1")
                    {
                        MessageBox.Show("Minecraft 4 does not seem to be installed.");
                        keyMC4.Close();
                        return;
                    }

                    MessageBoxResult delMC4Box = System.Windows.MessageBox.Show("Are you sure you want to uninstall Minecraft 4?", "Minecraft 4", System.Windows.MessageBoxButton.YesNo);
                    if (delMC4Box == MessageBoxResult.Yes)
                    {
                        Object obMC4Path = keyMC4.GetValue("InstallPath");
                        if (obMC4Path != null)
                        {
                            mc4dir = (obMC4Path as String);

                            try
                            {
                                Directory.Delete(mc4dir, true);
                                keyMC4.SetValue("Installed", "0");
                                keyMC4.Close();
                                Status = MC4Status.noInstall;
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show("Minecraft 4 has been successfully uninstalled!", "Minecraft 4");
                            }
                            catch (Exception ex)
                            {
                                keyMC4.Close();
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show($"Error uninstallig Minecraft 4: {ex}");
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

        private void MC4Logo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://decentgamestudio.itch.io/minecraft-4");
        }
    }
}