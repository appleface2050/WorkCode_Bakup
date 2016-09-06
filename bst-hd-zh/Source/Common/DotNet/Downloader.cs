using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Cloud.Services;

public class Downloader
{
	public delegate void UpdateProgressCallback(int percent);
	public delegate void DownloadCompletedCallback(string filePath);
	public delegate void ExceptionCallback(Exception e);
	public delegate bool ContentTypeCallback(string contentType);
	public delegate void SizeDownloadedCallback(Int64 size);

	private List<KeyValuePair<Thread, Worker>> mWorkers;
	private WebHeaderCollection mResponseHeaders;

	private String mUrl;
	private String mFileName;
	private int mNrWorkers;

	private UpdateProgressCallback mUpdateProgressCallback;
	private DownloadCompletedCallback mDownloadCompletedCallback;
	private ExceptionCallback mExceptionCallback;
	private ContentTypeCallback mContentTypeCallback;
	private SizeDownloadedCallback mSizeDownloadedCallback;

	public Downloader(int nrWorkers,
			String url,
			String fileName)
	{
		this.mUrl = url;
		this.mFileName = fileName;
		this.mNrWorkers = nrWorkers;
	}

	public void Download(
			UpdateProgressCallback updateProgressCb,
			DownloadCompletedCallback downloadedCb,
			ExceptionCallback exceptionCb)
	{
		this.Download(updateProgressCb, downloadedCb, exceptionCb, null);
	}

	public double GetPayloadSize(string url)
	{
		PayloadInfo payloadInfo = GetRemotePayloadInfo(url);
		double size = payloadInfo.Size / (1024 * 1024 * 1.0F);
		size = Math.Round(size, 2);
		return size;
	}

	public void Download(
			UpdateProgressCallback updateProgressCb,
			DownloadCompletedCallback downloadedCb,
			ExceptionCallback exceptionCb,
			ContentTypeCallback contentTypeCb)
	{
		this.Download(updateProgressCb, downloadedCb, exceptionCb, contentTypeCb, null);
	}

	public void Download(
			UpdateProgressCallback updateProgressCb,
			DownloadCompletedCallback downloadedCb,
			ExceptionCallback exceptionCb,
			ContentTypeCallback contentTypeCb,
			SizeDownloadedCallback sizeDownloadedCb)
	{
		this.mUpdateProgressCallback = updateProgressCb;
		this.mDownloadCompletedCallback = downloadedCb;
		this.mExceptionCallback = exceptionCb;
		this.mContentTypeCallback = contentTypeCb;
		this.mSizeDownloadedCallback = sizeDownloadedCb;

		Logger.Info("Downloading {0} to: {1}", mUrl, mFileName);
		string filePath = mFileName;
		PayloadInfo payloadInfo;

		try
		{
			string currDir = Path.GetDirectoryName(Application.ExecutablePath);
			string currDirFileName = Path.Combine(currDir, Path.GetFileName(mFileName));
			if (File.Exists(currDirFileName))
			{
				Logger.Info("{0} already downloaded to {1}",
						mUrl, currDirFileName);
				filePath = currDirFileName;
				goto download_completed;
			}
			else
			{
				try
				{
					payloadInfo = GetRemotePayloadInfo(mUrl);
					string contentType = mResponseHeaders["Content-Type"];
					if (contentType == "application/vnd.android.package-archive")
						mFileName = Path.ChangeExtension(mFileName, ".apk");
					filePath = mFileName;
					if (contentTypeCb != null && !contentTypeCb(contentType))
					{
						Logger.Info("Cancelling download");
						return;
					}
				}
				catch (Exception)
				{
					Logger.Error(string.Format("Unable to send to {0}", mUrl));
					if (mUrl.Contains(Service.Host))
					{
						mUrl = mUrl.Replace(Service.Host, Service.Host2);
						Logger.Info(string.Format("Trying {0}", mUrl));
						payloadInfo = GetRemotePayloadInfo(mUrl);
					}
					else
						throw;
				}

				if (File.Exists(mFileName))
				{
					if (IsPayloadOk(mFileName, payloadInfo.Size))
					{
						Logger.Info(mUrl + " already downloaded");
						goto download_completed;
					}
					else
						File.Delete(mFileName);
				}
			}

			if (!payloadInfo.SupportsRangeRequest)
				mNrWorkers = 1;

			mWorkers = MakeWorkers(mNrWorkers,
					mUrl,
					mFileName,
					payloadInfo.Size);

			Logger.Info("Starting download of " + mFileName);
			int prevAverageTotalPercent = 0;
			StartWorkers(mWorkers,
					delegate ()
					{
						int totalPercent = 0;
						int averageTotalPercent = 0;
						Int64 totalFileDownloaded = 0;
						foreach (KeyValuePair<Thread, Worker> o in mWorkers)
						{
							totalPercent += o.Value.PercentComplete;
							if (File.Exists(o.Value.PartFileName))
							{
								FileInfo fileInfo = new FileInfo(o.Value.PartFileName);
								totalFileDownloaded += fileInfo.Length;
							}
						}
						if (sizeDownloadedCb != null)
						{
							sizeDownloadedCb(totalFileDownloaded);
						}
						averageTotalPercent = totalPercent / mWorkers.Count;
						if (averageTotalPercent != prevAverageTotalPercent)
							updateProgressCb(averageTotalPercent);
						prevAverageTotalPercent = averageTotalPercent;
					});

			WaitForWorkers(mWorkers);

			MakePayload(mNrWorkers, mFileName);

			if (!IsPayloadOk(mFileName, payloadInfo.Size))
			{
				String errMsg = "Downloaded file not of the correct size";
				Logger.Info(errMsg);
				File.Delete(mFileName);
				throw new Exception(errMsg);
			}
			else
			{
				Logger.Info("File downloaded correctly");
				DeletePayloadParts(mNrWorkers, mFileName);
			}

			download_completed:
			downloadedCb(filePath);
		}
		catch (Exception e)
		{
			Logger.Error("Exception in Download. err: " + e.ToString());
			exceptionCb(e);
		}
	}

