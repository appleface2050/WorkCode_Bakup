using System;
using System.ComponentModel;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
using Microsoft.Win32;

namespace BlueStacks.hyperDroid.Frontend
{


	/*
	 * Legal Frontend States
	 *
	 *     Initial
	 *
	 *     The user has started the frontend and we don't yet know
	 *     if the requested VM is running.  We don't stay in this
	 *     state very long, so we don't display anything interesting.
	 *
	 *     Possible transitions from this state are:
	 *         1)  Stopped - if the requested VM is not running
	 *         2)  ConnectingBlank - if the requested VM is running
	 *             and the ConnectingBlankEnabled configuration
	 *             parameter is set
	 *         3)  Connecting - otherwise
	 *
	 *     Stopped
	 *
	 *     We know for sure that the requested VM is either stopped
	 *     or broken.  This screen gives the user the options of
	 *     trying to start the VM or closing the frontend.
	 *
	 *     Possible transitions from this state are:
	 *         1)  Starting - if the user requested we start the VM
	 *         2)  Connecting - if the VM was started out of band
	 *             and the HideBootProgress configuration parameter
	 *             is set
	 *         3)  Connected - if the VM was started out of band
	 *             and the HideBootProgress configuration parameter
	 *             is not set
	 *
	 *     Starting
	 *
	 *     The user has requested that we start the VM.  This screen
	 *     reflects that the VM is starting and gives the user the
	 *     option of closing the frontend.
	 *
	 *     Possible transitions from this state are:
	 *         1)  CannotStart - if something bad happened while
	 *             trying to start the VM
	 *         2)  Connecting - if the frontend attaches to the VM
	 *         3)  Connected - if the HideBootProgress configuration
	 *             parameter is not set
	 *
	 *     CannotStart
	 *
	 *     Something went wrong while we were trying to start the
	 *     VM.  This screen shows a message to the user and gives
	 *     him/her the option of closing the frontend.
	 *
	 *     ConnectingBlank
	 *
	 *     The frontend managed to immediately attach to a VM, so
	 *     we should try to connect without showing the Connecting
	 *     screen.
	 *
	 *     Possible transitions from this state are:
	 *         1)  Connected - if the frontend receives the VM
	 *             event handler notification
	 *         2)  Connecting - if the frontend doesn't receive
	 *             the VM event handler notification before a
	 *             timeout has expired
	 *
	 *     Connecting
	 *
	 *     The frontend managed to attach to a VM and it is waiting
	 *     for a notification from the VM event handler that the
	 *     VM has finished booting.  This screen reflects that the
	 *     VM is booting and gives the user the option of closing
	 *     the frontend.
	 *
	 *     Possible transitions from this state are:
	 *         1)  Stopped - if the VM stops before receiving
	 *             the VM event handler notification
	 *         2)  Connected - if the frontend receives the VM
	 *             event handler notification
	 *
	 *     Connected
	 *
	 *     The frontend is connected to the VM, displaying the VM's
	 *     console and receiving input.
	 *
	 *     Possible transitions from this state are:
	 *         1)  Stopped - if the VM stops
	 */
	public enum State
	{
		Initial,
		Stopped,
		Stopping,
		Starting,
		CannotStart,
		ConnectingBlank,
		ConnectingVideo,
		ConnectingGuest,
		Connecting,
		Connected,
		Quitting,
		Error,
		__StateCount,
	};

	public class StateMachine
	{

		public delegate void TransitionCallback(State oldState, State newState);

		private static TimeSpan WAIT_TIME = new TimeSpan(0, 0, 30);

		private struct CallbackPair
		{
			public TransitionCallback FromCallback;
			public TransitionCallback ToCallback;
		};

		private delegate void BooleanCallback(bool success);

		private State mState = State.Initial;
		private SerialWorkQueue mSerialQueue = new SerialWorkQueue();

		private Control mControl;
		private String mVmName;
		private bool mCallerFailure;

		private System.Windows.Forms.Timer mStateTimer;
		private CallbackPair[,] mCallbackTable;

		private Interop.Monitor mMonitor;
		private Interop.Video mVideo;
		private Interop.Manager mManager;

		private bool mFirstMonitorAttachAttempt = true;
		private bool mFirstVideoAttachAttempt = true;

		public StateMachine(Control control, String vmName)
		{
			mControl = control;
			mVmName = vmName;

			mCallbackTable = new CallbackPair[(int)State.__StateCount, (int)State.__StateCount];
		}

		public State CurrentState
		{
			get
			{
				return mState;
			}
			set
			{
				mState = value;
				Console.s_Console.UpdateStateToParentWindow();
			}
		}

		public Interop.Monitor Monitor
		{
			get
			{
				return mMonitor;
			}
			set
			{
				mMonitor = value;
			}
		}

