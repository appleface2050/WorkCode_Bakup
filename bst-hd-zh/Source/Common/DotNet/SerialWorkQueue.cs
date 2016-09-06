using System;
using System.Collections.Generic;
using System.Threading;

namespace BlueStacks.hyperDroid.Common
{

	public class SerialWorkQueue
	{

		public delegate void Work();
		public delegate void ExceptionHandlerCallback(Exception exc);

		private static Int32 sAutoId = 0;

		private Thread mThread;
		private Queue<Work> mQueue;
		private Object mLock;

		private ExceptionHandlerCallback mExceptionHandler;

		public SerialWorkQueue()
		{
			String name = String.Format("SerialWorkQueue.{0}",
				Interlocked.Increment(ref sAutoId));

			Initialize(name);
		}

		public SerialWorkQueue(String name)
		{
			Initialize(name);
		}

		private void Initialize(String name)
		{
			mQueue = new Queue<Work>();
			mLock = new Object();

			mThread = new Thread(Run);
			mThread.Name = name;
			mThread.IsBackground = true;
		}

		public ExceptionHandlerCallback ExceptionHandler
		{
			set
			{
				mExceptionHandler = value;
			}
		}

		public void Start()
		{
			mThread.Start();
		}

		public void Join()
		{
			mThread.Join();
		}

		public void Stop()
		{
			Enqueue(null);
		}

		public void Enqueue(Work work)
		{
			lock (mLock)
			{
				mQueue.Enqueue(work);
				Monitor.PulseAll(mLock);
			}
		}

		public void DispatchAsync(Work work)
		{
			Enqueue(work);
		}

		public void DispatchAfter(double delay, Work work)
		{
			System.Timers.Timer timer = new System.Timers.Timer();

			timer.Elapsed += delegate (Object source,
				System.Timers.ElapsedEventArgs evt)
			{
				DispatchSync(work);
				timer.Close();
			};

			timer.Interval = delay;
			timer.Enabled = true;
		}

		public void DispatchSync(Work work)
		{
			EventWaitHandle waitHandle = new EventWaitHandle(false,
				EventResetMode.ManualReset);

			Enqueue(delegate ()
			{
				work();
				waitHandle.Set();
			});

			waitHandle.WaitOne();
			waitHandle.Close();
		}

		public bool IsCurrentWorkQueue()
		{
			return Thread.CurrentThread == mThread;
		}

		private void Run()
		{
			while (true)
			{

				Work work;

				lock (mLock)
				{

					while (mQueue.Count == 0)
						Monitor.Wait(mLock);

					work = mQueue.Dequeue();
				}

				if (work == null)
					break;

				try
				{
					work();

				}
				catch (Exception exc)
				{

					if (mExceptionHandler != null)
						mExceptionHandler(exc);
					else
						throw exc;
				}
			}
		}
	}

}
