#ifndef TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CMULTIDATA_H
#define TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CMULTIDATA_H
#include <Windows.h>
#include <string>
#include <map>
#include <cstdio>
#include <iostream>

#include "CFileUtil.h"


class VersionInfo{
public:
	VersionInfo(){
		dVersion = 0;
		sp = 0;
		isClient = false;
		isFull = false;
		isFullInstall = false;
		isClientInstall = false;
		canRunCase = false;
	}
public:
	std::wstring versionItem;
	std::wstring installPathShortName;
	double dVersion;
	DWORD sp;
	bool isClient;
	bool isFull;
	bool isFullInstall;
	bool isClientInstall;
	bool canRunCase;

};

class Parameters
{
public:
	typedef std::map<std::wstring, std::wstring> ParameterData;
private:
	ParameterData paMap;
	

public:
	typedef std::map<std::wstring, std::wstring> ParameterData;
	const static wchar_t * REG;		 //if do not set, it will be 0
	const static wchar_t * CASE;     //if do not set, it will be "data/HD-CaseDotnet.exe"
	const static wchar_t * MODE;     //if do not set, it will be 0
	const static wchar_t * TOOL;
	const static wchar_t * NET2SP2;
	const static wchar_t * NET4;
	const static wchar_t * REPAIR;
	const static wchar_t * HELP;
	const static wchar_t * WORKPATH;  //if do not set , it will be current process path
	const static wchar_t * INFO;      //whether out info to console
	const static wchar_t * DOWNLOAD;  //

	void OutputHelp();

public:
	ParameterData & GetParameterMap();
	void AddParamter(const wchar_t * key, wchar_t * value);
	void AddParamter(const wchar_t * keyValue);
	void AddParamters(const wchar_t * keyValues[]);

public:
	bool GetReg();
	std::wstring GetCase();
	std::wstring GetNet4();
	std::wstring GetNet2Sp2();
	std::wstring GetTool();
	int GetMode();
	const std::wstring GetWorkPath();
	bool GetRepair();
	bool GetInfo() const;
	bool GetHelp();
	bool GetDownload() const;

public:
	std::wstring GetInstallParamOfNet4()
	{
		return std::wstring(L" /q /norestart");
	}

	std::wstring GetRepairParamOfNet4()
	{
		return std::wstring(L" /q /norestart /repair");
	}

	std::wstring GetInstallParamOfNet2Sp2()
	{
		return std::wstring(L" /q /norestart");
	}

	std::wstring GetRepairParamOfNet2Sp2()
	{
		return std::wstring(L" /q /norestart");
	}

	std::wstring GetParamOfRepairTool()
	{
		return L"/n /q /r ";
	}

	std::wstring GetRequiredConfig()
	{
		return std::wstring(L"\
<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n\
<configuration>\r\n\
<startup>\r\n\
<supportedRuntime version=\"{0}\" safemode=\"true\"/>\r\n\
</startup>\r\n\
<system.net>\r\n\
<defaultProxy useDefaultCredentials=\"true\"/>\r\n\
</system.net>\r\n\
</configuration>");
	}

	std::wstring GetSupportedRuntimeConfig()
	{
		return std::wstring(L"\
							 <?xml version=\"1.0\" encoding=\"utf-8\" ?>\
							 <configuration>\
							 <startup>\
							 <supportedRuntime version=\"v4.0\" />\
							 <supportedRuntime version=\"v2.0.50727\" />\
							 </startup>\
							 <system.net>\
							 <defaultProxy useDefaultCredentials=\"true\"/>\
							 </system.net>\
							 </configuration>\
			");
	}

	std::wstring GetDotnetV2()
	{
		return std::wstring(L"v2.0.50727");
	}

	std::wstring GetDotnetV4()
	{
		return std::wstring(L"v4.0");
	}

	std::wstring GetTempName()
	{
		return std::wstring(L"temp");
	}

	const wchar_t * GetDataName()
	{
		return L"data";
	}

	std::wstring GetUrlNet4() const;

	std::wstring GetUrlRepairTool();


	std::wstring GetUrlNet2Sp2() const;
private:

	std::wstring GetUrlNet2Sp2x64()  const;
	std::wstring GetUrlNet2Sp2x86() const;
};

#endif //TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CMULTIDATA_H
