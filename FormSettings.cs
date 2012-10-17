using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Hotkeys;
using System.IO;

namespace Parahalf
{
    public partial class FormSettings : Form
    {
        bool keyInput = false;
        bool winKeyPressed = false;
        Keys selectedKey;

        int prevSelected = -1;

        Settings settings;

        BindingList<string> avNames;

        public FormSettings()
        {
            InitializeComponent();
        }

        private void button6_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void button6_KeyDown(object sender, KeyEventArgs e)
        {
            if (!keyInput) return;
            e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
                 winKeyPressed = true;
        }

        private void button6_KeyUp(object sender, KeyEventArgs e)
        {
            if (!keyInput) return;
            e.SuppressKeyPress = true;
            if (!KeyIsMod(e.KeyCode))
            {
                Keys kComp = (e.Modifiers & Keys.LWin);
                cbModAlt.Checked = e.Alt;
                cbModCtrl.Checked = e.Control;
                cbModShift.Checked = e.Shift;
                cbModWin.Checked = winKeyPressed;
                SetHotkey(e.KeyCode);
                keyInput = false;
                winKeyPressed = false;
                bSetKeys.Text = (string)bSetKeys.Tag;
            }
        }

        private void SetHotkey(Keys k)
        {
            selectedKey = k;
            tbKey.Text = k.ToString();
            tbLocalKey.Text = GetLocalKey(k);
        }

        private string GetLocalKey(Keys k)
        {
            return User32.GetCharsFromKeys(k, false, false).ToString();
        }

        private bool KeyIsMod(Keys k)
        {
            return k == Keys.ControlKey
                || k == Keys.Alt
                || k == Keys.ShiftKey
                || k == Keys.LWin
                || k == Keys.RWin
                || k == Keys.Menu;
        }

