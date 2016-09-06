#include <windows.h>
#include "RegUtils.h"


RegUtils::RegUtils(void)
{
}


RegUtils::~RegUtils(void)
{
}

bool RegUtils::ReadRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, const DWORD dType, LPBYTE lpData)
{
	bool result = false;
	HKEY phkResult;
	if (ERROR_SUCCESS == ::RegOpenKeyEx(hKey,lpSubKey,0,KEY_READ,&phkResult))
	{
		DWORD dSize;
		DWORD tType = dType;
		if (::RegQueryValueEx(phkResult,lpValueName, 0, &tType, lpData, &dSize) == ERROR_SUCCESS)
		{
			result = true;
		}
		RegCloseKey(phkResult);
	}
	return result;
}

bool RegUtils::WriteRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, const DWORD dType, LPBYTE lpData, DWORD cbData)
{
	bool result = false;
	HKEY phkResult;
	//Y_READ | KEY_WRITE,  Y_READ | KEY_WRITE, 
	LONG resultOpen = ::RegOpenKeyEx(hKey,lpSubKey,0,KEY_WRITE,&phkResult);
	if(ERROR_SUCCESS != resultOpen)
	{
		DWORD disposition;
		resultOpen = ::RegCreateKeyEx(hKey,lpSubKey,0,NULL,0,KEY_WRITE,NULL,&phkResult,&disposition);
	}
	if (ERROR_SUCCESS == resultOpen)
	{
		DWORD tType = dType;
		if (::RegSetValueEx(phkResult,lpValueName, 0, tType, lpData, cbData) == ERROR_SUCCESS)
		{
			result = true;
		}
		RegCloseKey(phkResult);
	}
	return result;
}

bool RegUtils::ReadRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, std::wstring & str)
{
	bool result = false;
	HKEY phkResult;
	if (ERROR_SUCCESS == ::RegOpenKeyEx(hKey,lpSubKey,0,KEY_READ,&phkResult))
	{
		DWORD dSize;
		DWORD dType = REG_SZ;
		if (::RegQueryValueEx(phkResult,lpValueName, 0, &dType, NULL, &dSize) == ERROR_SUCCESS)
		{
			std::vector<wchar_t> tempStr(dSize/sizeof(wchar_t));
			if (::RegQueryValueEx(phkResult,lpValueName, 0, &dType, reinterpret_cast<LPBYTE>(&tempStr[0]), &dSize) == ERROR_SUCCESS)
			{
				str = &tempStr[0];
				result = true;
			}
		}
		::RegCloseKey(phkResult);
	}
	return result;
}

bool RegUtils::ReadRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, std::vector<std::wstring> & strs)
{
	bool result = false;
	HKEY phkResult;
	if (ERROR_SUCCESS == ::RegOpenKeyEx(hKey,lpSubKey,0,KEY_READ,&phkResult))
	{
		DWORD dSize;
		DWORD dType = REG_MULTI_SZ;
		if (::RegQueryValueEx(phkResult,lpValueName, 0,&dType , NULL, &dSize) == ERROR_SUCCESS)
		{
			std::vector<wchar_t> tempStr(dSize/sizeof(wchar_t));
			if (::RegQueryValueEx(phkResult,lpValueName, 0, &dType, reinterpret_cast<LPBYTE>(&tempStr[0]), &dSize) == ERROR_SUCCESS)
			{
				if(dSize == tempStr.size() * sizeof(wchar_t))
				{
					strs.clear();
					size_t index = 0;
					do{
						strs.push_back(&tempStr[index]);
						index += (wcslen(&tempStr[0]) + 1);				
					}while(index < tempStr.size());
					result = true;
				}
			}
		}
		::RegCloseKey(phkResult);
	}
	return result;
}

bool RegUtils::ReadRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, std::vector<BYTE> & val)
{
	bool result = false;
	HKEY phkResult;
	if (ERROR_SUCCESS == ::RegOpenKeyEx(hKey,lpSubKey,0,KEY_READ,&phkResult))
	{
		DWORD dSize;
		DWORD dType = REG_BINARY;
		if (::RegQueryValueEx(phkResult,lpValueName, 0, &dType, NULL, &dSize) == ERROR_SUCCESS)
		{
			std::vector<BYTE> temp(dSize);
			if (::RegQueryValueEx(phkResult,lpValueName, 0, &dType, reinterpret_cast<LPBYTE>(&temp[0]), &dSize) == ERROR_SUCCESS)
			{
				val.swap(temp);
				result = true;
			}
		}
		::RegCloseKey(phkResult);
	}
	return result;
}

bool RegUtils::ReadRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, DWORD & val)
{
	bool result = RegUtils::ReadRegValue(hKey, lpSubKey, lpValueName,REG_DWORD, reinterpret_cast<LPBYTE>(&val));
	return result;
}

bool RegUtils::WriteRegValue(HKEY hKey, LPCWSTR lpSubKey, LPCWSTR lpValueName, DWORD val)
{
	bool result = RegUtils::WriteRegValue(hKey, lpSubKey, lpValueName,REG_DWORD, reinterpret_cast<LPBYTE>(&val),sizeof(DWORD));
	return result;
}

bool RegUtils::ExistRegItem(const RegItem & regItem)
{
	return false;

}

bool RegUtils::ExistRegItems(const std::vector<RegItem> & registerItems)
{
	return false;
}

