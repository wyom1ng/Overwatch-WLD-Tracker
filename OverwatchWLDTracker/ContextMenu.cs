using System;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

namespace OverwatchWLDTracker
{
    public class CustomMenu : Form
    {
        Format format = new Format();
        public NotifyIcon trayIcon = new NotifyIcon();
        public ContextMenu trayMenu = new ContextMenu();

        public CustomMenu()
        {
            try
            {
                trayMenu.MenuItems.Add("Overwatch WLD Tracker v" + Vars.version);
                trayMenu.MenuItems.Add("Format", formatShow);
                trayMenu.MenuItems.Add("Manual Override", overrideShow);
                trayMenu.MenuItems.Add("-");
                trayMenu.MenuItems.Add("Reset", Reset);
                trayMenu.MenuItems.Add("Exit", OnExit);
                trayMenu.MenuItems[0].Enabled = false;
                

                trayIcon.Text = "Waiting for Overwatch, idle...";
                trayIcon.Icon = Properties.Resources.Idle;
                trayIcon.ContextMenu = trayMenu;
                trayIcon.Visible = true;
            }
            catch
            {
            }
        }

        private void overrideShow(object sender, EventArgs e)
        {
            ManualOverride manualOverride = new ManualOverride();
            manualOverride.Show();
        }

        private void Reset(object sender, EventArgs e)
        {
            DialogResult result1 = MessageBox.Show("This will reset the W/L/D counter as well as your starting SR. Proceed?",
                                                    "Reset Tracker",
                                                    MessageBoxButtons.YesNo);
            if (result1 == DialogResult.Yes)
            {
                Application.Restart();
            }
        }

        private void formatShow(object sender, EventArgs e)
        {
            format.Show();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        public void trayPopup(string title, string text, int timeout)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, string, int>(trayPopup), new object[] { title, text, timeout });
                return;
            }
            Functions.DebugMessage(title);
            trayIcon.ShowBalloonTip(timeout, title, text, ToolTipIcon.None);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}