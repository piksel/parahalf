using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Hotkeys;
using System.IO;
using System.Threading;

namespace Parahalf
{
    public partial class FormView : Form
    {
        Rectangle screenBounds;
        Boolean shouldShow;

        int viewHeight;
        double viewAlpha;

        Settings settings;

        List<AppViewHandle> appViewHandles;

        GlobalHotkeyManager ghm;

        int currView = -1;

        bool disableHotkeys = false;

        delegate void KeyDelegate();

        public FormView()
        {
            InitializeComponent();
            screenBounds = Screen.PrimaryScreen.Bounds;
            Top = -6 - screenBounds.Top - (screenBounds.Height / 2);
            Width = screenBounds.Width;
            Height = screenBounds.Height / 2;
            Opacity = 0.0;
            ghm = new GlobalHotkeyManager();
        }

        private AppViewHandle spawnProc(AppView av)
        {

            AppViewHandle avh = new AppViewHandle();

            // Start the process

            if (String.IsNullOrWhiteSpace(av.appFile)) return avh;

            avh.settings = av;

            var psh = new ProcessStartInfo(av.appFile, av.appArgs);
            psh.WindowStyle = ProcessWindowStyle.Minimized;
            avh.process = new Process();
            avh.process.StartInfo = psh;
            avh.process.Start();
            avh.process.WaitForInputIdle();

            int retries = 0;
            while (retries<10)
            {
                switch (av.findWindowMethod)
                {
                    case FindWindowMethod.CaptionSearch:
                        avh.handle = User32.FindWindowByCaption(av.captionMatch);
                        break;
                    case FindWindowMethod.MainWindow:
                        avh.handle = avh.process.MainWindowHandle;
                        break;
                }
                if (avh.handle == IntPtr.Zero)
                {
                    Thread.Sleep(100);
                    retries++;
                }
                else break;
            }
            avh.panel = new Panel();
            avh.panel.BackColor = Color.FromArgb(av.background);
            avh.panel.Width = Width;
            avh.panel.Height = GetHeightFromPercent(avh.settings.coverPercent);
            avh.panel.Top = 0;
            avh.panel.Left = 0;
            avh.panel.Visible = false;
            this.Controls.Add(avh.panel);

            Thread.Sleep(av.initDelay);

            // Set the panel control as the application's parent
            User32.SetParent(avh.handle, avh.panel.Handle);

            // Maximize application
            if(av.needsMaximize)
                User32.SendMessage(avh.handle, 274, 61488, 0);

            // Set window style to no borders
            var lStyle = User32.GetWindowLong(avh.handle, User32.GWL_STYLE);
            lStyle &= ~(User32.WS_CAPTION | User32.WS_THICKFRAME | User32.WS_SYSMENU);
            User32.SetWindowLong(avh.handle, User32.GWL_STYLE, lStyle);

            int lExStyle = User32.GetWindowLong(avh.handle, User32.GWL_EXSTYLE);
            lExStyle &= ~(User32.WS_EX_DLGMODALFRAME | User32.WS_EX_CLIENTEDGE | User32.WS_EX_STATICEDGE);
            User32.SetWindowLong(avh.handle, User32.GWL_EXSTYLE, lExStyle);

            // Call pRedraw
            pRedraw(avh, av);

            return avh;

        }

        private void pRedraw(AppViewHandle avh, AppView av)
        {
            int swpFlags = User32.SWP_FRAMECHANGED | User32.SWP_NOZORDER;
            if (!av.needsMove) swpFlags |= User32.SWP_NOMOVE;
            if (!av.needsResize) swpFlags |= User32.SWP_NOSIZE;

            User32.SetWindowPos(avh.handle, IntPtr.Zero, 0, 0, screenBounds.Width, GetHeightFromPercent(av.coverPercent), swpFlags);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            initAll();
        }

        GlobalHotkeyManager.ActionDelegate GetDelegate(int i)
        {
            return delegate()
            {
                HandleHotkey(i);
            };
        }

