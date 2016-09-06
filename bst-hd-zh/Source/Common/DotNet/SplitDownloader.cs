using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

using BlueStacks.hyperDroid.Common;

class SplitDownloader
{
	public delegate void ProgressCb(float percent);
	public delegate void CompletedCb(string filePath);
	public delegate void ExceptionCb(Exception e);
	public delegate void FileSizeCb(long fileSize);

	private String m_ManifestURL;
	private String m_DirPath;
	private String m_UserGUID;
	private String m_UserAgent;
	private ProgressCb m_ProgressCb;
	private CompletedCb m_CompletedCb;
	private ExceptionCb m_ExceptionCb;
	private FileSizeCb m_FileSizeCb;
	private int m_NrWorkers;
	private SerialWorkQueue[] m_Workers;
	private bool m_WorkersStarted;
	private Manifest m_Manifest;
	private float m_PercentDownloaded;

	public SplitDownloader(String manifestURL,
			String dirPath,
			String userGUID,
			int nrWorkers)
	{
		m_ManifestURL = manifestURL;
		m_DirPath = dirPath;
		m_UserGUID = userGUID;
		m_UserAgent = String.Format("SplitDownloader {0}/{1}/{2}",
				BlueStacks.hyperDroid.Version.PRODUCT,
				BlueStacks.hyperDroid.Version.STRING,
				m_UserGUID);
		m_NrWorkers = nrWorkers;
		m_Workers = new SerialWorkQueue[nrWorkers];
		for (int i = 0; i < m_NrWorkers; ++i)
			m_Workers[i] = new SerialWorkQueue();

		m_WorkersStarted = false;
	}

	public void Download(ProgressCb progressCb,
			CompletedCb completedCb,
			ExceptionCb exceptionCb)
	{
		Download(progressCb, completedCb, exceptionCb, null);
	}

	public void Download(ProgressCb progressCb,
			CompletedCb completedCb,
			ExceptionCb exceptionCb,
			FileSizeCb fileSizeCb
			)
	{
		m_ProgressCb = progressCb;
		m_CompletedCb = completedCb;
		m_ExceptionCb = exceptionCb;
		m_FileSizeCb = fileSizeCb;

		try
		{
			m_Manifest = this.GetManifest();
			String manifestPath = GetManifestFilePath();
			FilePart filePart = null;

			if (m_FileSizeCb != null)
			{
				m_FileSizeCb(m_Manifest.FileSize);
			}

			this.StartWorkers();

			m_ProgressCb(m_Manifest.PercentDownloaded());

			for (int i = 0; i < m_Manifest.Count; ++i)
			{
				filePart = m_Manifest[i];
				SerialWorkQueue.Work work = this.MakeWork(filePart);
				m_Workers[i % m_NrWorkers].Enqueue(work);
			}

			this.StopAndWaitWorkers();

			if (m_Manifest.Check() == false)
				throw new Manifest.CheckFailed();

			String filePath = m_Manifest.MakeFile();
			m_Manifest.DeleteFileParts();
			m_Manifest.DeleteManifest();
			m_CompletedCb(filePath);
		}
		catch (Exception e)
		{
			Logger.Error(e.ToString());
			m_ExceptionCb(e);
		}
		finally
		{
			if (m_WorkersStarted == true)
				this.StopAndWaitWorkers();
		}
	}

	private void StartWorkers()
	{
		for (int i = 0; i < m_NrWorkers; ++i)
			m_Workers[i].Start();

		m_WorkersStarted = true;
	}

	private void StopAndWaitWorkers()
	{
		for (int i = 0; i < m_NrWorkers; ++i)
			m_Workers[i].Stop();

		for (int i = 0; i < m_NrWorkers; ++i)
			m_Workers[i].Join();

		m_WorkersStarted = false;
	}

	private SerialWorkQueue.Work MakeWork(FilePart filePart)
	{
		SerialWorkQueue.Work work = delegate ()
		{
			try
			{
				if (filePart.Check() == true)
				{
					Logger.Info(filePart.Path + " is already downloaded");
					return;
				}

				DownloadFilePart(filePart);
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}
		};

		return work;
	}


	private String GetManifestFilePath()
	{
		String fileName = Path.GetFileName(new Uri(m_ManifestURL).AbsolutePath);
		String filePath = Path.Combine(m_DirPath, fileName);
		return filePath;
	}

