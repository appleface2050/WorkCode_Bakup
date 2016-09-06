using System;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Text;
using System.Net.Security;
using System.Globalization;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Collections;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Cloud.Services;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.GameManager
{
    public class StreamViewTimeStats
    {
        private string mKey;
        private object mlockObject = new object();
        private string mTwitchStreamId;
        private string mStreamLanguage;
        private string mStreamerName;
        private string mStreamStatus;
        private string mViewer;
        private string mFollowers;
        private int mRank;
        private bool mIsRecommended;
        private bool mIsBTVStream;
        private bool mIsPriorityStream;
        private string mImpressionTimestamp;
        private string mCreatedAt;
        private string mRecommadationType;
        private string mGameName;
        private string mStreamerBroadcasterLanguage;

        private Stopwatch mSessionStopWatch, mVideoPlayStopWatch;
        private Stopwatch mTabVisibleStopWatch;
        private int mPauseCount = 0;
        private int mResumeCount = 0;
        private int mTabVisibleCount = 0;
        private int mTabHiddenCount = 0;
        private string mTabCloseReason;
        private Stopwatch mWindowVisibleStopWatch;
        private Stopwatch mWindowActiveStopWatch;
        private int mWindowActivatedCount;
        private int mWindowDeactivatedCount;
        private int mWindowVisibleCount;
        private int mWindowHiddenCount;
        private string mSessionId;

        public static object slockObject = new object();
        public static Dictionary<string, StreamViewTimeStats> sStreamViewTimeStatsList = new
            Dictionary<string, StreamViewTimeStats>();

        public StreamViewTimeStats(string key, string jsonString)
        {
            Logger.Info("StreamViewStats: Stats called for key: {0}", key);
            if (sStreamViewTimeStatsList.ContainsKey(key))
            {
                Logger.Info("Creating StreamViewStats, key: {0} already exist", key);
                return;
            }

            mKey = key;

            JSonReader readJson = new JSonReader();
            IJSonObject res = readJson.ReadAsJSonObject(jsonString);
            CustomLogger.Info(jsonString);

            if (res.Contains("viewers"))
                mViewer = res["viewers"].StringValue;
            if (res.Contains("channel"))
            {
                IJSonObject resChannel = (IJSonObject)res["channel"];

                if (resChannel.Contains("rank"))
                    mRank = resChannel["rank"].Int32Value;
                if (resChannel.Contains("recommendation_type"))
                    mRecommadationType = resChannel["recommendation_type"].StringValue;
                if (resChannel.Contains("followers"))
                    mFollowers = resChannel["followers"].StringValue;
                if (resChannel.Contains("broadcaster_language"))
                    mStreamerBroadcasterLanguage = resChannel["broadcaster_language"].StringValue;
                if (resChannel.Contains("status"))
                    mStreamStatus = resChannel["status"].StringValue;
                if (resChannel.Contains("impression_timestamp"))
                    mImpressionTimestamp = resChannel["impression_timestamp"].StringValue;
                if (resChannel.Contains("game"))
                    mGameName = resChannel["game"].StringValue;
                if (resChannel.Contains("language"))
                    mStreamLanguage = resChannel["language"].StringValue;
                if (resChannel.Contains("name"))
                    mStreamerName = resChannel["name"].StringValue;
                if (resChannel.Contains("is_btv_stream"))
                    mIsBTVStream = resChannel["is_btv_stream"].BooleanValue;
                if (resChannel.Contains("is_priority"))
                    mIsPriorityStream = resChannel["is_priority"].BooleanValue;
                if (resChannel.Contains("is_recommended"))
                    mIsRecommended = resChannel["is_recommended"].BooleanValue;
                if (resChannel.Contains("_id"))
                    mTwitchStreamId = resChannel["_id"].StringValue;
            }

            Logger.Info("StreamViewStats: AddRequiredStreamInfo called for recommendationType: {0}" +
                    ", twitchStramId: {1}, gameName: {2}, streamerName: {3}", mRecommadationType,
                    mTwitchStreamId, mGameName, mStreamerName);

            mSessionStopWatch = new Stopwatch();
            mSessionStopWatch.Start();

            mTabVisibleStopWatch = new Stopwatch();
            mTabVisibleStopWatch.Start();

            mVideoPlayStopWatch = new Stopwatch();

            mWindowVisibleStopWatch = new Stopwatch();
            mWindowVisibleStopWatch.Start();

            mWindowActiveStopWatch = new Stopwatch();
            mWindowActiveStopWatch.Start();

            mWindowVisibleCount = 1;
            mWindowActivatedCount = 1;
            mTabVisibleCount += 1;

            mCreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            mSessionId = Stats.GetSessionId();

            sStreamViewTimeStatsList.Add(key, this);
        }

        private static int ConvertToSeconds(Stopwatch stopWatch)
        {
            return (int)Math.Round((stopWatch.ElapsedMilliseconds * 1.0) / 1000.0);
        }

        private void HandleStartSession()
        {
            if (!mSessionStopWatch.IsRunning)
                mSessionStopWatch.Start();
        }

        private void HandleEndSession(string tabCloseReason)
        {
            CustomLogger.Info("HandleEndSession called");
            HandleWindowHiddenStats(false);
            HandleWindowDeactivatedStats(false);
            HandleEndTabVisible(false);
            HandleEndVideoPlay(false);
            mSessionStopWatch.Stop();
            mTabCloseReason = tabCloseReason;
            CustomLogger.Info(String.Format("streamViewSession time is {0} sec, tabCloseReason: {1}",
                        ConvertToSeconds(mSessionStopWatch), mTabCloseReason));
        }

        private void HandleStartTabVisible()
        {
            if (!mTabVisibleStopWatch.IsRunning)
            {
                CustomLogger.Info("HandleStartTabVisible called");
                mTabVisibleCount += 1;
                CustomLogger.Info(String.Format("Tab visible time was {0} sec, mTabVisibleCount is now {1}",
                        ConvertToSeconds(mTabVisibleStopWatch), mTabVisibleCount));
                mTabVisibleStopWatch.Start();
            }
        }

        private void HandleEndTabVisible(bool increaseTabHiddenCount)
        {
            if (mTabVisibleStopWatch.IsRunning)
            {
                CustomLogger.Info("HandleEndTabVisible called");
                if (increaseTabHiddenCount)
                    mTabHiddenCount += 1;

                CustomLogger.Info(String.Format("Tab visible time is {0} sec, mTabHiddenCount is now {1}",
                        ConvertToSeconds(mTabVisibleStopWatch), mTabHiddenCount));
                mTabVisibleStopWatch.Stop();
            }
        }

        private void HandleWindowActivatedStats()
        {
            if (!mWindowActiveStopWatch.IsRunning)
            {
                CustomLogger.Info("HandleWindowActivatedStats called");
                mWindowActivatedCount += 1;
                CustomLogger.Info(String.Format("Window active time was {0} sec, mWindowActivatedCount is now {1}",
                            ConvertToSeconds(mWindowActiveStopWatch), mWindowActivatedCount));
                mWindowActiveStopWatch.Start();
            }
        }

        private void HandleWindowDeactivatedStats(bool increaseWindowDeactivatedCount)
        {
            if (mWindowActiveStopWatch.IsRunning)
            {
                CustomLogger.Info("HandleWindowDeactivatedStats called");
                if (increaseWindowDeactivatedCount)
                    mWindowDeactivatedCount += 1;
                mWindowActiveStopWatch.Stop();
                CustomLogger.Info(String.Format("Window active time is {0} sec, mWindowDeactivatedCount is now {1}",
                            ConvertToSeconds(mWindowActiveStopWatch), mWindowDeactivatedCount));
            }
        }

        private void HandleStartVideoPlay()
        {
            if (!mVideoPlayStopWatch.IsRunning)
            {
                CustomLogger.Info("HandleStartVideoPlay called");
                mResumeCount += 1;
                CustomLogger.Info(String.Format("Video play time was {0} sec, mResumeCount is now {1}",
                        ConvertToSeconds(mVideoPlayStopWatch), mResumeCount));
                mVideoPlayStopWatch.Start();
            }
        }

        private void HandleEndVideoPlay(bool increasePauseCount)
        {
            if (mVideoPlayStopWatch != null && mVideoPlayStopWatch.IsRunning)
            {
                CustomLogger.Info("HandleEndVideoPlay called");
                if (increasePauseCount)
                    mPauseCount += 1;

                CustomLogger.Info(String.Format("Video play time is {0} sec, mPauseCount is now {1}",
                        ConvertToSeconds(mVideoPlayStopWatch), mPauseCount));
                mVideoPlayStopWatch.Stop();
            }
        }

        private void HandleWindowVisibleStats()
        {
            if (!mWindowVisibleStopWatch.IsRunning)
            {
                CustomLogger.Info("HandleWindowVisibleStats called");
                mWindowVisibleCount += 1;
                CustomLogger.Info(String.Format("Window visible time was {0} sec, mWindowVisibleCount is now {1}",
                            ConvertToSeconds(mWindowVisibleStopWatch), mWindowVisibleCount));
                mWindowVisibleStopWatch.Start();
            }
        }

        private void HandleWindowHiddenStats(bool increaseWindowHiddenStats)
        {
            if (mWindowVisibleStopWatch.IsRunning)
            {
                CustomLogger.Info("HandleWindowHiddenStats called");
                if (increaseWindowHiddenStats)
                    mWindowHiddenCount += 1;
                mWindowVisibleStopWatch.Stop();
                CustomLogger.Info(String.Format("Window visible time is {0} sec, mWindowHiddenCount is {1}",
                            ConvertToSeconds(mWindowVisibleStopWatch), mWindowHiddenCount));
            }
        }

        private void AddDataToJsonWriter(JSonWriter jsonWriter)
        {
            jsonWriter.WriteMember("guid", User.GUID);
            jsonWriter.WriteMember("rank", mRank);
            jsonWriter.WriteMember("viewer", mViewer);
            jsonWriter.WriteMember("followers", mFollowers);
            jsonWriter.WriteMember("isRecommended", mIsRecommended);
            jsonWriter.WriteMember("isBtvStream", mIsBTVStream);
            jsonWriter.WriteMember("isPriorityStream", mIsPriorityStream);
            jsonWriter.WriteMember("impressionTimestamp", mImpressionTimestamp);
            jsonWriter.WriteMember("recommadationType", mRecommadationType);
            jsonWriter.WriteMember("viewerLang", Thread.CurrentThread.CurrentCulture.Name);
            jsonWriter.WriteMember("windowVisibleTime", ConvertToSeconds(mWindowVisibleStopWatch));
            jsonWriter.WriteMember("windowVisibleCount", mWindowVisibleCount);
            jsonWriter.WriteMember("windowHiddenCount", mWindowHiddenCount);
            jsonWriter.WriteMember("windowActiveTime", ConvertToSeconds(mWindowActiveStopWatch));
            jsonWriter.WriteMember("windowActivatedCount", mWindowActivatedCount);
            jsonWriter.WriteMember("windowDeactivatedCount", mWindowDeactivatedCount);
            jsonWriter.WriteMember("twitchStreamId", mTwitchStreamId);
            jsonWriter.WriteMember("tabCloseReason", mTabCloseReason);
            jsonWriter.WriteMember("streamLanguage", mStreamLanguage);
            jsonWriter.WriteMember("streamerName", mStreamerName);
            jsonWriter.WriteMember("streamStatus", mStreamStatus);
            jsonWriter.WriteMember("gameName", mGameName);
            jsonWriter.WriteMember("streamerBrodcasterLang", mStreamerBroadcasterLanguage);
            jsonWriter.WriteMember("videoPlayTime", ConvertToSeconds(mVideoPlayStopWatch));
            jsonWriter.WriteMember("pauseCount", mPauseCount);
            jsonWriter.WriteMember("resumeCount", mResumeCount);
            jsonWriter.WriteMember("tabVisibleTime", ConvertToSeconds(mTabVisibleStopWatch));
            jsonWriter.WriteMember("tabVisibleCount", mTabVisibleCount);
            jsonWriter.WriteMember("tabHiddenCount", mTabHiddenCount);
            jsonWriter.WriteMember("sessionTime", ConvertToSeconds(mSessionStopWatch));
            jsonWriter.WriteMember("userCreatedAt", mCreatedAt);
            jsonWriter.WriteMember("sessionId", mSessionId);
        }

        private static void HandleEndWindowSession(string windowCloseEvent)
        {
            CustomLogger.Info(String.Format("HandleEndWindowSession called for event: {0}",
                windowCloseEvent));
            lock (slockObject)
            {
                foreach (StreamViewTimeStats streamViewTimeStats in sStreamViewTimeStatsList.Values)
                {
                    lock (streamViewTimeStats.mlockObject)
                    {
                        if (streamViewTimeStats.mSessionStopWatch.IsRunning)
                        {
                            streamViewTimeStats.HandleEndSession(windowCloseEvent);
                        }
                    }
                }

                if (sStreamViewTimeStatsList.Values.Count > 0)
                {
                    string dataForStats = ConvertToJsonString();
                    UploadToCloud(dataForStats);
                    sStreamViewTimeStatsList = new Dictionary<string, StreamViewTimeStats>();
                }
                else
                    CustomLogger.Info("No data for stream stats");
            }
        }

        private static string ConvertToJsonString()
        {
            JSonWriter jsonWriter = new JSonWriter();
            jsonWriter.WriteArrayBegin();

            lock (slockObject)
            {
                foreach (StreamViewTimeStats streamViewTimeStats
                    in sStreamViewTimeStatsList.Values)
                {
                    jsonWriter.WriteObjectBegin();
                    streamViewTimeStats.AddDataToJsonWriter(jsonWriter);
                    jsonWriter.WriteObjectEnd();
                }
            }
            jsonWriter.WriteArrayEnd();
            return jsonWriter.ToString();
        }

        private static string ConvertToJsonString(StreamViewTimeStats streamViewTimeStats)
        {
            JSonWriter jsonWriter = new JSonWriter();
            jsonWriter.WriteArrayBegin();
            jsonWriter.WriteObjectBegin();
            streamViewTimeStats.AddDataToJsonWriter(jsonWriter);
            jsonWriter.WriteObjectEnd();
            jsonWriter.WriteArrayEnd();
            return jsonWriter.ToString();
        }

        private static void UploadToCloud(string data)
        {
            CustomLogger.Info(data);
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);

            string installDir = (string)key.GetValue("InstallDir");
            string url = Common.Utils.GetHostUrl() + "/btv/RecommendedVideosViewsBigQuery";
            string subkey = Common.Strings.GMPendingStats;
            string randomString = Guid.NewGuid().ToString();

            JSonWriter jsonWriter = new JSonWriter();
            jsonWriter.WriteObjectBegin();
            jsonWriter.WriteMember("url", url);
            jsonWriter.WriteMember("data", data);
            jsonWriter.WriteMember("isArray", true);
            jsonWriter.WriteObjectEnd();

            RegistryKey statsKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMPendingStats, true);
            if (statsKey == null)
            {
                RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath, true);
                baseKey.CreateSubKey("Stats");
                baseKey.Close();
                statsKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMPendingStats, true);
            }

            statsKey.SetValue(randomString, jsonWriter.ToString(), RegistryValueKind.String);
            statsKey.Close();

            CustomLogger.Info("Params: " + "\"" + subkey + "\" \"" + randomString + "\"");

            Process proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = Path.Combine(installDir, "HD-CloudPost.exe");
            proc.StartInfo.Arguments = "\"" + subkey + "\" \"" + randomString + "\"";
            proc.Start();
        }

        public static void HandleWindowCloseSession()
        {
            HandleEndWindowSession("WindowClosed");
        }

        public static void HandleWindowCrashSession()
        {
            HandleEndWindowSession("WindowCrashed");
        }

        public static void NotifyToAllTabStats(string eventName)
        {
            lock (slockObject)
            {
                foreach (string key in sStreamViewTimeStatsList.Keys)
                {
                    HandleStreamViewStatsEvent(key, eventName);
                }
            }
        }

        public static void HandleStreamViewStatsEvent(string key, string eventName)
        {
            if (!sStreamViewTimeStatsList.ContainsKey(key))
            {
                Logger.Info("Handling StreamViewStats Event, unable to find key: {0}", key);
                return;
            }

            StreamViewTimeStats streamViewTimeStats = sStreamViewTimeStatsList[key];

            lock (streamViewTimeStats.mlockObject)
            {
                switch (eventName)
                {
                    case StreamViewStatsEventName.SessionStart:
                        streamViewTimeStats.HandleStartSession();
                        break;
                    case StreamViewStatsEventName.TabCloseSessionEnd:
                        streamViewTimeStats.HandleEndSession("TabClosed");
                        string data = ConvertToJsonString(streamViewTimeStats);
                        lock (slockObject)
                        {
                            sStreamViewTimeStatsList.Remove(key);
                        }
                        UploadToCloud(data);
                        break;
                    case StreamViewStatsEventName.TabVisibleStart:
                        streamViewTimeStats.HandleStartTabVisible();
                        break;
                    case StreamViewStatsEventName.TabVisibleEnd:
                        streamViewTimeStats.HandleEndTabVisible(true);
                        break;
                    case StreamViewStatsEventName.WindowVisible:
                        streamViewTimeStats.HandleWindowVisibleStats();
                        break;
                    case StreamViewStatsEventName.WindowHidden:
                        streamViewTimeStats.HandleWindowHiddenStats(true);
                        break;
                    case StreamViewStatsEventName.WindowActivated:
                        streamViewTimeStats.HandleWindowActivatedStats();
                        break;
                    case StreamViewStatsEventName.WindowDeactivated:
                        streamViewTimeStats.HandleWindowDeactivatedStats(true);
                        break;
                    case StreamViewStatsEventName.VideoPlay:
                        streamViewTimeStats.HandleStartVideoPlay();
                        break;
                    case StreamViewStatsEventName.VideoPause:
                        streamViewTimeStats.HandleEndVideoPlay(true);
                        break;
                    default:
                        Logger.Error("Unknown event {0}", eventName);
                        break;
                }
            }
        }
    }

    public class CustomLogger
    {
        private static bool printLog = false;

        public static void Info(string info)
        {
            if (printLog)
                Logger.Info("StreamViewStats: " + info);
        }
    }

    public class StreamViewStatsEventName
    {
        public const string SessionStart = "SessionStart";
        public const string SessionEnd = "SessionEnd";
        public const string TabCloseSessionEnd = "TabCloseSessionEnd";
        public const string WindowCloseSessionEnd = "WindowCloseSessionEnd";
        public const string WindowCrashSessionEnd = "WindowCrashSessionEnd";
        public const string TabVisibleStart = "TabVisibleStart";
        public const string TabVisibleEnd = "TabVisibleEnd";
        public const string WindowActivated = "WindowActivated";
        public const string WindowDeactivated = "WindowDeactivated";
        public const string WindowVisible = "WindowVisible";
        public const string WindowHidden = "WindowHidden";
        public const string VideoPlay = "VideoPlay";
        public const string VideoPause = "VideoPause";

    }
}