        private void bSetKeys_Click(object sender, EventArgs e)
        {
            bSetKeys.Text = "Press a key combination";
            keyInput = true;
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {

            MenuAddKeyRange(Keys.A, Keys.Z, lettersToolStripMenuItem);

            MenuAddKeyRange(Keys.D0, Keys.D9, digitsToolStripMenuItem);

            MenuAddKeyRange(Keys.NumPad0, Keys.NumPad9, numPadToolStripMenuItem); 

            MenuAddKeyRange(Keys.F1, Keys.F24, functionKeysToolStripMenuItem); 

            MenuAddKeys(new List<Keys> {
                {Keys.BrowserBack},
                {Keys.BrowserFavorites},
                {Keys.BrowserForward},
                {Keys.BrowserHome},
                {Keys.BrowserRefresh},
                {Keys.BrowserSearch},
                {Keys.BrowserStop},
                {Keys.MediaPlayPause},
                {Keys.MediaNextTrack},
                {Keys.MediaPreviousTrack},
                {Keys.MediaStop},
                {Keys.SelectMedia},
                {Keys.VolumeUp},
                {Keys.VolumeMute},
                {Keys.VolumeDown}
            }, multimediaToolStripMenuItem);

            MenuAddKeys(new List<Keys> {
                {Keys.Back},
                {Keys.Scroll},
                {Keys.NumLock},
                {Keys.PrintScreen},
                {Keys.Pause},
                {Keys.PageUp},
                {Keys.PageDown},
                {Keys.Home},
                {Keys.End},
                {Keys.Insert},
                {Keys.Delete}
            }, specialToolStripMenuItem);

            MenuAddKeys(new List<Keys> {
                {Keys.Oem1},
                {Keys.Oem2},
                {Keys.Oem3},
                {Keys.Oem4},
                {Keys.Oem5},
                {Keys.Oem6},
                {Keys.Oem7},
                {Keys.Oem8},
                {Keys.Oem102}
            }, oEMToolStripMenuItem);

            bSetKeys.Text = (string)bSetKeys.Tag;

            string settingsFile = "settings.json";

            if(File.Exists(settingsFile)){
                try
                {
                    var fs = File.Open(settingsFile, FileMode.OpenOrCreate);
                    var sr = new StreamReader(fs);

                    settings = Settings.FromJson(sr.ReadToEnd());
                }
                catch (Exception x)
                {
                    MessageBox.Show("Error when reading settings:\n  "  + x.Message);
                    settings = new Settings();
                }
            }
            else {
                settings = new Settings();
            }

            //settings.AddTestData();

            avNames = new BindingList<string>();
            foreach (AppView av in settings.appViews)
                avNames.Add(av.name);

            lbAppViews.DataSource = avNames;
            lbAppViews.DisplayMember = "name";

            tbAnimationSpeed.Value = (100 / settings.animationDelay);
            tbDownMovementSpeed.Value = settings.downMovementSpeed;
            tbUpMovementSpeed.Value = settings.upMovementSpeed;

            tbFadeInSpeed.Value = settings.fadeInSpeed * 5;
            tbFadeOutSpeed.Value = settings.fadeOutSpeed * 5;

            UpdateValues();

            
        }

        private void UpdateValues()
        {
            cbFindWindowMethod_SelectedIndexChanged(null, null);
            trackBar1_ValueChanged(null, null);
            tbFadeOutSpeed_ValueChanged(null, null);
            tbDownMovementSpeed_ValueChanged(null, null);
            tbUpMovementSpeed_ValueChanged(null, null);
            tbAnimationSpeed_ValueChanged(null, null);

        }

        void MenuAddKeyRange(Keys start, Keys end, ToolStripMenuItem menu)
        {
            for (int i = (int)start; i <= (int)end; i++)
            {
                var m = menu.DropDownItems.Add(((Keys)i).ToString());
                m.Tag = i;
                m.Click += new EventHandler(keyMenuItem_Click);
            }
        }

        void keyMenuItem_Click(object sender, EventArgs e)
        {
            Keys k = (Keys)(((ToolStripMenuItem)sender).Tag);
            SetHotkey(k);
        }

        void MenuAddKeys(List<Keys> kl, ToolStripMenuItem menu)
        {
            for (int i = 0; i < kl.Count; i++)
            {
                var m = menu.DropDownItems.Add(kl[i].ToString());
                m.Tag = kl[i];
                m.Click += new EventHandler(keyMenuItem_Click);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var c = (Control)sender;
            cmsKeys.Show(c, new Point(c.Width,0));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Save first tab
            int curIndex = lbAppViews.SelectedIndex;
            lbAppViews.DataSource = null;
            if(curIndex>=0) SaveAV(curIndex);

            // Save second tab
            settings.animationDelay = 100 / tbAnimationSpeed.Value;
            settings.downMovementSpeed = tbDownMovementSpeed.Value;
            settings.upMovementSpeed = tbUpMovementSpeed.Value;

            settings.fadeInSpeed = tbFadeInSpeed.Value / 5;
            settings.fadeOutSpeed = tbFadeOutSpeed.Value / 5;

            var fs = File.Open("settings.json", FileMode.Create);
            var sw = new StreamWriter(fs);
            sw.Write(settings.ToJson());
            sw.Close();
            Close();
        }

        private void lbAppViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (prevSelected >= 0 && lbAppViews.SelectedIndex != prevSelected)
            {
                SaveAV(prevSelected);
            }
            if (lbAppViews.SelectedIndex < 0)
            {
                gbViewSettings.Enabled = false;
                return;
            }
            //UpdateList();
            var av = (settings.appViews[lbAppViews.SelectedIndex]);
            LoadAV(av);
            gbViewSettings.Enabled = true;
            prevSelected = lbAppViews.SelectedIndex;
        }

        private void UpdateList()
        {
            int oldIndex;
            oldIndex = lbAppViews.SelectedIndex;
            lbAppViews.DataSource = null;
            lbAppViews.DataSource = settings.appViews;
            lbAppViews.SelectedIndex = (oldIndex >= 0) ? oldIndex : 0;
        }

        private void SaveAV(int index)
        {
            var av = new AppView
            {
                name = tbName.Text,
                appFile = tbAppFile.Text,
                appArgs = tbAppArgs.Text,
                captionMatch = tbCaptionMatch.Text,
                findWindowMethod = (FindWindowMethod)cbFindWindowMethod.SelectedIndex,
                hotKey = selectedKey,
                hotKeyModifier = (cbModAlt.Checked ? KeyModifier.Alt : 0)
                    | (cbModCtrl.Checked ? KeyModifier.Ctrl : 0)
                    | (cbModShift.Checked ? KeyModifier.Shift : 0)
                    | (cbModWin.Checked ? KeyModifier.Win : 0),
                launchAtStart = cbLaunchAtStart.Checked,
                needsMaximize = cbMaximize.Checked,
                needsMove = cbMove.Checked,
                needsResize = cbResize.Checked,
                initDelay = int.Parse(tbDelay.Text),
                alpha = tbAlpha.Value,
                coverPercent = 100 - tbCover.Value,
                background = pBackground.BackColor.ToArgb()
            };
            if (settings.appViews[index] != av)
            {
                settings.appViews[index] = av;
                avNames[index] = av.name;
            }
        }

        private void LoadAV(AppView av)
        {
            tbName.Text = av.name;
            tbAppFile.Text = av.appFile;
            tbAppArgs.Text = av.appArgs;
            tbCaptionMatch.Text = av.captionMatch;
            cbFindWindowMethod.SelectedIndex = (int)av.findWindowMethod;
            SetHotkey(av.hotKey);
            cbModAlt.Checked = (av.hotKeyModifier & KeyModifier.Alt) == KeyModifier.Alt;
            cbModCtrl.Checked = (av.hotKeyModifier & KeyModifier.Ctrl) == KeyModifier.Ctrl;
            cbModShift.Checked = (av.hotKeyModifier & KeyModifier.Shift) == KeyModifier.Shift;
            cbModWin.Checked = (av.hotKeyModifier & KeyModifier.Win) == KeyModifier.Win;
            cbLaunchAtStart.Checked = av.launchAtStart;
            cbResize.Checked = av.needsResize;
            cbMove.Checked = av.needsMove;
            cbMaximize.Checked = av.needsMaximize;
            tbDelay.Text = av.initDelay.ToString();

            tbAlpha.Value = av.alpha;
            tbCover.Value = 100 - av.coverPercent;
            pBackground.BackColor = Color.FromArgb(av.background);
        }

        private void bDirAppFile_Click(object sender, EventArgs e)
        {
            ofdAppFile.FileName = tbAppFile.Text;
            if (ofdAppFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbAppFile.Text = ofdAppFile.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var reload = lbAppViews.SelectedIndex == 0;
            settings.appViews.Add(new AppView
            {
                name = "new View",
                appFile = "",
                appArgs = "",
                captionMatch = "",
                findWindowMethod = FindWindowMethod.MainWindow,
                launchAtStart = false,
                hotKey = Keys.None,
                hotKeyModifier = 0
            });
            avNames.Add("new View");
            lbAppViews.SelectedIndex = lbAppViews.Items.Count - 1;
            if(reload) lbAppViews_SelectedIndexChanged(null, null);
            //UpdateList();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cbFindWindowMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbCaptionMatch.Enabled = (cbFindWindowMethod.SelectedIndex == 0);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            lFadeInSpeedValue.Text = ((float)tbFadeInSpeed.Value / 50).ToString() + "%/tick";
        }

        private void tbFadeOutSpeed_ValueChanged(object sender, EventArgs e)
        {
            lFadeOutSpeedValue.Text = ((float)tbFadeOutSpeed.Value / 50).ToString() + "%/tick";
        }

        private void tbDownMovementSpeed_ValueChanged(object sender, EventArgs e)
        {
            lDownMovementSpeedValue.Text = tbDownMovementSpeed.Value.ToString() + "px/tick";
        }

        private void tbUpMovementSpeed_ValueChanged(object sender, EventArgs e)
        {
            lUpMovementSpeedValue.Text = tbUpMovementSpeed.Value.ToString() + "px/tick";
        }

        private void tbAnimationSpeed_ValueChanged(object sender, EventArgs e)
        {
            lAnimationSpeedValue.Text = (tbAnimationSpeed.Value * 10).ToString() + " ticks/s";
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lbAppViews.BeginUpdate();
            settings.appViews.RemoveAt(lbAppViews.SelectedIndex);
            prevSelected = -1;
            avNames.RemoveAt(lbAppViews.SelectedIndex);
            lbAppViews.EndUpdate();
        }

        private void lbAppViews_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //select the item under the mouse pointer
                lbAppViews.SelectedIndex = lbAppViews.IndexFromPoint(e.Location);
            }
        }

        private void tbCover_ValueChanged(object sender, EventArgs e)
        {
            lbCover.Text = (100 - tbCover.Value).ToString() + "%";
        }

        private void tbAlpha_ValueChanged(object sender, EventArgs e)
        {
            lbAlpha.Text = tbAlpha.Value.ToString();
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            cdBackground.Color = pBackground.BackColor;
            if (cdBackground.ShowDialog() == DialogResult.OK)
                pBackground.BackColor = cdBackground.Color;
        }
    }
}
