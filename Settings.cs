using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using Hotkeys;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Parahalf
{
    public class AppView
    {
        public string name;
        public string appFile;
        public string appArgs;
        public FindWindowMethod findWindowMethod;
        public string captionMatch;
        public bool launchAtStart;
        public Keys hotKey;
        public int hotKeyModifier;
        public bool needsMaximize = false;
        public bool needsMove = false;
        public bool needsResize = false;
        public int initDelay = 0;
        public int alpha = 160;
        public int coverPercent = 50;
        public int background = 255 << 24 + 0 << 16 + 0 << 8 + 0;

        public override string ToString()
        {
            return name;
        }
    }

    public enum FindWindowMethod { CaptionSearch = 0, MainWindow = 1 }

    public class Settings
    {
        public BindingList<AppView> appViews;

        public int animationDelay = 10;

        public int fadeInSpeed = 5;
        public int fadeOutSpeed = 5;

        public int upMovementSpeed = 20;
        public int downMovementSpeed = 20;

        public Settings()
        {
            appViews = new BindingList<AppView>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static Settings FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Settings>(json);
        }

        internal void AddTestData()
        {
            appViews.Add(new AppView
            {
                name = "notepad",
                appFile = "notepad.exe",
                appArgs = "",
                captionMatch = "",
                findWindowMethod = FindWindowMethod.MainWindow,
                launchAtStart = true,
                hotKeyModifier = KeyModifier.CtrlAlt,
                hotKey = Keys.N
            });
            appViews.Add(new AppView
            {
                name = "console",
                appFile = "console.exe",
                appArgs = "",
                captionMatch = "Console2",
                findWindowMethod = FindWindowMethod.CaptionSearch,
                launchAtStart = true,
                hotKeyModifier = KeyModifier.None,
                hotKey = Keys.Oem5
            });
        }
    }
}
