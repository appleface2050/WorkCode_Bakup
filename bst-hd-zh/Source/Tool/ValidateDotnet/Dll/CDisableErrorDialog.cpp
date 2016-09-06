#include <iostream>
#include "CDisableErrorDialog.h"
#include "RegUtils.h"

DisableErrorDialog::DisableErrorDialog(void):errorMode(-1)
{
	ReadErrorMode();
	SetErroMode(SEM_NOGPFAULTERRORBOX | SEM_FAILCRITICALERRORS);
#ifdef _DEBUG
	std::cout  <<std::endl << "DisableErrorDialog£º"  <<errorMode;
#endif
}


DisableErrorDialog::~DisableErrorDialog(void)
{
	if(-1 != errorMode)
	{
		SetErroMode(errorMode);
#ifdef _DEBUG
		std::cout  <<std::endl << "~DisableErrorDialog£º"  <<errorMode;
#endif
	}
}

void DisableErrorDialog::ReadErrorMode()
{
	DWORD mode = 0;
	if(RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,L"SYSTEM\\CurrentControlSet\\Control\\Windows",L"ErrorMode",mode))
	{
		errorMode = mode;
	}
}

void DisableErrorDialog::SetErroMode(DWORD mode)
{
	RegUtils::WriteRegValue(HKEY_LOCAL_MACHINE,L"SYSTEM\\CurrentControlSet\\Control\\Windows",L"ErrorMode",mode);
}
