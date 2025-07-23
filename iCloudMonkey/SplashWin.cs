using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace iCloudMonkey
{
    /// <summary>
    /// Represents a splash screen window displayed during application startup or specific operations.
    /// </summary>
    /// <remarks>The <see cref="SplashWin"/> class is a non-interactive, borderless window designed to provide
    /// visual feedback to the user during application initialization or other processes. It is displayed in the center
    /// of the screen and remains on top of all other windows.</remarks>
    public partial class SplashWin : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplashWin"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the splash screen with no border, centers it on the screen, 
        /// hides it from the taskbar, and ensures it is displayed above all other windows.</remarks>
        public SplashWin()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.TopMost = true;
        }

        /// <summary>
        /// Displays a splash window with application information and an optional action count message.
        /// </summary>
        /// <remarks>The splash window provides basic application details, including the version and
        /// description, and remains visible for a short duration before automatically closing. This method is typically
        /// used to provide a brief loading screen or introductory message to the user.</remarks>
        /// <param name="showActionCount">A boolean value indicating whether to display the current action count in the splash window. If <see
        /// langword="true"/>, the action count is shown; otherwise, it is omitted.</param>
        static public void ShowSplashWindow(bool showActionCount = false)
        {
            using (SplashWin splash = new SplashWin())
            {
                Assembly? assembly = Assembly.GetExecutingAssembly();
                Version? version = Assembly.GetExecutingAssembly().GetName().Version;
                splash.label1.Text = $"iCloudMonkey {version}";
                splash.label2.Text = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
                splash.label3.Text = showActionCount?$"{MainWin.ActionCount} wiped popups so far!":"";
                splash.Show();
                Application.DoEvents(); // Ensure the splash window is shown immediately
                System.Threading.Thread.Sleep(3000); // Simulate loading time
                splash.Close();
            }
        }
    }
}
