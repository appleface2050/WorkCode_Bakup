using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.Win32;
using System.Windows.Forms;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Cloud.Services;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.GameManager
{

    public class TimelineStatsSender
    {
        static long UtcToUnixTimestampSecs(DateTime value)
        {
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (long)span.TotalSeconds;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern long GetTickCount64();

        static long TicksInSeconds()
        {
            return GetTickCount64() / 1000;
        }

        static long sSequenceNumber = 1000000 * UtcToUnixTimestampSecs(DateTime.UtcNow);

        private static Object sLockObject = new Object();

        private static long sGmLaunchTicks;
        private static long sGmActiveTicks;
        private static long sGmVisibleTicks;

        class TimelineEvent
        {
            public DateTime Time;
            public long Ticks;
            public string Event;
        }

        static Queue<TimelineEvent> sEventQueue = new Queue<TimelineEvent>();
        static Mutex sEventQueueMutex = new Mutex();

        public static void Init()
        {
            if (Utils.IsOSWinXP())
            {
                Logger.Warning("TimelineStats: Not supported for WindowsXP");
                return;
            }

            Logger.Info("TimelineStats: Initalizing");

            sGmLaunchTicks = TicksInSeconds();
            sGmActiveTicks = TicksInSeconds();
            sGmVisibleTicks = TicksInSeconds();
        }

        public static void HandleStatusUpdate(string eventName)
        {
            if (Utils.IsOSWinXP())
                return;

            TimelineEvent e = new TimelineEvent();
            e.Time = DateTime.UtcNow;
            e.Ticks = TicksInSeconds();
            e.Event = eventName;

            switch (e.Event)
            {
                case "gm-launched":
                    sGmLaunchTicks = TicksInSeconds();
                    SendTimelineStats(
                            e.Time,
                            e.Event,
                            0,
                            "none",     // dummy
                            "none",     // dummy
                            "none",     // dummy
                            e.Time,     // dummy
                            e.Time,     // dummy
                            0,      // dummy
                            0);     // dummy
                    break;

                case "gm-closed":
                    SendTimelineStats(
                            e.Time,
                            e.Event,
                            e.Ticks - sGmLaunchTicks,
                            "none",     // dummy
                            "none",     // dummy
                            "none",     // dummy
                            e.Time,     // dummy
                            e.Time,     // dummy
                            0,      // dummy
                            0);     // dummy
                    break;

                case "gm-activated":
                    sGmActiveTicks = TicksInSeconds();
                    SendTimelineStats(
                            e.Time,
                            e.Event,
                            0,
                            "none",     // dummy
                            "none",     // dummy
                            "none",     // dummy
                            e.Time,     // dummy
                            e.Time,     // dummy
                            0,      // dummy
                            0);     // dummy
                    break;

                case "gm-deactivated":
                    SendTimelineStats(
                            e.Time,
                            e.Event,
                            e.Ticks - sGmActiveTicks,
                            "none",     // dummy
                            "none",     // dummy
                            "none",     // dummy
                            e.Time,     // dummy
                            e.Time,     // dummy
                            0,      // dummy
                            0);     // dummy
                    break;

                case "gm-shown":
                    sGmVisibleTicks = TicksInSeconds();
                    SendTimelineStats(
                            e.Time,
                            e.Event,
                            0,
                            "none",     // dummy
                            "none",     // dummy
                            "none",     // dummy
                            e.Time,     // dummy
                            e.Time,     // dummy
                            0,      // dummy
                            0);     // dummy
                    break;

                case "gm-hidden":
                    SendTimelineStats(
                            e.Time,
                            e.Event,
                            e.Ticks - sGmVisibleTicks,
                            "none",     // dummy
                            "none",     // dummy
                            "none",     // dummy
                            e.Time,     // dummy
                            e.Time,     // dummy
                            0,      // dummy
                            0);     // dummy
                    break;

                default:
                    Logger.Error("Unknown event {0}", e.Event);
                    break;
            }
        }

        static void SendTimelineStats(
                DateTime timestamp,
                string evt,
                long duration,
                string s1,
                string s2,
                string s3,
                DateTime fromTimestamp,
                DateTime toTimestamp,
                long fromTicks,
                long toTicks
                )
        {
            lock (sLockObject)
            {
                string timezone = TimeZone.CurrentTimeZone.DaylightName;
                string locale = Thread.CurrentThread.CurrentCulture.Name;

                Common.Stats.SendTimelineStats(
                        UtcToUnixTimestampSecs(timestamp),
                        sSequenceNumber++,
                        evt,
                        duration,
                        s1,
                        s2,
                        s3,
                        timezone,
                        locale,
                        UtcToUnixTimestampSecs(fromTimestamp),
                        UtcToUnixTimestampSecs(toTimestamp),
                        fromTicks,
                        toTicks);
            }
        }

    }

}
