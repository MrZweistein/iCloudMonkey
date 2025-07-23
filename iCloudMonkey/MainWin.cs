using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Automation;


namespace iCloudMonkey
{
    /// <summary>
    /// Represents the main application window for managing system tray interactions and monitoring window events.
    /// </summary>
    /// <remarks>The <see cref="MainWin"/> class is responsible for initializing the system tray icon, context
    /// menu, and event handlers for monitoring specific window events. It operates primarily as a background
    /// application, hiding its main window and providing functionality through the system tray. Key features include:
    /// <list type="bullet"> <item>Monitoring and hiding specific windows based on their title.</item> <item>Providing a
    /// context menu for toggling monitoring, managing autostart settings, and exiting the application.</item>
    /// <item>Handling UI Automation events to detect and act on window state changes.</item> </list> This class is
    /// designed to run as a minimized application, with its primary interface being the system tray icon.</remarks>
    public partial class MainWin : Form
    {
        // Win32 API imports
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsIconic(IntPtr hWnd);

        private const int SW_HIDE = 0;

        private static string targetWindowTitle = "iCloud"; // CHANGE TO TARGET WINDOW TITLE
        private static string resumeText = "Resume";
        private static string pauseText = "Pause";
        private static string autostartText = "Auto Start";
        private static string aboutText = "Info";

        private static bool isMonitoring = true;

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem toggleItem;
        private ToolStripMenuItem autoStartItem;
        private AutomationEventHandler windowOpenedHandler;

