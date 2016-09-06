
#ifndef TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CVALIDATEDOTNET_H
#define TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CVALIDATEDOTNET_H
#include <vector>
#include <sstream>
#include "RegUtils.h"
#include "CMultiData.h"

class CValidateDotnet {
public:
	CValidateDotnet(void);
	// TODO: add your methods here.
public:
	static DWORD ValidateDotNet(Parameters parameters);

private:


	static bool Versions2_4(bool info, std::vector<VersionInfo> & versions);

	static bool Handle4_0(VersionInfo & versionInfo, const wchar_t * name );
	static bool Handle2_0(VersionInfo & versionInfo, const wchar_t * name );
	static bool Handle3_0(VersionInfo & versionInfo, const wchar_t * name );
	static bool Handle3_5(VersionInfo & versionInfo, const wchar_t * name );
	static bool HandleGt4_0(VersionInfo & versionInfo, const wchar_t * name );
	//************************************
	// Method:    ExistRegisterItems
	// FullName:  CValidateDotnet::ExistRegisterItems
	// Access:    private static 
	// Returns:   bool
	// Qualifier:
	// Parameter: std::vector<std::wstring[]> registerItems. register Items
	//************************************
	static bool ExistRegisterItems(const std::vector<RegItem> & registerItems);

	//************************************
	// Method:    TryProgramOfDotnet
	// FullName:  CValidateDotnet::TryProgramOfDotnet
	// Access:    private static 
	// Returns:   bool
	// Qualifier:
	// Parameter: std::wstring file, include the path.
	//************************************
	static bool DebugDotnetProgram(const std::wstring& file);

	static bool TryRunDotnetProgram(Parameters parameters, std::vector<VersionInfo> & versions);

	static VersionInfo* FindNet(std::vector<VersionInfo> & versions, double dversion);

	static bool GetLastErrorText(std::wstring & text);
	
	//************************************
	// Method:    CompareVersion
	// FullName:  CValidateDotnet::CompareVersion
	// Access:    private static 
	// Returns:   bool 
	// Qualifier:
	// Parameter: 
	static bool CValidateDotnet::CompareVersion(const wchar_t * lpszVer1, const wchar_t * lpszVer2, __out short & nResult);

	//************************************
	// Method:    TryToRepairDotnet
	// FullName:  CValidateDotnet::TryToRepairDotnet
	// Access:    private static 
	// Returns:   bool, true: just do if, false: done nothing
	// Qualifier:
	// Parameter: Parameters & pamas
	// Parameter: std::vector<VersionInfo> & versions
	//************************************
	bool TryRepairDotnet(Parameters& pamas, std::vector<VersionInfo>& versions);

	//************************************
	// Method:    TryToRepairDotnet2
	// FullName:  CValidateDotnet::TryToRepairDotnet2
	// Access:    private 
	// Returns:   bool, true: just do if, false: done nothing
	// Qualifier:
	// Parameter: Parameters & pamas
	// Parameter: std::vector<VersionInfo> & pversion
	//************************************
	bool TryRepairDotnet2(Parameters& pamas,const VersionInfo * pversion);

	//************************************
	// Method:    TryRepairDotnet4
	// FullName:  CValidateDotnet::TryRepairDotnet4
	// Access:    private 
	// Returns:   bool, true: just do if, false: done nothing
	// Qualifier:
	// Parameter: Parameters & pamas
	// Parameter: co VersionInfo * pversion
	//************************************
	bool TryRepairDotnet4(Parameters& pamas, const VersionInfo * pversion);

	//************************************
	// Method:    TryUseRepairTool
	// FullName:  CValidateDotnet::TryUseRepairTool
	// Access:    private 
	// Returns:   bool, true: just do if, false: done nothing
	// Qualifier:
	//************************************
	bool TryUseRepairTool(Parameters pamas);

	bool DownloadNet2Sp2(Parameters& parameters,const wchar_t * outFile);

	bool DownloadNet4(Parameters& parameters,const wchar_t * outFile);

	bool DownloadRepairTool(Parameters& parameters,const wchar_t * outFile);
private:
	DWORD repairNet4;
	DWORD installNet4;
	DWORD installNet2sp2;
	DWORD repairNet2sp2;
	DWORD runRepairTool;

	/////
	const DWORD MAX_TIME;
public:
	enum ExitCode{
		SUCCESS     = 0,
		NET2_NOT_INSTALL        = 0x1,    //not installed the .net 2.0
		NET2SP2_NOT_INSTALL     = 0x2,    //not installed the .net sp2
		NET2_INVALIDATE         = 0x4,    //.net 2.0 is not validate

		NET4_CLIENT             = 0x10,   //installed the .net4 client 
		NET4_FULL_NOT_INSTALL   = 0x20,   //not installed the .net4(4.0,4.5,<5.0) full
		NET4_INVALIDATE         = 0x40,   //.net 4.0(4.0,4.5,<5.0) is not validate

	//	NET3                    = 0x80,   //installed the .net3.0
	//	//NET3_NOT              = 0x100,  //installed

	//	NET35                   = 0x400,  //installed the .net3.5 client
	//	NET35_FULL              = 0x800,  //installed the .net3.5 full
	//	//NET35_NOT             = 0x1000, //

		FAIL_RUN_CASE           = 0x2000, //fail run the case
		FAIL_REPAIR             = 0x4000  //fail repair
	};
};
#endif //TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CVALIDATEDOTNET_H