using System.Windows;

namespace Notification.FrameworkDependent
{
    public static class WpfHelpers
    {
        public static void ActivateApp()
        {
            Application.Current.MainWindow.Activate();
        }
    }
}
