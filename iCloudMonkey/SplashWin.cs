using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iCloudMonkey
{
    public partial class SplashWin : Form
    {
        public SplashWin()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.TopMost = true;
        }

        static public void ShowSplashWindow()
        {
            using (SplashWin splash = new SplashWin())
            {
                splash.Show();
                Application.DoEvents(); // Ensure the splash window is shown immediately
                System.Threading.Thread.Sleep(3000); // Simulate loading time
                splash.Close();
            }
        }
    }
}