		public Interop.Manager Manager
		{
			get
			{
				return mManager;
			}
			set
			{
				mManager = value;
			}
		}

		public Interop.Video Video
		{
			get
			{
				return mVideo;
			}
			set
			{
				mVideo = value;
			}
		}

		public void SignalFailure()
		{
			mCallerFailure = true;
		}

		public void SetCallbacks(State oldState, State newState, TransitionCallback fromCallback,
			TransitionCallback toCallback)
		{
			mCallbackTable[(int)oldState, (int)newState].FromCallback = fromCallback;
			mCallbackTable[(int)oldState, (int)newState].ToCallback = toCallback;
		}

		public void Start()
		{
			Logger.Info("StateMachine.{0}()", MethodBase.GetCurrentMethod().Name);
			AssertUIThread();

			/*
			 * Transition to the appropriate state depending on whether or not the Android
			 * service is running.
			 */

			ServiceController svcCtl = new ServiceController(Common.Strings.GetAndroidServiceName(mVmName));
			ServiceControllerStatus status = svcCtl.Status;

			if (status == ServiceControllerStatus.StartPending ||
				status == ServiceControllerStatus.StopPending)
				EnterStateStopping();
			else if (status == ServiceControllerStatus.Running)
				EnterStateConnectingVideo();
			else
				EnterStateStarting();
		}

		public void Quit()
		{
			CurrentState = State.Quitting;
			Logger.Info("StateMachine.{0}()", MethodBase.GetCurrentMethod().Name);
			AssertUIThread();
		}

		private void AssertUIThread()
		{
			UIHelper.AssertUIThread(mControl);
		}

		private void Transition(State newState)
		{
			Logger.Info("StateMachine.{0}({1})", MethodBase.GetCurrentMethod().Name, newState);
			AssertUIThread();

			if (mStateTimer != null)
			{
				mStateTimer.Stop();
				mStateTimer = null;
			}

			CallbackPair pair = mCallbackTable[(int)CurrentState, (int)newState];
			if (pair.FromCallback == null || pair.ToCallback == null)
				throw new ApplicationException(String.Format("Missing transition callback for {0} -> {1}",
					CurrentState, newState));

			State oldState = CurrentState;

			pair.FromCallback(oldState, newState);
			if (CurrentState != State.Quitting)
				CurrentState = newState;
			pair.ToCallback(oldState, newState);
		}

		private void EnterStateStopping()
		{
			Logger.Info("StateMachine.{0}()", MethodBase.GetCurrentMethod().Name);
			AssertUIThread();

			Transition(State.Stopping);

			if (!Common.Utils.IsHDPlusDebugMode())
			{
				ServiceStopAsync(delegate (bool success)
				{
					UIHelper.RunOnUIThread(mControl, delegate ()
					{
						if (!success)
							EnterStateError();
						else
							EnterStateStarting();
					});
				});
			}
			else
			{
				UIHelper.RunOnUIThread(mControl, delegate ()
				{
					EnterStateStarting();
				});
			}
		}

		private void EnterStateStarting()
		{
			Logger.Info("StateMachine.{0}()", MethodBase.GetCurrentMethod().Name);
			AssertUIThread();

			Transition(State.Starting);

			if (!Common.Utils.IsHDPlusDebugMode())
			{
				ServiceStartAsync(delegate (bool success)
				{
					UIHelper.RunOnUIThread(mControl, delegate ()
					{
						if (!success)
							EnterStateError();
						else
							EnterStateConnectingVideo();
					});
				});
			}
			else
			{
				UIHelper.RunOnUIThread(mControl, delegate ()
				{
					EnterStateConnectingVideo();
				});
			}
		}

		private void EnterStateError()
		{
			Logger.Info("StateMachine.{0}()", MethodBase.GetCurrentMethod().Name);
			AssertUIThread();

			Transition(State.Error);
		}

		private void EnterStateConnectingVideo()
		{
			Logger.Info("StateMachine.{0}()", MethodBase.GetCurrentMethod().Name);
			AssertUIThread();

			Transition(State.ConnectingVideo);

			mStateTimer = new System.Windows.Forms.Timer();
			mStateTimer.Interval = 250;

			mStateTimer.Tick += delegate (Object obj, EventArgs args)
			{

				if (!ConnectedTickCommon())
				{
					EnterStateError();
					return;
				}

				if (TryConnectVideo())
				{
					EnterStateConnectingGuest();
					return;
				}
			};

			mStateTimer.Start();
		}

		private bool ConnectedTickCommon()
		{
			AssertUIThread();

			if (CurrentState == State.Quitting)
			{
				Logger.Info("Quitting State");
				return true;
			}

			if (!Common.Utils.IsHDPlusDebugMode())
			{
				if (!IsServiceRunning())
				{
					Logger.Error("Android service has stopped");
					return false;
				}
			}

			if (mCallerFailure)
			{
				Logger.Error("Caller signaled failure");
				return false;
			}

			return true;
		}