	private delegate void ProgressCallback();

	public static String MakePartFileName(String fileName, int id)
	{
		return String.Format(@"{0}_part_{1}", fileName, id);
	}

	class Range
	{
		Int64 m_From;
		Int64 m_To;

		public Range(Int64 from, Int64 to)
		{
			m_From = from;
			m_To = to;
		}

		public Int64 From { get { return m_From; } set { m_From = value; } }
		public Int64 To { get { return m_To; } }
		public Int64 Length { get { return m_To - m_From + 1; } }
	}


	class WorkerException : Exception
	{
		public WorkerException(String msg, Exception e) : base(msg, e)
		{
		}
	}

	class Worker
	{
		int m_Id;
		String m_URL;
		String m_PayloadName;
		Range m_Range;
		int m_PercentComplete;
		ProgressCallback m_ProgressCallback;
		Exception m_Exception;

		public Worker(int id, String url, String payloadName, Range range)
		{
			m_Id = id;
			m_URL = url;
			m_PayloadName = payloadName;
			m_Range = range;
		}

		public int Id
		{
			get
			{
				return m_Id;
			}
		}

		public String PartFileName
		{
			get
			{
				return MakePartFileName(m_PayloadName, m_Id);
			}
		}

		public Range Range
		{
			get
			{
				return m_Range;
			}
		}

		public String URL
		{
			get
			{
				return m_URL;
			}
		}

		public int PercentComplete
		{
			get
			{
				return m_PercentComplete;
			}
			set
			{
				m_PercentComplete = value;
				ProgressCallback();
			}
		}

		public ProgressCallback ProgressCallback
		{
			get
			{
				return m_ProgressCallback;
			}
			set
			{
				m_ProgressCallback = value;
			}
		}

		public Exception Exception
		{
			get
			{
				return m_Exception;
			}
			set
			{
				m_Exception = value;
			}
		}
	}

	class PayloadInfo
	{
		bool m_SupportsRangeRequest;
		long m_Size;

		public PayloadInfo(bool supportsRangeRequest, Int64 size)
		{
			m_SupportsRangeRequest = supportsRangeRequest;
			m_Size = size;
		}

