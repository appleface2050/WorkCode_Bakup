
#ifndef TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CDISABLEERRORDIALOG_H
#define TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CDISABLEERRORDIALOG_H

#include <windows.h>
class DisableErrorDialog
{
public:
	DisableErrorDialog(void);
	~DisableErrorDialog(void);
private:
	int errorMode;
private:
	void ReadErrorMode();
	void SetErroMode(DWORD mode);
};
#endif

