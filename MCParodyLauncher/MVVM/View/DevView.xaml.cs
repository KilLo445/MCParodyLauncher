using System.Windows;
using System.Windows.Controls;
using Microsoft.Toolkit.Uwp.Notifications;

namespace MCParodyLauncher.MVVM.View
{
    public partial class DevView : UserControl
    {
        public DevView()
        {
            InitializeComponent();
        }

        private void btnToast_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MainWindow.usNotifications == true)
            {
                new ToastContentBuilder()
                .AddText("Toast Notification")
                .AddText("This is a test Toast Notification.")
                .Show();
            }
            else
            {
                MessageBox.Show("Notifications are disabled!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnHideWindow_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Hide();
        }

        private void pbVisibility_Checked(object sender, RoutedEventArgs e)
        {
            DLProgress.Visibility = Visibility.Visible;
            pbVisibility.IsChecked = true;
        }

        private void pbVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            DLProgress.Visibility = Visibility.Hidden;
            pbVisibility.IsChecked = false;
        }
    }
}
