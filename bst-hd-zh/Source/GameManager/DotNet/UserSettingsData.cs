using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Win32;

namespace BlueStacks.hyperDroid.GameManager.gamemanager
{
    class UserSettingsData
    {
        public int keyValue { get; set; }
        public string keyString { get; set; }

        public void Init()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.FrameBufferRegKeyPath, false))
            {
                Size temp = new Size();
                temp.Width = (int)key.GetValue("GuestWidth");
                temp.Height = (int)key.GetValue("GuestHeight");
                GuestSize = temp;

                temp = new Size();
                temp.Width = (int)key.GetValue("WindowWidth");
                temp.Height = (int)key.GetValue("WindowHeight");

                GMSize = temp;

                keyString = key.GetValue("BossKeyString","").ToString();
                keyValue = (int)key.GetValue("BossKeyValue",0);
            }
        }

        public Size GuestSize { get; set; }

        public Size GMSize { get; set; }

        public static Size CountSize(Size gmsize, int left, int top)
        {
            top += 20;
            left += 20;
            int sh = System.Windows.Forms.SystemInformation.WorkingArea.Height;
            int sw = System.Windows.Forms.SystemInformation.WorkingArea.Width;
            Size result = new Size(gmsize.Width, gmsize.Height);

            if (sh < result.Height + top)
            {
                result.Height = sh - top;
                result.Width = gmsize.Width * (result.Height) / gmsize.Height;
            }

            if (sw < result.Width + left)
            {
                result.Width = sw - left;
                result.Height = gmsize.Height * result.Width / gmsize.Width;
            }
            return result;
        }

        public void SaveToReg()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.FrameBufferRegKeyPath, true))
            {
                key.SetValue("GuestWidth", GuestSize.Width);
                key.SetValue("GuestHeight", GuestSize.Height);

                key.SetValue("WindowWidth", GMSize.Width);
                key.SetValue("WindowHeight", GMSize.Height);

                
                key.SetValue("BossKeyString", this.keyString);
                key.SetValue("BossKeyValue",this.keyValue);

            }
        }
    }
}
