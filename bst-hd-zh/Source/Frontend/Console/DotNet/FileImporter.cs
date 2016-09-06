/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Console Frontend
 
	*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

using CodeTitans.JSon;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Frontend
{

	public class FileImporter
	{

		public static DragEventHandler MakeDragDropHandler()
		{
			return delegate (Object obj, DragEventArgs evt)
			{

				Thread thread = new Thread(delegate ()
				{
					HandleDragDropAsync(evt);
				});

				thread.IsBackground = true;
				thread.Start();
			};
		}

		private static void HandleDragDropAsync(DragEventArgs evt)
		{
			if (Utils.IsSharedFolderEnabled() == false)
				return;

			try
			{
				// When file(s) are dragged from Explorer to the form, IDataObject
				// contains array of file names.
				Array fileList = (Array)evt.Data.GetData(DataFormats.FileDrop);

				for (int iter = 0; iter < fileList.Length; iter++)
				{
					// Extract string from first array element
					// (ignore all files except first if number of files are dropped).
					string filePath = fileList.GetValue(iter).ToString();
					string fileName = Path.GetFileName(filePath);

					if (String.Equals(Path.GetExtension(filePath), ".apk", StringComparison.InvariantCultureIgnoreCase))
					{
						Thread t = new Thread(delegate ()
								{
									Utils.CallApkInstaller(filePath, false);
								});
						t.IsBackground = true;
						t.Start();
					}
					else
					{
						string sharedFolder = Common.Strings.SharedFolderDir;
						string sharedFolderName = Common.Strings.SharedFolderName;
						string targetFilePath = Path.Combine(sharedFolder, fileName);

						string mimeType = Utils.GetMimeFromFile(filePath);
						Logger.Info("DragDrop File: {0}, mime: {1}", filePath, mimeType);

						FileSystem.CopyFile(filePath, targetFilePath, UIOption.AllDialogs);

						string androidPath = "/mnt/sdcard/windows/" + sharedFolderName + "/" + fileName;
						Logger.Info("dragDrop androidPath: " + androidPath);

						string url = String.Format("http://127.0.0.1:{0}/{1}",
								Common.VmCmdHandler.s_ServerPort, Common.Strings.FileDropUrl);

						JSonWriter json = new JSonWriter();
						json.WriteArrayBegin();
						json.WriteObjectBegin();
						json.WriteMember("filepath", androidPath);
						json.WriteMember("mime", mimeType);
						json.WriteObjectEnd();
						json.WriteArrayEnd();

						Dictionary<string, string> data = new Dictionary<string, string>();
						data.Add("data", json.ToString());

						Logger.Info("Sending drag drop request: " + json.ToString());

						try
						{
							Common.HTTP.Client.Post(url, data, null, false);
						}
						catch (Exception ex)
						{
							Logger.Error("Failed to send FileDrop request. err: " + ex.ToString());
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Error in DragDrop function: " + ex.Message);
			}
		}

		public static void HandleDragEnter(Object obj, DragEventArgs evt)
		{
			if (evt.Data.GetDataPresent(DataFormats.FileDrop))
				evt.Effect = DragDropEffects.Copy;
			else
			{
				Logger.Debug("FileDrop DataFormat not supported");
				String[] allFormats = evt.Data.GetFormats();
				Logger.Debug("Supported formats:");
				foreach (String format in allFormats)
					Logger.Debug(format);
				evt.Effect = DragDropEffects.None;
			}
		}
	}

}
