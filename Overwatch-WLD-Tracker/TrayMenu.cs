using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OverwatchWLDTracker.Forms;
using OverwatchWLDTracker.Properties;
using Microsoft.Win32;

namespace OverwatchWLDTracker
{
    public class TrayMenu : Form
    {
        public NotifyIcon trayIcon = new NotifyIcon();
        public ContextMenu trayMenu = new ContextMenu();
        public Format format = new Format();
        public bool formatOpen = false;

        public TrayMenu()
        {
            try
            {
                MenuItem debugTools = new MenuItem("Tools");
                debugTools.MenuItems.Add("Open logs", OpenLogs);

                trayMenu.MenuItems.Add("Overwatch WLD Tracker v" + Functions.GetFileVersion());
                trayMenu.MenuItems.Add("-");
                trayMenu.MenuItems.Add("Start with Windows", ToggleWindows);
                trayMenu.MenuItems.Add("-");
                trayMenu.MenuItems.Add("Format", formatShow);
                trayMenu.MenuItems.Add("Manual Override", overrideShow);
                trayMenu.MenuItems.Add("-");
                trayMenu.MenuItems.Add(debugTools);
                trayMenu.MenuItems.Add("Exit", OnExit);
                trayMenu.MenuItems[0].Enabled = false;

                if (Vars.settings.startWithWindows)
                {
                    trayMenu.MenuItems[2].Checked = true;
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        key?.SetValue("BetterOverwatch", "\"" + Application.ExecutablePath + "\"");
                    }
                }
                ChangeTray("Overwatch WLD Tracker by Wyoming", Resources.Icon);
                trayIcon.ContextMenu = trayMenu;
                trayIcon.Visible = true;
            }
            catch (Exception e) {
                throw e;
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }
        private void OnExit(object sender, EventArgs e)
        {
            if (formatOpen)
                format.Dispose(); // Need to terminate this manually, as the application won't close otherwise
            Application.Exit();
        }
        private void TrayPopup(string text, int timeout)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, int>(TrayPopup), text, timeout);
                return;
            }
            trayIcon.ShowBalloonTip(timeout, "Overwatch WLD Tracker", text, ToolTipIcon.None);
        }
        private void overrideShow(object sender, EventArgs e)
        {
            ManualOverride manualOverride = new ManualOverride();
            manualOverride.Show();
        }
        private void formatShow(object sender, EventArgs e)
        {
            formatOpen = true;
            format.Show();
        }
        private void OpenLogs(object sender, EventArgs e)
        {
            if (File.Exists(Path.Combine(Vars.configPath, "debug.log")))
            {
                Process.Start(Path.Combine(Vars.configPath, "debug.log"));
            }
        }
        private void ToggleWindows(object sender, EventArgs e)
        {
            if (trayMenu.MenuItems[2].Checked)
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("BetterOverwatch");
                    }
                }
            }
            else
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        key.SetValue("BetterOverwatch", "\"" + Application.ExecutablePath + "\"");
                    }
                }
            }
            trayMenu.MenuItems[2].Checked = !trayMenu.MenuItems[2].Checked;
            Vars.settings.startWithWindows = trayMenu.MenuItems[2].Checked;
            Settings.Save();
        }
        public void ChangeTray(string text, Icon icon)
        {
            Console.WriteLine(text);
            TrayPopup(text, 5000);
            trayIcon.Text = text;
            trayIcon.Icon = icon;
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