	private Manifest GetManifest()
	{
		String manifestFilePath = this.GetManifestFilePath();

		Logger.Info("Downloading " + m_ManifestURL + " to " + manifestFilePath);

		bool downloaded = false;
		Exception capturedException = null;
		DownloadFile(m_ManifestURL,
				manifestFilePath,
				m_UserAgent,
				delegate (long downloadedSize, long totalSize)
				{
					Logger.Info("Downloaded (" + downloadedSize + " bytes) out of " + totalSize);
				},
				delegate (String filePath)
				{
					downloaded = true;
					Logger.Info("Downloaded " + m_ManifestURL + " to " + filePath);
				},
				delegate (Exception e)
				{
					downloaded = false;
					capturedException = e;
					Logger.Error(e.ToString());
				});

		if (downloaded == false)
			throw capturedException;

		Manifest mf = new Manifest(manifestFilePath);
		mf.Build();
		return mf;
	}

	private void DownloadFilePart(FilePart filePart)
	{
		String filePartURL = filePart.URL(m_ManifestURL);

		Logger.Info("Downloading " + filePartURL + " to " + filePart.Path);

		bool downloaded = false;
		Exception capturedException = null;
		DownloadFile(filePartURL,
				filePart.Path,
				m_UserAgent,
				delegate (long downloadedSize, long totalSize) // Progress
				{
					filePart.DownloadedSize = downloadedSize;
					if (this.m_PercentDownloaded != m_Manifest.PercentDownloaded())
						m_ProgressCb(m_Manifest.PercentDownloaded());
					m_PercentDownloaded = m_Manifest.PercentDownloaded();
				},
				delegate (String filePath) // Completed
				{
					downloaded = true;
					Logger.Info("Downloaded " + filePartURL + " to " + filePart.Path);
				},
				delegate (Exception e) // Exception
				{
					downloaded = false;
					capturedException = e;
					Logger.Error(e.ToString());
				});

		if (downloaded == false)
			throw capturedException;
	}

	public delegate void DownloadFileProgressCb(long downloaded, long size);
	public delegate void DownloadFileCompletedCb(String filePath);
	public delegate void DownloadFileExceptionCb(Exception e);

	private static void DownloadFile(String url,
			String filePath,
			String userAgent,
			DownloadFileProgressCb progressCb,
			DownloadFileCompletedCb completedCb,
			DownloadFileExceptionCb exceptionCb)
	{
		FileStream file = null;
		HttpWebRequest req = null;
		HttpWebResponse res = null;
		Stream resStream = null;
		bool completed = false;

		try
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			req = (HttpWebRequest)WebRequest.Create(url);
			req.UserAgent = userAgent;
			req.KeepAlive = false;

			req.ReadWriteTimeout = 60 * 1000;

			res = (HttpWebResponse)req.GetResponse();
			long contentLength = res.ContentLength;
			resStream = res.GetResponseStream();

			Logger.Warning(String.Format("HTTP Response Header\n" +
						"StatusCode: {0}\n{1}", (int)res.StatusCode, res.Headers));

			int blockSize = 4 * 1024;
			byte[] buff = new byte[blockSize];
			int sz = 0;
			long totalContentRead = 0;

			file = new FileStream(filePath,
					FileMode.Create, FileAccess.Write, FileShare.None);

			while ((sz = resStream.Read(buff, 0, blockSize)) > 0)
			{
				file.Write(buff, 0, sz);
				totalContentRead += sz;
				progressCb(totalContentRead, contentLength);
			}

			if (contentLength != totalContentRead)
			{
				String errMsg = String.Format("totalContentRead({0}) != contentLength({1})",
						totalContentRead, contentLength);
				throw new Exception(errMsg);
			}

			completed = true;
		}
		catch (Exception e)
		{
			Logger.Error(e.ToString());
			exceptionCb(e);
		}
		finally
		{
			if (resStream != null)
				resStream.Close();

			if (res != null)
				res.Close();

			if (file != null)
			{
				file.Flush();
				file.Close();
				Thread.Sleep(1000);
			}
		}

