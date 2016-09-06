#ifndef TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_SYSTEMUTIL_H
#define TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_SYSTEMUTIL_H


class SystemUtil
{
private:
	SystemUtil(void);
	~SystemUtil(void);
public:
	static bool IsSystem64();
	static bool IsWindowsXPSP3OrGreater();
};
#endif //TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_SYSTEMUTIL_H

