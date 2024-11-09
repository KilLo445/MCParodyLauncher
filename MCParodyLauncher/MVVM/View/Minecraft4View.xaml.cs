using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Microsoft.Toolkit.Uwp.Notifications;
using WinForms = System.Windows.Forms;
using System.Threading.Tasks;
using Wsh = IWshRuntimeLibrary;
using System.Windows.Input;

namespace MCParodyLauncher.MVVM.View
{
    enum MC4Status
    {
        ready,
        noInstall,
        downloading,
        checkUpdate,
        update,
        updating,
        installing,
        error
    }
    public partial class Minecraft4View : UserControl
    {
        public string GameName = "Minecraft 4";
        public string GameNameS = "MC4";
        public string GameProcess = "Minecraft4";
        public string GameProcessO = "Minecraft4Otherside";

        private string gameLink = "https://www.dropbox.com/scl/fi/p22nxxkjjkktdeebgvo0t/mc4.zip?rlkey=a17asnmdcy3od23q9jtzn4pft&st=arh4pwmd&dl=1";
        private string gameStore = "https://decentgamestudio.itch.io/minecraft-4";
        private string gameStoreO = "https://decentgamestudio.itch.io/minecraft-4-otherside";
        private string verLink = "https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Games/MC4/version.txt";
        private string sizeLink = "https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Games/MC4/size.txt";

        // Paths
        private string rootPath;
        private string tempPath;
        private string gamesPath;
        private string gameDir;

        // Files
        private string gameZip;
        private string gameVer;

        // Bools
        public static bool gameInstalled;
        public static bool updateAvailable;
        public static bool downloadActive = false;

        private MC4Status _status;

