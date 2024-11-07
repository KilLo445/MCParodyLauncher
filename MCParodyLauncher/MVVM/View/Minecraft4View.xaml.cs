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
        public static string GameName = "Minecraft 4";

        private string mc4link = "https://www.dropbox.com/s/dq5vl127q7gza4r/mc4.zip?dl=1";
        private string mc4olink = "https://www.dropbox.com/s/6xd3c6dtd889ekb/mc4o.zip?dl=1";
        private string verLink = "https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Games/MC4/version.txt";
        private string verLinkO = "https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Games/MC4O/version.txt";
        private string sizeLink = "https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/Versions/Games/MC4/size.txt";

        // Paths
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string gamesPath;
        private string mc4dir;
        private string mc4odir;

        // Files
        private string mc4;
        private string mc4ver;
        private string mc4zip;
        private string mc4o;
        private string mc4over;
        private string mc4ozip;

        // Settings
        string mc4Installed;
        public static bool downloadActive = false;
        bool mc4UpToDate = true;
        bool mc4oUpToDate = true;
        bool mc4DL = false;
        bool mc4oDL = false;
        bool forceMC4Uninst = false;

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
            mc4ozip = Path.Combine(mcplTempPath, "mc4o.zip");

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
                    Object obMC4oPath = keyMC4.GetValue("InstallPathOtherside");
                    if (obMC4Path != null)
                    {
                        mc4dir = (obMC4Path as String);
                        if (obMC4oPath != null)
                        {
                            mc4odir = (obMC4oPath as String);
                        }
                        else
                        {
                            forceMC4Uninst = true;
                        }

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

        private void ForceMC4Uninstall()
        {
            if (forceMC4Uninst == true)
            {
                RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true);
                MessageBoxResult mc4oInfo = System.Windows.MessageBox.Show("Hi there!\n\nTo get Minecraft 4: Otherside to install properly, please press OK to uninstall Minecraft 4, then please reinstall both games.", "Minecraft 4: Otherside", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                if (mc4oInfo == MessageBoxResult.OK)
                {
                    try
                    {
                        if (Directory.Exists(mc4dir)) { Directory.Delete(mc4dir, true); }
                        if (Directory.Exists(mc4odir)) { Directory.Delete(mc4odir, true); }
                        keyMC4.SetValue("Installed", "0");
                        keyMC4.Close();
                        Status = MC4Status.noInstall;
                        forceMC4Uninst = false;
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show("Minecraft 4 has been successfully uninstalled!", "Minecraft 4");
                    }
                    catch (Exception ex)
                    {
                        keyMC4.Close();
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show($"Error uninstalling Minecraft 4: {ex}");
                    }
                }
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

            if (forceMC4Uninst == true) { ForceMC4Uninstall(); }

            RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true);
            Object obMC4Installed = keyMC4.GetValue("Installed", null);

            if (MainWindow.offlineMode == true)
            {
                Object obMC4Path = keyMC4.GetValue("InstallPath");
                if (obMC4Path != null)
                {
                    mc4dir = (obMC4Path as String);
                    mc4 = Path.Combine(mc4dir, "Minecraft4.exe");

                    if (File.Exists(mc4))
                    {
                        try
                        {
                            LaunchMC4 mc4Window = new LaunchMC4();
                            mc4Window.Show();
                            return;
                        }
                        catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        return;
                    }

                    else
                    {
                        MessageBox.Show("Please launch Minecraft Parody Launcher in online mode to install Minecraft 4.");
                        return;
                    }
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
            if (Status == MC4Status.ready)
            {
                try
                {
                    LaunchMC4 mc4Window = new LaunchMC4();
                    mc4Window.Show();
                    return;
                }
                catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private async void InstallMC4()
        {
            InstallGame.installConfirmed = false;
            InstallGame.installCanceled = false;

            WebClient webClient = new WebClient();
            string mc4Size = webClient.DownloadString(sizeLink);

            RegistryKey keyGames = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games", true);
            keyGames.CreateSubKey("mc4");
            RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true);
            keyGames.Close();

            InstallGame installWindow = new InstallGame("Minecraft 4", mc4Size);
            installWindow.Show();
            PlayMC4.IsEnabled = false;
            downloadActive = true;
            while (InstallGame.installConfirmed == false)
            {
                if (InstallGame.installCanceled == true) { PlayMC4.IsEnabled = true; downloadActive = false; return; }
                await Task.Delay(100);
            }
            PlayMC4.IsEnabled = true;
            downloadActive = false;
            mc4dir = Path.Combine(InstallGame.InstallPath, "Minecraft 4");
            mc4odir = Path.Combine(InstallGame.InstallPath, "Minecraft 4 Otherside");
            keyMC4.SetValue("InstallPath", mc4dir);
            keyMC4.SetValue("InstallPathOtherside", mc4odir);
            keyMC4.Close();
            DownloadMC4();
        }

        private void DownloadMC4()
        {
            CreateTemp();
            Directory.CreateDirectory(mc4dir);
            Directory.CreateDirectory(mc4odir);

            if (File.Exists(mc4zip))
            {
                try
                {
                    File.Delete(mc4zip);
                }
                catch (Exception ex)
                {
                    Status = MC4Status.failed;
                    MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            if (File.Exists(mc4ozip))
            {
                try
                {
                    File.Delete(mc4ozip);
                }
                catch (Exception ex)
                {
                    Status = MC4Status.failed;
                    MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            DLProgress.Visibility = Visibility.Visible;

            try
            {
                Status = MC4Status.downloading;

                DLProgress.IsIndeterminate = false;
                WebClient webClient = new WebClient();
                if (MainWindow.usDownloadStats == true) { lblProgress.Visibility = Visibility.Visible; }
                stopwatch.Start();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC4CompletedCallback);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri(mc4link), mc4zip);
            }
            catch (Exception ex)
            {
                Status = MC4Status.failed;
                MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadMC4CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            stopwatch.Reset();
            lblProgress.Visibility = Visibility.Hidden;

            ExtractZipAsync(mc4zip, mc4dir);

            mc4DL = true;

            Status = MC4Status.downloading;

            DLProgress.IsIndeterminate = false;
            WebClient webClient = new WebClient();
            if (MainWindow.usDownloadStats == true) { lblProgress.Visibility = Visibility.Visible; }
            stopwatch.Start();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadMC4CompletedCallback2);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileAsync(new Uri(mc4olink), mc4ozip);
        }

        private void DownloadMC4CompletedCallback2(object sender, AsyncCompletedEventArgs e)
        {
            stopwatch.Reset();
            lblProgress.Visibility = Visibility.Hidden;
            RegistryKey keyMC4 = Registry.CurrentUser.OpenSubKey(@"Software\decentgames\MinecraftParodyLauncher\games\mc4", true);
            keyMC4.SetValue("Installed", "1");
            keyMC4.Close();
            mc4oDL = true;
            ExtractZipAsync(mc4ozip, mc4odir);
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
                        Object obMC4oPath = keyMC4.GetValue("InstallPathOtherside");
                        if (obMC4Path != null && obMC4oPath != null)
                        {
                            mc4dir = (obMC4Path as String);
                            mc4ver = Path.Combine(mc4dir, "version.txt");
                            mc4odir = (obMC4oPath as String);
                            mc4over = Path.Combine(mc4odir, "version.txt");
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

            if (File.Exists(mc4ver) && File.Exists(mc4over))
            {
                Version localVersionMC4 = new Version(File.ReadAllText(mc4ver));
                Version localVersionMC4o = new Version(File.ReadAllText(mc4over));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersionMC4 = new Version(webClient.DownloadString(verLink));
                    Version onlineVersionMC4o = new Version(webClient.DownloadString(verLinkO));

                    if (onlineVersionMC4.IsDifferentThan(localVersionMC4))
                    {
                        mc4UpToDate = false;
                    }
                    if (onlineVersionMC4o.IsDifferentThan(localVersionMC4o))
                    {
                        mc4oUpToDate = false;
                    }
                    if (mc4UpToDate == false || mc4oUpToDate == false)
                    {
                        InstallUpdateMC4();
                    }
                    else
                    {
                        mc4UpToDate = true;
                        mc4oUpToDate = true;
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
                mc4UpToDate = false;
                mc4oUpToDate = false;
                InstallUpdateMC4();
            }
        }

        private void InstallUpdateMC4()
        {
            if (mc4UpToDate == false)
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
                            if (Directory.Exists(mc4dir))
                            {
                                Directory.Delete(mc4dir, true);
                            }

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
                                if (MainWindow.usDownloadStats == true) { lblProgress.Visibility = Visibility.Visible; }
                                stopwatch.Start();
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
                            Status = MC4Status.failed;
                            MessageBox.Show($"Error updating Minecraft 4: {ex}");
                        }
                    }
                    else
                    {
                        Status = MC4Status.ready;
                    }
                }
                catch (Exception ex)
                {
                    SystemSounds.Exclamation.Play();
                    Status = MC4Status.failed;
                    MessageBox.Show($"Error: {ex}");
                }
            }
            if (mc4oUpToDate == false)
            {
                try
                {
                    MessageBoxResult messageBoxResultMC4oUpdate = System.Windows.MessageBox.Show("An update for Minecraft 4: Otherside has been found! Would you like to download it?", "Minecraft 4: Otherside", System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResultMC4oUpdate == MessageBoxResult.Yes)
                    {
                        Status = MC4Status.update;

                        CreateTemp();

                        try
                        {
                            if (Directory.Exists(mc4odir))
                            {
                                Directory.Delete(mc4odir, true);
                            }

                            Directory.CreateDirectory(mc4odir);

                            if (File.Exists(mc4ozip))
                            {
                                try
                                {
                                    File.Delete(mc4ozip);
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
                                if (MainWindow.usDownloadStats == true) { lblProgress.Visibility = Visibility.Visible; }
                                stopwatch.Start();
                                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UpdateMC4OCompletedCallback);
                                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                                webClient.DownloadFileAsync(new Uri(mc4olink), mc4ozip);
                            }
                            catch (Exception ex)
                            {
                                Status = MC4Status.failed;
                                MessageBox.Show($"Error updating Minecraft 4: Otherside : {ex}");
                            }
                        }
                        catch (Exception ex)
                        {
                            SystemSounds.Exclamation.Play();
                            MessageBox.Show($"Error updating Minecraft 4: Otherside: {ex}");
                        }
                    }
                    else
                    {
                        Status = MC4Status.ready;
                    }
                }
                catch (Exception ex)
                {
                    SystemSounds.Exclamation.Play();
                    Status = MC4Status.failed;
                    MessageBox.Show($"Error: {ex}");
                }
            }
            StartMC4();
        }

        private void UpdateMC4CompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            stopwatch.Reset();
            lblProgress.Visibility = Visibility.Hidden;
            mc4DL = true;
            mc4oDL = true;
            ExtractZipAsync(mc4zip, mc4dir);
        }

        private void UpdateMC4OCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            stopwatch.Reset();
            lblProgress.Visibility = Visibility.Hidden;
            mc4DL = true;
            mc4oDL = true;
            ExtractZipAsync(mc4ozip, mc4odir);
        }

        private async Task ExtractZipAsync(string zipfile, string output)
        {
            try
            {
                Status = MC4Status.unzip;
                DLProgress.IsIndeterminate = true;
                await Task.Run(() => ZipFile.ExtractToDirectory(zipfile, output));
                File.Delete(mc4zip);
                if (mc4DL == true && mc4oDL == true)
                {
                    Status = MC4Status.ready;
                    DLProgress.Visibility = Visibility.Hidden;
                    mc4DL = false;
                    mc4oDL = false;
                    if (MainWindow.usNotifications == true)
                    {
                        new ToastContentBuilder()
                        .AddText("Download complete!")
                        .AddText("Minecraft 4 has finished downloading.")
                        .Show();
                    }
                }
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
                        string shortcutAddressO = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Minecraft 4 Otherside.lnk";
                        Wsh.IWshShortcut shortcut = (Wsh.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                        Wsh.IWshShortcut shortcutO = (Wsh.IWshShortcut)shell.CreateShortcut(shortcutAddressO);
                        shortcut.TargetPath = mc4dir + "\\Minecraft4.exe";
                        shortcutO.TargetPath = mc4odir + "\\Minecraft4Otherside.exe";
                        shortcut.Save();
                        shortcutO.Save();

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
                        Object obMC4oPath = keyMC4.GetValue("InstallPathOtherside");
                        if (obMC4Path != null && obMC4oPath != null)
                        {
                            mc4dir = (obMC4Path as String);
                            mc4odir = (obMC4oPath as String);
                            keyMC4.Close();

                            try
                            {
                                Process.Start(new ProcessStartInfo { FileName = mc4dir, UseShellExecute = true });
                                Process.Start(new ProcessStartInfo { FileName = mc4odir, UseShellExecute = true });
                            }
                            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
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

                        WinForms.FolderBrowserDialog _selectInstallDialog = new WinForms.FolderBrowserDialog();
                        _selectInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                        _selectInstallDialog.Description = "Please select the folder that containes \"Minecraft4Otherside.exe\".";
                        _selectInstallDialog.ShowNewFolderButton = true;
                        WinForms.DialogResult __mc4Result = _selectInstallDialog.ShowDialog();

                        if (__mc4Result == WinForms.DialogResult.OK)
                        {
                            string ___mc4Result = Path.Combine(_selectInstallDialog.SelectedPath, "Minecraft4Otherside.exe");
                            if (!File.Exists(___mc4Result))
                            {
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show("Please select the location with Minecraft4Otherside.exe");
                                return;
                            }

                            mc4odir = Path.Combine(_selectInstallDialog.SelectedPath);
                            keyMC4.SetValue("InstallPathOtherside", mc4odir);
                            keyMC4.SetValue("Installed", "1");
                            Status = MC4Status.ready;
                            return;
                        }
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
                    MessageBox.Show("Please select where you would like to move Minecraft 4 and Minecraft 4: Otherside to, a folder called \"Minecraft 4\" and \"Minecraft 4 Otherside\" will be created.", "Minecraft 4", MessageBoxButton.OK, MessageBoxImage.Information);
                    WinForms.FolderBrowserDialog moveInstallDialog = new WinForms.FolderBrowserDialog();
                    moveInstallDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    moveInstallDialog.ShowNewFolderButton = true;
                    WinForms.DialogResult mc4Result = moveInstallDialog.ShowDialog();

                    if (mc4Result == WinForms.DialogResult.OK)
                    {
                        string dirOld = mc4dir;
                        string dirOldo = mc4odir;
                        mc4dir = Path.Combine(moveInstallDialog.SelectedPath, "Minecraft 4");
                        mc4odir = Path.Combine(moveInstallDialog.SelectedPath, "Minecraft 4 Otherside");
                        keyMC4.SetValue("InstallPath", mc4dir);
                        keyMC4.SetValue("InstallPathOtherside", mc4odir);
                        keyMC4.Close();

                        string dirOldChar = dirOld.Substring(0, 1);
                        string mc4dirChar = mc4dir.Substring(0, 1);
                        string dirOldoChar = dirOldo.Substring(0, 1);
                        string mc4odirChar = mc4odir.Substring(0, 1);
                        bool sameVolume = dirOldChar.Equals(mc4dirChar);
                        bool sameVolumeO = dirOldoChar.Equals(mc4odirChar);

                        if (sameVolume == true && sameVolumeO == true)
                        {
                            try
                            {
                                Directory.Move(dirOld, mc4dir);
                                Directory.Move(dirOldo, mc4odir);
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
                                Directory.CreateDirectory(mc4odir);

                                foreach (string dirPath in Directory.GetDirectories(dirOld, "*", SearchOption.AllDirectories))
                                {
                                    Directory.CreateDirectory(dirPath.Replace(dirOld, mc4dir));
                                }
                                foreach (string newPath in Directory.GetFiles(dirOld, "*.*", SearchOption.AllDirectories))
                                {
                                    File.Copy(newPath, newPath.Replace(dirOld, mc4dir), true);
                                }
                                foreach (string dirPath in Directory.GetDirectories(dirOldo, "*", SearchOption.AllDirectories))
                                {
                                    Directory.CreateDirectory(dirPath.Replace(dirOldo, mc4odir));
                                }
                                foreach (string newPath in Directory.GetFiles(dirOldo, "*.*", SearchOption.AllDirectories))
                                {
                                    File.Copy(newPath, newPath.Replace(dirOldo, mc4odir), true);
                                }
                                Directory.Delete(dirOld, true);
                                Directory.Delete(dirOldo, true);
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
                        Object obMC4oPath = keyMC4.GetValue("InstallPathOtherside");
                        if (obMC4Path != null && obMC4oPath != null)
                        {
                            mc4dir = (obMC4Path as String);
                            mc4odir = (obMC4oPath as String);

                            try
                            {
                                Directory.Delete(mc4dir, true);
                                Directory.Delete(mc4odir, true);
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
                                MessageBox.Show($"Error uninstalling Minecraft 4: {ex}");
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

        private void StorePage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://decentgamestudio.itch.io/minecraft-4") { UseShellExecute = true });
            Process.Start(new ProcessStartInfo("https://decentgamestudio.itch.io/minecraft-4-otherside") { UseShellExecute = true });
        }
    }
}