		public Int64 Size { get { return m_Size; } set { m_Size = value; } }
		public bool SupportsRangeRequest { get { return m_SupportsRangeRequest; } }
	}

	private Int64 GetSizeFromContentRange(HttpWebResponse res)
	{
		String data = res.Headers["Content-Range"];
		char[] delims = { '/' };
		String[] parts = data.Split(delims);
		return Convert.ToInt64(parts[parts.Length - 1]);
	}

	private PayloadInfo GetRemotePayloadInfo(String url)
	{
		HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
		req.Method = "Head";

		HttpWebResponse res = null;
		String headers = null;
		PayloadInfo payloadInfo = null;

		try
		{
			Add64BitRange(req, 0, 0);
			res = (HttpWebResponse)req.GetResponse();
			headers = GetHTTPResponseHeaders(res);
			mResponseHeaders = res.Headers;
			Logger.Warning(headers);
			if ((int)res.StatusCode == 206) // Supports Range Request
			{
				payloadInfo = new PayloadInfo(true, res.ContentLength);
			}
			else if ((int)res.StatusCode == 200)
			{
				if (headers.Contains("Accept-Ranges: bytes"))
				{
					payloadInfo = new PayloadInfo(true, res.ContentLength);

				}
				else
				{
					payloadInfo = new PayloadInfo(false, res.ContentLength);
				}
			}
		}
		catch (Exception e)
		{
			Logger.Error(e.ToString());
			throw;
		}

		res.Close();
		return payloadInfo;
	}

	List<KeyValuePair<Thread, Worker>> MakeWorkers(int nrWorkers, String url, String payloadFileName, Int64 payloadSize)
	{
		Int64 chunkSize = payloadSize / nrWorkers;
		List<KeyValuePair<Thread, Worker>> workers = new List<KeyValuePair<Thread, Worker>>();

		for (int i = 0; i < nrWorkers; ++i)
		{
			Int64 from = i * chunkSize;
			Int64 to = -1;

			if (i == (nrWorkers - 1))
				to = (i + 1) * chunkSize + payloadSize % nrWorkers - 1;
			else
				to = (i + 1) * chunkSize - 1;

			Thread workerThread = new Thread(DoWork);
			workerThread.IsBackground = true;
			Worker worker = new Worker(i, url, payloadFileName, new Range(from, to));

			KeyValuePair<Thread, Worker> o = new KeyValuePair<Thread, Worker>(workerThread,
					worker);
			workers.Add(o);
		}

		return workers;
	}

	void StartWorkers(List<KeyValuePair<Thread, Worker>> workers, ProgressCallback progressCallback)
	{
		foreach (KeyValuePair<Thread, Worker> o in workers)
		{
			o.Value.ProgressCallback = progressCallback;
			o.Key.Start(o.Value);
		}
	}

	void MakePayload(int nrWorkers, String payloadName)
	{
		Stream payloadFile = new FileStream(payloadName, FileMode.Create, FileAccess.Write, FileShare.None);

		int blockSize = 16 * 1024;
		byte[] buff = new byte[blockSize];
		int sz = 0;

		for (int i = 0; i < nrWorkers; ++i)
		{
			String partFileName = MakePartFileName(payloadName, i);
			Stream partFile = new FileStream(partFileName, FileMode.Open, FileAccess.Read);
			while ((sz = partFile.Read(buff, 0, blockSize)) > 0)
			{
				payloadFile.Write(buff, 0, sz);
			}
			partFile.Close();
		}

		payloadFile.Flush();
		payloadFile.Close();
	}

	void DeletePayloadParts(int nrParts, String payloadName)
	{
		for (int i = 0; i < nrParts; ++i)
		{
			String partFileName = MakePartFileName(payloadName, i);
			File.Delete(partFileName);
		}
	}

	String GetHTTPResponseHeaders(HttpWebResponse res)
	{
		String headers = "HTTP Response Headers\n";
		headers += String.Format("StatusCode: {0}\n", (int)res.StatusCode);
		headers += res.Headers;
		return headers;
	}

