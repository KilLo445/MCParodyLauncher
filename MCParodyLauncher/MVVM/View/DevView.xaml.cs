using System;
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
            new ToastContentBuilder()
            .AddText("Toast Notification")
            .AddText("This is a test Toast Notification.")
            .Show();
        }
    }
}
