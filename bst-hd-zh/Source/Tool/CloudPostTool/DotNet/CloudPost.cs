using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using Microsoft.Win32;
using System.Threading;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Common
{
	public class CloudPost
	{
		public static string PostData(string url, string arg, bool isArray)
		{
			try
			{
				Dictionary<string, string> data = new Dictionary<string, string>();
				if (isArray)
					data.Add("data", arg);
				else
				{
					JSonReader jsonReader = new JSonReader();
					IJSonObject obj = jsonReader.ReadAsJSonObject(arg);

					foreach (string key in obj.Names)
					{
						if (!obj[key].IsNull)
							data.Add(key, obj[key].StringValue);
					}
				}

				string res = Common.HTTP.Client.Post(url, data, null, false);
				Logger.Info("response: " + res);

				return res;
			}

			catch (Exception e)
			{
				Logger.Error("Exception when trying to post");
				Logger.Error(e.ToString());
				return "{\"success\":false}";
			}
		}

		private static string[] ParsePostDataJson(string jsonData)
		{
				JSonReader reader = new JSonReader();
				IJSonObject obj = reader.ReadAsJSonObject(jsonData);

				string[] valuePair = new string[3];

				valuePair[0] = obj["url"].StringValue;
				valuePair[1] = obj["data"].StringValue;
				if (obj.Contains("isArray"))
					valuePair[2] = obj["isArray"].StringValue.ToLower();
				else	
					valuePair[2] = "false";

				return valuePair;
		}

		private static void PostFromSubKey(string kLoc)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(kLoc, true);
			if (key == null)
			{
				Logger.Error("registry not found");
				return;
			}

			foreach (string keyValueName in key.GetValueNames())
			{
				string val = Convert.ToString(key.GetValue(keyValueName));

				string[] valuePair = ParsePostDataJson(val);

				try
				{
					string response = PostData(valuePair[0], valuePair[1], valuePair[2].Equals("true"));
					JSonReader reader = new JSonReader();
					IJSonObject obj = reader.ReadAsJSonObject(response);
					if (obj.Contains("success") && obj["success"].BooleanValue)
						key.DeleteValue(keyValueName);
				}
				catch (Exception e)
				{
					Logger.Error("Error in sending request for {0}", keyValueName);
					Logger.Error(e.ToString());
				}
			}
		}

		private static void PostFromRegistry(string kLoc, string keyValueName)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(kLoc, true);
			if (key == null)
			{
				Logger.Error("registry not found");
				return;
			}

			string val = Convert.ToString(key.GetValue(keyValueName));
			string[] valuePair = ParsePostDataJson(val);

			try
			{
				string response = PostData(valuePair[0], valuePair[1], valuePair[2].Equals("true"));
				JSonReader reader = new JSonReader();
				IJSonObject obj = reader.ReadAsJSonObject(response);
				if (obj.Contains("success") && obj["success"].BooleanValue)
					key.DeleteValue(keyValueName);
			}
			catch (Exception e)
			{
				Logger.Error("Error in sending request for {0}", keyValueName);
				Logger.Error(e.ToString());
			}
		}
		
		private static bool ValidateRemoteCertificate(object sender, X509Certificate cert,
				X509Chain chain, SslPolicyErrors policyErrors)
		{
			return true;
		}

		static void Main(string[] args)
		{
			Logger.InitUserLog();
			Logger.Info("Post to Cloud called");

			ServicePointManager.ServerCertificateValidationCallback += 
				new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

			if (args.Length == 0 || args.Length > 2)
			{
				Logger.Info("Invalid number of args");
				Logger.Info("Please give 3 arguments");
				Logger.Info("Usage::");
				Logger.Info("\n\tArg 1: RegistryKey where value to be stored in case post call fails relative to HKLM/Software");
				Logger.Info("\tArg 2: URL where post call has to be made");
				Logger.Info("\tArg 3: data to post call with");
				Logger.Info("\n\n\t\t eg. HD-CloudPost.exe \"BlueStacks\\Agent\" \"http://website.com\" \"someData\"");
				Environment.Exit(0);
			}

			string regKey = args[0];
			if (args.Length == 1)
			{
				PostFromSubKey(regKey);
			}
			else
			{
				PostFromRegistry(regKey, args[1]);
			}
		}
	}
}
