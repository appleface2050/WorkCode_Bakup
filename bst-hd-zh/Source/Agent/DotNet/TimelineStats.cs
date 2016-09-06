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

namespace BlueStacks.hyperDroid.Agent {

public class TimelineStatsSender
{
    static long UtcToUnixTimestampSecs(DateTime value)
    {
	TimeSpan span = value - new DateTime(1970, 1, 1, 0, 0, 0, 0);
	return (long)span.TotalSeconds;
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern long GetTickCount64();

    static long TicksInSeconds()
    {
	return GetTickCount64() / 1000;
    }

    static long sSequenceNumber = 1000000*UtcToUnixTimestampSecs(DateTime.UtcNow);

    class TimelineEvent
    {
	public DateTime		Time;
	public long		Ticks;
	public string		Event;
	public string		S1;
	public string		S2;
	public string		S3;

	public TimelineEvent (string evt, string s1, string s2, string s3)
	{
	    this.Time = DateTime.UtcNow;
	    this.Ticks = TicksInSeconds();
	    this.Event = evt;
	    this.S1 = s1;
	    this.S2 = s2;
	    this.S3 = s3;
	}
    }

    static Queue<TimelineEvent>	sEventQueue 		= new Queue<TimelineEvent>();
    static Mutex		sEventQueueMutex	= new Mutex();

    public static void Init()
    {
	if (Utils.IsOSWinXP())
	{
	    Logger.Warning("TimelineStats: Not supported for WindowsXP");
	    return;
	}

	Logger.Info("TimelineStats: Initalizing: Staring thread.");
	Thread thr = new Thread(StatsSenderThread);
	thr.IsBackground = true;
	thr.Start();
    }

    public static void HandleTopActivityInfo (RequestData data)
    {
	if (Utils.IsOSWinXP())
	    return;

	Logger.Info("TimelineStats: HandleTopActivityInfo: {0}/{1}",
		data.data["packageName"], data.data["activityName"]);
		
	TimelineEvent e = new TimelineEvent(
		"app-activity",
		data.data["packageName"],
		data.data["activityName"],
		"");

	sEventQueueMutex.WaitOne();
	sEventQueue.Enqueue(e);
	sEventQueueMutex.ReleaseMutex();
    }

    public static void HandleFrontendStatusUpdate (RequestData data)
    {
	if (Utils.IsOSWinXP())
	    return;

	Logger.Info("TimelineStats: HandleFrontendStatusUpdate: {0}",
		data.data["event"]);

	TimelineEvent e = new TimelineEvent(
		data.data["event"],
		"",
		"",
		"");

	sEventQueueMutex.WaitOne();
	sEventQueue.Enqueue(e);
	sEventQueueMutex.ReleaseMutex();
    }

    public static void HandleS2PEvents (RequestData data)
    {
	if (Utils.IsOSWinXP())
	    return;

	Logger.Info("TimelineStats: HandleS2PEvents: {0}",
		data.data["event"]);

	TimelineEvent e = new TimelineEvent(
		data.data["event"],
		"",
		"",
		"");

	sEventQueueMutex.WaitOne();
	sEventQueue.Enqueue(e);
	sEventQueueMutex.ReleaseMutex();
    }

    public static void HandleAppInstallEvents (IJSonObject json)
    {
	if (Utils.IsOSWinXP())
	    return;

	Logger.Info("TimelineStats: HandleAppInstallEvents: " +
		"event {0}, package {1}, name {2}, source {3}, update {4}",
		"app-installed",
		json["package"].StringValue.Trim(),
		json["name"].StringValue.Trim(),
		json["source"].StringValue.Trim(),
		json["update"].StringValue.Trim());

	TimelineEvent e = new TimelineEvent(
		((string.Compare(json["update"].StringValue.Trim(), "true") == 0)
		 ? "app-updated"
		 : "app-installed"),
		json["package"].StringValue.Trim(),
		json["name"].StringValue.Trim(),
		json["source"].StringValue.Trim());

	sEventQueueMutex.WaitOne();
	sEventQueue.Enqueue(e);
	sEventQueueMutex.ReleaseMutex();
    }

    public static void HandleAppUninstallEvents (IJSonObject json)
    {
	if (Utils.IsOSWinXP())
	    return;

	Logger.Info("TimelineStats: HandleAppUninstallEvents: " +
		"event {0}, package {1}, name {2}",
		"app-uninstalled",
		json["package"].StringValue.Trim(),
		json["source"].StringValue.Trim());

	TimelineEvent e = new TimelineEvent(
		"app-uninstalled",
		json["package"].StringValue.Trim(),
		"",
		json["source"].StringValue.Trim());

	sEventQueueMutex.WaitOne();
	sEventQueue.Enqueue(e);
	sEventQueueMutex.ReleaseMutex();
    }

    public static void HandleNotificationUpdates (
	    string evt,
	    string package,
	    string activity,
	    string id)
    {
	if (Utils.IsOSWinXP())
	    return;

	Logger.Info("TimelineStats: HandleNotificationUpdates: " +
		"event {0}, package {1}, activity {2}, id {3}",
		evt, package, activity, id);

	TimelineEvent e = new TimelineEvent(
		evt,
		package,
		activity,
		id);

	sEventQueueMutex.WaitOne();
	sEventQueue.Enqueue(e);
	sEventQueueMutex.ReleaseMutex();
    }

    public static void HandleAdEvents (RequestData data)
    {
	if (Utils.IsOSWinXP())
	    return;

	Logger.Info("TimelineStats: HandleAdEvents : " +
		"event {0}, " +
		"status {1}, " +
		"ecode {2}, " +
		"estring {3},",
		data.data["event"],
		data.data["status"],
		data.data["ecode"],
		data.data["estring"]);
		
	TimelineEvent e = new TimelineEvent(
		data.data["event"],
		data.data["status"],
		data.data["ecode"],
		data.data["estring"]);

	sEventQueueMutex.WaitOne();
	sEventQueue.Enqueue(e);
	sEventQueueMutex.ReleaseMutex();
    }


    public static void SendTimelineStats(
	    DateTime	timestamp,
	    string	evt,
	    long	duration,
	    string	s1,
	    string	s2,
	    string	s3,
	    DateTime	fromTimestamp,
	    DateTime	toTimestamp,
	    long	fromTicks,
	    long	toTicks
	    )
    {
	    string timezone = TimeZone.CurrentTimeZone.DaylightName;
	    string locale = Thread.CurrentThread.CurrentCulture.Name;

	    Logger.Info("TimelineStats: SendTimelineStats: sequence {0} " +
			    "timestamp {1}, evt {2}, duration {3}, s1 {4}, " +
			    "s2 {5}, s3 {6}, timezone {7}, locale {8}, " +
			    "from_timestamp {9}, to_timestamp {10} " +
			    "from_ticks {11}, to_ticks {12}",
			    sSequenceNumber, timestamp, evt, duration, s1, s2, s3,
			    timezone, locale,
			    fromTimestamp, toTimestamp, fromTicks, toTicks);

	    Thread thr = new Thread(delegate() {
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
			    });
	    thr.IsBackground = true;
	    thr.Start();
    }

    static void StatsSenderThread ()
    {
	DateTime	frontendLaunchTime = DateTime.UtcNow;
	long		frontendLaunchTicks = TicksInSeconds();

	bool		isFrontendActive = false;
	bool		isUserActive = false;
	DateTime	lastUserActivityTime = DateTime.UtcNow;
	long		lastUserActivityTicks = TicksInSeconds();

	string		currentPackage = "";
	string		currentActivity = "";
	DateTime	currentActivityStartTime = DateTime.UtcNow;
	long		currentActivityStartTicks = TicksInSeconds();

	DateTime	s2pAuthStartedTime = DateTime.UtcNow;
	long		s2pAuthStartedTicks = TicksInSeconds();
	DateTime	s2pPayInstallPopupTime = DateTime.UtcNow;
	long		s2pPayInstallPopupTicks = TicksInSeconds();

	DateTime	notificationDisplayTime = DateTime.UtcNow;
	long		notificationDisplayTicks = TicksInSeconds();

	/*
	 * Loop infinitely to get events from Frontend and Android
	 * and process them to generate Timeline records and send
	 * them.
	 */
	for (;;)
	{
	    /*
	     * If no data in queue, Sleep()
	     */
	    sEventQueueMutex.WaitOne();
	    if (sEventQueue.Count <= 0)
	    {
		sEventQueueMutex.ReleaseMutex();

		/*
		 * No events.  10mins of inactivity means user is idle.
		 */
		if (    isUserActive &&
			TicksInSeconds() > lastUserActivityTicks + 600)
		{
		    if (lastUserActivityTicks < currentActivityStartTicks)
		    {
			Logger.Error("TimelineStats: user-idle: " +
				"lastUserActivityTicks {0} < currentActivityStartTicks {1}",
				lastUserActivityTicks, currentActivityStartTicks);
		    }
		    else if (string.Compare(currentPackage, "", true) != 0)
		    {
			SendTimelineStats(
				lastUserActivityTime,
				"app-usage",
				lastUserActivityTicks - currentActivityStartTicks,
				currentPackage,
				currentActivity,
				"user-idle",
				currentActivityStartTime,
				lastUserActivityTime,
				currentActivityStartTicks,
				lastUserActivityTicks);
		    }

		    isUserActive = false;
		}

		Thread.Sleep(1000);
		continue;
	    }

	    TimelineEvent e = (TimelineEvent) sEventQueue.Dequeue();
	    sEventQueueMutex.ReleaseMutex();

	    if (string.Compare(e.Event, "frontend-launched", true) == 0)
	    {
		isFrontendActive = true;
		frontendLaunchTime = e.Time;
		frontendLaunchTicks = e.Ticks;

		SendTimelineStats (
			e.Time,
			e.Event,
			0,		// dummy
			"none",		// dummy
			"none",		// dummy
			"none",		// dummy
			e.Time,		// dummy
			e.Time,		// dummy
			0,		// dummy
			0);		// dummy
	    }

	    else if (string.Compare(e.Event, "frontend-ready", true) == 0)
	    {
		SendTimelineStats(
			e.Time,
			e.Event,
			e.Ticks - frontendLaunchTicks,
			"none",		// dummy
			"none",		// dummy
			"none",		// dummy
			frontendLaunchTime,
			e.Time,
			frontendLaunchTicks,
			e.Ticks);
	    }

	    else if (string.Compare(e.Event, "frontend-closed", true) == 0)
	    {
		/*
		 * If user was active this is time to generate an
		 * app-usage report.
		 */
		if (isUserActive && isFrontendActive)
		{
		    isUserActive = false;
		    isFrontendActive = false;

		    if (e.Ticks < currentActivityStartTicks)
		    {
			Logger.Error("TimelineStats: frontend-closed: " +
				"e.Ticks {0} < currentActivityStartTicks {1}",
				e.Ticks, currentActivityStartTicks);
		    }
		    else if (string.Compare(currentPackage, "", true) != 0)
		    {
			SendTimelineStats(
				e.Time,
				"app-usage",
				e.Ticks - currentActivityStartTicks,
				currentPackage,
				currentActivity,
				"frontend-closed",
				currentActivityStartTime,
				e.Time,
				currentActivityStartTicks,
				e.Ticks);
		    }
		}

		SendTimelineStats(
			e.Time,
			e.Event,
			e.Ticks - frontendLaunchTicks,
			"none",		// dummy
			"none",		// dummy
			"none",		// dummy
			frontendLaunchTime,
			e.Time,
			frontendLaunchTicks,
			e.Ticks);

		isFrontendActive = false;
		isUserActive = false;
		currentPackage = "";
		currentActivity = "";
	    }

	    else if (string.Compare(e.Event, "frontend-activated", true) == 0)
	    {
		isFrontendActive = true;
		isUserActive = true;
		lastUserActivityTime = e.Time;
		lastUserActivityTicks = e.Ticks;

		/* If frontend just became active, start measuring activity time */
		currentActivityStartTime = e.Time;
		currentActivityStartTicks = e.Ticks;
	    }

	    else if (string.Compare(e.Event, "frontend-deactivated", true) == 0)
	    {
		if (isUserActive)
		{
		    if (e.Ticks < currentActivityStartTicks)
		    {
			Logger.Error("TimelineStats: frontend-deactivated: " +
				"e.Ticks {0} < currentActivityStartTicks {1}",
				e.Ticks, currentActivityStartTicks);
		    }
		    else if (string.Compare(currentPackage, "", true) != 0)
		    {
			SendTimelineStats(
				e.Time,
				"app-usage",
				e.Ticks - currentActivityStartTicks,
				currentPackage,
				currentActivity,
				"frontend-deactivated",
				currentActivityStartTime,
				e.Time,
				currentActivityStartTicks,
				e.Ticks);
		    }
		}

		isFrontendActive = false;
		isUserActive = false;
	    }

	    else if (string.Compare(e.Event, "user-active", true) == 0)
	    {
		/* If user just became active, start measuing activity time */
		if (isUserActive == false)
		{
		    currentActivityStartTime = e.Time;
		    currentActivityStartTicks = e.Ticks;
		}

		/* 
		 * Mark frontend active just in case it is not already.  This
		 * might happen if something like Task Manager is running with
		 * Always On Top configuraiton.  In such cases we don't receive
		 * frontend-activated.
		 */
		isFrontendActive = true;
		isUserActive = true;
		lastUserActivityTime = e.Time;
		lastUserActivityTicks = e.Ticks;
	    }

	    else if (string.Compare(e.Event, "app-activity", true) == 0)
	    {
		/* No change in activity.  Nothing to report. */
		if (String.Compare(e.S1, currentPackage, true) == 0 &&
		    String.Compare(e.S2, currentActivity, true) == 0)
		    continue;

		if (isUserActive && isFrontendActive &&
			string.Compare(currentPackage, "", true) != 0)
		{
		    if (e.Ticks < currentActivityStartTicks)
		    {
			Logger.Error("TimelineStats: app-activity: " +
				"e.Ticks {0} < currentActivityStartTicks {1}",
				e.Ticks, currentActivityStartTicks);
		    }
		    else 
		    {
			SendTimelineStats(
				e.Time,
				"app-usage",
				e.Ticks - currentActivityStartTicks,
				currentPackage,
				currentActivity,
				"new-app-activity",
				currentActivityStartTime,
				e.Time,
				currentActivityStartTicks,
				e.Ticks);
		    }
		}

		currentPackage = e.S1;
		currentActivity = e.S2;
		currentActivityStartTime = e.Time;
		currentActivityStartTicks = e.Ticks;
	    }

	    else if (string.Compare(e.Event, "s2p-auth-started") == 0)
	    {
		s2pAuthStartedTime = e.Time;
		s2pAuthStartedTicks = e.Ticks;

		SendTimelineStats(
			e.Time,
			e.Event,
			0,
			"",
			"",
			"",
			e.Time,
			e.Time,
			e.Ticks,
			e.Ticks);
	    }

	    else if (string.Compare(e.Event, "s2p-auth-completed") == 0)
	    {
		SendTimelineStats(
			e.Time,
			e.Event,
			e.Ticks - s2pAuthStartedTicks,
			"success",
			"",
			"",
			s2pAuthStartedTime,
			e.Time,
			s2pAuthStartedTicks,
			e.Ticks);
	    }

	    else if (string.Compare(e.Event, "s2p-pay-install-popup") == 0)
	    {
		s2pPayInstallPopupTime = e.Time;
		s2pPayInstallPopupTicks = e.Ticks;

		SendTimelineStats(
			e.Time,
			e.Event,
			0,
			"",
			"",
			"",
			e.Time,
			e.Time,
			e.Ticks,
			e.Ticks);
	    }

	    else if (
		    string.Compare(e.Event, "s2p-pay-clicked") == 0 ||
		    string.Compare(e.Event, "s2p-install-clicked") == 0)
	    {
		SendTimelineStats(
			e.Time,
			e.Event,
			e.Ticks - s2pPayInstallPopupTicks,
			"success",
			"",
			"",
			s2pPayInstallPopupTime,
			e.Time,
			s2pPayInstallPopupTicks,
			e.Ticks);
	    }

	    else if (
		    string.Compare(e.Event, "app-installed") == 0 ||
		    string.Compare(e.Event, "app-uninstalled") == 0 ||
		    string.Compare(e.Event, "app-updated") == 0)
	    {
		SendTimelineStats(
			e.Time,
			e.Event,
			0,
			e.S1,
			e.S2,
			e.S3,
			e.Time,
			e.Time,
			e.Ticks,
			e.Ticks);
	    }

	    else if (string.Compare(e.Event, "notification-displayed") == 0)
	    {
		notificationDisplayTime = e.Time;
		notificationDisplayTicks = e.Ticks;

		SendTimelineStats(
			e.Time,
			e.Event,
			0,
			e.S1,
			e.S2,
			e.S3,
			e.Time,
			e.Time,
			e.Ticks,
			e.Ticks);
	    }

	    else if (
		    string.Compare(e.Event, "notification-clicked") == 0 ||
		    string.Compare(e.Event, "notification-muted") == 0 ||
		    string.Compare(e.Event, "notification-expired") == 0 ||
		    string.Compare(e.Event, "notification-dismissed") == 0)
	    {
		/*
		 * XXX: Fix duration computation.  If multiple notifications
		 * arrive at same time computation will be wrong.
		 */
		SendTimelineStats(
			e.Time,
			e.Event,
			e.Ticks - notificationDisplayTicks,
			e.S1,
			e.S2,
			e.S3,
			notificationDisplayTime,
			e.Time,
			notificationDisplayTicks,
			e.Ticks);
	    }

	    else if (
		    string.Compare(e.Event, "ad-request") == 0 ||
		    string.Compare(e.Event, "ad-response") == 0 ||
		    string.Compare(e.Event, "ad-click") == 0)
	    {
		SendTimelineStats(
			e.Time,
			e.Event,
			0,
			e.S1,
			e.S2,
			e.S3,
			e.Time,
			e.Time,
			e.Ticks,
			e.Ticks);
	    }

	    else
	    {
		Logger.Error("Unknown event {0}", e.Event);
	    }
	}
    }
}

}