		public bool TryConnectVideo()
		{
			if (mManager != null)
				throw new SystemException(
					"A connection to the manager is already open");
			if (mMonitor != null)
				throw new SystemException(
					"Another monitor is already attached");
			if (mVideo != null)
				throw new SystemException(
					"Another frame buffer is already attached");

			uint vmId = MonitorLocator.Lookup(mVmName);
			Interop.Manager manager = null;
			Interop.Monitor monitor = null;
			Interop.Video video = null;
			bool verbose = false;

			try
			{
				verbose = mFirstMonitorAttachAttempt;
				mFirstMonitorAttachAttempt = false;

				manager = Interop.Manager.Open();

				if (Common.Strings.IsEngineLegacy())
				{
					monitor = manager.Attach(vmId,
						delegate ()
						{
							Console.s_Console.guestHasStopped = true;
						});
				}
				else
				{
					monitor = manager.Attach(vmId, verbose, false);
				}

			}
			catch (Exception exc)
			{

				/*
				 * Only print a log message if we get something other than
				 * ERROR_FILE_NOT_FOUND.
				 */

				if (!IsExceptionFileNotFound(exc))
					Logger.Error(exc.ToString());

				if (manager != null)
				{
					manager.Close();
					manager = null;
				}
			}

			if (monitor == null)
				return false;

			verbose = mFirstVideoAttachAttempt;
			mFirstVideoAttachAttempt = false;

			if (verbose)
			{
				Logger.Info("Attached to VM {0}, ID {1}", mVmName, vmId);
				Logger.Info("Attaching to framebuffer");
			}

			try
			{
				if (Common.Strings.IsEngineLegacy())
				{
					video = monitor.VideoAttach();
				}
				else
				{
					video = monitor.VideoAttach(verbose);
				}
			}
			catch (Exception exc)
			{
				if (verbose)
				{
					Logger.Error("Cannot attach to guest video");
					Logger.Error(exc.ToString());
				}
			}

			if (video == null)
			{
				if (verbose)
					Logger.Info("Video not yet attached...");
				return false;
			}

			if (!Common.Strings.IsEngineLegacy())
			{
				//Plus Mode - create a new monitor that has a handle.
				monitor = manager.Attach(vmId, verbose, true);
				if (monitor == null)
				{
					Logger.Info("Could not Attach to a monitor");
				}
			}

			DumpFrameBufferInfo(video);

			//for legacy state machine
			Console.s_Console.forceVideoModeChange = true;

			mMonitor = monitor;
			mVideo = video;
			mManager = manager;

			return true;
		}


		private void DumpFrameBufferInfo(Interop.Video video)
		{
			Interop.Video.Mode mode = video.GetMode();
			uint stride = video.GetStride();
			IntPtr bufferAddr = video.GetBufferAddr();
			IntPtr bufferEnd = video.GetBufferEnd();
			uint bufferSize = video.GetBufferSize();

			Logger.Info("mode    = {0}x{1}x{2}", mode.Width, mode.Height, mode.Depth);
			Logger.Info("stride  = {0}", stride);
			Logger.Info("addr    = 0x{0}", bufferAddr.ToString("x"));
			Logger.Info("end     = 0x{0}", bufferEnd.ToString("x"));
			Logger.Info("size    = 0x{0}", bufferSize.ToString("x"));
		}

		private bool IsExceptionFileNotFound(Exception exc)
		{
			Exception inner = exc.InnerException;
			if (inner == null || inner.GetType() != typeof(Win32Exception))
				return false;

			Win32Exception win32 = (Win32Exception)inner;
			if (win32.NativeErrorCode != 2)
				return false;

			return true;
		}

		private void EnterStateConnectingGuest()
		{
			Logger.Info("StateMachine.{0}()", MethodBase.GetCurrentMethod().Name);
			AssertUIThread();

			Transition(State.ConnectingGuest);

			mStateTimer = new System.Windows.Forms.Timer();
			mStateTimer.Interval = 1000;

			mStateTimer.Tick += delegate (Object obj, EventArgs args)
			{

				if (!ConnectedTickCommon())
				{
					EnterStateError();
					return;
				}

				CheckGuestReadyAsync(delegate (bool success)
				{

					EnterStateConnectedOnUIThread(success);
				});
			};

			mStateTimer.Start();
		}

