#ifndef TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CCONFIG_H
#define TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CCONFIG_H
#include <string>

class Config
{
private:
	Config(void);
	~Config(void);
public:
	static Config & GetConfig();

	std::wstring& UrlNet4();
	std::wstring& UrlRepairTool();
	std::wstring& UrlNet2Sp2x64();
	std::wstring& UrlNet2Sp2x86();

private:
	std::wstring urlNet4;
	std::wstring urlRepairTool;
	std::wstring urlNet2Sp2x64;
	std::wstring urlNet2Sp2x86;
	bool init;
};

#endif //TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CCONFIG_H
