using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Automation;


namespace iCloudMonkey
{
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
        private static string pauseText = "Pause";
        private static string resumeText = "Resume";
        private static string autostartText = "Auto Start";
        private static string aboutText = "About";

        private static bool isMonitoring = true;

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem toggleItem;
        private ToolStripMenuItem autoStartItem;
        private AutomationEventHandler windowOpenedHandler;

        public static uint ActionCount { get; private set; } = 0;

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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            trayIcon.Visible = false;
            base.OnFormClosing(e);
        }

        private void ToggleMonitoring(object? sender, EventArgs e)
        {
            isMonitoring = !isMonitoring;
            toggleItem.Text = isMonitoring ? pauseText : resumeText;
            InitialCheck();
        }

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

        private void TrayIcon_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                MethodInfo? mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi?.Invoke(trayIcon, null);
            }

        }

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

        private void HideWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_HIDE); // Hide the window if found
            ActionCount++;
        }

        private void SetAutostartMenuItem()
        {
            autoStartItem.Checked = StartupManager.IsAutoStartEnabled();
        }

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