		private void EnterStateConnectedOnUIThread(bool success)
		{
			UIHelper.RunOnUIThread(mControl, delegate ()
			{
				if (CurrentState == State.ConnectingGuest)
				{
					if (success)
						EnterStateConnected();
				}
			});
		}
		private void CheckGuestReadyAsync(BooleanCallback callback)
		{
			ThreadPool.QueueUserWorkItem(delegate (Object stateInfo)
			{

				bool success = false;

				if (!Common.Utils.sIsWaitLockExist)
				{
					try
					{
						String url = String.Format("http://127.0.0.1:{0}/{1}",
							Common.VmCmdHandler.s_ServerPort, Common.VmCmdHandler.s_PingPath);
						String rsp = Common.HTTP.Client.Get(url, null, false, 1000);

						IJSonReader reader = new JSonReader();
						IJSonObject json = reader.ReadAsJSonObject(rsp);

						String text = json["result"].StringValue;
						success = text.Equals("ok");

					}
					catch (Exception exc)
					{

						Logger.Error("Guest not completely booted yet: " + exc.Message);
					}
				}
				else
				{
					Logger.Info("StateMachine.CheckGuestReadyAsync: Already thread running for ping command");
				}

				callback(success);
			});
		}

		private void EnterStateConnected()
		{
			Logger.Info("StateMachine.{0}()", MethodBase.GetCurrentMethod().Name);
			AssertUIThread();

			Transition(State.Connected);

			mStateTimer = new System.Windows.Forms.Timer();
			mStateTimer.Interval = 1000;

			mStateTimer.Tick += delegate (Object obj, EventArgs args)
			{

				if (!ConnectedTickCommon())
				{
					EnterStateError();
					return;
				}
			};

			mStateTimer.Start();
		}

		private bool IsServiceRunning()
		{
			ServiceController svcCtl = new ServiceController(Common.Strings.AndroidServiceName);
			return svcCtl.Status == ServiceControllerStatus.Running;
		}

		private void ServiceStopAsync(BooleanCallback callback)
		{
			Logger.Info("StateMachine.{0}()", MethodBase.GetCurrentMethod().Name);

			String serviceName = Common.Strings.AndroidServiceName;
			ServiceController svcCtl = new ServiceController(serviceName);

			ThreadPool.QueueUserWorkItem(delegate (Object stateInfo)
			{

				/*
				 * If the service is currently starting, wait for it to start before we
				 * try stopping it.
				 */

				if (svcCtl.Status == ServiceControllerStatus.StartPending)
					svcCtl.WaitForStatus(ServiceControllerStatus.Running, WAIT_TIME);

				if (svcCtl.Status == ServiceControllerStatus.StopPending)
					svcCtl.WaitForStatus(ServiceControllerStatus.Stopped, WAIT_TIME);

				if (svcCtl.Status == ServiceControllerStatus.Running)
				{

					Logger.Info("Trying to stop service {0}", serviceName);

					try
					{
						svcCtl.Stop();
						svcCtl.WaitForStatus(ServiceControllerStatus.Stopped, WAIT_TIME);

					}
					catch (Exception exc)
					{

						svcCtl.Refresh();

						if (svcCtl.Status == ServiceControllerStatus.StopPending)
							svcCtl.WaitForStatus(ServiceControllerStatus.Stopped, WAIT_TIME);

						if (svcCtl.Status != ServiceControllerStatus.Stopped)
						{
							Logger.Error("Cannot stop service {0}, status {1}",
								serviceName, svcCtl.Status);
							Logger.Error(exc.ToString());
						}
					}
				}

				if (svcCtl.Status != ServiceControllerStatus.Stopped)
				{
					Logger.Error("Service in state {0} after stop attempt", svcCtl.Status);
					callback(false);
				}
				else
				{
					callback(true);
				}
			});
		}

		private void ServiceStartAsync(BooleanCallback callback)
		{
			Logger.Info("StateMachine.{0}()", MethodBase.GetCurrentMethod().Name);

			String serviceName = Common.Strings.AndroidServiceName;
			ServiceController svcCtl = new ServiceController(serviceName);

			ThreadPool.QueueUserWorkItem(delegate (Object stateInfo)
			{

				try
				{
					Utils.EnableService(serviceName, "demand");

					svcCtl.Start();
					svcCtl.WaitForStatus(ServiceControllerStatus.Running, WAIT_TIME);

				}
				catch (Exception exc)
				{

					svcCtl.Refresh();

					if (svcCtl.Status == ServiceControllerStatus.StartPending)
						svcCtl.WaitForStatus(ServiceControllerStatus.Running, WAIT_TIME);

					if (svcCtl.Status != ServiceControllerStatus.Running)
					{
						Logger.Error("Cannot start service {0}, status {1}",
							serviceName, svcCtl.Status);
						Logger.Error(exc.ToString());
					}
				}

				if (svcCtl.Status != ServiceControllerStatus.Running)
				{
					Logger.Error("Service in state {0} after start attempt", svcCtl.Status);
					callback(false);
				}
				else
				{
					callback(true);
				}
			});
		}
	}

}
