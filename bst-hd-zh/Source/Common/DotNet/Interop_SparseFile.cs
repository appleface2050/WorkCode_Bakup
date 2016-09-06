/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Common Interop SparseFile 
 */

using System;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Common.Interop
{
	public class SparseFile
	{

		[Flags]
		public enum FileSystemFeature : uint
		{
			FILE_CASE_SENSITIVE_SEARCH = 0x1,
			FILE_CASE_PRESERVED_NAMES = 0x2,
			FILE_UNICODE_ON_DISK = 0x4,
			FILE_PERSISTENT_ACLS = 0x8,
			FILE_FILE_COMPRESSION = 0x10,
			FILE_VOLUME_QUOTAS = 0x20,
			FILE_SUPPORTS_SPARSE_FILES = 0x40,
			FILE_SUPPORTS_REPARSE_POINTS = 0x80,
			FILE_VOLUME_IS_COMPRESSED = 0x8000,
			FILE_SUPPORTS_OBJECT_IDS = 0x10000,
			FILE_SUPPORTS_ENCRYPTION = 0x20000,
			FILE_NAMED_STREAMS = 0x40000,
			FILE_READONLY_VOLUME = 0x80000,
			FILE_SEQUENTIAL_WRITE_ONCE = 0x100000,
			FILE_SUPPORTS_TRANSACTIONS = 0x200000,
			FILE_SUPPORTS_HARD_LINKS = 0x400000,
			FILE_SUPPORTS_EXTENDED_ATTRIBUTES = 0x800000,
			FILE_SUPPORTS_OPEN_BY_FILE_ID = 0x1000000,
			FILE_SUPPORTS_USN_JOURNAL = 0x2000000
		}

		[DllImport("kernel32.dll")]
		private extern static bool GetVolumeInformation(
				String rootPathName,
				StringBuilder volumeNameBuffer,
				int volumeNameSize,
				out uint volumeSerialNumber,
				out uint maximumComponentLength,
				out FileSystemFeature fileSystemFlags,
				StringBuilder fileSystemNameBuffer,
				int nFileSystemNameSize);


		const uint FILE_ATTRIBUTE_NORMAL = 0x80;
		const uint FILE_END = 0x2;
		const short INVALID_HANDLE_VALUE = -1;
		const uint GENERIC_READ = 0x80000000;
		const uint GENERIC_WRITE = 0x40000000;
		const uint CREATE_NEW = 1;
		const uint CREATE_ALWAYS = 2;
		const uint OPEN_EXISTING = 3;

		const uint FSCTL_SET_SPARSE = 590020;

		[DllImport("kernel32.dll")]
		private extern static IntPtr CreateFile(String lpFileName,
				uint dwDesiredAccess,
				uint dwShareMode,
				IntPtr lpSecurityAttributes,
				uint dwCreationDisposition,
				uint dwFlagsAndAttributes,
				IntPtr hTemplateFile);


		[DllImport("kernel32.dll")]
		static extern bool DeviceIoControl(IntPtr hDevice,
				uint dwIoControlCode,
				IntPtr lpInBuffer,
				uint nInBufferSize,
				IntPtr lpOutBuffer,
				uint nOutBufferSize,
				out uint lpBytesReturned,
				IntPtr lpOverlapped);

		[DllImport("kernel32.dll")]
		static extern bool SetFilePointerEx(IntPtr hFile,
				Int64 liDistanceToMove,
				IntPtr lpNewFilePointer,
				uint dwMoveMethod);

		[DllImport("kernel32.dll")]
		static extern bool SetEndOfFile(IntPtr hFile);

		[DllImport("kernel32.dll")]
		static extern bool CloseHandle(IntPtr hObject);

		public static bool SupportsSparseFiles(String fileName)
		{
			String rootPath = Path.GetPathRoot(fileName);
			StringBuilder volumeNameBuffer = new StringBuilder(261);
			StringBuilder fileSystemNameBuffer = new StringBuilder(261);
			uint volumeSerialNumber;
			uint maximumComponentLength;
			FileSystemFeature fileSystemFlags;

			GetVolumeInformation(rootPath,
					volumeNameBuffer,
					volumeNameBuffer.Capacity,
					out volumeSerialNumber,
					out maximumComponentLength,
					out fileSystemFlags,
					fileSystemNameBuffer,
					fileSystemNameBuffer.Capacity);

			if ((fileSystemFlags & FileSystemFeature.FILE_SUPPORTS_SPARSE_FILES) == FileSystemFeature.FILE_SUPPORTS_SPARSE_FILES)
				return true;

			return false;
		}

		public unsafe static void CreateSparse(String fileNamePath, Int64 sz)
		{
			if (File.Exists(fileNamePath))
				File.Delete(fileNamePath);

			IntPtr handle = CreateFile(fileNamePath,
					GENERIC_READ | GENERIC_WRITE,
					0,
					IntPtr.Zero,
					CREATE_NEW,
					FILE_ATTRIBUTE_NORMAL,
					IntPtr.Zero);

			int err = Marshal.GetLastWin32Error();

			if (handle.ToInt32() == -1)
				throw new SystemException("CreateFile failed: " + err, new Win32Exception(err));


			uint dummy;
			if (!DeviceIoControl(handle,
						FSCTL_SET_SPARSE,
						IntPtr.Zero,
						0,
						IntPtr.Zero,
						0,
						out dummy,
						IntPtr.Zero))
				throw new SystemException("DeviceIoControl failed: ", new Win32Exception(Marshal.GetLastWin32Error()));

			UInt64 n = 0;
			IntPtr ptr = new IntPtr(&n);

			if (!SetFilePointerEx(handle,
						sz,
						ptr,
						FILE_END))
				throw new SystemException("SetFilePointerEx failed: ", new Win32Exception(Marshal.GetLastWin32Error()));

			SetEndOfFile(handle);
			CloseHandle(handle);
		}

		public unsafe static void CreateNonSparse(String fileNamePath, Int64 sz)
		{
			IntPtr handle = CreateFile(fileNamePath,
					GENERIC_READ | GENERIC_WRITE,
					0,
					IntPtr.Zero,
					CREATE_NEW,
					FILE_ATTRIBUTE_NORMAL,
					IntPtr.Zero);

			int err = Marshal.GetLastWin32Error();

			if (handle.ToInt32() == -1)
				throw new SystemException("CreateFile failed: " + err, new Win32Exception(err));

			UInt64 n = 0;
			IntPtr ptr = new IntPtr(&n);

			if (!SetFilePointerEx(handle,
						sz,
						ptr,
						FILE_END))
				throw new SystemException("SetFilePointerEx failed: ", new Win32Exception(Marshal.GetLastWin32Error()));

			SetEndOfFile(handle);
			CloseHandle(handle);
		}
	}
}
