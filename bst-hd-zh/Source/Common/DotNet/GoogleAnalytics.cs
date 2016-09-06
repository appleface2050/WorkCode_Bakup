using System;
using System.Net;
using System.Web;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace BlueStacks.hyperDroid.Common
{
	class GoogleAnalytics
	{
		private static String s_AccountName = "UA-48153097-1";
		private static String s_PageDomain = Common.Strings.ChannelsUrl;
		private static String s_UserAgent = String.Format("Mozilla/5.0 (compatible; MSIE {0}; Windows NT {1}.{2})",
				BlueStacks.hyperDroid.Version.STRING,
				Environment.OSVersion.Version.Major,
				Environment.OSVersion.Version.Minor);
		private static String s_Locale = CultureInfo.CurrentCulture.Name;


		private static int s_ScreenWidth = -1;
		private static int s_ScreenHeight = -1;

		[DllImport("user32.dll")]
		private static extern int GetSystemMetrics(int which);

		private const int SM_CXSCREEN = 0;
		private const int SM_CYSCREEN = 1;

		private static int ScreenWidth
		{
			get
			{
				if (s_ScreenWidth == -1)
				{
					s_ScreenWidth = GetSystemMetrics(SM_CXSCREEN);
				}

				return s_ScreenWidth;

			}
		}

		private static String OSName()
		{
			if (Utils.IsOSWin8())
				return "Win8";
			else if (Utils.IsOSWin7())
				return "Win7";
			else if (Utils.IsOSWinXP())
				return "WinXP";
			else if (Utils.IsOSVista())
				return "Vista";

			return "None";
		}

		private static int ScreenHeight
		{
			get
			{
				if (s_ScreenHeight == -1)
				{
					s_ScreenHeight = GetSystemMetrics(SM_CYSCREEN);
				}

				return s_ScreenHeight;
			}
		}

		public class Event
		{
			private String m_Category;
			private String m_Action;
			private String m_Label;
			private int m_Value;


			public String Category { get { return m_Category; } }

			public String Action { get { return m_Action; } }

			public String Label { get { return m_Label; } }

			public int Value { get { return m_Value; } }

			public Event(String category, String action, String label, int value)
			{
				m_Category = category;
				m_Action = action;
				m_Label = label;
				m_Value = value;
			}
		}

		// reference : http://www.google.com/support/forum/p/Google+Analytics/thread?tid=626b0e277aaedc3c&hl=en
		private static int DomainHash
		{
			get
			{
				int a = 1;
				int c = 0;
				int h;
				char cChar;
				int iChar;

				a = 0;
				for (h = s_PageDomain.Length - 1; h >= 0; h--)
				{
					cChar = char.Parse(s_PageDomain.Substring(h, 1));
					iChar = (int)cChar;
					a = (a << 6 & 268435455) + iChar + (iChar << 14);
					c = a & 266338304;
					a = c != 0 ? a ^ c >> 21 : a;
				}

				return a;
			}
		}

		public static int UtcToUnixTimestampSecs(DateTime value)
		{
			TimeSpan span = value - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return (int)span.TotalSeconds;
		}

		private static String FakeUtmcCookieString
		{
			get
			{
				String ReferralSource = "(direct)";
				String Campaign = "(direct)";
				String Medium = "(none)";
				int visitCount = 2;

				int now = UtcToUnixTimestampSecs(DateTime.Now);

				String utma = String.Format("{0}.{1}.{2}.{3}.{4}.{5}",
						DomainHash,
						int.Parse(RandomGenerator.Next(1000000000).ToString()),
						now,
						now,
						now,
						visitCount);

				String utmz = String.Format("{0}.{1}.{2}.{3}.utmcsr={4}|utmccn={5}|utmcmd={6}",
						DomainHash,
						now,
						"1",
						"1",
						ReferralSource,
						Campaign,
						Medium);

				String utmcc = Uri.EscapeDataString(String.Format("__utma={0};+__utmz={1};",
							utma,
							utmz
							));
				return utmcc;
			}
		}

		private static void SendTrackEvent(String pageTitle, String pageURL, Event evt, String accountName)
		{
			try
			{
				List<KeyValuePair<String, String>> q = new List<KeyValuePair<String, String>>();

				// Details from http://code.google.com/apis/analytics/docs/tracking/gaTrackingTroubleshooting.html

				q.Add(new KeyValuePair<string, string>("utmwv", "5.2.4"));
				q.Add(new KeyValuePair<string, string>("utmn", RandomGenerator.Next(1000000000).ToString()));
				q.Add(new KeyValuePair<string, string>("utmhn", s_PageDomain));
				q.Add(new KeyValuePair<string, string>("utmcs", "UTF-8"));
				q.Add(new KeyValuePair<string, string>("utmul", s_Locale));
				q.Add(new KeyValuePair<string, string>("utmsr", String.Format("{0}x{1}", ScreenWidth, ScreenHeight)));  // Screen size widthxheight 
				q.Add(new KeyValuePair<string, string>("utmsc", Device.Profile.OEM));
				q.Add(new KeyValuePair<string, string>("utmje", "0"));
				q.Add(new KeyValuePair<string, string>("utmfl", OSName()));
				q.Add(new KeyValuePair<string, string>("utmdt", Uri.EscapeDataString(pageTitle)));
				q.Add(new KeyValuePair<string, string>("utmhid", RandomGenerator.Next(1000000000).ToString()));
				q.Add(new KeyValuePair<string, string>("utmr", "-"));
				q.Add(new KeyValuePair<string, string>("utmp", pageURL));
				q.Add(new KeyValuePair<string, string>("utmac", accountName));
				q.Add(new KeyValuePair<string, string>("utmcc", FakeUtmcCookieString));
				q.Add(new KeyValuePair<string, string>("utmt", "event"));

				string evtString = String.Format("5({0}*{1}*{2})({3})",
						evt.Category, evt.Action, evt.Label, evt.Value);
				q.Add(new KeyValuePair<string, string>("utme", Uri.EscapeDataString(evtString)));

				StringBuilder qBuilder = new StringBuilder();
				foreach (KeyValuePair<String, String> pair in q)
				{
					qBuilder.Append(String.Format("{0}={1}&", pair.Key, pair.Value));
				}

				String qString = qBuilder.ToString();
				qString = qString.Substring(0, qString.Length - 1);
				String utmGifUrl = String.Format("https://www.google-analytics.com/__utm.gif?{0}", qString);


				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(new Uri(utmGifUrl));

				req.UserAgent = s_UserAgent;

				Logger.Debug("Request utmGifUrl = " + utmGifUrl);
				HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
				resp.Close();
				Logger.Debug("Response utmGifUrl = " + utmGifUrl);
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}
		}

		private static void TrackEventAsync(String pageTitle, String pageURL, Event evt, String accountName)
		{
			Thread t = new Thread(delegate ()
					{
						SendTrackEvent(pageTitle, pageURL, evt, accountName);
					}, 1); // 1 to use the minimum stack size 
			t.IsBackground = true;
			t.Start();
		}

		public static void TrackEvent(String pageTitle, Event evt)
		{
			String pageURL = String.Format("/{0}", pageTitle);
			SendTrackEvent(pageTitle, pageURL, evt, s_AccountName);
		}

		public static void TrackEvent(String pageTitle, Event evt, String accountName)
		{
			String pageURL = String.Format("/{0}", pageTitle);
			SendTrackEvent(pageTitle, pageURL, evt, accountName);
		}

		public static void TrackEventAsync(String pageTitle, Event evt)
		{
			String pageURL = String.Format("/{0}", pageTitle);
			TrackEventAsync(pageTitle, pageURL, evt, s_AccountName);
		}

		public static void TrackEventAsync(String pageTitle, Event evt, String accountName)
		{
			String pageURL = String.Format("/{0}", pageTitle);
			TrackEventAsync(pageTitle, pageURL, evt, accountName);
		}

		public static void TrackEventAsync(Event evt)
		{
			String pageTitle = Process.GetCurrentProcess().ProcessName;
			String pageURL = String.Format("/{0}", pageTitle);
			TrackEventAsync(pageTitle, pageURL, evt, s_AccountName);
		}

		public static void TrackEventAsync(Event evt, String accountName)
		{
			String pageTitle = Process.GetCurrentProcess().ProcessName;
			String pageURL = String.Format("/{0}", pageTitle);
			TrackEventAsync(pageTitle, pageURL, evt, accountName);
		}

		public static void UpdateDefaultAccountName(
				String accountName
				)
		{
			s_AccountName = accountName;
		}
	}
}
