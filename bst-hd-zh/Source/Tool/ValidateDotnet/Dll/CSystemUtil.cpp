#include "CSystemUtil.h"
#include <Windows.h>

SystemUtil::SystemUtil(void)
{
}


SystemUtil::~SystemUtil(void)
{
}

bool SystemUtil::IsSystem64()
{
	SYSTEM_INFO si = { 0 }; 
	GetNativeSystemInfo(&si);
	return (si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64 || si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_IA64 ); 
}

bool SystemUtil::IsWindowsXPSP3OrGreater()
{
	OSVERSIONINFOEX osvi;
	memset(&osvi,0x0, sizeof(osvi));
	osvi.dwOSVersionInfoSize = sizeof(osvi);
	osvi.dwMajorVersion = 5;
	osvi.dwMinorVersion = 1;
	osvi.wServicePackMajor = 3;
	osvi.wServicePackMinor = 0;

	DWORDLONG dwlConditionMask = 0;
	{
		int op=VER_GREATER_EQUAL;
		// Initialize the condition mask.
		VER_SET_CONDITION( dwlConditionMask, VER_MAJORVERSION, op );
		VER_SET_CONDITION( dwlConditionMask, VER_MINORVERSION, op );
		VER_SET_CONDITION( dwlConditionMask, VER_SERVICEPACKMAJOR, op );
		VER_SET_CONDITION( dwlConditionMask, VER_SERVICEPACKMINOR, op );
	}
	bool resu = (TRUE == VerifyVersionInfo(&osvi, VER_MAJORVERSION | VER_MINORVERSION | VER_SERVICEPACKMAJOR | VER_SERVICEPACKMINOR,	dwlConditionMask));
	return resu;
}
