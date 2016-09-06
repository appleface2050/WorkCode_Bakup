using CLRBrowserSourcePlugin.Shared;
using CLROBS;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Xilium.CefGlue;

namespace CLRBrowserSourcePlugin.Browser
{
    internal class BrowserWrapper
    {
        private BrowserClient browserClient;
        private CefBrowser browser;
        private object browserLock = new object();
        private HTTPServer httpServer;
        private bool loadUrl = true;

        public BrowserConfig BrowserConfig { get; private set; }

        public String oemName;
        public RenderProcess renderProcess;

        public BrowserWrapper()
        {
            browserClient = null;
            browser = null;

            Dictionary<String, HTTPHandler.RequestHandler> routes = new Dictionary<String, HTTPHandler.RequestHandler>();
            routes.Add("/changetheme", HTTPHandler.ChangeThemeHandler);
            routes.Add("/updatesettings", HTTPHandler.UpdateSettingsHandler);
            routes.Add("/ping", HTTPHandler.PingHandler);

            Thread t = new Thread(delegate() {
                String keyPath = @"Software\BlueStacks";
                try
                {
                    String oemName = File.ReadAllText("tag.txt");
                    if (!oemName.Equals("gamemanager"))
                    {
						keyPath += oemName;
					}
                }
                catch (Exception e)
                {
                    API.Instance.Log("tag.txt not found: %s", e.Message);
                }
                keyPath += @"\BlueStacksGameManager\Config";
               
                for (int port = 2911; port <= 2920; ++port)
                {
                    try
                    {
                        httpServer = new HTTPServer(3, routes, this);
                        httpServer.Start(port);
                        API.Instance.Log("Setting CLRBrowserServerPort: " + port.ToString()); 
                        RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true);
						key.SetValue("CLRBrowserServerPort", port, RegistryValueKind.DWord);
                        key.Close();
						break;
                    } 
                    catch (Exception ex)
                    {
                        if (port == 2920)
                        {
                            API.Instance.Log("No free port available: " + ex.Message);
                        }
                        continue;
                    }
                }
               
            });
            t.IsBackground = true;
            t.Start();
        }

        private void InitClient(BrowserSource browserSource)
        {
            Debug.Assert(browserClient == null);

            browserClient = new BrowserClient();

            browserClient.LoadHandler.OnLoadEndEvent =
                new OnLoadEndEventHandler(OnLoadEnd);
            browserClient.RenderHandler.SizeEvent =
                new SizeEventHandler(Size);
            browserClient.RenderHandler.PaintEvent =
                new PaintEventHandler(browserSource.RenderTexture);
            browserClient.RenderHandler.CreateTextureEvent =
                new CreateTextureEventHandler(browserSource.CreateTexture);
            browserClient.RenderHandler.DestroyTextureEvent =
                new DestroyTextureEventHandler(browserSource.DestroyTexture);
        }

        private void UninitClient()
        {
            Debug.Assert(browserClient != null);

            browserClient.LoadHandler = null;
            browserClient.DisplayHandler = null;
            browserClient.RenderHandler.Cleanup();
            browserClient.RenderHandler = null;

            browserClient = null;
        }

        public bool CreateBrowser(BrowserSource browserSource,
            BrowserConfig browserConfig)
        {
            if (browserClient == null)
            {
                InitClient(browserSource);
            }


            Debug.Assert(browserClient != null);
            Debug.Assert(browserConfig != null);

            BrowserConfig = browserConfig;

            CefWindowInfo windowInfo = CefWindowInfo.Create();
            windowInfo.Width = (int)browserConfig.BrowserSourceSettings.Width;
            windowInfo.Height = (int)browserConfig.BrowserSourceSettings.Height;
            windowInfo.SetAsWindowless(IntPtr.Zero, true);

            BrowserInstanceSettings settings = AbstractSettings.DeepClone(
                BrowserSettings.Instance.InstanceSettings);
            settings.MergeWith(browserConfig.BrowserInstanceSettings);

            CefBrowserSettings browserSettings = new CefBrowserSettings
            {
                WindowlessFrameRate = browserConfig.BrowserSourceSettings.Fps,
                ApplicationCache = settings.ApplicationCache,
                CaretBrowsing = settings.CaretBrowsing,
                CursiveFontFamily = settings.CursiveFontFamily,
                Databases = settings.Databases,
                DefaultEncoding = settings.DefaultEncoding,
                DefaultFixedFontSize = settings.DefaultFixedFontSize,
                DefaultFontSize = settings.DefaultFontSize,
                FantasyFontFamily = settings.FantasyFontFamily,
                FileAccessFromFileUrls = settings.FileAccessFromFileUrls,
                FixedFontFamily = settings.FixedFontFamily,
                ImageLoading = settings.ImageLoading,
                ImageShrinkStandaloneToFit = settings.ImageShrinkStandaloneToFit,
                Java = settings.Java,
                JavaScript = settings.JavaScript,
                JavaScriptAccessClipboard = settings.JavaScriptAccessClipboard,
                JavaScriptCloseWindows = settings.JavaScriptCloseWindows,
                JavaScriptDomPaste = settings.JavaScriptDomPaste,
                JavaScriptOpenWindows = settings.JavaScriptOpenWindows,
                LocalStorage = settings.LocalStorage,
                MinimumFontSize = settings.MinimumFontSize,
                MinimumLogicalFontSize = settings.MinimumLogicalFontSize,
                Plugins = settings.Plugins,
                RemoteFonts = settings.RemoteFonts,
                SansSerifFontFamily = settings.SansSerifFontFamily,
                SerifFontFamily = settings.SerifFontFamily,
                StandardFontFamily = settings.StandardFontFamily,
                //TabToLinks = settings.TabToLinks,
                //TextAreaResize = settings.TextAreaResize,
                UniversalAccessFromFileUrls =
                    settings.UniversalAccessFromFileUrls,
                WebGL = settings.WebGL,
                WebSecurity = settings.WebSecurity
            };

            String url = browserConfig.BrowserSourceSettings.Url;

            if (browserConfig.BrowserSourceSettings.IsApplyingTemplate)
            {
                url = "http://absolute";
            }

            lock (browserLock)
            {
                ManualResetEventSlim createdBrowserEvent =
                    new ManualResetEventSlim();
                CefRuntime.PostTask(CefThreadId.UI, BrowserTask.Create(() =>
                {
                    try
                    {
                        browser = CefBrowserHost.CreateBrowserSync(windowInfo,
                            browserClient, browserSettings, new Uri(url));
                        BrowserManager.Instance.RegisterBrowser(browser.Identifier,
                            this);
                       

                        // request the render process id for volume control
                        browser.SendProcessMessage(CefProcessId.Renderer,
                            CefProcessMessage.Create("renderProcessIdRequest"));
                    }
                    catch (Exception)
                    {
                        browser = null;
                    }
                    finally
                    {
                        createdBrowserEvent.Set();
                    }
                }));
                createdBrowserEvent.Wait();
            }

            return browser != null;
        }

