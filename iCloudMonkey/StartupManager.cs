using Microsoft.Win32;

namespace iCloudMonkey
{
    internal class StartupManager
    {
        private const string AppName = "iCloudMonkey"; // Change to your app name

        public static void EnableAutoStart()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key?.SetValue(AppName, Application.ExecutablePath);
            }
        }

        public static void DisableAutoStart()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key?.DeleteValue(AppName, false);
            }
        }

        public static bool IsAutoStartEnabled()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return key?.GetValue(AppName) != null;
            }
        }
    }
}
