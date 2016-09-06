using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;

using Microsoft.Win32;

using CodeTitans.JSon;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Cloud.Services
{
	public class EService : Exception
	{
		public EService(string reason)
			: base(reason)
		{
		}
	}


	public class Service
	{
		public static string Host
		{
			get
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.CloudRegKeyPath))
				{
					if (key == null)
						return Common.Strings.ChannelsUrl;
					else
						return (string)key.GetValue("Host", "http://127.0.0.1:8080");
				}
			}
		}

		public static String Host2
		{
			get
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.CloudRegKeyPath))
				{
					if (key == null)
						return "https://23.23.194.123";
					else
						return (string)key.GetValue("Host2", "http://127.0.0.1:8080");
				}
			}
		}

		public delegate void OnSuccess(IJSonObject o);
		public delegate void OnFailed(Exception exc);

		public static bool Success(IJSonObject o)
		{
			return (o.Contains("success") && o["success"].IsTrue);
		}

		public static string ErrorReason(IJSonObject o)
		{
			if (o.Contains("reason")) return o["reason"].StringValue;
			return "";
		}

		public static System.ComponentModel.BackgroundWorker CreateWorkerAsync(OnSuccess success, OnFailed failed)
		{
			System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
			worker.RunWorkerCompleted += delegate (object o, RunWorkerCompletedEventArgs args)
			{
				if (args.Error != null)
				{
					if (failed != null)
						failed(args.Error);
				}
				else
				{
					IJSonObject json = (IJSonObject)args.Result;

					if (Auth.Success(json))
					{
						success(json);
					}
					else
					{
						EService exc = new EService(ErrorReason(json));
						failed(exc);
					}
				}
			};
			return worker;
		}

	}


	class Auth : Service
	{
		private static String s_GuidSecret = "3921330286be3e2cb90a";

		public static String API_URL
		{
			get
			{
				return String.Format("{0}/api/{1}", Service.Host, "auth"); //http://10.0.1.12:8086/api/auth";
			}
		}

		public static String Route
		{
			get
			{
				return new Uri(API_URL).PathAndQuery;
			}
		}

		public const String X_BST_AUTH_KEY = "X-Bst-Auth-Key";
		public const String X_BST_AUTH_TIMESTAMP = "X-Bst-Auth-Timestamp";
		public const String X_BST_AUTH_SIGN = "X-Bst-Auth-Sign";

		public class ELogin : EService
		{
			public ELogin(String reason)
				: base(reason)
			{
			}
		}

		public class ESignUp : EService
		{
			public ESignUp(String reason)
				: base(reason)
			{
			}
		}

		public static String GuidSecret(String guid)
		{
			String sign = HMACSign(Encoding.UTF8.GetBytes(s_GuidSecret), Encoding.UTF8.GetBytes(guid));
			return sign;
		}

		public static Dictionary<String, String> CreateHeaders(String key)
		{
			long utcMilliseconds = (DateTime.UtcNow.Ticks - 621355968000000000) / TimeSpan.TicksPerMillisecond;
			Dictionary<String, String> headers = new Dictionary<String, String>();

			headers[Auth.X_BST_AUTH_KEY] = key;
			headers[Auth.X_BST_AUTH_TIMESTAMP] = Convert.ToString(utcMilliseconds);
			return headers;
		}

		public static String Sign(String verb,
				String route,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				String[] paramsOrder,
				String secret)
		{
			ArrayList payload = new ArrayList();
			payload.Add(verb);
			payload.Add(route);
			payload.Add(String.Format("{0}:{1}", X_BST_AUTH_KEY, headers[X_BST_AUTH_KEY]));
			payload.Add(String.Format("{0}:{1}", X_BST_AUTH_TIMESTAMP, headers[X_BST_AUTH_TIMESTAMP]));

			foreach (String o in paramsOrder)
			{
				payload.Add(String.Format("{0}={1}", o, data[o]));
			}

			String payloadString = String.Join("\n", payload.ToArray(typeof(String)) as String[]);
			payloadString += "\n";

			Byte[] encodedPayload = Encoding.UTF8.GetBytes(payloadString);
			String sign = HMACSign(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payloadString));

			return sign;
		}

		public static String HMACSign(Byte[] secret, Byte[] data)
		{
			Byte[] hash = null;
			using (HMACSHA1 hmac = new HMACSHA1(secret))
			{
				hash = hmac.ComputeHash(data);
			}

			String hex = "";
			String hexDigest = "";
			foreach (Byte b in hash)
			{
				hex = b.ToString("X").ToLower();
				hexDigest += (hex.Length == 1 ? "0" : "") + hex;
			}
			return hexDigest;
		}

		public static IJSonObject Login(String email, String password)
		{
			Dictionary<String, String> data = new Dictionary<String, String>();
			data.Add("email", email);
			data.Add("password", password);
			bool gzip = true;
			String r = Common.HTTP.Client.Post(Auth.API_URL + "/login", data, null, gzip);
			IJSonReader json = new JSonReader();
			return json.ReadAsJSonObject(r);
		}

		public static IJSonObject SignUp(String name, String email, String password)
		{
			Dictionary<String, String> data = new Dictionary<String, String>();
			data.Add("name", name);
			data.Add("email", email);
			data.Add("password", password);
			bool gzip = true;
			String r = Common.HTTP.Client.Post(Auth.API_URL + "/signup", data, null, gzip);
			IJSonReader json = new JSonReader();
			return json.ReadAsJSonObject(r);
		}

		public static void LoginAsync(String email, String password, OnSuccess success, OnFailed failed)
		{
			System.ComponentModel.BackgroundWorker worker = Service.CreateWorkerAsync(success, failed);
			worker.DoWork += delegate (Object o, DoWorkEventArgs args)
			{
				args.Result = Auth.Login(email, password);
			};
			worker.RunWorkerAsync();
		}

		public static void SignUpAsync(String name, String email, String password, OnSuccess success, OnFailed failed)
		{
			System.ComponentModel.BackgroundWorker worker = Service.CreateWorkerAsync(success, failed);
			worker.DoWork += delegate (Object o, DoWorkEventArgs args)
			{
				args.Result = Auth.SignUp(name, email, password);
			};
			worker.RunWorkerAsync();
		}

		public static IJSonObject CCPC(String pcGUID)
		{
			Dictionary<String, String> data = new Dictionary<String, String>();
			data.Add("pc_guid", pcGUID);
			bool gzip = true;
			String r = Common.HTTP.Client.Post(Auth.API_URL + "/cc/pc", data, null, gzip);
			IJSonReader json = new JSonReader();
			return json.ReadAsJSonObject(r);
		}

		public static IJSonObject CCPCNoCache(String pcGUID)
		{
			Dictionary<String, String> data = new Dictionary<String, String>();
			data.Add("pc_guid", pcGUID);
			data.Add("cache", "no");
			bool gzip = true;
			String r = Common.HTTP.Client.Post(Auth.API_URL + "/cc/pc", data, null, gzip);
			IJSonReader json = new JSonReader();
			return json.ReadAsJSonObject(r);
		}

		public static IJSonObject CCPCAdd(String email, String pcGUID)
		{
			Dictionary<String, String> data = new Dictionary<String, String>();
			data.Add("email", email);
			data.Add("pc_guid", pcGUID);
			bool gzip = true;
			string r = Common.HTTP.Client.Post(Auth.API_URL + "/cc/pc/add", data, null, gzip);
			IJSonReader json = new JSonReader();
			return json.ReadAsJSonObject(r);
		}

		public class Token
		{
			public class EMalformed : Exception
			{
				public EMalformed(String reason)
					: base(reason)
				{
				}
			}

			public static String Key
			{
				get
				{
					String clearText = GetUnSecureRegValue("Key");
					return clearText;
				}

				set
				{
					PutSecureRegValue("Key", value);
				}
			}

			public static String Secret
			{
				get
				{
					String clearText = GetUnSecureRegValue("Secret");
					return clearText;
				}

				set
				{
					PutSecureRegValue("Secret", value);
				}
			}

			private static String GetUnSecureRegValue(String key)
			{
				String clearText = null;
				using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.CloudRegKeyPath))
				{
					String regValue = (String)regKey.GetValue(key, "");
					if (regValue == "")
						throw new EMalformed("Empty");

					try
					{
						byte[] buff = Convert.FromBase64String(regValue);
						clearText = SecureUserData.Decrypt(buff);
					}
					catch (System.FormatException exc)
					{
						throw new EMalformed(exc.ToString());
					}
					catch (System.Security.Cryptography.CryptographicException exc)
					{
						throw new EMalformed(exc.ToString());
					}
				}
				return clearText;

			}

			private static void PutSecureRegValue(String key, String value)
			{
				byte[] buff = SecureUserData.Encrypt(value);
				String regValue = Convert.ToBase64String(buff);

				using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey(Common.Strings.CloudRegKeyPath))
				{
					regKey.SetValue(key, regValue, RegistryValueKind.String);
					regKey.Flush();
				}

			}

		}
	}


	class Sync : Service
	{
		public static String API_URL
		{
			get
			{
				return String.Format("{0}/api/{1}", Service.Host, "sync");
			}
		}

		public static String API_V2_URL
		{
			get
			{
				return String.Format("{0}/api/v2/{1}", Service.Host, "sync");
			}
		}

		public static String Route
		{
			get
			{
				return new Uri(API_URL).PathAndQuery;
			}
		}

		public static String RouteV2
		{
			get
			{
				return new Uri(API_V2_URL).PathAndQuery;
			}
		}

		public static IJSonObject Echo(String param1, String param2, String key, String secret)
		{
			Dictionary<String, String> headers = Auth.CreateHeaders(key);

			String[] paramsOrder = new String[] { "param1", "param2" };

			Dictionary<String, String> data = new Dictionary<String, String>();
			data["param1"] = param1;
			data["param2"] = param2;

			String sign = Auth.Sign("POST", Sync.Route + "/echo", data, headers,
					paramsOrder,
					secret);

			headers.Add(Auth.X_BST_AUTH_SIGN, sign);
			bool gzip = true;
			String r = Common.HTTP.Client.Post(Sync.API_URL + "/echo", data, headers, gzip);
			IJSonReader json = new JSonReader();
			return json.ReadAsJSonObject(r);
		}

		public static void EchoAsync(String param1, String param2, String key, String secret, OnSuccess success, OnFailed failed)
		{
			System.ComponentModel.BackgroundWorker worker = Service.CreateWorkerAsync(success, failed);
			worker.DoWork += delegate (Object o, DoWorkEventArgs args)
			{
				args.Result = Sync.Echo(param1, param2, key, secret);
			};
			worker.RunWorkerAsync();
		}

		public static IJSonObject AppInfo(String key, String secret)
		{
			throw new NotImplementedException();
		}

		public static void AppInfoAsync()
		{
		}

		public static IJSonObject AppList(String key, String secret)
		{
			Dictionary<String, String> headers = Auth.CreateHeaders(key);

			String[] paramsOrder = new String[] { };

			Dictionary<String, String> data = new Dictionary<String, String>();
			String sign = Auth.Sign("POST", Sync.Route + "/app/list", data, headers,
					paramsOrder,
					secret);

			headers.Add(Auth.X_BST_AUTH_SIGN, sign);
			bool gzip = true;
			String r = Common.HTTP.Client.Post(Sync.API_URL + "/app/list", data, headers, gzip);
			IJSonReader json = new JSonReader();
			return json.ReadAsJSonObject(r);
		}

		public static IJSonObject AppList2(String key, String secret)
		{
			Dictionary<String, String> headers = Auth.CreateHeaders(key);

			String[] paramsOrder = new String[] { };

			Dictionary<String, String> data = new Dictionary<String, String>();
			String sign = Auth.Sign("GET", Sync.RouteV2 + "/app/list", data, headers,
					paramsOrder,
					secret);

			headers.Add(Auth.X_BST_AUTH_SIGN, sign);
			bool gzip = true;
			String r = Common.HTTP.Client.Get(Sync.API_V2_URL + "/app/list", headers, gzip);
			IJSonReader json = new JSonReader();
			return json.ReadAsJSonObject(r);
		}

		public static void AppListAsync()
		{
		}

		public static IJSonObject UploadApp()
		{
			throw new NotImplementedException();
		}

		public static void UploadAppAsync()
		{
		}

		public static IJSonObject DestroyApp()
		{
			throw new NotImplementedException();
		}

		public static void DestroyAppAsync()
		{
		}

		public static void DownloadApp(String srcUrl, String dest, String key, String secret)
		{
			Dictionary<String, String> headers = Auth.CreateHeaders(key);

			String[] paramsOrder = new String[] { };

			Dictionary<String, String> data = new Dictionary<String, String>();
			String route = new Uri(srcUrl).PathAndQuery;
			String sign = Auth.Sign("GET", route, data, headers,
					paramsOrder,
					secret);

			headers.Add(Auth.X_BST_AUTH_SIGN, sign);

			using (WebClient client = new WebClient())
			{
				Logger.Debug("URI of proxy = " + client.Proxy.GetProxy(new Uri(Service.Host)));

				foreach (KeyValuePair<String, String> o in headers)
				{
					client.Headers.Set(o.Key, o.Value);
				}

				client.Headers.Add("User-Agent", Common.Utils.UserAgent(User.GUID));

				client.DownloadFile(srcUrl, dest);
			}
		}

		public static void DownloadAppAsync()
		{
		}

		public static IJSonObject UploadAppIcon()
		{
			throw new NotImplementedException();
		}

		public static void UploadAppIconAsync()
		{
		}

		public static IJSonObject DownloadAppIcon()
		{
			throw new NotImplementedException();
		}

		public static void DownloadAppIconAsync()
		{
		}
	}

	class SMS : Service
	{
		public static String API_URL
		{
			get
			{
				return String.Format("{0}/api/{1}", Service.Host, "sms");
			}
		}
		public static String Route
		{
			get
			{
				return new Uri(API_URL).PathAndQuery;
			}
		}

		public static IJSonObject ReadSMS(String key, String secret)
		{
			Dictionary<String, String> headers = Auth.CreateHeaders(key);

			String[] paramsOrder = new String[] { };

			Dictionary<String, String> data = new Dictionary<String, String>();
			String sign = Auth.Sign("GET", SMS.Route + "/readsms", data, headers,
					paramsOrder,
					secret);

			headers.Add(Auth.X_BST_AUTH_SIGN, sign);
			bool gzip = true;
			Logger.Debug("SMS: Host " + (SMS.API_URL + "/readsms"));
			String r = Common.HTTP.Client.Get(SMS.API_URL + "/readsms", headers, gzip);
			IJSonReader json = new JSonReader();
			return json.ReadAsJSonObject(r);
		}

	}

}