        private ManualResetEventSlim closeFinishedEvent;

        private void OnBeforeClose(CefBrowser browser)
        {
            // Remove the transient life span handler
            browserClient.LifeSpanHandler = null;
            closeFinishedEvent.Set();
        }

        public void CloseBrowser(bool isForcingClose)
        {
            httpServer.Stop();
            closeFinishedEvent =
                new ManualResetEventSlim(false);

            // make sure we arent mid browser creation
            // lock on browser
            lock (browserLock)
            {
                CefRuntime.PostTask(CefThreadId.UI, BrowserTask.Create(() =>
                {
                    if (browser != null)
                    {
                        browserClient.LifeSpanHandler.OnBeforeCloseEvent =
                            new OnBeforeCloseEventHandler(OnBeforeClose);

                        browser.GetHost().CloseBrowser(isForcingClose);
                    }
                    else
                    {
                        closeFinishedEvent.Set();
                    }
                }));

                closeFinishedEvent.Wait();

                BrowserManager.Instance.UnregisterBrowser(
                    browser.Identifier);

                // clean up the other client stuff
                UninitClient();

                // make sure browser doesn't get disposed before close has finished
                browser = null;
            }

            closeFinishedEvent = null;
        }

        public void OnLoadEnd(CefBrowser browser, CefFrame frame,
            int httpStatusCode)
        {
            // main frame
            if (frame.IsMain)
            {
                string base64EncodedCss = "data:text/css;charset=utf-8;base64,";
                base64EncodedCss +=
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(
                        BrowserConfig.BrowserSourceSettings.CSS));

                string script = ""
                    + "document.location.href = \"" + BrowserConfig.BrowserSourceSettings.LoadUrl + "\";";

                if (loadUrl)
                {
                    API.Instance.Log("Sleeping for 500 ms as seen some bug where page load gives error");
                    Thread.Sleep(500);
                    loadUrl = false;
                    API.Instance.Log("Loading url : " + BrowserConfig.BrowserSourceSettings.LoadUrl);
                    frame.LoadUrl(BrowserConfig.BrowserSourceSettings.LoadUrl);
                }
            }
        }

        public void ExecuteJS(string lines)
        {
            CefFrame frame = browser.GetFocusedFrame();
            // main frame
            if (frame.IsMain)
            {
                string base64EncodedCss = "data:text/css;charset=utf-8;base64,";
                base64EncodedCss +=
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(
                        BrowserConfig.BrowserSourceSettings.CSS));

                string script = lines;
                    

                frame.ExecuteJavaScript(script,
                    null, 0);
            }

        }

        public bool Size(ref CefRectangle rect)
        {
            rect.X = 0;
            rect.Y = 0;
            rect.Width = BrowserConfig.BrowserSourceSettings.Width;
            rect.Height = BrowserConfig.BrowserSourceSettings.Height;

            return true;
        }

        public void UpdateRenderProcessId(int renderProcessId)
        {
            RenderProcess renderProcess = new RenderProcess(renderProcessId);
            renderProcess.IsMuted = BrowserConfig.BrowserSourceSettings.IsMuted;
            renderProcess.Volume = BrowserConfig.BrowserSourceSettings.Volume;
        }
    }
}
