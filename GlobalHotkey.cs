using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Hotkeys
{
    public class GlobalHotkeyManager
    {

        //windows message id for hotkey
        public const int HotkeyWMId = 0x0312;

        public Dictionary<int, RegisteredKey> RegisteredKeys;

        public delegate void ActionDelegate();

        public GlobalHotkeyManager()
        {
            RegisteredKeys = new Dictionary<int, RegisteredKey>();
        }

        public int Register(int modifier, Keys key, Form form, ActionDelegate action)
        {
            var r = new RegisteredKey()
            {
                modifier = modifier,
                key = (int)key,
                hWnd = form.Handle,
                form = form,
                action = action
            };
            RegisteredKeys.Add(GetHashCode(r),r);
            if (RegisterHotKey(r.hWnd, r.id, r.modifier, r.key))
                return r.id;
            else
                return 0;
        }

        public bool Unregister(int id)
        {
            if (UnregisterHotKey(RegisteredKeys[id].hWnd, id))
            {
                RegisteredKeys.Remove(id);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UnregisterAll()
        {
            List<int> keys = new List<int>();

            foreach (int i in RegisteredKeys.Keys)
                keys.Add(i);
            foreach (int i in keys)
                Unregister(i);
        }

        private int GetHashCode(RegisteredKey r)
        {
            return r.modifier ^ r.key ^ r.hWnd.ToInt32();
        }

        public void HandleMessage(Message m)
        {
            if (m.Msg == HotkeyWMId) {

                int iParam = (int)m.LParam;

                int key = ( (iParam>> 16) & 0xFFFF );
                int modifier = iParam & 0xFFFF;

                foreach( KeyValuePair<int, RegisteredKey> kvp in RegisteredKeys )
                {
                    if(kvp.Value.key == key && kvp.Value.modifier == modifier)
                    {
                        kvp.Value.form.Invoke(kvp.Value.action);// action();
                    }
                }
            }

        }

        [DllImport("user32.dll", SetLastError=true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    public class RegisteredKey {
        public int key;
        public int modifier;
        public IntPtr hWnd;
        public Form form;
        public int id;
        public Hotkeys.GlobalHotkeyManager.ActionDelegate action;
    }

    public class KeyModifier
    {
        private const int
        _None = 0x0000,
        _Alt = 0x0001,
        _Ctrl = 0x0002,
        _Shift = 0x0004,
        _Win = 0x0008;

        public static int Alt {
            get { return _Alt; }
        }

        public static int Ctrl {
            get { return _Ctrl; }
        }

        public static int Shift {
            get { return _Shift; }
        }

        public static int Win {
            get { return _Win; }
        }

        public static int None {
            get { return _None; }
        }

        public static int CtrlAlt
        {
            get { return _Ctrl | _Alt; }
        }

        public static int CtrlShift {
            get { return _Ctrl | _Shift; }
        }

        public static int ShiftAlt
        {
            get { return _Shift | _Alt; }
        }

        public static int CtrlAltShift
        {
            get { return _Ctrl | _Alt | _Shift; }
        }
    }
}