		if (completed == true)
		{
			completedCb(filePath);
		}
	}

	public class FilePart
	{
		private String m_Name;
		private long m_Size;
		private String m_SHA1;
		private String m_Path;
		private long m_DownloadedSize;

		public FilePart(String name, long size, String sha1, String path)
		{
			m_Name = name;
			m_Size = size;
			m_SHA1 = sha1;
			m_Path = path;
			m_DownloadedSize = 0;
		}

		public String Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		public long Size
		{
			get { return m_Size; }
			set { m_Size = value; }
		}

		public String SHA1
		{
			get { return m_SHA1; }
			set { m_SHA1 = value; }
		}

		public String URL(String manifestURL)
		{
			String dirPath = manifestURL.Substring(0, manifestURL.LastIndexOf('/') + 1);
			return dirPath + this.Name;
		}

		public String Path
		{
			get { return m_Path; }
		}

		public long DownloadedSize
		{
			get { return m_DownloadedSize; }
			set { m_DownloadedSize = value; }
		}

		public bool Check()
		{
			Logger.Info("Will check " + this.Path);
			bool result = false;

			if (!File.Exists(this.Path))
			{
				Logger.Error("File missing");
				return false;
			}

			using (Stream fs = File.OpenRead(this.Path))
			{
				if (fs.Length != this.Size)
				{
					Logger.Error("File size incorrect: " + fs.Length);
					return false;
				}

				String sha1 = SplitFile.CheckSum(fs);
				if (sha1 == this.SHA1)
				{
					this.DownloadedSize = this.Size;
					Logger.Info("File size correct");
					result = true;
				}
			}

			return result;
		}
	}

	public class Manifest
	{
		private List<FilePart> m_FileParts;
		private String m_FilePath;
		private long m_FileSize;

		public Manifest(String filePath)
		{
			m_FileParts = new List<FilePart>();
			m_FilePath = filePath;
		}

		public class CheckFailed : Exception
		{
			public CheckFailed()
				: base()
			{
			}
		}

		public bool Check()
		{
			int i = 0;
			foreach (FilePart filePart in m_FileParts)
			{
				if (filePart.Check() == false)
				{
					Logger.Error("Check failed for part " + i);
					return false;
				}
				i++;
			}

			return true;
		}

		public void Build()
		{
			using (StreamReader reader = new StreamReader(File.OpenRead(m_FilePath)))
			{
				String name;
				long size;
				String sha1;
				String line;
				String[] field;
				String path;
				while ((line = reader.ReadLine()) != null)
				{
					field = line.Split(' ');
					name = field[0];
					size = Convert.ToInt64(field[1]);
					sha1 = field[2];
					path = Path.Combine(Path.GetDirectoryName(m_FilePath), name);
					FilePart filePart = new FilePart(name, size, sha1, path);

					if (filePart.Check() == true)
						filePart.DownloadedSize = filePart.Size;

					m_FileParts.Add(filePart);
					m_FileSize += size;
				}
			}
		}

		public void Dump()
		{
			foreach (FilePart filePart in m_FileParts)
			{
				Logger.Info("{0} {1} {2}", filePart.Name, filePart.Size, filePart.SHA1);
			}
		}

		public long Count
		{
			get { return m_FileParts.Count; }
		}


		public FilePart this[int i]
		{
			get { return m_FileParts[i]; }
		}

		[DllImport("kernel32", SetLastError = true)]
		private static extern bool FlushFileBuffers(IntPtr handle);

		public String MakeFile()
		{
			int blockSize = 16 * 1024;
			byte[] buff = new byte[blockSize];
			int size = 0;
			String fileName = Path.GetFileNameWithoutExtension(m_FilePath);
			String fileDir = Path.GetDirectoryName(m_FilePath);
			String filePath = Path.Combine(fileDir, fileName);

			using (FileStream file = new FileStream(filePath,
						FileMode.Create, FileAccess.Write, FileShare.None))
			{
				foreach (FilePart filePart in m_FileParts)
				{
					using (Stream partFile = new FileStream(filePart.Path,
								FileMode.Open, FileAccess.Read))
					{
						while ((size = partFile.Read(buff, 0, blockSize)) > 0)
							file.Write(buff, 0, size);
					}
				}

				// Ensure disk flush
				file.Flush();
#pragma warning disable 618, 612
				if (!FlushFileBuffers(file.Handle))
#pragma warning restore 618, 612
				{
					throw new SystemException("Win32 FlushFileBuffers failed for " + filePath,
							new Win32Exception(Marshal.GetLastWin32Error()));
				}
			}

			return filePath;
		}

		public void DeleteFileParts()
		{
			foreach (FilePart filePart in m_FileParts)
				File.Delete(filePart.Path);
		}

		public void DeleteManifest()
		{
			File.Delete(m_FilePath);
		}

		public long DownloadedSize
		{
			get
			{
				long size = 0;
				foreach (FilePart filePart in m_FileParts)
					size += filePart.DownloadedSize;

				return size;
			}
		}

		public long FileSize
		{
			get { return m_FileSize; }
		}

		public float PercentDownloaded()
		{
			double percent = (double)this.DownloadedSize * 100 / this.FileSize;
			percent = Math.Round(percent, 1);
			return (float)percent;
		}
	}
}