        private void initAll()
        {
            appViewHandles = new List<AppViewHandle>();

            string settingsFile = "settings.json";
            if (File.Exists(settingsFile))
            {
                var fs = File.Open(settingsFile, FileMode.Open);
                var sr = new StreamReader(fs);
                settings = Settings.FromJson(sr.ReadToEnd());
                ghm = new GlobalHotkeyManager();

                for (int i = 0; i < settings.appViews.Count; i++)
                {
                    appViewHandles.Add(spawnProc(settings.appViews[i]));
                    ghm.Register(
                        settings.appViews[i].hotKeyModifier,
                        settings.appViews[i].hotKey,
                        this,
                        GetDelegate(i));

                    var tsmiView = new ToolStripMenuItem(settings.appViews[i].name);

                    var tsmiRebind = new ToolStripMenuItem("Rebind");
                    tsmiRebind.Enabled = false;
                    tsmiView.DropDownItems.Add(tsmiRebind);

                    var tsmiRelaunch = new ToolStripMenuItem("Relaunch");
                    tsmiRelaunch.Enabled = false;
                    tsmiView.DropDownItems.Add(tsmiRelaunch);

                    viewsToolStripMenuItem.DropDownItems.Add(tsmiView);
                }
                tAnimate.Interval = settings.animationDelay;
            }
            else
            {
                settings = new Settings();
                if (MessageBox.Show("You have not configured any settings.\nDo you want to do that now?",
                    "No settings file found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    settingsToolStripMenuItem_Click(null, null);
                }
            }

        }

        private void deInitAll()
        {
            if (ghm != null)
                ghm.UnregisterAll();

            foreach(AppViewHandle avh in appViewHandles)
                if (avh.process != null && !avh.process.HasExited)
                {
                    User32.SendMessage(avh.handle, User32.WM_SYSCOMMAND, User32.SC_CLOSE, 0);
                    if(!avh.process.WaitForExit(1000)) avh.process.Kill();
                }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {

        }

        protected override void WndProc(ref Message m)
        {
            ghm.HandleMessage(m);
            base.WndProc(ref m);
        }

        void HandleHotkey(int view)
        {
            if (currView == -1 || currView == view)
            {
                currView = view;
                viewAlpha = ((float)appViewHandles[currView].settings.alpha / 255.0);
                viewHeight = GetHeightFromPercent(appViewHandles[currView].settings.coverPercent);
                Height = viewHeight;
                toggleShow();
            }
        }

        private int GetHeightFromPercent(int p)
        {
            float percent = (float)p / 100;
            return (int)Math.Round(screenBounds.Height * percent);
        }


        private void toggleShow()
        {
            tAnimate.Enabled = true;
            shouldShow = !shouldShow;
        }

        private const int CS_DROPSHADOW = 0x00020000;
        private bool disabled;

        protected override CreateParams CreateParams
        {
            get
            {
                // add the drop shadow flag for automatically drawing
                // a drop shadow around the form
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private void tAnimate_Tick(object sender, EventArgs e)
        {
            if (shouldShow){
                if (!appViewHandles[currView].panel.Visible)
                {
                    User32.SetForegroundWindow(this.Handle);
                    appViewHandles[currView].panel.Visible = true;
                }
                if (Top + settings.downMovementSpeed > 0)
                {
                    Top = 0;
                    tAnimate.Enabled = false;

                    User32.SetForegroundWindow(appViewHandles[currView].handle);
                }
                else Top += settings.downMovementSpeed;

                if (Opacity < viewAlpha) Opacity += ((float)settings.fadeInSpeed / 100);
                if (Opacity > viewAlpha) Opacity = viewAlpha;
            }
            else {
                if (Top > (-6 - viewHeight)) Top -= settings.upMovementSpeed;
                if (Top <= (-6 - viewHeight))
                {
                    Top = (-6 - viewHeight);
                    tAnimate.Enabled = false;
                    appViewHandles[currView].panel.Visible = false;
                    currView = -1;
                }
                if (Opacity > 0.0) Opacity -= ((float)settings.fadeOutSpeed / 100);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            deInitAll();
        }

        private void tWatcher_Tick(object sender, EventArgs e)
        {
            //if (proc.HasExited)
            //{
            //    spawnProc();
            //}
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fs = new FormSettings();
            if (fs.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                deInitAll();
                initAll();
            }
        }

        public struct AppViewHandle
        {
            public Process process;
            public IntPtr handle;
            public Panel panel;
            public AppView settings;
        }

        private void parahalfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fa = new FormAbout();
            fa.ShowDialog();
        }

        private void disableHotkeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            disableHotkeys = !disableHotkeys;

            if (disableHotkeys)
            {
                ghm.UnregisterAll();
                disableHotkeysToolStripMenuItem.Text = "Enable hotkeys";
            }
            else
            {
                for (int i = 0; i < settings.appViews.Count; i++)
                {
                    ghm.Register(
                        settings.appViews[i].hotKeyModifier,
                        settings.appViews[i].hotKey,
                        this,
                        GetDelegate(i));
                }
                disableHotkeysToolStripMenuItem.Text = "Disable hotkeys";
            }
        }
    }
}
