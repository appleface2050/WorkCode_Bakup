/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Common Interop UUID 
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Common.Interop
{
	public class UUID
	{
		private const long RPC_S_OK = 0L;
		private const long RPC_S_UUID_LOCAL_ONLY = 1824L;
		private const long RPC_S_UUID_NO_ADDRESS = 1739L;

		[DllImport("rpcrt4.dll")]
		static extern int UuidCreateSequential(out Guid guid);

		public class EUUID : Exception
		{
			public EUUID()
				: base()
			{
			}

			public EUUID(String reason)
				: base(reason)
			{
			}
		}

		public class EUUIDLocalOnly : EUUID
		{
			public EUUIDLocalOnly()
				: base()
			{
			}
		}

		public class EUUIDNoAddress : EUUID
		{
			public EUUIDNoAddress()
				: base()
			{
			}
		}

		public enum UUIDTYPE
		{
			GLOBAL,
			LOCAL
		}

		public static Guid GenerateUUID(UUIDTYPE type)
		{
			long rc;

			Guid g;
			rc = UuidCreateSequential(out g);

			if (rc != RPC_S_OK)
			{
				if (rc == RPC_S_UUID_NO_ADDRESS)
					throw new EUUIDNoAddress();
				else if (rc == RPC_S_UUID_LOCAL_ONLY && type == UUIDTYPE.GLOBAL)
					throw new EUUIDLocalOnly();
				else
					throw new EUUID("UuidToString failed. rc = " + rc);
			}

			return g;
		}

		public static bool CreateUserGuid(out Guid guid)
		{
			try
			{
				guid = GenerateUUID(UUID.UUIDTYPE.GLOBAL);
				Logger.Info("Using UUID.GenerateUUID()", guid.ToString());
			}
			catch (UUID.EUUIDLocalOnly)
			{
				guid = Guid.NewGuid();
				Logger.Info("Using Guid.NewGuid()");
			}
			catch (UUID.EUUID e)
			{
				guid = Guid.NewGuid();
				Logger.Error(e.ToString());
				return false;
			}
			return true;

		}

		public static bool GetGuidAndMakeBackup(out Guid guid, out bool isNewGuid)
		{
			guid = new Guid();
			isNewGuid = true;
			string oldGuid = Utils.GetUserGUID();
			if (!String.IsNullOrEmpty(oldGuid))
			{
				try
				{
					Logger.Info("the value of old guid is " + oldGuid);
					guid = new Guid(oldGuid);
					isNewGuid = false;
				}
				catch (Exception ex)
				{
					Logger.Info("Back up guid incorrect, ex: {0}", ex.ToString());
					isNewGuid = true;
				}
			}

			if (isNewGuid)
			{
				if (!CreateUserGuid(out guid))
				{
					Logger.Info("Cannot create user guid, returing with -2 return code");
					return false;
				}
			}
			User.GUID = guid.ToString();
			Utils.BackUpGuid(guid.ToString());
			return true;
		}

	}
}
