using Microsoft.Win32;

namespace iCloudMonkey
{
    /// <summary>
    /// Provides methods to manage the application's auto-start behavior on Windows.
    /// </summary>
    /// <remarks>This class allows enabling, disabling, and checking the auto-start status of the application
    /// by interacting with the Windows Registry. It modifies the "Run" key under the current user's registry hive to
    /// control whether the application starts automatically when the user logs in.</remarks>
    internal class StartupManager
    {
        private const string AppName = "iCloudMonkey"; // Change to your app name

        /// <summary>
        /// Configures the application to start automatically when the user logs into Windows.
        /// </summary>
        /// <remarks>This method adds the application's executable path to the Windows registry under the 
        /// "Run" key for the current user. The application will start automatically on user login  if this method is
        /// successfully executed.</remarks>
        public static void EnableAutoStart()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key?.SetValue(AppName, Application.ExecutablePath);
            }
        }

        /// <summary>
        /// Disables the automatic startup of the application by removing its entry from the Windows registry.
        /// </summary>
        /// <remarks>This method modifies the registry key located at 
        /// <c>HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run</c>.  It removes the value associated
        /// with the application name, if it exists. No exception is thrown if the value does not exist.</remarks>
        public static void DisableAutoStart()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key?.DeleteValue(AppName, false);
            }
        }

        /// <summary>
        /// Determines whether the application is configured to start automatically when the user logs in.
        /// </summary>
        /// <remarks>This method checks the Windows Registry under the current user's "Run" key to
        /// determine if the application is registered for auto-start. Ensure the application has the necessary
        /// permissions to access the registry when calling this method.</remarks>
        /// <returns><see langword="true"/> if the application is set to start automatically at login; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool IsAutoStartEnabled()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return key?.GetValue(AppName) != null;
            }
        }
    }
}