        private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        internal MC4Status Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case MC4Status.ready:
                        PlayBTN.Content = "Play";
                        PlayBTN.IsEnabled = true;
                        downloadActive = false;
                        gameInstalled = true;
                        break;
                    case MC4Status.noInstall:
                        PlayBTN.Content = "Download";
                        PlayBTN.IsEnabled = true;
                        downloadActive = false;
                        gameInstalled = false;
                        break;
                    case MC4Status.downloading:
                        PlayBTN.Content = "Downloading";
                        PlayBTN.IsEnabled = false;
                        downloadActive = true;
                        gameInstalled = false;
                        break;
                    case MC4Status.checkUpdate:
                        PlayBTN.Content = "Checking...";
                        PlayBTN.IsEnabled = false;
                        downloadActive = false;
                        gameInstalled = true;
                        break;
                    case MC4Status.update:
                        PlayBTN.Content = "Update";
                        PlayBTN.IsEnabled = true;
                        downloadActive = false;
                        gameInstalled = true;
                        break;
                    case MC4Status.updating:
                        PlayBTN.Content = "Updating";
                        PlayBTN.IsEnabled = false;
                        downloadActive = true;
                        gameInstalled = false;
                        break;
                    case MC4Status.installing:
                        PlayBTN.Content = "Installing";
                        PlayBTN.IsEnabled = false;
                        downloadActive = true;
                        gameInstalled = false;
                        break;
                    case MC4Status.error:
                        PlayBTN.Content = "Error";
                        PlayBTN.IsEnabled = false;
                        downloadActive = false;
                        gameInstalled = false;
                        break;
                }
            }
        }

        public Minecraft4View()
        {
            InitializeComponent();
            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.Combine(Path.GetTempPath(), "MCParodyLauncher");
            gameZip = Path.Combine(tempPath, $"{GameNameS.ToLower()}.zip");
            CheckInstall();
            if (gameInstalled == true) { CheckForOtherside(); CheckForUpdates(); }
            if (updateAvailable == true) { UpdateNotif(); }
        }

        private void CheckForOtherside()
        {
            using(RegistryKey keyMC4O = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4"))
            {
                if (keyMC4O != null)
                {
                    Object obMC4OPath = keyMC4O.GetValue("InstallPathOtherside");
                    if (obMC4OPath != null)
                    {
                        string mc4odir = (obMC4OPath as String);
                        if (Directory.Exists((obMC4OPath as String)))
                        {
                            Directory.Delete(mc4odir, true);
                            GetInstallPath();
                            gameVer = Path.Combine(gameDir, "version.txt");
                            File.Delete(gameVer);
                            System.Windows.Forms.Application.Restart();
                            Application.Current.Shutdown();
                        }
                    }
                }
            }
        }

        private void CheckInstall()
        {
            try
            {
                RegistryKey keyGame = Registry.CurrentUser.OpenSubKey(@$"{MainWindow.regPath}\{GameNameS.ToLower()}", true);
                Object obGameInstalled = keyGame.GetValue("Installed", null);
                if (obGameInstalled != null)
                {
                    if (obGameInstalled as String != "0")
                    {
                        Object obMC2RPath = keyGame.GetValue("InstallPath");
                        if (obMC2RPath != null) { gameDir = (obMC2RPath as String); }
                        Status = MC4Status.ready;
                    }
                    else { Status = MC4Status.noInstall; }
                }
                else { Status = MC4Status.noInstall; }
                keyGame.Close();
                return;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void GetInstallPath()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@$"{MainWindow.regPath}\{GameNameS.ToLower()}"))
                {
                    if (key != null)
                    {
                        Object obPath = key.GetValue("InstallPath");
                        if (obPath != null)
                        {
                            gameDir = (obPath as String);
                            key.Close();
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private async Task CheckForUpdates()
        {
            Status = MC4Status.checkUpdate;
            try
            {
                await Task.Run(() => GetInstallPath());
                gameVer = Path.Combine(gameDir + "\\mc4\\", "version.txt");
                if (!File.Exists(gameVer)) { updateAvailable = false; UpdateNotif(); return; }
                Version localVer = new Version(File.ReadAllText(gameVer));
                WebClient webClient = new();
                Version onlineVer = new Version(await webClient.DownloadStringTaskAsync(verLink));
                if (onlineVer.IsDifferentThan(localVer)) { updateAvailable = true; Status = MC4Status.update; }
                else { updateAvailable = false; Status = MC4Status.ready; }
                return;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void DownloadGame()
        {
            try
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = false;
                CreateTemp();
                if (Directory.Exists(gameDir)) { Directory.Delete(gameDir, true); }
                Directory.CreateDirectory(gameDir);
                if (File.Exists(gameZip)) { File.Delete(gameZip); }
                WebClient webClient = new WebClient();
                if (MainWindow.usDownloadStats == true) { DownloadStats.Visibility = Visibility.Visible; }
                stopwatch.Start();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameComplete);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri(gameLink), gameZip);
                return;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void DownloadGameComplete(object sender, AsyncCompletedEventArgs e)
        {
            stopwatch.Reset();
            DownloadStats.Visibility = Visibility.Hidden;
            ExtractZipAsync(gameZip, gameDir);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@$"{MainWindow.regPath}\{GameNameS.ToLower()}", true);
            key.SetValue("Installed", "1");
            key.Close();
            return;
        }

        private void UpdateNotif()
        {
            MessageBoxResult updateResult = System.Windows.MessageBox.Show($"An update for {GameName} has been found! Would you like to download it?", $"{GameName}", System.Windows.MessageBoxButton.YesNo);
            if (updateResult == MessageBoxResult.Yes) { Status = MC4Status.updating; DownloadGame(); }
            else { Status = MC4Status.ready; return; }
        }

        private async void PlayBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Status == MC4Status.noInstall)
                {
                    InstallGame.installConfirmed = false;
                    InstallGame.installCanceled = false;
                    WebClient webClient = new WebClient();
                    string gameSize = webClient.DownloadString(sizeLink);
                    RegistryKey keyGames = Registry.CurrentUser.OpenSubKey($"{MainWindow.regPath}", true);
                    keyGames.CreateSubKey($"{GameNameS.ToLower()}");
                    RegistryKey keyGame = Registry.CurrentUser.OpenSubKey(@$"{MainWindow.regPath}\{GameNameS.ToLower()}", true);
                    keyGames.Close();
                    InstallGame installWindow = new InstallGame($"{GameName}", gameSize);
                    installWindow.Show();
                    PlayBTN.IsEnabled = false;
                    downloadActive = true;
                    while (InstallGame.installConfirmed == false)
                    {
                        if (InstallGame.installCanceled == true) { PlayBTN.IsEnabled = true; downloadActive = false; return; }
                        await Task.Delay(100);
                    }
                    downloadActive = false;
                    gameDir = Path.Combine(InstallGame.InstallPath, $"{GameName}");
                    keyGame.SetValue("InstallPath", gameDir);
                    keyGame.Close();
                    Status = MC4Status.downloading;
                    DownloadGame();
                    return;
                }
                if (Status == MC4Status.downloading || Status == MC4Status.updating) { return; }
                if (!Keyboard.IsKeyDown(Key.LeftShift))
                {
                    if (Process.GetProcessesByName($"{GameProcess}").Length > 0)
                    {
                        MessageBox.Show($"{GameName} is already running.", $"{GameName}", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                GetInstallPath();
                LaunchMC4 mc4Window = new LaunchMC4();
                mc4Window.Show();
                return;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private async Task ExtractZipAsync(string zipfile, string output)
        {
            try
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;
                Status = MC4Status.installing;
                await Task.Run(() => ZipFile.ExtractToDirectory(zipfile, output));
                File.Delete(gameZip);
                ProgressBar.Visibility = Visibility.Hidden;
                if (MainWindow.usNotifications == true)
                {
                    string dlStatus;
                    if (Status == MC4Status.updating) { dlStatus = "updating"; }
                    else { dlStatus = "downloading"; }
                    new ToastContentBuilder()
                    .AddText("Download complete!")
                    .AddText($"{GameName} has finished {dlStatus}.")
                    .Show();
                }
                Status = MC4Status.ready;
                return;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                string downloadProgress = e.ProgressPercentage + "%";
                string downloadSpeed = string.Format("{0} MB/s", (e.BytesReceived / 1024.0 / 1024.0 / stopwatch.Elapsed.TotalSeconds).ToString("0.00"));
                string downloadedMBs = Math.Round(e.BytesReceived / 1024.0 / 1024.0) + " MB";
                string totalMBs = Math.Round(e.TotalBytesToReceive / 1024.0 / 1024.0) + " MB";
                string progress = $"{downloadedMBs} / {totalMBs} ({downloadProgress}) ({downloadSpeed})";
                DownloadStats.Content = progress;
                ProgressBar.Value = e.ProgressPercentage;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void CreateTemp() { Directory.CreateDirectory(tempPath); return; }

        private void ErrorMSG(Exception exception) { Status = MC4Status.error; Dispatcher.BeginInvoke(new Action(() => System.Windows.MessageBox.Show($"{exception}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)), System.Windows.Threading.DispatcherPriority.Normal); return; }

        private void StorePage_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo(gameStore) { UseShellExecute = true }); Process.Start(new ProcessStartInfo(gameStoreO) { UseShellExecute = true }); }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void CheckForUpdatesBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckInstall();
                if (gameInstalled == false) { MessageBox.Show($"{GameName} does not appear to be installed.", "Minecraft Parody Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                CheckForUpdates();
                if (updateAvailable == false) { MessageBox.Show("No updates are available.", $"{GameName}", MessageBoxButton.OK, MessageBoxImage.Information); }
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void DesktopShortcut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckInstall();
                if (gameInstalled == false) { MessageBox.Show($"{GameName} does not appear to be installed.", "Minecraft Parody Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                GetInstallPath();
                object shDesktop = (object)"Desktop";
                Wsh.WshShell shell = new Wsh.WshShell();
                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @$"\{GameName}.lnk";
                Wsh.IWshShortcut shortcut = (Wsh.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.TargetPath = gameDir + $"\\{GameProcess}.exe";
                shortcut.Save();
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void FileLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckInstall();
                if (gameInstalled == false) { MessageBox.Show($"{GameName} does not appear to be installed.", "Minecraft Parody Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                GetInstallPath();
                Process.Start(new ProcessStartInfo { FileName = gameDir, UseShellExecute = true });
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void LocateInstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckInstall();
                if (gameInstalled == true) { MessageBox.Show($"{GameName} is already installled.", "Minecraft Parody Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                MessageBox.Show($"Please select the folder that containes \"{GameProcess}.exe\".", $"{GameName}", MessageBoxButton.OK, MessageBoxImage.Information);
                WinForms.FolderBrowserDialog selectInstallDialog = new WinForms.FolderBrowserDialog();
                selectInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                selectInstallDialog.ShowNewFolderButton = true;
                WinForms.DialogResult gameResult = selectInstallDialog.ShowDialog();
                if (gameResult == WinForms.DialogResult.OK)
                {
                    if (!File.Exists(Path.Combine(selectInstallDialog.SelectedPath, $"{GameProcess}.exe"))) { MessageBox.Show("Please select the location with Minecraft2Remake.exe", "Minecraft Parody Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                    gameDir = Path.Combine(selectInstallDialog.SelectedPath);
                    RegistryKey keyGames = Registry.CurrentUser.OpenSubKey($"{MainWindow.regPath}", true);
                    keyGames.CreateSubKey($"{GameNameS.ToLower()}");
                    RegistryKey keyGame = Registry.CurrentUser.OpenSubKey(@$"{MainWindow.regPath}\{GameNameS.ToLower()}", true);
                    keyGames.SetValue("InstallPath", gameDir);
                    keyGames.SetValue("Installed", "1");
                    keyGame.Close();
                    keyGames.Close();
                    Status = MC4Status.ready;
                }
                return;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void MoveInstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckInstall();
                if (gameInstalled == false) { MessageBox.Show($"{GameName} does not appear to be installed.", "Minecraft Parody Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                GetInstallPath();
                MessageBox.Show($"Please select where you would like to move {GameName} to.\n\nA folder called \"{GameName}\" will be created.", "Move Install", MessageBoxButton.OK, MessageBoxImage.Information);
                WinForms.FolderBrowserDialog moveInstallDialog = new WinForms.FolderBrowserDialog();
                moveInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                moveInstallDialog.ShowNewFolderButton = true;
                WinForms.DialogResult moveResult = moveInstallDialog.ShowDialog();
                if (moveResult == WinForms.DialogResult.OK)
                {
                    string dirOld = gameDir;
                    gameDir = Path.Combine(moveInstallDialog.SelectedPath, $"{GameName}");
                    RegistryKey keyGame = Registry.CurrentUser.OpenSubKey(@$"{MainWindow.regPath}\{GameNameS.ToLower()}", true);
                    keyGame.SetValue("InstallPath", gameDir);
                    keyGame.Close();
                    MessageBox.Show("Minecraft Parody Launcher may freeze during the move.\n\nDo not panic.", "Move Install", MessageBoxButton.OK, MessageBoxImage.Information);
                    bool sameVolume = dirOld.Substring(0, 1).Equals(gameDir.Substring(0, 1));
                    if (sameVolume == true)
                    {
                        Directory.Move(dirOld, gameDir);
                        MessageBox.Show("Install has been moved!", $"{GameName}", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Directory.CreateDirectory(gameDir);
                        foreach (string dirPath in Directory.GetDirectories(dirOld, "*", SearchOption.AllDirectories)) { Directory.CreateDirectory(dirPath.Replace(dirOld, gameDir)); }
                        foreach (string newPath in Directory.GetFiles(dirOld, "*.*", SearchOption.AllDirectories)) { File.Copy(newPath, newPath.Replace(dirOld, gameDir), true); }
                        Directory.Delete(dirOld, true);
                        MessageBox.Show("Install has been moved!", $"{GameName}", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                return;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckInstall();
                if (gameInstalled == false) { MessageBox.Show($"{GameName} does not appear to be installed.", "Minecraft Parody Launcher", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                GetInstallPath();
                MessageBoxResult uninstallMSG = System.Windows.MessageBox.Show($"Are you sure you want to uninstall {GameName}?", $"{GameName}", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (uninstallMSG == MessageBoxResult.Yes)
                {
                    Directory.Delete(gameDir, true);
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@$"{MainWindow.regPath}\{GameNameS.ToLower()}", true);
                    key.SetValue("Installed", "0");
                    key.Close();
                    Status = MC4Status.noInstall;
                    MessageBox.Show($"{GameName} has been successfully uninstalled!", $"{GameName}", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                return;
            }
            catch (Exception ex) { ErrorMSG(ex); return; }
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
