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
        private string mc3link = "https://www.dropbox.com/s/k6kqkmgndyed9kg/mc3.zip?dl=1";
        private string verLink = "https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC3/version.txt";
        private string sizeLink = "https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Games/MC3/size.txt";

        // Paths
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string gamesPath;
        private string mc3dir;

        // Files
        private string mc3;
        private string mc3ver;
        private string mc3zip;

        // Settings
        string mc3Installed;
        public static bool downloadActive = false;

        private MC3Status _status;

        internal MC3Status Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case MC3Status.ready:
                        PlayMC3.Content = "Play";
                        downloadActive = false;
                        break;
                    case MC3Status.noInstall:
                        PlayMC3.Content = "Download";
                        downloadActive = false;
                        break;
                    case MC3Status.failed:
                        PlayMC3.Content = "Error";
                        downloadActive = false;
                        break;
                    case MC3Status.downloading:
                        PlayMC3.Content = "Downloading";
                        downloadActive = true;
                        break;
                    case MC3Status.unzip:
                        PlayMC3.Content = "Installing";
                        downloadActive = true;
                        break;
                    case MC3Status.update:
                        PlayMC3.Content = "Updating";
                        downloadActive = true;
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

            mc3zip = Path.Combine(mcplTempPath, "mc3.zip");

            CheckInst();
        }

        private void CheckInst()
        {
            RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3", true);
            Object obMC3Installed = keyMC3.GetValue("Installed", null);

            if (obMC3Installed != null)
            {
                string MC3Installed = (obMC3Installed as String);

                if (MC3Installed != "0")
                {
                    Object obMC3Path = keyMC3.GetValue("InstallPath");
                    if (obMC3Path != null)
                    {
                        mc3dir = (obMC3Path as String);
                        keyMC3.Close();
                    }

                    keyMC3.Close();
                    Status = MC3Status.ready;
                }
                else
                {
                    keyMC3.Close();
                    Status = MC3Status.noInstall;
                }
            }
            else
            {
                keyMC3.Close();
                Status = MC3Status.noInstall;
            }
        }

        private void CreateTemp()
        {
            Directory.CreateDirectory(mcplTempPath);
        }

        private void DownloadWarning()
        {
            MessageBox.Show("Please do not switch game tabs or close the launcher until your download finishes, it may cause issues if you do so.");
        }

        private void PlayMC3_Click(object sender, RoutedEventArgs e)
        {
            if (Status == MC3Status.downloading)
            {
                return;
            }
            
            RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3", true);
            Object obMC3Installed = keyMC3.GetValue("Installed", null);

            if (MainWindow.offlineMode == true)
            {
                Object obMC3Path = keyMC3.GetValue("InstallPath");
                if (obMC3Path != null)
                {
                    mc3dir = (obMC3Path as String);
                    mc3 = Path.Combine(mc3dir, "Game.exe");
                }

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

            keyMC3.Close();
            if (obMC3Installed != null)
            {
                mc3Installed = (obMC3Installed as String);

                if (mc3Installed == "1")
                {
                    CheckForUpdatesMC3();
                }
                else
                {
                    InstallMC3();
                }
            }
            else
            {
                InstallMC3();
            }
        }

        private void StartMC3()
        {
            using (RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3"))
            {
                if (keyMC3 != null)
                {
                    Object obMC3Path = keyMC3.GetValue("InstallPath");
                    if (obMC3Path != null)
                    {
                        mc3dir = (obMC3Path as String);
                        mc3 = Path.Combine(mc3dir, "Game.exe");
                        keyMC3.Close();
                        try
                        {
                            Process.Start(mc3);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error launching Minecraft 3: {ex}");
                        }
                    }
                }
            }
        }

        private void InstallMC3()
        {
            WebClient webClient = new WebClient();
            string mc3Size = webClient.DownloadString(sizeLink);

            MessageBoxResult mc3InstallConfirm = System.Windows.MessageBox.Show($"Minecraft 3 requires {mc3Size}Do you want to continue?", "Minecraft 3", System.Windows.MessageBoxButton.YesNo);
            if (mc3InstallConfirm == MessageBoxResult.Yes)
            {
                RegistryKey keyGames = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games", true);
                keyGames.CreateSubKey("mc3");
                RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3", true);
                keyGames.Close();

                MessageBoxResult mc3InstallLocationMB = System.Windows.MessageBox.Show($"Would you like to install Minecraft 3 at {rootPath}\\games", "Minecraft 3", System.Windows.MessageBoxButton.YesNo);
                if (mc3InstallLocationMB == MessageBoxResult.Yes)
                {
                    keyMC3.SetValue("InstallPath", $"{rootPath}\\games\\Minecraft 3");
                    keyMC3.Close();
                    mc3dir = Path.Combine(rootPath, "games", "Minecraft 3");
                    DownloadMC3();
                }
                if (mc3InstallLocationMB == MessageBoxResult.No)
                {
                    WinForms.FolderBrowserDialog mc3FolderDialog = new WinForms.FolderBrowserDialog();
                    mc3FolderDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    mc3FolderDialog.Description = "Please select where you would like to install Minecraft 3, a folder called \"Minecraft 3\" will be created.";
                    mc3FolderDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc3Result = mc3FolderDialog.ShowDialog();

                    if (mc3Result == WinForms.DialogResult.OK)
                    {
                        mc3dir = Path.Combine(mc3FolderDialog.SelectedPath, "Minecraft 3");
                        keyMC3.SetValue("InstallPath", mc3dir);
                        keyMC3.Close();
                        DownloadMC3();
                    }
                }

                keyMC3.Close();
            }
        }

        private void DownloadMC3()
        {
            CreateTemp();
            Directory.CreateDirectory(mc3dir);

            if (File.Exists(mc3zip))
            {
                try
                {
                    File.Delete(mc3zip);
                }
                catch (Exception ex)
                {
                    Status = MC3Status.failed;
                    MessageBox.Show($"Error deleting zip: {ex}");
                }
            }

            DLProgress.Visibility = Visibility.Visible;

            try
            {
                Status = MC3Status.downloading;

                DLProgress.IsIndeterminate = false;
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC3CompletedCallback);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri(mc3link), mc3zip);
            }
            catch (Exception ex)
            {
                Status = MC3Status.failed;
                MessageBox.Show($"Error downloading Minecraft 3: {ex}");
            }
        }

        private void DownloadMC3CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3", true);
            keyMC3.SetValue("Installed", "1");
            keyMC3.Close();
            ExtractZipAsync(mc3zip, mc3dir);
        }

        private void CheckForUpdatesMC3()
        {
            Status = MC3Status.checkUpdate;

            try
            {
                using (RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3"))
                {
                    if (keyMC3 != null)
                    {
                        Object obMC3Path = keyMC3.GetValue("InstallPath");
                        if (obMC3Path != null)
                        {
                            mc3dir = (obMC3Path as String);
                            mc3ver = Path.Combine(mc3dir, "version.txt");
                            keyMC3.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                Status = MC3Status.failed;
                MessageBox.Show($"Error: {ex}");
            }

            if (File.Exists(mc3ver))
            {
                Version localVersionMC3 = new Version(File.ReadAllText(mc3ver));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersionMC3 = new Version(webClient.DownloadString(verLink));

                    if (onlineVersionMC3.IsDifferentThan(localVersionMC3))
                    {
                        InstallUpdateMC3(true, onlineVersionMC3);
                    }
                    else
                    {
                        Status = MC3Status.ready;
                        StartMC3();
                    }
                }
                catch (Exception ex)
                {
                    Status = MC3Status.failed;
                    MessageBox.Show($"Error checking for updates: {ex}");
                }
            }
            else
            {
                InstallUpdateMC3(false, Version.zero);
            }
        }

        private void InstallUpdateMC3(bool isUpdate, Version _onlineVersionMC3)
        {
            try
            {
                MessageBoxResult messageBoxResultMC3Update = System.Windows.MessageBox.Show("An update for Minecraft 3 has been found! Would you like to download it?", "Minecraft 3", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResultMC3Update == MessageBoxResult.Yes)
                {
                    Status = MC3Status.update;

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
                                Status = MC3Status.failed;
                                MessageBox.Show($"Error deleting zip: {ex}");
                            }
                        }

                        DLProgress.Visibility = Visibility.Visible;

                        try
                        {
                            Status = MC3Status.downloading;

                            DLProgress.IsIndeterminate = false;
                            WebClient webClient = new WebClient();
                            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC3CompletedCallback);
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                            webClient.DownloadFileAsync(new Uri(mc3link), mc3zip);
                        }
                        catch (Exception ex)
                        {
                            Status = MC3Status.failed;
                            MessageBox.Show($"Error updating Minecraft 3: {ex}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemSounds.Exclamation.Play();
                        Status = MC3Status.failed;
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
                Status = MC3Status.failed;
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void UpdateMC3CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            ExtractZipAsync(mc3zip, mc3dir);
        }

        private async Task ExtractZipAsync(string zipfile, string output)
        {
            try
            {
                Status = MC3Status.unzip;
                DLProgress.IsIndeterminate = true;
                await Task.Run(() => ZipFile.ExtractToDirectory(zipfile, output));
                File.Delete(mc3zip);
                Status = MC3Status.ready;
                DLProgress.Visibility = Visibility.Hidden;
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Download complete!", "Minecraft 3");
                return;
            }
            catch (Exception ex)
            {
                Status = MC3Status.failed;
                MessageBox.Show($"Error Updating Minecraft 3: {ex}");
                return;
            }
        }

        private void DesktopShortcut_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3"))
            {
                if (keyMC3 != null)
                {
                    Object obMC3Install = keyMC3.GetValue("Installed");
                    mc3Installed = (obMC3Install as String);

                    if (mc3Installed == "1")
                    {
                        object shDesktop = (object)"Desktop";
                        Wsh.WshShell shell = new Wsh.WshShell();
                        string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Minecraft 3.lnk";
                        Wsh.IWshShortcut shortcut = (Wsh.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                        shortcut.TargetPath = mc3dir + "\\Game.exe";
                        shortcut.IconLocation = mc3dir + "\\www\\icon\\icon.ico";
                        shortcut.Save();

                        keyMC3.Close();
                    }
                    else
                    {
                        MessageBox.Show("Minecraft 3 does not seem to be installed.");
                        keyMC3.Close();
                    }
                }
            }
        }

        private void FileLocation_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3"))
            {
                if (keyMC3 != null)
                {
                    Object obMC3Install = keyMC3.GetValue("Installed");
                    mc3Installed = (obMC3Install as String);

                    if (mc3Installed == "1")
                    {
                        Object obMC3Path = keyMC3.GetValue("InstallPath");
                        if (obMC3Path != null)
                        {
                            mc3dir = (obMC3Path as String);
                            keyMC3.Close();

                            Process.Start(mc3dir);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Minecraft 3 does not seem to be installed.");
                        keyMC3.Close();
                    }
                }
            }
        }

        private void SelectLocation_Click(object sender, RoutedEventArgs e)
        {
            using (RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3", true))
            {
                if (keyMC3 != null)
                {
                    WinForms.FolderBrowserDialog selectInstallDialog = new WinForms.FolderBrowserDialog();
                    selectInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    selectInstallDialog.Description = "Please select the folder that containes \"Game.exe\".";
                    selectInstallDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc3Result = selectInstallDialog.ShowDialog();

                    if (mc3Result == WinForms.DialogResult.OK)
                    {
                        string _mc3Result = Path.Combine(selectInstallDialog.SelectedPath, "Game.exe");
                        if (!File.Exists(_mc3Result))
                        {
                            SystemSounds.Exclamation.Play();
                            MessageBox.Show("Please select the location with Game.exe");
                            return;
                        }

                        mc3dir = Path.Combine(selectInstallDialog.SelectedPath);
                        keyMC3.SetValue("InstallPath", mc3dir);
                        keyMC3.SetValue("Installed", "1");
                        keyMC3.Close();
                        Status = MC3Status.ready;
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
            using (RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3", true))
            {
                if (keyMC3 != null)
                {
                    Object obMC3Install = keyMC3.GetValue("Installed");
                    mc3Installed = (obMC3Install as String);

                    if (mc3Installed != "1")
                    {
                        MessageBox.Show("Minecraft 3 does not seem to be installed.");
                        keyMC3.Close();
                        return;
                    }

                    WinForms.FolderBrowserDialog moveInstallDialog = new WinForms.FolderBrowserDialog();
                    moveInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    moveInstallDialog.Description = "Please select where you would like to move Minecraft 3 to, a folder called \"Minecraft 3\" will be created.";
                    moveInstallDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc3Result = moveInstallDialog.ShowDialog();

                    if (mc3Result == WinForms.DialogResult.OK)
                    {
                        string dirOld = mc3dir;
                        mc3dir = Path.Combine(moveInstallDialog.SelectedPath, "Minecraft 3");
                        keyMC3.SetValue("InstallPath", mc3dir);
                        keyMC3.Close();

                        string dirOldChar = dirOld.Substring(0, 1);
                        string mc3dirChar = mc3dir.Substring(0, 1);
                        bool sameVolume = dirOldChar.Equals(mc3dirChar);

                        if (sameVolume == true)
                        {
                            try
                            {
                                Directory.Move(dirOld, mc3dir);
                                MessageBox.Show("Install has been moved!", "Minecraft 3", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error Moving Minecraft 3: {ex}");
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                Directory.CreateDirectory(mc3dir);

                                foreach (string dirPath in Directory.GetDirectories(dirOld, "*", SearchOption.AllDirectories))
                                {
                                    Directory.CreateDirectory(dirPath.Replace(dirOld, mc3dir));
                                }
                                foreach (string newPath in Directory.GetFiles(dirOld, "*.*", SearchOption.AllDirectories))
                                {
                                    File.Copy(newPath, newPath.Replace(dirOld, mc3dir), true);
                                }
                                Directory.Delete(dirOld, true);
                                MessageBox.Show("Install has been moved!", "Minecraft 3", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error Moving Minecraft 3: {ex}");
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
            using (RegistryKey keyMC3 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc3", true))
            {
                if (keyMC3 != null)
                {
                    Object obMC3Install = keyMC3.GetValue("Installed");
                    mc3Installed = (obMC3Install as String);

                    if (mc3Installed != "1")
                    {
                        MessageBox.Show("Minecraft 3 does not seem to be installed.");
                        keyMC3.Close();
                        return;
                    }

                    MessageBoxResult delMC3Box = System.Windows.MessageBox.Show("Are you sure you want to delete Minecraft 3?", "Minecraft 3", System.Windows.MessageBoxButton.YesNo);
                    if (delMC3Box == MessageBoxResult.Yes)
                    {
                        Object obMC3Path = keyMC3.GetValue("InstallPath");
                        if (obMC3Path != null)
                        {
                            mc3dir = (obMC3Path as String);

                            try
                            {
                                Directory.Delete(mc3dir, true);
                                keyMC3.SetValue("Installed", "0");
                                keyMC3.Close();
                                Status = MC3Status.noInstall;
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show("Minecraft 3 has been successfully deleted!", "Minecraft 3");
                            }
                            catch (Exception ex)
                            {
                                keyMC3.Close();
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show($"Error deleting Minecraft 3: {ex}");
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

        private void MC3Logo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://roggzerz.itch.io/minecraft-3");
        }
    }
}
