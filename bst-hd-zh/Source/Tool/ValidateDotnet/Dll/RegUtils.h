#ifndef TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_REGUTILS_H
#define TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_REGUTILS_H

#include <string>
#include <vector>

struct RegItem{
	HKEY hKey;
	std::wstring lpSubKey;
	//the lpValueName can be empty
	std::wstring lpValueName;
	RegItem(HKEY hKeyIn,std::wstring lpSubKeyIn, std::wstring lpValueNameIn):hKey(hKeyIn),lpSubKey(lpSubKeyIn),lpValueName(lpValueNameIn)
	{
	
	}
};

class RegUtils
{
private:
	RegUtils(void);
	~RegUtils(void);
public:
	
	//************************************
	// Method:    GetValue
	// FullName:  Reg::GetValue
	// Access:    public static 
	// Returns:   bool
	// Qualifier:
	// Parameter: HKEY hKey, such as HKEY_CLASSES_ROOT HKEY_CURRENT_CONFIG HKEY_CURRENT_USER HKEY_LOCAL_MACHINE HKEY_PERFORMANCE_DATA HKEY_PERFORMANCE_NLSTEXT HKEY_PERFORMANCE_TEXT HKEY_USERS

	// Parameter: PCWSTR lpSubKey, such as SOFTWARE\BlueStacks_funplay_dt\Guests\Android\Config
	// Parameter: PCWSTR lpValueName 
	// Parameter: PDWORD lpType, such as REG_BINARY REG_DWORD REG_DWORD_LITTLE_ENDIAN REG_DWORD_BIG_ENDIAN REG_EXPAND_SZ REG_LINK REG_MULTI_SZ REG_NONE REG_QWORD REG_QWORD_LITTLE_ENDIAN REG_SZ
	// Parameter: PBYTE lpData
	//************************************
	static bool ReadRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, const DWORD dType, LPBYTE lpData);

	static bool WriteRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, const DWORD dType, LPBYTE lpData, DWORD cbData);

	//************************************
	// Method:    GetValue
	// FullName:  Reg::GetValue
	// Access:    public static 
	// Returns:   bool
	// Qualifier: read the type REG_SZ
	// Parameter: HKEY hKey
	// Parameter: PCWSTR lpSubKey
	// Parameter: PCWSTR lpValueName
	// Parameter: std::wstring & str
	//************************************
	static bool ReadRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, std::wstring & str);
	//************************************
	// Method:    GetValue
	// FullName:  Reg::GetValue
	// Access:    public static 
	// Returns:   bool
	// Qualifier: read the type REG_MULTI_SZ
	// Parameter: HKEY hKey
	// Parameter: PCWSTR lpSubKey
	// Parameter: PCWSTR lpValueName
	// Parameter: std::vector<std::wstring> & strs
	//************************************
	static bool ReadRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, std::vector<std::wstring> & strs);


	//************************************
	// Method:    ReadRegValue
	// FullName:  RegUtils::ReadRegValue
	// Access:    public static 
	// Returns:   bool
	// Qualifier: read the type REG_BINARY
	// Parameter: HKEY hKey
	// Parameter: LPCWSTR lpSubKey
	// Parameter: LPCWSTR lpValueName
	// Parameter: std::vector<BYTE> & val
	//************************************
	static bool ReadRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, std::vector<BYTE> & val);

	//************************************
	// Method:    ReadRegValue
	// FullName:  RegUtils::ReadRegValue
	// Access:    public static 
	// Returns:   bool
	// Qualifier: read the type REG_DWORD
	// Parameter: HKEY hKey
	// Parameter: LPCWSTR lpSubKey
	// Parameter: LPCWSTR lpValueName
	// Parameter: DWORD & val
	//************************************
	static bool ReadRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, DWORD & val);
		//************************************
	// Method:    WriteRegValue
	// FullName:  RegUtils::ReadRegValue
	// Access:    public static 
	// Returns:   bool
	// Qualifier: read the type REG_DWORD
	// Parameter: HKEY hKey
	// Parameter: LPCWSTR lpSubKey
	// Parameter: LPCWSTR lpValueName
	// Parameter: DWORD val
	static bool WriteRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, DWORD val);

	//************************************
	// Method:    ExistReg
	// FullName:  RegUtils::ExistReg
	// Access:    public static 
	// Returns:   bool
	// Qualifier: 
	// Parameter: HKEY hKey
	// Parameter: LPCWSTR lpSubKey
	// Parameter: LPCWSTR lpValueName
	//************************************
	static bool ExistRegItem(const RegItem & regItem);

	static bool ExistRegItems(const std::vector<RegItem> & registerItems);
	
};

#endif //TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_REGUTILS_H