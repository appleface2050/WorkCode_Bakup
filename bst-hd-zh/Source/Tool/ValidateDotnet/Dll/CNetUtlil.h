#ifndef TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CNETUTLIL_H
#define TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CNETUTLIL_H
class NetUtlil
{
private:
	NetUtlil(void);
	~NetUtlil(void);

public:
	static bool HttpGet(const wchar_t * url, const wchar_t * fileName);
};
#endif //TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CNETUTLIL_H

