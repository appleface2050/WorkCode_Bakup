using System;
using System.Runtime.InteropServices;
using System.Text;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.GameManager;

namespace BlueStacks.hyperDroid.GameManager
{
    public class InputMapper
    {

        private static InputMapper sInstance = new InputMapper();

        private void SendMessageOverload(Common.Utils.GamePadEventType eventType, int[] data)
        {
            Logger.Debug("Received SendMessageOverload request for GamePadEventType = {0}", eventType);
            //if (GameManager.GameManager.sGameManager.mFrontendHandle == IntPtr.Zero)
            //{
            //	Logger.Info("InputMapperHelper: Received invalid handle with GamePadEventType: {0}", eventType);
            //	return;
            //}

            if (!Oem.Instance.IsGamePadEnabled)
            {
                Logger.Debug("GamePad is disabled from registry.");
                return;
            }

            byte[] buff = new byte[data.Length * sizeof(int)];
            for (int i = 0; i < data.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(data[i]), 0, buff, i * sizeof(int), sizeof(int));
            }

            IntPtr unManArray = Marshal.AllocHGlobal(buff.Length);

            try
            {
                Common.Interop.Window.COPYGAMEPADDATASTRUCT cds = new Common.Interop.Window.COPYGAMEPADDATASTRUCT();

                cds.dwData = (IntPtr)eventType;
                cds.lpData = unManArray;
                Marshal.Copy(buff, 0, cds.lpData, buff.Length);
                cds.size = buff.Length;

                //Common.Interop.Window.SendMessage(
                //		GameManager.GameManager.sGameManager.mFrontendHandle,
                //		Common.Interop.Window.WM_COPYDATA,
                //		(IntPtr)Convert.ToUInt32(eventType),
                //		ref cds
                //		);

                int result = Marshal.GetLastWin32Error();
                if (result != 0)
                {
                    Logger.Debug("InputMapperHelper: SendMessage Failed with error: {0}", result);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(unManArray);
            }
        }

        public void DispatchGamePadAttach(int identity, int vendor,
                int product)
        {
            int[] data = new int[] {
                identity,
                vendor,
                product
            };

            SendMessageOverload(
                    Utils.GamePadEventType.TYPE_GAMEPAD_ATTACH,
                    data
                    );
        }

        public void DispatchGamePadDetach(int identity)
        {
            int[] data = new int[] {
                identity
            };

            SendMessageOverload(
                    Utils.GamePadEventType.TYPE_GAMEPAD_DETACH,
                    data
                    );
        }

        public void DispatchGamePadUpdate(int identity, Common.GamePad gamepad)
        {
            int[] data = new int[] {
                identity,
                    gamepad.X,
                    gamepad.Y,
                    gamepad.Z,
                    gamepad.Rx,
                    gamepad.Ry,
                    gamepad.Rz,
                    gamepad.Hat,
                    (int)gamepad.Mask
            };

            SendMessageOverload(
                    Utils.GamePadEventType.TYPE_GAMEPAD_UPDATE,
                    data
                    );
        }

        public static InputMapper Instance()
        {
            return sInstance;
        }
    }
}
