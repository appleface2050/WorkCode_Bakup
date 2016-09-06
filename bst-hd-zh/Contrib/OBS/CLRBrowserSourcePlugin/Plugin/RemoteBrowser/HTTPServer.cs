using System;
using System.IO;
using System.Net;
using System.Web;
using System.Collections.Specialized;
using System.Text;
using System.Timers;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Reflection;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net.Security;
using System.Windows.Forms;
using System.Management;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;

using CLROBS;

namespace CLRBrowserSourcePlugin.Browser
{
    class HTTPServer : IDisposable
    {
        private readonly HttpListener mListener;
        private readonly Thread mListenerThread;
        private readonly Thread[] mWorkers;
        private readonly ManualResetEvent mStopEvent, mReadyEvent;
        private Queue<HttpListenerContext> mQueue;
        private BrowserWrapper mBrowserWrapper;

        Dictionary<String, HTTPHandler.RequestHandler> mRoutes = null;

        public HTTPServer(int maxThreads, Dictionary<String, HTTPHandler.RequestHandler> routes,
            BrowserWrapper browserWrapper)
        {
            mRoutes = routes;
            mWorkers = new Thread[maxThreads];
            mQueue = new Queue<HttpListenerContext>();
            mStopEvent = new ManualResetEvent(false);
            mReadyEvent = new ManualResetEvent(false);
            mListener = new HttpListener();
            mListenerThread = new Thread(HandleRequests);
            mBrowserWrapper = browserWrapper;
        }

        public void Start(int port)
        {
            mListener.Prefixes.Add(String.Format(@"http://*:{0}/", port));
            API.Instance.Log("in start server");
            try
            {
                mListener.Start();
                mListenerThread.Start();

                for (int i = 0; i < mWorkers.Length; i++)
                {
                    mWorkers[i] = new Thread(Worker);
                    mWorkers[i].Start();
                }
            }
            catch (Exception ex)
            {
                API.Instance.Log(ex.Message);
            }
        }

        public void Dispose()
        { Stop(); }

        public void Stop()
        {
            mStopEvent.Set();
            mListenerThread.Join();
            foreach (Thread worker in mWorkers)
                worker.Join();
            mListener.Stop();
        }

        private void HandleRequests()
        {
            while (mListener.IsListening)
            {
                var context = mListener.BeginGetContext(ContextReady, null);

                if (0 == WaitHandle.WaitAny(new[] { mStopEvent, context.AsyncWaitHandle }))
                    return;
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                lock (mQueue)
                {
                    mQueue.Enqueue(mListener.EndGetContext(ar));
                    mReadyEvent.Set();
                }
            }
            catch { return; }
        }

        private void Worker()
        {
            WaitHandle[] wait = new[] { mReadyEvent, mStopEvent };
            while (0 == WaitHandle.WaitAny(wait))
            {
                HttpListenerContext context;
                lock (mQueue)
                {
                    if (mQueue.Count > 0)
                        context = mQueue.Dequeue();
                    else
                    {
                        mReadyEvent.Reset();
                        continue;
                    }
                }

                try { ProcessRequestInBg(context); }
                catch (Exception e) { Console.Error.WriteLine(e); }
            }
        }

        public void ProcessRequestInBg(HttpListenerContext context)
        {
            string url = context.Request.Url.AbsolutePath;
            HTTPHandler.RequestHandler handler = (HTTPHandler.RequestHandler)mRoutes[url];
            API.Instance.Log("request received: " + url);
            Thread t = new Thread(delegate()
            {
                handler(context.Request, context.Response, mBrowserWrapper);
            });
            t.IsBackground = true;
            t.Start();

            return;
        }
    }

    class HTTPHandler
    {
        public delegate void RequestHandler(HttpListenerRequest req, HttpListenerResponse res, BrowserWrapper browserWrapper);

        public static void UpdateSettingsHandler(HttpListenerRequest req, HttpListenerResponse res, BrowserWrapper browserWrapper)
        {
            RequestData requestData = GetRequestData(req);
            API.Instance.Log("change Url: {0}", requestData.data["settings"].ToString());
            var script = ""
                    + "var clrBrowserSettingJSONObj = JSON.parse('" + requestData.data["settings"].ToString() + "');"
                    + "window.setconfig(clrBrowserSettingJSONObj);";
            //var script = "document.body.innerHTML='"+requestData.data["settings"].ToString().Replace("\"", "\\\"")+"';";
            browserWrapper.ExecuteJS(script);
            WriteSuccessJson(res);
            API.Instance.Log("done");
        }

        public static void ChangeThemeHandler(HttpListenerRequest req, HttpListenerResponse res, BrowserWrapper browserWrapper)
        {
            RequestData requestData = GetRequestData(req);
            string url = browserWrapper.BrowserConfig.BrowserSourceSettings.LoadUrl;
            int index = url.IndexOf("/filters/theme/");
            int subStringLength = index + "/filters/theme/".Length;

            string changeUrl = url.Substring(0, subStringLength);

            string theme = requestData.data["theme"];
            string appPkg = requestData.data["appPkg"];
            string queryParam = requestData.data["queryParam"];
            changeUrl += appPkg + "/" + theme + "/index.html?" + queryParam;

            API.Instance.Log("change Url: {0}", changeUrl);

            var script = "document.location.href=\"" + changeUrl + "\";";
            browserWrapper.ExecuteJS(script);
            WriteSuccessJson(res);
        }

        public static void PingHandler(HttpListenerRequest req, HttpListenerResponse res, BrowserWrapper browserWrapper)
        {
            RequestData requestData = GetRequestData(req);
            string url = requestData.data["url"];

            API.Instance.Log("change Url: {0}", url);

            var script = "document.location.href=\"" + url + "\";";
            browserWrapper.ExecuteJS(script);
            WriteSuccessJson(res);
        }

        public static void WriteSuccessJson(HttpListenerResponse res)
        {
            Byte[] b = Encoding.UTF8.GetBytes("{\"success\":true}");
            res.ContentLength64 = b.Length;
            res.OutputStream.Write(b, 0, b.Length);
            res.OutputStream.Flush();
        }

        public static RequestData GetRequestData(HttpListenerRequest req)
        {
            RequestData requestData = new RequestData();

            Stream streamData = req.InputStream;
            byte[] byteData;

            byte[] buffer = new byte[16 * 1024];
            MemoryStream ms = new MemoryStream();
            int read;
            while ((read = streamData.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }

            byteData = ms.ToArray();
            ms.Close();
            streamData.Close();

            string stringData = Encoding.UTF8.GetString(byteData);

            requestData.data = HttpUtility.ParseQueryString(stringData);
            return requestData;
        }
    }

    public class RequestData
    {
        public NameValueCollection headers;
        public NameValueCollection queryString;
        public NameValueCollection data;
        public NameValueCollection files;

        public RequestData()
        {
            headers = new NameValueCollection();
            queryString = new NameValueCollection();
            data = new NameValueCollection();
            files = new NameValueCollection();
        }
    }

}