using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Device;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Common.HTTP
{
	public class Utils
	{
		public static void Write(StringBuilder sb, HttpListenerResponse res)
		{
			Write(sb.ToString(), res);
		}

		public static void Write(String s, HttpListenerResponse res)
		{
			Byte[] b = Encoding.UTF8.GetBytes(s);
			res.ContentLength64 = b.Length;
			res.OutputStream.Write(b, 0, b.Length);
			res.OutputStream.Flush();
		}
	}

	public class Server
	{
		public delegate void RequestHandler(HttpListenerRequest req, HttpListenerResponse res);

		private HttpListener m_Listener;
		private int m_Port;
		private Dictionary<String, RequestHandler> m_Routes;
		private String m_RootDir;
		private bool m_ShutDown = false;

		public static bool s_FileWriteComplete = true;

		public class ENoPortAvailable : Exception
		{
			public ENoPortAvailable(String reason)
				: base(reason)
			{
			}
		}

		public int Port
		{
			get { return m_Port; }
		}

		public Dictionary<String, RequestHandler> Routes
		{
			get { return m_Routes; }
		}

		public String RootDir
		{
			get { return m_RootDir; }
			set { m_RootDir = value; }
		}

		public Server(int port, Dictionary<String, RequestHandler> routes, String rootDir)
		{
			m_Port = port;
			m_Routes = routes;
			m_RootDir = rootDir;
		}

		public void Start()
		{
			String prefix = String.Format("http://{0}:{1}/", "*", m_Port);
			m_Listener = new HttpListener();
			m_Listener.Prefixes.Add(prefix);
			try
			{
				m_ShutDown = false;
				m_Listener.Start();
			}
			catch (System.Net.HttpListenerException e)
			{
				Logger.Error("Failed to start listener. err: " + e.ToString());
				throw new ENoPortAvailable(String.Format("No free port available"));
			}
		}

		public void Run()
		{
			while (!m_ShutDown)
			{
				HttpListenerContext ctx = null;
				try
				{
					ctx = m_Listener.GetContext();
				}
				catch (Exception ex)
				{
					Logger.Error("Exception while processing HTTP context: " + ex.ToString());
					continue;
				}

				Worker worker = new Worker(ctx, this.Routes, this.RootDir);
				Thread workerThread = new Thread(worker.ProcessRequest);
				workerThread.SetApartmentState(ApartmentState.STA);
				workerThread.IsBackground = true;
				workerThread.Start();
			}
		}

		public void Stop()
		{
			if (m_Listener != null)
			{
				try
				{
					m_ShutDown = true;
					m_Listener.Close();
				}
				catch (System.Net.HttpListenerException e)
				{
					Logger.Error("Failed to stop listener. err: " + e.ToString());
				}
			}
		}

		class Worker
		{
			private Dictionary<String, RequestHandler> m_Routes;
			private HttpListenerContext m_Ctx;
			private String m_RootDir;

			public Worker(HttpListenerContext ctx, Dictionary<String, RequestHandler> routes, String rootDir)
			{
				m_Ctx = ctx;
				m_Routes = routes;
				m_RootDir = rootDir;
			}
			[STAThread]
			public void ProcessRequest()
			{
				try
				{
					if (m_Ctx.Request.Url.AbsolutePath.StartsWith("/static/"))
					{
						StaticFileHandler(m_Ctx.Request, m_Ctx.Response);
					}
					else if (m_Ctx.Request.Url.AbsolutePath.StartsWith("/static2/"))
					{
						StaticFileChunkHandler(m_Ctx.Request, m_Ctx.Response);
					}
					else
					{
						RequestHandler handler = (RequestHandler)m_Routes[m_Ctx.Request.Url.AbsolutePath];
						if (handler != null)
						{
							if (m_Ctx.Request.UserAgent != null && m_Ctx.Request.UserAgent.Contains("Open Broadcaster Software") == false)
							{
								Logger.Info("Request received for {0}, UserAgent = {1}", m_Ctx.Request.Url.AbsolutePath, m_Ctx.Request.UserAgent);
							}
							handler(m_Ctx.Request, m_Ctx.Response);
						}
					}
				}
				catch (KeyNotFoundException)
				{
					Logger.Error("Exception: No Handler registered for " + m_Ctx.Request.Url.AbsolutePath);
					m_Ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
				}
				catch (Exception ex)
				{
					Logger.Error("Exception while processing HTTP handler: " + ex.ToString());
					m_Ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				}
				finally
				{
					try
					{
						m_Ctx.Response.OutputStream.Close();
					}
					catch (Exception e)
					{
						Logger.Error("Exception during m_Ctx.Response.OutputStream.Close(): " + e.ToString());
					}
				}
			}

			public void StaticFileHandler(HttpListenerRequest req, HttpListenerResponse res)
			{
				string url = req.Url.AbsolutePath;
				url = url.Substring(url.Substring(1).IndexOf("/") + 2);
				String filePath = Path.Combine(m_RootDir, url.Replace("/", "\\"));
				//			Logger.Info(String.Format("StaticFileHandler: serving {0} from {1}", req.Url, filePath));

				if (File.Exists(filePath))
				{
					byte[] content = File.ReadAllBytes(filePath);

					if (filePath.EndsWith(".css"))
					{
						res.Headers.Add("Content-Type: text/css");
					}
					else if (filePath.EndsWith(".js"))
					{
						res.Headers.Add("Content-Type: application/javascript");
					}

					/*
					if (filePath.EndsWith(".png") 
							|| filePath.EndsWith(".jpg")
							|| filePath.EndsWith(".jpeg")
							|| filePath.EndsWith(".gif")
							|| filePath.EndsWith(".js")
							|| filePath.EndsWith(".css")
							|| filePath.EndsWith(".json"))
					{
						res.Headers.Add("Cache-Control: max-age=2592000"); // 30 days. XXX: Vikram: Do we need this?
					}
					*/

					res.OutputStream.Write(content, 0, content.Length);
				}
				else
				{
					Logger.Error(String.Format("File {0} doesn't exist", filePath));
					res.StatusCode = 404;
					res.StatusDescription = "Not Found.";
				}
			}

			public void StaticFileChunkHandler(HttpListenerRequest req, HttpListenerResponse res)
			{
				string url = req.Url.AbsolutePath;
				url = url.Substring(url.Substring(1).IndexOf("/") + 2);
				String filePath = Path.Combine(m_RootDir, url.Replace("/", "\\"));
				Logger.Info(String.Format("StaticFileChunkHandler: serving {0} from {1}", req.Url, filePath));

				int retry = 50;
				while (!File.Exists(filePath))
				{
					retry++;
					Thread.Sleep(100);
					if (retry == 50)
						break;
				}

				retry = 0;
				if (File.Exists(filePath))
				{
					if (filePath.EndsWith(".flv"))
					{
						res.Headers.Add("Content-Type: video/x-flv");
					}

					int maxBytesToWrite = 1 * 1024 * 1024;  // 1 MB

					FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					while (true)
					{
						byte[] buffer = new byte[maxBytesToWrite];
						int read = fileStream.Read(buffer, 0, maxBytesToWrite);

						if (read != 0)
						{
							//Logger.Info("Serving data {0} {1}", read, Common.HTTP.Server.s_FileWriteComplete);
							res.OutputStream.Write(buffer, 0, read);
							//Thread.Sleep(10);
							retry = 0;
							continue;
						}
						Thread.Sleep(100);

						if (retry++ == 50)
							break;
					}
					Logger.Info("File write complete");
					fileStream.Close();
				}
				else
				{
					Logger.Error(String.Format("File {0} doesn't exist", filePath));
					res.StatusCode = 404;
					res.StatusDescription = "Not Found.";
				}
			}
		}

	}

	public class Client
	{
		public const int TIMEOUT_10SECONDS = 10000;
		public static string sCampaignName = null;

		public static String Encode(Dictionary<String, String> data)
		{
			StringBuilder s = new StringBuilder();
			foreach (KeyValuePair<String, String> o in data)
			{
				s.AppendFormat("{0}={1}&", o.Key, HttpUtility.UrlEncode(o.Value));
			}

			char[] trim = { '&' };
			String ret = s.ToString().TrimEnd(trim);
			return ret;
		}

		public static String Get(String url,
				Dictionary<String, String> headers,
				bool gzip)
		{
			return Get(url, headers, gzip, 0);
		}

		public static String Get(String url,
				Dictionary<String, String> headers,
				bool gzip,
				int timeout)
		{
			try
			{
				Logger.Info("url = " + url);
				if (Features.IsFeatureEnabled(Features.CHINA_CLOUD) &&
						url.Contains(Service.Host))
				{
					url = url.Replace(Service.Host, Service.Host2);
					Logger.Info("CHINA_CLOUD enabled, new url = {0}", url);
				}
				return GetInternal(url, headers, gzip, timeout);
			}
			catch (Exception e)
			{
				if (url.Contains(Service.Host))
				{
					//				Logger.Error("Could not send to " + url);
					Logger.Error(e.Message);
					url = url.Replace(Service.Host, Service.Host2);
					Logger.Info("New url = " + url);
					return GetInternal(url, headers, gzip, timeout);
				}
				else if (url.Contains(Service.Host2))
				{
					Logger.Error("Could not send to " + url);
					Logger.Error(e.Message);
					url = url.Replace(Service.Host2, Service.Host);
					Logger.Info("New url = " + url);
					return GetInternal(url, headers, gzip, timeout);
				}
				else
				{
					throw e;
				}
			}
		}

		public static bool UrlForBstCommandProcessor(string url)
		{
			try
			{
				Uri uri = new Uri(url);

				if ((uri.Segments.Length > 1 && String.Compare("ping", uri.Segments[1]) != 0) &&
						uri.Port == Common.VmCmdHandler.s_ServerPort)
					return true;
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
			}
			return false;
		}

		private static String GetInternal(String url,
				Dictionary<String, String> headers,
				bool gzip,
				int timeout)
		{
			if (UrlForBstCommandProcessor(url))
			{
				Common.Utils.WaitForBootComplete();
			}

			HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
			req.Method = "GET";
			if (timeout != 0)
				req.Timeout = timeout;

			if (gzip)
			{
				req.AutomaticDecompression = DecompressionMethods.GZip;
				req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
			}

			if (headers != null)
			{
				foreach (KeyValuePair<String, String> o in headers)
				{
					req.Headers.Set(o.Key, o.Value);
				}

			}
			req.Headers.Set("x_oem", Device.Profile.OEM);

			req.UserAgent = Common.Utils.UserAgent(User.GUID);

			Uri u = new Uri(url);

			if (!(u.Host.Contains("localhost") || u.Host.Contains("127.0.0.1")))
			{
				Logger.Debug("URI of proxy = " + req.Proxy.GetProxy(u));
			}

			String ret = null;
			using (HttpWebResponse res = req.GetResponse() as HttpWebResponse)
			{
				using (Stream s = res.GetResponseStream())
				{
					using (StreamReader r = new StreamReader(s, Encoding.UTF8))
					{
						ret = r.ReadToEnd();
					}
				}
			}
			return ret;
		}

		private static string GetCampaignHeader()
		{
			if (sCampaignName != null)
			{
				return sCampaignName;
			}

			sCampaignName = "bluestacks";
			try
			{
				RegistryKey key = Registry.LocalMachine.CreateSubKey(Strings.RegBasePath);
				if (key != null)
				{
					sCampaignName = (string)key.GetValue(Strings.CampaignKeyName, "bluestacks");
					//				Logger.Info("CampaignName = {0}", campaign);
				}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}

			return sCampaignName;
		}

		public static String Post(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				bool gzip)
		{
			return Post(url, data, headers, gzip, 0);
		}

		public static String Post(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				bool gzip,
				int timeout)
		{
			return Post(url, data, headers, gzip, timeout, null);
		}

		public static String Post(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				bool gzip,
				int timeout,
				string version)
		{
			try
			{
				Logger.Info("url = " + url);
				if (Features.IsFeatureEnabled(Features.CHINA_CLOUD) &&
						url.Contains(Service.Host))
				{
					url = url.Replace(Service.Host, Service.Host2);
					Logger.Info("CHINA_CLOUD enabled, new url = {0}", url);
				}
				return PostInternal(url, data, headers, gzip, timeout, version);
			}
			catch (Exception e)
			{
				if (url.Contains(Service.Host))
				{
					Logger.Error("Could not send to " + url);
					Logger.Error(e.Message);
					url = url.Replace(Service.Host, Service.Host2);
					Logger.Info("New url = " + url);
					return PostInternal(url, data, headers, gzip, timeout, version);
				}
				else if (url.Contains(Service.Host2))
				{
					Logger.Error("Could not send to " + url);
					Logger.Error(e.Message);
					url = url.Replace(Service.Host2, Service.Host);
					Logger.Info("New url = " + url);
					return PostInternal(url, data, headers, gzip, timeout, version);
				}
				else
				{
					throw e;
				}
			}
		}

		public static String PostWithoutBootCheck(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				bool gzip,
				int timeout,
				string version)
		{
			try
			{
				Logger.Info("url = " + url);
				if (Features.IsFeatureEnabled(Features.CHINA_CLOUD) &&
						url.Contains(Service.Host))
				{
					url = url.Replace(Service.Host, Service.Host2);
					Logger.Info("CHINA_CLOUD enabled, new url = {0}", url);
				}
				return PostInternal(url, data, headers, gzip, timeout, version, 1);
			}
			catch (Exception e)
			{
				if (url.Contains(Service.Host))
				{
					Logger.Error("Could not send to " + url);
					Logger.Error(e.Message);
					url = url.Replace(Service.Host, Service.Host2);
					Logger.Info("New url = " + url);
					return PostInternal(url, data, headers, gzip, timeout, version, 1);
				}
				else if (url.Contains(Service.Host2))
				{
					Logger.Error("Could not send to " + url);
					Logger.Error(e.Message);
					url = url.Replace(Service.Host2, Service.Host);
					Logger.Info("New url = " + url);
					return PostInternal(url, data, headers, gzip, timeout, version, 1);
				}
				else
				{
					throw e;
				}
			}
		}

		private static String PostInternal(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				bool gzip,
				int timeout,
				string version)
		{
			return PostInternal(url, data, headers, gzip, timeout, version, 120);
		}

		private static String PostInternal(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				bool gzip,
				int timeout,
				string version,
				int bootFailureRetries)
		{
			if (UrlForBstCommandProcessor(url))
			{
				Common.Utils.WaitForBootComplete(bootFailureRetries);
			}

			HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
			req.Method = "POST";
			if (timeout != 0)
				req.Timeout = timeout;

			if (gzip)
			{
				req.AutomaticDecompression = DecompressionMethods.GZip;
				req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
			}

			if (headers != null)
			{
				foreach (KeyValuePair<String, String> o in headers)
				{
					req.Headers.Set(o.Key, o.Value);
				}

			}
			req.Headers.Set("x_oem", Oem.Instance.OEM);

			if (data == null)
				data = new Dictionary<String, String>();

			byte[] rawData = Encoding.UTF8.GetBytes(Encode(data));
			req.ContentType = "application/x-www-form-urlencoded";
			req.ContentLength = rawData.Length;

			req.UserAgent = Common.Utils.UserAgent(User.GUID, version);

			Uri u = new Uri(url);
			if (!(u.Host.Contains("localhost") || u.Host.Contains("127.0.0.1")))
			{
				Logger.Debug("URI of proxy = " + req.Proxy.GetProxy(u));
			}
			String ret = null;
			using (Stream s = req.GetRequestStream())
			{
				s.Write(rawData, 0, rawData.Length);
				using (HttpWebResponse res = req.GetResponse() as HttpWebResponse)
				{
					using (Stream s2 = res.GetResponseStream())
					{
						using (StreamReader r = new StreamReader(s2, Encoding.UTF8))
						{
							ret = r.ReadToEnd();
						}
					}
				}
			}
			return ret;
		}

		public static String PostWithRetries(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				bool gzip,
				int retries,
				int sleepTimeMSecs,
				string vmName)
		{
			return PostWithRetries(url, data, headers, gzip, retries, sleepTimeMSecs, 0, vmName);
		}

		public static String PostWithRetries(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				bool gzip,
				int retries,
				int sleepTimeMSecs,
				int timeout,
				string vmName)
		{
			string res = null;
			int r = retries;
			while (r > 0)
			{
				try
				{
					res = Common.HTTP.Client.Post(url, data, headers, false, timeout);
					break;
				}
				catch (Exception e)
				{
					if (r == retries)
					{
						RegistryKey HKLMregistry = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
						string installDir = (string)HKLMregistry.GetValue("InstallDir");
						if (!Common.Utils.IsGlHotAttach(vmName))
						{
							Common.Utils.StartHiddenFrontend(vmName);
						}
						Process.Start(Path.Combine(installDir, @"HD-Agent.exe"));
					}

					Logger.Error("Exception when posting");
					Logger.Error(e.Message);
				}
				r--;
				Thread.Sleep(sleepTimeMSecs);
			}
			return res;
		}

		public static String HTTPGaeFileUploader(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				String filepath,
				String contentType,
				bool gzip
				)
		{
			return HTTPGaeFileUploader(url, data, headers, filepath, contentType, gzip, null);
		}

		public static String HTTPGaeFileUploader(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				String filepath,
				String contentType,
				bool gzip,
				string version
				)
		{
			try
			{
				Logger.Info("url = " + url);
				if (Features.IsFeatureEnabled(Features.CHINA_CLOUD) &&
						url.Contains(Service.Host))
				{
					url = url.Replace(Service.Host, Service.Host2);
					Logger.Info("CHINA_CLOUD enabled, new url = {0}", url);
				}
				return HTTPGaeFileUploaderInternal(url, data, headers, filepath, contentType, gzip, version);
			}
			catch (Exception e)
			{
				if (url.Contains(Service.Host))
				{
					//				Logger.Error("Could not send to " + url);
					Logger.Error(e.Message);
					url = url.Replace(Service.Host, Service.Host2);
					Logger.Info("New url = " + url);
					return HTTPGaeFileUploaderInternal(url, data, headers, filepath, contentType, gzip, version);
				}
				else if (url.Contains(Service.Host2))
				{
					Logger.Error("Could not send to " + url);
					Logger.Error(e.Message);
					url = url.Replace(Service.Host2, Service.Host);
					Logger.Info("New url = " + url);
					return HTTPGaeFileUploaderInternal(url, data, headers, filepath, contentType, gzip, version);
				}
				else
				{
					throw e;
				}
			}
		}

		private static String HTTPGaeFileUploaderInternal(String url,
				Dictionary<String, String> data,
				Dictionary<String, String> headers,
				String filepath,
				String contentType,
				bool gzip,
				string version
				)
		{
			if (filepath == null || !File.Exists(filepath))
				return Post(url, data, headers, gzip);

			String res = Get(url, null, false);

			JSonReader reader = new JSonReader();
			IJSonObject obj = reader.ReadAsJSonObject(res);

			String postUrl = null;

			if (obj["success"].BooleanValue)
				postUrl = obj["url"].StringValue;

			return HttpUploadFile(postUrl, filepath, "file", contentType, headers, data, version);
		}

		public static String HttpUploadFile(string url, string file, string paramName, string contentType,
				Dictionary<string, string> headers, Dictionary<string, string> data)
		{
			return HttpUploadFile(url, file, paramName, contentType, headers, data, null);
		}

		public static String HttpUploadFile(string url, string file, string paramName, string contentType,
				Dictionary<string, string> headers, Dictionary<string, string> data, string version)
		{
			try
			{
				Logger.Info("url = " + url);
				if (Features.IsFeatureEnabled(Features.CHINA_CLOUD) &&
						url.Contains(Service.Host))
				{
					url = url.Replace(Service.Host, Service.Host2);
					Logger.Info("CHINA_CLOUD enabled, new url = {0}", url);
				}
				return HttpUploadFileInternal(url, file, paramName, contentType, headers, data, version);
			}
			catch (Exception e)
			{
				if (url.Contains(Service.Host))
				{
					//				Logger.Error("Could not send to " + url);
					Logger.Error(e.Message);
					url = url.Replace(Service.Host, Service.Host2);
					Logger.Info("New url = " + url);
					return HttpUploadFileInternal(url, file, paramName, contentType, headers, data, version);
				}
				else if (url.Contains(Service.Host2))
				{
					Logger.Error("Could not send to " + url);
					Logger.Error(e.Message);
					url = url.Replace(Service.Host2, Service.Host);
					Logger.Info("New url = " + url);
					return HttpUploadFileInternal(url, file, paramName, contentType, headers, data, version);
				}
				else
				{
					throw e;
				}
			}
		}

		private static String HttpUploadFileInternal(string url, string file, string paramName, string contentType,
				Dictionary<string, string> headers, Dictionary<string, string> data, string version)
		{
			Logger.Info(string.Format("Uploading {0} to {1}", file, url));
			string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
			byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

			Uri u = new Uri(url);
			//		Logger.Info("Resolving proxy");

			HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
			wr.ContentType = "multipart/form-data; boundary=" + boundary;
			wr.Method = "POST";
			wr.KeepAlive = true;
			wr.Timeout = 300000;

			wr.UserAgent = Common.Utils.UserAgent(User.GUID, version);

			if (!(u.Host.Contains("localhost") || u.Host.Contains("127.0.0.1")))
			{
				Logger.Debug("URI of proxy = " + wr.Proxy.GetProxy(u));
			}

			if (headers != null)
			{
				foreach (KeyValuePair<String, String> o in headers)
				{
					wr.Headers.Set(o.Key, o.Value);
				}
			}
			wr.Headers.Set("x_oem", Device.Profile.OEM);

			if (data == null)
				data = new Dictionary<String, String>();

			//		Logger.Info("Making request");
			Stream rs = wr.GetRequestStream();

			string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
			//		Logger.Info("Reading data");
			foreach (KeyValuePair<string, string> val in data)
			{
				rs.Write(boundarybytes, 0, boundarybytes.Length);
				string formitem = string.Format(formdataTemplate, val.Key, val.Value);
				byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
				rs.Write(formitembytes, 0, formitembytes.Length);
			}
			rs.Write(boundarybytes, 0, boundarybytes.Length);
			//		Logger.Info("Data read");

			string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
			string header = string.Format(headerTemplate, paramName, file, contentType);
			byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
			rs.Write(headerbytes, 0, headerbytes.Length);

			//		Logger.Info("Reading file");
			string path = Environment.ExpandEnvironmentVariables("%TEMP%");
			path = Path.Combine(path, Path.GetFileName(file)) + "_bst";
			File.Copy(file, path);
			if (contentType.Equals("text/plain"))
			{
				int maxBytesToWrite = 1 * 1024 * 1024;  // 1 MB
				string bodyText = File.ReadAllText(path);
				byte[] bodybytes = new byte[maxBytesToWrite];
				bodybytes = System.Text.Encoding.UTF8.GetBytes(bodyText);
				rs.Write(bodybytes, 0, bodybytes.Length);
			}
			else
			{
				FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
				byte[] buffer = new byte[4096];
				int bytesRead = 0;
				while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
				{
					rs.Write(buffer, 0, bytesRead);
				}
				fileStream.Close();
			}
			File.Delete(path);
			//		Logger.Info("File read");

			byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
			rs.Write(trailer, 0, trailer.Length);
			rs.Close();

			string res = null;
			WebResponse wresp = null;
			try
			{
				//			Logger.Info("Sending request");
				wresp = wr.GetResponse();
				Stream stream2 = wresp.GetResponseStream();
				StreamReader reader2 = new StreamReader(stream2);

				res = reader2.ReadToEnd();
				Logger.Info(string.Format("File uploaded, server response is: {0}", res));
			}
			catch (Exception ex)
			{
				Logger.Error("Error uploading file", ex);
				if (wresp != null)
				{
					wresp.Close();
					wresp = null;
				}

				throw ex;
			}
			finally
			{
				wr = null;
			}
			return res;
		}

		public static void LogHeaders(WebHeaderCollection h)
		{
			for (int i = 0; i < h.Count; ++i)
				Logger.Info("{0} = {1}", h.Keys[i], h[i]);
		}

	}
}
