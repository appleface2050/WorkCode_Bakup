using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.GameManager.gamemanager
{
    public delegate void HotkeyEventHandler(int HotKeyID);

    public class SystemHotKey : System.Windows.Forms.IMessageFilter
    {
        public event HotkeyEventHandler OnHotkey;
        List<UInt32> keyIDs = new List<UInt32>();
        IntPtr hWnd;
        public int BossKey_HotKey;

        public enum KeyFlags
        {
            Alt = 0x1,
            Ctrl = 0x2,
            Shift = 0x4,
            Win = 0x8,
            //组合键等于值相加
            Alt_Ctrl = 0x3,
            Alt_Shift = 0x5,
            Ctrl_Shift = 0x6,
            Alt_Ctrl_Shift = 0x7
        }
        [DllImport("user32.dll")]
        public static extern UInt32 RegisterHotKey(IntPtr hWnd, UInt32 id, UInt32 fsModifiers, UInt32 vk);

        [DllImport("user32.dll")]
        public static extern UInt32 UnregisterHotKey(IntPtr hWnd, UInt32 id);

        [DllImport("kernel32.dll")]
        public static extern UInt32 GlobalAddAtom(String lpString);

        [DllImport("kernel32.dll")]
        public static extern UInt32 GlobalDeleteAtom(UInt32 nAtom);

        public SystemHotKey(IntPtr hWnd)
        {
            this.hWnd = hWnd;
        }
        public void UpdateHandler(IntPtr hWnd)
        {
            this.hWnd = hWnd;
        }

        public bool RegisterHotkey(KeyFlags keyflags, System.Windows.Forms.Keys Key)
        {
            System.Windows.Forms.Application.AddMessageFilter(this);
            UInt32 hotkeyid = GlobalAddAtom(System.Guid.NewGuid().ToString());
            int ret = (int)RegisterHotKey((IntPtr)hWnd, hotkeyid, (UInt32)keyflags, (UInt32)Key);
            if (ret != 0)
            {
                BossKey_HotKey = (int)hotkeyid;
                keyIDs.Add(hotkeyid);
                return true ;
            }
            return false;   
        }

        public bool UpdateBossHotKey(IntPtr handle)
        {
            UnregisterHotkeys();
            UpdateHandler(handle);

            UserSettingsData settingsData = new UserSettingsData();
            settingsData.Init();
            if ((!string.IsNullOrEmpty(settingsData.keyString)) && (settingsData.keyValue > 0))
            {
                SystemHotKey.KeyFlags flags = (SystemHotKey.KeyFlags)(settingsData.keyValue >> 16);
                System.Windows.Forms.Keys key = (System.Windows.Forms.Keys)(0xffff & settingsData.keyValue);
                if (!RegisterHotkey(flags, key))
                {
                    Logger.Info(string.Format("RegisterHotkey fail flags: {0} key: {1}", flags, key));
                    string prompt = string.Format("“{0}”{1}", settingsData.keyString, Locale.Strings.BosskeyRegisterFailed);
                    PromptForm frm = new PromptForm(prompt);
                    frm.ShowDialog();
                    return false;
                }
            }
            return true;
        }

        public void UnregisterHotkeys()
        {
            if (keyIDs.Count > 0)
            {
                System.Windows.Forms.Application.RemoveMessageFilter(this);
                foreach (UInt32 key in keyIDs)
                {
                    UnregisterHotKey(hWnd, key);
                    GlobalDeleteAtom(key);
                }
                keyIDs.Clear();
            }
        }

        public bool PreFilterMessage(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == 0x312)
            {
                if (OnHotkey != null)
                {
                    foreach (UInt32 key in keyIDs)
                    {
                        if ((UInt32)m.WParam == key)
                        {
                            OnHotkey((int)m.WParam);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