        public static uint ActionCount { get; private set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWin"/> class, setting up the system tray icon, context
        /// menu, and event handlers for monitoring window events.
        /// </summary>
        /// <remarks>This constructor configures the application's system tray icon and its associated
        /// context menu, hides the main window, and sets up event handlers for monitoring window-opened events using UI
        /// Automation. It also initializes the autostart menu item and performs an initial check for the application's
        /// state.</remarks>
        public MainWin()
        {
            toggleItem = new ToolStripMenuItem(pauseText, null, ToggleMonitoring);
            autoStartItem = new ToolStripMenuItem(autostartText, null, ToggleAutostart);

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add(toggleItem);
            trayMenu.Items.Add(autoStartItem);
            trayMenu.Items.Add(aboutText, null, (s, e) => SplashWin.ShowSplashWindow(true));
            trayMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

            trayIcon = new NotifyIcon
            {
                Icon = Properties.Resources.AppIcon,
                ContextMenuStrip = trayMenu,
                Visible = true,
                Text = "iCloudMonkey",
            };
            trayIcon.MouseUp += TrayIcon_MouseUp;

            // Hide main window
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Load += (s, e) => this.Hide();

            // Hook window opened events
            windowOpenedHandler = new AutomationEventHandler(OnWindowOpened);
            Automation.AddAutomationEventHandler(
                WindowPattern.WindowOpenedEvent,
                AutomationElement.RootElement,
                TreeScope.Subtree,
                windowOpenedHandler
            );

            SetAutostartMenuItem();
            InitialCheck();
        }

        /// <summary>
        /// Handles the <see cref="Form.FormClosing"/> event.
        /// </summary>
        /// <remarks>This method ensures that the tray icon is hidden before the form is closed.  It also
        /// invokes the base class implementation to perform standard form-closing operations.</remarks>
        /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            trayIcon.Visible = false;
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Toggles the monitoring state between active and inactive.
        /// </summary>
        /// <remarks>This method updates the monitoring state and adjusts the associated UI element's text
        /// to reflect the current state. It also performs an initial check after toggling.</remarks>
        /// <param name="sender">The source of the event that triggered the toggle.</param>
        /// <param name="e">The event data associated with the toggle action.</param>
        private void ToggleMonitoring(object? sender, EventArgs e)
        {
            isMonitoring = !isMonitoring;
            toggleItem.Text = isMonitoring ? pauseText : resumeText;
            InitialCheck();
        }

        /// <summary>
        /// Handles the event triggered when a new window is opened in the UI Automation tree.
        /// </summary>
        /// <remarks>This method checks if the opened window matches the specified target window title
        /// and, if so, hides the window. The method will not perform any action if monitoring is disabled or if the
        /// sender is not a valid <see cref="AutomationElement"/>.</remarks>
        /// <param name="sender">The source of the event, typically an <see cref="AutomationElement"/> representing the window that was
        /// opened.</param>
        /// <param name="e">The event data associated with the window opened event.</param>
        private void OnWindowOpened(object sender, AutomationEventArgs e)
        {
            if (!isMonitoring) return;

            var element = sender as AutomationElement;
            if (element == null) return;

            string name = element.Current.Name;
            if (name == targetWindowTitle)
            {
                IntPtr hWnd = new IntPtr(element.Current.NativeWindowHandle);
                if (hWnd != IntPtr.Zero)
                {
                    HideWindow(hWnd); // Hide the window 
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="NotifyIcon.MouseUp"/> event to display the context menu for the tray icon.
        /// </summary>
        /// <remarks>This method displays the context menu associated with the tray icon when the user
        /// releases either the left or right mouse button.</remarks>
        /// <param name="sender">The source of the event, typically the <see cref="NotifyIcon"/> instance.</param>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data, including information about the mouse button
        /// pressed.</param>
        private void TrayIcon_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                MethodInfo? mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi?.Invoke(trayIcon, null);
            }

        }

        /// <summary>
        /// Performs an initial check to determine if the target window is open, visible, and not minimized, and hides
        /// it if these conditions are met.
        /// </summary>
        /// <remarks>This method checks for a window with the specified title and evaluates its visibility
        /// and state. If the window is found, visible, and not minimized, it will be hidden.</remarks>
        private void InitialCheck()
        {
            // Check if the target window is already open
            IntPtr hWnd = FindWindow(null, targetWindowTitle);
            if (hWnd != IntPtr.Zero)
            {
                bool isVisible = IsWindowVisible(hWnd);
                bool isMinimized = IsIconic(hWnd);

                if (isVisible && !isMinimized)
                {
                    HideWindow(hWnd); // Hide the window if it is visible and not minimized
                }
            }
        }

        /// <summary>
        /// Hides the specified window by setting its visibility to hidden and incrementing the action count.
        /// </summary>
        /// <remarks>This method uses the <see cref="ShowWindow"/> function to change the visibility of
        /// the window. The window will no longer be visible on the screen after this method is called.</remarks>
        /// <param name="hWnd">A handle to the window to be hidden. Must be a valid window handle.</param>
        private void HideWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_HIDE); // Hide the window if found
            ActionCount++;
        }

        /// <summary>
        /// Updates the state of the autostart menu item to reflect whether the application is configured to start
        /// automatically.
        /// </summary>
        /// <remarks>This method checks the current autostart configuration using the <see
        /// cref="StartupManager.IsAutoStartEnabled"/> method  and updates the <c>Checked</c> property of the
        /// <c>autoStartItem</c> menu item accordingly.</remarks>
        private void SetAutostartMenuItem()
        {
            autoStartItem.Checked = StartupManager.IsAutoStartEnabled();
        }

        /// <summary>
        /// Toggles the application's autostart setting.
        /// </summary>
        /// <remarks>This method checks the current autostart status of the application and switches it to
        /// the opposite state. If autostart is enabled, it will be disabled; if it is disabled, it will be enabled. The
        /// method also updates the autostart menu item to reflect the new state.</remarks>
        /// <param name="sender">The source of the event that triggered the toggle. Can be <see langword="null"/>.</param>
        /// <param name="e">The event data associated with the toggle action.</param>
        private void ToggleAutostart(object? sender, EventArgs e)
        {
            bool isOn = StartupManager.IsAutoStartEnabled();
            if (isOn)
            {
                StartupManager.DisableAutoStart();
            }
            else
            {
                StartupManager.EnableAutoStart();
            }
            SetAutostartMenuItem();
        }

    }
}
