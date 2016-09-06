// ValidateDotnetExe.cpp : Defines the entry point for the console application.
//
#include <tchar.h>
#include <ctime>
#include <iostream>
#include <windows.h>
#include <string>
#include "../Dll/ValidateDotnetDll.h"

int _tmain(int argc, wchar_t* argv[])
{

	//SetErrorMode(SetErrorMode(0)|SEM_NOGPFAULTERRORBOX | SEM_FAILCRITICALERRORS);
	SetErrorMode(SEM_NOGPFAULTERRORBOX | SEM_FAILCRITICALERRORS);
	int result = 0;
	std::wstring msg;
	//ULONG ul = (ULONG)((LONGLONG)clock() * 1000 / CLOCKS_PER_SEC);
	result = static_cast<int>(::ValidateDotnet(argv, argc));
	std::cout  <<std::endl << "result: 0x"<< std::hex << result;
	//std::cout  <<std::endl <<(ULONG)((LONGLONG)clock() * 1000 / CLOCKS_PER_SEC) - ul ;
	//{
	//	wchar_t * pars[] = {L"reg",NULL};
	//	DWORD re = ValidateDotnet(pars, 1);
	//}
	return result;
}

