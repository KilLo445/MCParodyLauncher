using System;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace MCParodyLauncher
{
    public partial class Changelog : Window
    {
        string changelogLink = "https://raw.githubusercontent.com/KilLo445/MCParodyLauncher/master/MCParodyLauncher/changelog.txt";

        int fontSize = 14;

        public Changelog()
        {
            InitializeComponent();

            ChangelogText.Text = "Downloading changelog...";
            ChangelogText.FontSize = fontSize;

            this.Topmost = true;

            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
            wc.DownloadStringAsync(new Uri(changelogLink));
        }

        private void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            ChangelogText.Text = e.Result;
        }

        private void FontSizeIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                fontSize = fontSize + 10;
                if (fontSize >= 1)
                {
                    ChangelogText.FontSize = fontSize;
                }
                return;
            }
            else
            {
                fontSize = fontSize + 1;
                if (fontSize >= 1)
                {
                    ChangelogText.FontSize = fontSize;
                }
                return;
            }
        }

        private void FontSizeDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (fontSize == 1)
            {
                return;
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    fontSize = fontSize - 10;
                    if (fontSize >= 1)
                    {
                        ChangelogText.FontSize = fontSize;
                    }
                    else
                    {
                        fontSize = 1;
                        ChangelogText.FontSize = fontSize;
                    }
                    return;
                }
                else
                {
                    fontSize = fontSize - 1;
                    if (fontSize >= 1)
                    {
                        ChangelogText.FontSize = fontSize;
                    }
                    else
                    {
                        fontSize = 1;
                        ChangelogText.FontSize = fontSize;
                    }
                    return;
                }
            }
        }
    }
}