	public void DoWork(Object data)
	{
		Worker worker = (Worker)data;
		Range range = worker.Range;

		Stream partFile = null;
		HttpWebRequest req = null;
		HttpWebResponse res = null;
		Stream resStream = null;

		try
		{
			Logger.Info("WorkerId {0} range.From = {1}, range.To = {2}", worker.Id, range.From, range.To);
			req = (HttpWebRequest)WebRequest.Create(worker.URL);
			req.KeepAlive = false;
			if (File.Exists(worker.PartFileName))
			{
				partFile = new FileStream(worker.PartFileName,
						FileMode.Append,
						FileAccess.Write,
						FileShare.None);

				if (partFile.Length == range.Length)
				{
					worker.PercentComplete = 100;
					Logger.Info("WorkerId {0} already downloaded", worker.Id);
					return;
				}

				worker.PercentComplete = (int)((partFile.Length * 100) / range.Length);
				Logger.Info("WorkerId {0} Resuming from range.From = {1}, range.To = {2}",
						worker.Id, range.From + partFile.Length, range.To);
				Add64BitRange(req, range.From + partFile.Length, range.To);
			}
			else
			{
				worker.PercentComplete = 0;
				partFile = new FileStream(worker.PartFileName, FileMode.Create, FileAccess.Write, FileShare.None);
				Add64BitRange(req, range.From, range.To);
			}

			req.ReadWriteTimeout = 60 * 1000;

			res = (HttpWebResponse)req.GetResponse();
			long contentLength = res.ContentLength;
			resStream = res.GetResponseStream();

			int blockSize = 64 * 1024;
			byte[] buff = new byte[blockSize];
			int sz = 0;
			long totalContentRead = 0;

			String headers = String.Format("WorkerId {0}\n", worker.Id);
			headers += GetHTTPResponseHeaders(res);

			while ((sz = resStream.Read(buff, 0, blockSize)) > 0)
			{
				partFile.Write(buff, 0, sz);
				totalContentRead += sz;
				worker.PercentComplete = (int)((partFile.Length * 100) / range.Length);
			}

			if (contentLength != totalContentRead)
			{
				String errMsg = String.Format("totalContentRead({0}) != contentLength({1})",
					totalContentRead, contentLength);
				throw new Exception(errMsg);
			}

		}
		catch (Exception e)
		{
			worker.Exception = e;
			Logger.Error(e.ToString());
		}
		finally
		{
			if (resStream != null)
				resStream.Close();

			if (res != null)
				res.Close();

			if (partFile != null)
			{
				partFile.Flush();
				partFile.Close();
			}
		}

		Logger.Info("WorkerId {0} Finished", worker.Id);
		return;
	}

	bool IsPayloadOk(String payloadFileName, Int64 remoteSize)
	{
		Int64 payloadSize = new FileInfo(payloadFileName).Length;
		Logger.Info("payloadSize = " + payloadSize +
				" remoteSize = " + remoteSize);
		return (payloadSize == remoteSize);
	}

	public void AbortDownload()
	{
		if (mWorkers == null)
			return;

		Logger.Info("Downloader: Aborting all threads...");
		foreach (KeyValuePair<Thread, Worker> o in mWorkers)
		{
			try
			{
				o.Key.Abort();
			}
			catch (Exception e)
			{
				Logger.Error("Downloader: could not abort thread. Error: " + e.Message);
			}
		}
	}

	void WaitForWorkers(List<KeyValuePair<Thread, Worker>> workers)
	{
		foreach (KeyValuePair<Thread, Worker> o in workers)
			o.Key.Join();

		foreach (KeyValuePair<Thread, Worker> o in workers)
		{
			if (o.Value.Exception != null)
				throw new WorkerException(o.Value.Exception.Message, o.Value.Exception);
		}
	}

	void Add64BitRange(HttpWebRequest req, long start, long end)
	{
		MethodInfo method = typeof(WebHeaderCollection).GetMethod("AddWithoutValidate",
				BindingFlags.Instance | BindingFlags.NonPublic);

		String key = "Range";
		String val = String.Format("bytes={0}-{1}", start, end);

		method.Invoke(req.Headers, new object[] { key, val });
	}

}
