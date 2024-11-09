using System.IO;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace MCParodyLauncher
{
    public partial class InstallGame : Window
    {
        public static string InstallPath;

        public static bool installConfirmed = false;
        public static bool installCanceled = false;

        private string rootPath;
        private string defaultPath;
        public string curGame;

        public InstallGame(string currentGame, string installSize)
        {
            InitializeComponent();

            curGame = currentGame;

            rootPath = Directory.GetCurrentDirectory();

            defaultPath = Path.Combine(rootPath, "games");

            InstallPath = defaultPath;

            SizeText.Text = $"Space required: {installSize}";
            InstallText.Text = $"Where would you like to install {curGame}?";
            InstallText2.Text = $"A folder named {curGame} will be created.";
            InstallPathBox.Text = defaultPath + "\\" + curGame;
            InstallPathBox.ToolTip = defaultPath;
        }

        private void SelectPath_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog installPathDialog = new WinForms.FolderBrowserDialog();
            installPathDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
            installPathDialog.ShowNewFolderButton = true;
            WinForms.DialogResult instResult = installPathDialog.ShowDialog();
            if (instResult == WinForms.DialogResult.OK)
            {
                InstallPath = installPathDialog.SelectedPath;

                InstallPathBox.Text = Path.Combine(InstallPath, curGame);
                InstallPathBox.ToolTip = Path.Combine(InstallPath, curGame);
            }
        }

        private void btnInst_Click(object sender, RoutedEventArgs e)
        {
            installConfirmed = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            installCanceled = true;
            this.Close();
        }
    }
}
