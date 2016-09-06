/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.Win32;
using System.Windows.Forms;

using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Common
{
	public class HTTPUtils
	{
		public static RequestData ParseRequest(HttpListenerRequest req)
		{
			//			Logger.Info("In ParseRequest Wrapper");
			return ParseRequest(req, true);
		}

		public static RequestData ParseRequest(HttpListenerRequest req, bool printData)
		{
			//			Logger.Info("In ParseRequest");

			RequestData requestData = new RequestData();

			bool m_Multipart = false;
			string m_Boundary = null;

			requestData.headers = req.Headers;
			foreach (string key in requestData.headers.AllKeys)
			{
				if (requestData.headers[key].Contains("multipart"))
				{
					m_Boundary = "--" + requestData.headers[key].Substring(requestData.headers[key].LastIndexOf("=") + 1);
					Logger.Debug("boundary: {0}", m_Boundary);
					m_Multipart = true;
				}
			}


			requestData.queryString = req.QueryString;
			if (!req.HasEntityBody)
			{
				Logger.Info("no body data");
				return requestData;
			}

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

			Logger.Debug("byte array size {0}", byteData.Length);
			string stringData = Encoding.UTF8.GetString(byteData);

			if (!m_Multipart)
			{
				/* string posted */
				requestData.data = HttpUtility.ParseQueryString(stringData);
				return requestData;
			}

			byte[] boundaryBytes = Encoding.UTF8.GetBytes(m_Boundary);
			List<int> positions = IndexOf(byteData, boundaryBytes);

			string name;
			string filename;
			string contentType;

			int partStart, partEnd, partLength;
			string part;

			for (int i = 0; i < positions.Count - 1; i++)
			{
				Logger.Info("Creating part");
				partStart = positions[i];
				partEnd = positions[i + 1];
				partLength = partEnd - partStart;
				byte[] bytePart = new byte[partLength];
				Logger.Debug("Start: {0}, End: {1}, Length: {2}", partStart, partEnd, partLength);
				Logger.Debug("byteData length: {0}", byteData.Length);
				Buffer.BlockCopy(byteData, partStart, bytePart, 0, partLength);
				Logger.Debug("bytePart length: {0}", bytePart.Length);
				part = Encoding.UTF8.GetString(bytePart);

				// Look for Content-Type
				Regex re = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n)");
				Match contentTypeMatch = re.Match(part);

				// Look for filename
				re = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
				Match filenameMatch = re.Match(part);

				// Look for name
				re = new Regex(@"(?<=name\=\"")(.*?)(?=\"")");
				Match nameMatch = re.Match(part);
				name = nameMatch.Value.Trim();
				Logger.Info("Got name: {0}", name);

				// Did we find a file? 
				if (contentTypeMatch.Success && filenameMatch.Success)
				{
					Logger.Debug("Found file");
					// Set properties
					contentType = contentTypeMatch.Value.Trim();
					Logger.Debug("Got contenttype: {0}", contentType);
					filename = filenameMatch.Value.Trim();
					Logger.Info("Got filename: {0}", filename);

					// Get the start & end indexes of the file contents
					int startIndex = part.IndexOf("\r\n\r\n") + "\r\n\r\n".Length;
					//int startIndex = contentTypeMatch.Index + contentTypeMatch.Length
					//	+ "\r\n\r\n".Length;

					byte[] delimiterBytes = Encoding.UTF8.GetBytes("\r\n" + m_Boundary);
					//int endIndex = IndexOf(byteData, delimiterBytes, startIndex);

					int contentLength = partLength - startIndex;

					// Extract the file contents from the byte array
					byte[] fileData = new byte[contentLength];

					Logger.Debug("startindex: {0}, contentlength: {1}", startIndex, contentLength);
					Buffer.BlockCopy(bytePart, startIndex, fileData, 0, contentLength);

					string destinationDir = Strings.BstUserDataDir;
					if (filename.StartsWith(Strings.TombStoneFilePrefix) == true)
						destinationDir = Strings.BstLogsDir;
					string filepath = Path.Combine(destinationDir, filename);
					Stream file = File.OpenWrite(filepath);
					file.Write(fileData, 0, contentLength);
					file.Close();
					requestData.files.Add(name, filepath);
				}
				else
				{
					Logger.Info("No file in this part");
					//int startIndex = nameMatch.Index + nameMatch.Length + "\r\n".Length;
					int startIndex = part.LastIndexOf("\r\n\r\n");
					string val = part.Substring(startIndex, part.Length - startIndex);
					val = val.Trim();
					if (printData)
						Logger.Info("Got value: {0}", val);
					else
						Logger.Info("Value hidden");
					requestData.data.Add(name, val);
				}
			}
			return requestData;
		}

		private static List<int> IndexOf(byte[] searchWithin, byte[] searchFor)
		{
			List<int> positions = new List<int>();
			int index;
			int startIndex = 0;
			int startPos = Array.IndexOf(searchWithin, searchFor[0], startIndex);

			Logger.Debug("boundary size = {0}", searchFor.Length);
			do
			{
				index = 0;
				while ((startPos + index) < searchWithin.Length)
				{
					if (searchWithin[startPos + index] == searchFor[index])
					{
						index++;
						if (index == searchFor.Length)
						{
							positions.Add(startPos);
							Logger.Debug("Got boundary postion: {0}", startPos);
							break;
						}
					}
					else
					{
						break;
					}
				}
				if (startPos + index > searchWithin.Length)
					break;
				startPos = Array.IndexOf<byte>(searchWithin, searchFor[0], startPos + index);
			} while (startPos != -1);

			return positions;
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
