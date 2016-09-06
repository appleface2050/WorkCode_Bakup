/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Common Library
 */

using System;
using System.IO;
using System.Security;
using System.Security.Principal;
using Microsoft.Win32;

namespace BlueStacks.hyperDroid.Common
{
	public class User
	{

		public const String FIRST_TIME_LAUNCH_URL = @"http://updates.bluestacks.com/check";
		public static string GUID
		{
			get
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(REG_PATH);
				if (key == null)
					key = Registry.CurrentUser.OpenSubKey(REG_PATH);
				if (key == null)
					return "";
				s_GUID = (String)key.GetValue("USER_GUID", "");
				return s_GUID;
			}
			set
			{
				RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH);
				key.SetValue("USER_GUID", value, RegistryValueKind.String);
				s_GUID = value;
			}
		}

		public static string RegVersion
		{
			get
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(REG_PATH);
				if (key == null)
					return null;
				string version = (String)key.GetValue("Version", null);
				return version;
			}
		}

		public static String Email
		{
			get
			{
				RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.CloudRegKeyPath);
				String email = (String)key.GetValue("Email", "null");
				key.Close();
				return email;
			}
			set
			{
				using (RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.CloudRegKeyPath))
				{
					key.SetValue("Email", value);
					key.Flush();
				}
			}
		}

		public static bool IsFirstTimeLaunch()
		{
			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			String firstTimeLaunch = (String)key.GetValue("FirstTimeLaunch", "");

			if (firstTimeLaunch == "")
				key.SetValue("FirstTimeLaunch", DateTime.Now.ToString());

			return firstTimeLaunch == "";
		}

		public static bool IsAdministrator()
		{
			bool admin = false;
			try
			{
				WindowsIdentity user = WindowsIdentity.GetCurrent();

				if (user == null)
					return false;

				WindowsPrincipal principal = new WindowsPrincipal(user);
				admin = principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (Exception)
			{
			}

			return admin;
		}

		private static string s_GUID;
		private static String REG_PATH = Common.Strings.RegBasePath;
	}
}
