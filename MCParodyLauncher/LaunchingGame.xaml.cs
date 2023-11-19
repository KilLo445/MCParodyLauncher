using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace MCParodyLauncher
{
    public partial class LaunchingGame : Window
    {
        string curProc;

        public static string mc2Proc = "Minecraft2Remake";
        public static string mc3Proc = "Game";
        public static string mc4Proc = "Minecraft4";
        public static string mc4oProc = "Minecraft4Otherside";
        public static string mc5Proc = "MC5";

        bool gameRunning = false;
         
        public LaunchingGame(string currentGame)
        {
            InitializeComponent();

            try
            {
                if (currentGame == "Minecraft 2") { currentGame = "Minecraft 2 Remake"; }

                this.Title = currentGame;
                LaunchText.Text = $"Launching {currentGame}...";

                if (currentGame == "Minecraft 2 Remake") { curProc = mc2Proc; }
                if (currentGame == "Minecraft 3") { curProc = mc3Proc; }
                if (currentGame == "Minecraft 4") { curProc = mc4Proc; }
                if (currentGame == "Minecraft 4 Otherside") { curProc = mc4oProc; }
                if (currentGame == "Minecraft 5") { curProc = mc5Proc; }

                this.Activate();
                CheckForGame();
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public async void CheckForGame()
        {
            try
            {
                while (gameRunning == false)
                {
                    if (Process.GetProcessesByName(curProc).Length > 0)
                    {
                        gameRunning = true;
                    }
                    else
                    {
                        gameRunning = false;
                    }
                }
                await Task.Delay(500);
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}
