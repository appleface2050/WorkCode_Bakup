// ValidateDotNetNative.cpp : Defines the exported functions for the DLL application.
//
#include <iostream>
#include <fstream>
#include <locale>
#include <windows.h>
#include <codecvt>
#include "CValidateDotnet.h"
#include "CDisableErrorDialog.h"
#include "RegUtils.h"
#include "CFileUtil.h"
#include "CNetUtlil.h"
#include "CSystemUtil.h"

const int MAX_REG_LEN = 256;
const std::wstring DotNetRegKeyName = L"SOFTWARE\\Microsoft\\NET Framework Setup\\NDP"; 

CValidateDotnet::CValidateDotnet():MAX_TIME(2)
{
	repairNet4 = 0;
	installNet4 = 0;
	installNet2sp2 = 0;
	repairNet2sp2 = 0;
	runRepairTool = 0;
	return;
}

DWORD CValidateDotnet::ValidateDotNet(Parameters parameters)
{
	bool info = parameters.GetInfo();

#ifdef _DEBUG
	std::cout  << "\r\n\r\n\r\n ValidateDotNet";
#endif
	const HKEY RE_HKEY = HKEY_CURRENT_USER;
	const wchar_t * RE_SUBKEY = L"SOFTWARE\\BlueStacks";
	const wchar_t * RE_VALUENAME = L"ValidateDotnet";
	DWORD result = FAIL_RUN_CASE;
	int checkItems = parameters.GetMode();
	std::wstring runFile =  parameters.GetCase();
	bool isRegistry = parameters.GetReg();

	for(int i = 0; i < 10; ++i)
	{
		CValidateDotnet validate;

		if(0 == checkItems)
		{
			checkItems = 3;
		}

		std::vector<VersionInfo> versions;

		do{

			if(0 != (checkItems & 0x1)) 
			{
				if(!Versions2_4(info,versions))
				{
#ifdef _DEBUG
					std::cout  <<std::endl << "!Versions2_4(versions)  size : " << versions.size();
#endif
					break;
				}


				std::vector<RegItem> items;
				items.push_back(RegItem(HKEY_LOCAL_MACHINE,L"",L""));
				if(!CValidateDotnet::ExistRegisterItems(items))
				{
					//result = 0x1;
					break;
				}
			}

			if((0 != (checkItems & 0x2)))
			{
				if (!CValidateDotnet::TryRunDotnetProgram(parameters,versions))
				{
					result |= FAIL_RUN_CASE;
					break;
				}else{
					result &= (~FAIL_RUN_CASE);
				}
			}
		}while(false);


		{
			
			VersionInfo * net2 = FindNet(versions, 2.0);

			if(NULL == net2)
			{
				result |= NET2_NOT_INSTALL;
				result |= NET2SP2_NOT_INSTALL;
				result |= NET2_INVALIDATE;
			}else
			{
				VersionInfo & temp = *(net2);
				if (temp.sp < 2)
				{
					result |= NET2SP2_NOT_INSTALL;
#ifdef _DEBUG
					std::cout  <<std::endl << "temp.sp < 2 : " << temp.sp;
#endif
				}

				if (!temp.isFullInstall)
				{
					result |= NET2_NOT_INSTALL;
				}

				if (!temp.canRunCase)
				{
					result |= NET2_INVALIDATE;
				}
			}
		}
		{
			VersionInfo * net4 = FindNet(versions, 4.0);;
			if(NULL == net4)
			{
				result |= NET4_FULL_NOT_INSTALL;
				result |= NET4_INVALIDATE;
			}else
			{
				VersionInfo & temp = *(net4);

				if (!temp.isFullInstall)
				{
					result |= NET4_FULL_NOT_INSTALL;
				}

				if (temp.isClientInstall)
				{
					result |= NET4_CLIENT;
				}

				if (!temp.canRunCase)
				{
					result |= NET4_INVALIDATE;
				}
			}
		}

		//if need, try to repair the .net framework
		if ((result & FAIL_RUN_CASE) == 0)
		{
			result = SUCCESS;
			break;
		}else
		{
			if (parameters.GetRepair())
			{
				if(!validate.TryRepairDotnet(parameters, versions))//do nothing 
				{
					break;
				}
			}else
			{
				break;
			}
		}
	}
	//write the registry
	if(isRegistry)
	{
#ifdef _DEBUG
		std::cout  <<std::endl << "WriteRegValue : " << result;
#endif
		RegUtils::WriteRegValue(RE_HKEY, RE_SUBKEY,RE_VALUENAME,result);
	}

	return result;
}

bool CValidateDotnet::Versions2_4(bool info, std::vector<VersionInfo> & versions)
{
	const std::wstring DotNetRegVersionName = L"Version";
	const std::wstring DotNetStandardRegValueName = L"Install";

	const DWORD MIN_DotNet45_RELEASE = 378389;

	HKEY hKey;
	bool bRet = false;
	if (ERROR_SUCCESS == RegOpenKeyEx(HKEY_LOCAL_MACHINE,DotNetRegKeyName.c_str(),0,KEY_READ,&hKey))
	{
		DWORD dwIndex = 0;
		DWORD dwLength = MAX_REG_LEN;
		wchar_t valueName[MAX_REG_LEN] = {0}; 
		while (RegEnumKeyEx(hKey,dwIndex,valueName,&dwLength,NULL,NULL,NULL,NULL) == ERROR_SUCCESS)
		{	
			VersionInfo vtemp;
			bool rtemp = false;
			if (0== wcscmp(valueName, L"v4") ) // the V4.0 is not use
			{
				rtemp = Handle4_0(vtemp, valueName);
			}else if (0 == wcsncmp(valueName, L"v2.0",4))
			{
				rtemp = Handle2_0(vtemp, valueName);
			}else if(0 == wcscmp(valueName, L"v3.0"))
			{
				rtemp = Handle3_0(vtemp, valueName);
			}else if(0 == wcscmp(valueName, L"v3.5"))
			{
				rtemp = Handle3_5(vtemp, valueName);
			}else if(0 == wcsncmp(valueName, L"v",1))//handle other version
			{
				//rtemp = HandleGt4_0(vtemp, valueName);
			}
			if(rtemp)
			{
				versions.push_back(vtemp);
			}

			dwLength = MAX_REG_LEN;	
			dwIndex++;
		}

	}else
	{
		if (info)
		{

			std::cout << "\r\n can not open registy\r\n the last error: ";
			std::wstring text;
			GetLastErrorText(text);
			std::wcout << text.c_str();
		}
	}

	if (hKey != NULL)
	{
		RegCloseKey(hKey);
	}


	for (std::vector<VersionInfo>::iterator it = versions.begin(); it != versions.end(); ++it)
	{
		VersionInfo & temp = *it;
		if(temp.dVersion == 2.0 && temp.sp > 1 && temp.isFullInstall)
		{
			bRet = true;
			break;
		}else if (temp.dVersion == 4.0 && temp.isFullInstall)
		{
			bRet = true;
			break;
		}else if(temp.dVersion > 2 && temp.isFullInstall)
		{
			bRet = true;
			break;
		}
	}

	return bRet;
}

bool CValidateDotnet::Handle4_0(VersionInfo & versionInfo, const wchar_t * name)
{
	versionInfo.dVersion = 4.0;
	versionInfo.versionItem = name;

	std::wstring baseRegKey(DotNetRegKeyName);

	baseRegKey.append(L"\\");
	baseRegKey.append(name);

	//if (0 == wcscmp(name, L"v4.0"))
	//{
	//	versionInfo.isClient = true;
	//	versionInfo.isFull = false;
	//	versionInfo.isFullInstall = false;
	//	std::wstring temp(baseRegKey);
	//	temp.append(L"\\Client");
	//	DWORD dwRegValue=0;
	//	bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,temp.c_str(),L"Install",dwRegValue);
	//	if(rtemp && dwRegValue ==1)
	//	{
	//		versionInfo.isClientInstall = true;
	//	}
	//}
	if (0 == wcscmp(name, L"v4"))
	{
		std::wstring temp(baseRegKey);
		temp.append(L"\\Client");
		DWORD dwRegValue=0;
		bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,temp.c_str(),L"Install",dwRegValue);
		if (rtemp)
		{
			versionInfo.isClient = true;
			versionInfo.isClientInstall = (dwRegValue ==1);
		}else
		{
			versionInfo.isClient = false;
			versionInfo.isClientInstall = false;
		}

		{
			std::wstring tempPath;
			bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,temp.c_str(),L"InstallPath",tempPath);
			if (rtemp && !tempPath.empty())
			{
				if(L'\\' == tempPath[tempPath.length()-1])
				{
					tempPath = tempPath.substr(0,tempPath.length()-1);
				}
				std::size_t index = tempPath.find_last_of(L"\\");
				if(index != std::wstring::npos)
				{
					versionInfo.installPathShortName = tempPath.substr(index + 1);
				}
			}
		}

		temp = baseRegKey;
		temp.append(L"\\Full");
		rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,temp.c_str(),L"Install",dwRegValue);
		if (rtemp)
		{
			versionInfo.isFull = true;
			versionInfo.isFullInstall = (dwRegValue ==1);
		}else
		{
			versionInfo.isFull = false;
			versionInfo.isFullInstall = false;
		}

		{
			std::wstring tempPath;
			bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,temp.c_str(),L"InstallPath",tempPath);
			if (rtemp && !tempPath.empty())
			{
				if(L'\\' == tempPath[tempPath.length()-1])
				{
					tempPath = tempPath.substr(0,tempPath.length()-1);
				}
				std::size_t index = tempPath.find_last_of(L"\\");
				if(index != std::wstring::npos)
				{
					versionInfo.installPathShortName = tempPath.substr(index + 1);
				}
			}
		}
	}
	return true;
}

bool CValidateDotnet::Handle2_0(VersionInfo & versionInfo, const wchar_t * name)
{
	versionInfo.dVersion = 2.0;
	versionInfo.versionItem = name;
	versionInfo.installPathShortName = name;

	std::wstring baseRegKey(DotNetRegKeyName);

	baseRegKey.append(L"\\");
	baseRegKey.append(name);

	{// install
		versionInfo.isFull = true;
		versionInfo.isFullInstall = false;
		DWORD dwRegValue=0;
		bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,baseRegKey.c_str(),L"Install",dwRegValue);
		if(rtemp && dwRegValue ==1)
		{
			versionInfo.isFullInstall = true;
		}
	}
	//sp
	{
		DWORD dwRegValue=0;
		bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,baseRegKey.c_str(),L"SP",dwRegValue);
		if(rtemp)
		{
			versionInfo.sp = dwRegValue;
		}
	}

	return true;
}

bool CValidateDotnet::Handle3_0(VersionInfo & versionInfo, const wchar_t * name)
{
	versionInfo.dVersion = 3.0;
	versionInfo.versionItem = name;
	//versionInfo.installPathShortName = name;
	std::wstring baseRegKey(DotNetRegKeyName);

	baseRegKey.append(L"\\");
	baseRegKey.append(name);

	{// install
		versionInfo.isFull = true;
		versionInfo.isFullInstall = false;
		DWORD dwRegValue=0;
		bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,baseRegKey.c_str(),L"Install",dwRegValue);
		if(rtemp && dwRegValue ==1)
		{
			versionInfo.isFullInstall = true;
		}

		std::wstring tempPath;
		rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,baseRegKey.c_str(),L"InstallPath",tempPath);
		if (rtemp && !tempPath.empty())
		{
			if(L'\\' == tempPath[tempPath.length()-1])
			{
				tempPath = tempPath.substr(0,tempPath.length()-1);
			}
			std::size_t index = tempPath.find_last_of(L"\\");
			if(index != std::wstring::npos)
			{
				versionInfo.installPathShortName = tempPath.substr(index + 1);
			}
		}
	}

	return true;
}

bool CValidateDotnet::Handle3_5(VersionInfo & versionInfo, const wchar_t * name)
{

	versionInfo.dVersion = 3.5;
	versionInfo.versionItem = name;
	versionInfo.installPathShortName = name;
	std::wstring baseRegKey(DotNetRegKeyName);

	baseRegKey.append(L"\\");
	baseRegKey.append(name);

	{// install
		versionInfo.isFull = true;
		versionInfo.isFullInstall = false;
		DWORD dwRegValue=0;
		bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,baseRegKey.c_str(),L"Install",dwRegValue);
		if(rtemp && dwRegValue ==1)
		{
			versionInfo.isFullInstall = true;
		}
	}
	//sp
	{
		DWORD dwRegValue=0;
		bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,baseRegKey.c_str(),L"Install",dwRegValue);
		if(rtemp)
		{
			versionInfo.sp = dwRegValue;
		}
	}

	std::wstring tempPath;
	bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,baseRegKey.c_str(),L"InstallPath",tempPath);
	if (rtemp && !tempPath.empty())
	{
		if(L'\\' == tempPath[tempPath.length()-1])
		{
			tempPath = tempPath.substr(0,tempPath.length()-1);
		}
		std::size_t index = tempPath.find_last_of(L"\\");
		if(index != std::wstring::npos)
		{
			versionInfo.installPathShortName = tempPath.substr(index + 1);
		}
	}
	return true;
}

bool CValidateDotnet::HandleGt4_0(VersionInfo & versionInfo, const wchar_t * name)
{
	{
		std::wstring temp = name;
		if(temp.length() > 2)
		{
			temp = temp.substr(1,2);
		}else if (temp.length() == 2)
		{
			temp = temp.substr(1,1);
		}else
		{
			return false;
		}
		wchar_t * pend = NULL;
		versionInfo.dVersion = std::wcstod(temp.c_str(),&pend);
	}

	versionInfo.versionItem = name;

	std::wstring baseRegKey(DotNetRegKeyName);

	baseRegKey.append(L"\\");
	baseRegKey.append(name);

	if (0 == wcscmp(name, L"v4.0"))
	{
		versionInfo.isClient = true;
		versionInfo.isFull = false;
		versionInfo.isFullInstall = false;
		std::wstring temp(baseRegKey);
		temp.append(L"\\Client");
		DWORD dwRegValue=0;
		bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,temp.c_str(),L"Install",dwRegValue);
		if(rtemp && dwRegValue ==1)
		{
			versionInfo.isClientInstall = true;
		}
	}
	if (0 == wcscmp(name, L"v4"))
	{
		std::wstring temp(baseRegKey);
		temp.append(L"\\Client");
		DWORD dwRegValue=0;
		bool rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,temp.c_str(),L"Install",dwRegValue);
		if (rtemp)
		{
			versionInfo.isClient = true;
			versionInfo.isClientInstall = (dwRegValue ==1);
		}else
		{
			versionInfo.isClient = false;
			versionInfo.isClientInstall = false;
		}

		temp = baseRegKey;
		temp.append(L"Full");
		rtemp = RegUtils::ReadRegValue(HKEY_LOCAL_MACHINE,temp.c_str(),L"Install",dwRegValue);
		if (rtemp)
		{
			versionInfo.isFull = true;
			versionInfo.isFullInstall = (dwRegValue ==1);
		}else
		{
			versionInfo.isFull = false;
			versionInfo.isFullInstall = false;
		}
	}
	return true;
}

bool CValidateDotnet::ExistRegisterItems(const std::vector<RegItem> & registerItems)
{
	return true;
}

bool CValidateDotnet::DebugDotnetProgram(const std::wstring& file)
{

#ifdef _DEBUG
	std::cout  << "\r\n\r\n\r\n\r\n DebugDotnetProgram";
#endif
	//see http://blog.csdn.net/simbi/article/details/3705719
	//see https://msdn.microsoft.com/en-us/library/ms681675%28v=vs.85%29.aspx
#if 1

	//Unhandled Exception: System.TypeLoadException
#define EXCEPTION_TypeLoadException ((DWORD)0xE06D7363)
#endif 

	bool result = true;
#ifdef _DEBUG
	std::cout  <<std::endl << "entry function TryProgramOfDotnet: ";
#endif
	if(!file.empty())
	{
		{
			STARTUPINFO startupInfo;   
			PROCESS_INFORMATION proInfo;   
			startupInfo.cb = sizeof(STARTUPINFO);   
			GetStartupInfo(&startupInfo);   
			startupInfo.wShowWindow = SW_HIDE;  

			startupInfo.dwFlags = STARTF_USESHOWWINDOW |STARTF_USESTDHANDLES;   
			DisableErrorDialog tempDisableErrorDialog;
			if(CreateProcess(NULL, const_cast<LPWSTR>(file.c_str()),NULL,NULL,TRUE,DEBUG_ONLY_THIS_PROCESS,NULL,NULL,&startupInfo,&proInfo))
			{   
#ifdef _DEBUG
				std::cout  <<std::endl << "after CreateProcess ";
#endif
				DEBUG_EVENT de;
				bool runWait = true;
				while (runWait && WaitForDebugEvent(&de,INFINITE)!=0)
				{
#ifdef _DEBUG
					std::cout  <<std::endl << "in WaitForDebugEvent: " <<de.dwDebugEventCode;
#endif
					switch (de.dwDebugEventCode) {
					case CREATE_PROCESS_DEBUG_EVENT:
						::CloseHandle(de.u.CreateProcessInfo.hFile);
						break;
					case LOAD_DLL_DEBUG_EVENT:
						CloseHandle(de.u.LoadDll.hFile);
						break;
					case EXIT_PROCESS_DEBUG_EVENT:
						runWait = FALSE;
						break;
					case EXCEPTION_DEBUG_EVENT:
						switch(de.u.Exception.ExceptionRecord.ExceptionCode)
						{ 
						case EXCEPTION_BREAKPOINT:
#ifdef _DEBUG
							std::cout  <<std::endl << "EXCEPTION_BREAKPOINT";
#endif
							break;
						case EXCEPTION_ACCESS_VIOLATION:
#ifdef _DEBUG
							std::cout  <<std::endl << "EXCEPTION_ACCESS_VIOLATION";
#endif
							result = false;
							TerminateProcess(proInfo.hProcess,1);
							break;
						case EXCEPTION_ARRAY_BOUNDS_EXCEEDED:
						case EXCEPTION_DATATYPE_MISALIGNMENT:
						case EXCEPTION_FLT_DENORMAL_OPERAND:
						case EXCEPTION_FLT_DIVIDE_BY_ZERO:
						case EXCEPTION_FLT_INEXACT_RESULT:
						case EXCEPTION_FLT_INVALID_OPERATION:
						case EXCEPTION_FLT_OVERFLOW:
						case EXCEPTION_FLT_STACK_CHECK:
						case EXCEPTION_FLT_UNDERFLOW:
						case EXCEPTION_ILLEGAL_INSTRUCTION:
						case EXCEPTION_IN_PAGE_ERROR:
						case EXCEPTION_INT_DIVIDE_BY_ZERO:
						case EXCEPTION_INT_OVERFLOW:
						case EXCEPTION_INVALID_DISPOSITION:
						case EXCEPTION_NONCONTINUABLE_EXCEPTION:
						case EXCEPTION_PRIV_INSTRUCTION:
						case EXCEPTION_SINGLE_STEP:
						case EXCEPTION_STACK_OVERFLOW:
#ifdef _DEBUG
							std::cout  <<std::endl << "u.Exception.ExceptionRecord.ExceptionCode:" << de.u.Exception.ExceptionRecord.ExceptionCode;
#endif
							break;
						case EXCEPTION_TypeLoadException:
#ifdef _DEBUG
							std::cout  <<std::endl << "EXCEPTION_TypeLoadException";
#endif
							result = false;
							TerminateProcess(proInfo.hProcess,1);
							break;
						default:
#ifdef _DEBUG
							std::cout  <<std::endl << "default u.Exception.ExceptionRecord.ExceptionCode:" << de.u.Exception.ExceptionRecord.ExceptionCode;
#endif
							break;
						} 
						break;
					default:
#ifdef _DEBUG
						std::cout  <<std::endl << "default dwDebugEventCode:" <<de.dwDebugEventCode;
#endif
						break;
					}

					if(!ContinueDebugEvent(de.dwProcessId,de.dwThreadId,DBG_CONTINUE))
					{
						std::wstring msg;
						GetLastErrorText(msg);
						result = false;
#ifdef _DEBUG
						std::cout  <<std::endl << "there is a inner error!!"; 
						std::wcout << msg.c_str();
#endif
						break;
					}
#ifdef _DEBUG
					std::cout  <<std::endl << "after ContinueDebugEvent"  <<de.dwDebugEventCode;
#endif
				}
				DWORD exitCode = 0;
				if(result && GetExitCodeProcess(proInfo.hProcess, &exitCode))
				{
					result = (exitCode == 0);
				}
				::CloseHandle( proInfo.hProcess );
				::CloseHandle( proInfo.hThread );
				//::CloseHandle( proInfo.hProcess );
			} 
		}
	}else
	{
		result = false;
	}

#ifdef _DEBUG
	std::cout  << "\r\nDebugDotnetProgram\r\n\r\n\r\n\r\n ";
#endif
	return result;
}

bool CValidateDotnet::TryRunDotnetProgram(Parameters parameters, std::vector<VersionInfo> & versions)
{
	bool result = false;
#ifdef _DEBUG
	std::cout  << "\r\n\r\nTryRunDotnetProgram";
#endif
	result = DebugDotnetProgram(parameters.GetCase());

	if (!result)
	{
		{//run in .net 2.0
			VersionInfo * net2 = FindNet(versions, 2.0);
			if (NULL != net2 && net2->isFullInstall)
			{
				std::wstring tempPath = parameters.GetWorkPath();
				FileUtil::PathCombine(tempPath, parameters.GetTempName().c_str());
				FileUtil::MakeDirs(tempPath.c_str());

				std::wstring tempConfig = tempPath;
				std::wstring caseName = parameters.GetCase();
				//create the config file
				FileUtil::PathCombine(tempConfig, FileUtil::GetFileName(caseName.c_str()).c_str());
				tempConfig.append(L".config");

				std::basic_ofstream<wchar_t> ofs(tempConfig.c_str());
				std::locale loc(std::locale(), new std::codecvt_utf8<wchar_t>);
				ofs.imbue(loc);
				{
					std::wstring config = parameters.GetRequiredConfig();
					config = config.replace(config.find(L"{0}"), sizeof(L"{0}")/sizeof(wchar_t) -1, net2->installPathShortName);
					ofs << config;
				}

				ofs.close();
				//copy the case file
				std::wstring tempCase = tempPath;
				FileUtil::PathCombine(tempCase, FileUtil::GetFileName(caseName.c_str()).c_str());
				FileUtil::CopyFile(tempPath.c_str(), caseName.c_str());
				if(CValidateDotnet::DebugDotnetProgram(tempCase))
				{
					net2->canRunCase = true;
				}
			}
		}

		{//run in .net 3.0
			VersionInfo * net3 = FindNet(versions, 3.0);
			if (NULL != net3 && net3->isFullInstall)
			{
				std::wstring tempPath = parameters.GetWorkPath();
				FileUtil::PathCombine(tempPath, parameters.GetTempName().c_str());
				FileUtil::MakeDirs(tempPath.c_str());

				std::wstring tempConfig = tempPath;
				std::wstring caseName = parameters.GetCase();
				//create the config file
				FileUtil::PathCombine(tempConfig, FileUtil::GetFileName(caseName.c_str()).c_str());
				tempConfig.append(L".config");

				std::basic_ofstream<wchar_t> ofs(tempConfig.c_str());
				std::locale loc(std::locale(), new std::codecvt_utf8<wchar_t>);
				ofs.imbue(loc);
				{
					std::wstring config = parameters.GetRequiredConfig();
					//(see: https://msdn.microsoft.com/zh-cn/library/w4atty68%28v=vs.100%29.aspx,  The .NET Framework version 3.0 and 3.5 use version 2.0.50727 of the CLR)
					{
						config = config.replace(config.find(L"{0}"), sizeof(L"{0}")/sizeof(wchar_t) -1, parameters.GetDotnetV2());
					}
					
					ofs << config;
				}

				ofs.close();
				//copy the case file
				std::wstring tempCase = tempPath;
				FileUtil::PathCombine(tempCase, FileUtil::GetFileName(caseName.c_str()).c_str());
				FileUtil::CopyFile(tempPath.c_str(), caseName.c_str());
				if(CValidateDotnet::DebugDotnetProgram(tempCase))
				{
					net3->canRunCase = true;
				}
			}
		}

		{//run in .net 3.5
			VersionInfo * net3_5 = FindNet(versions, 3.5);
			if (NULL != net3_5 && net3_5->isFullInstall)
			{
				std::wstring tempPath = parameters.GetWorkPath();
				FileUtil::PathCombine(tempPath, parameters.GetTempName().c_str());
				FileUtil::MakeDirs(tempPath.c_str());

				std::wstring tempConfig = tempPath;
				std::wstring caseName = parameters.GetCase();
				//create the config file
				FileUtil::PathCombine(tempConfig, FileUtil::GetFileName(caseName.c_str()).c_str());
				tempConfig.append(L".config");

				std::basic_ofstream<wchar_t> ofs(tempConfig.c_str());
				std::locale loc(std::locale(), new std::codecvt_utf8<wchar_t>);
				ofs.imbue(loc);
				{
					std::wstring config = parameters.GetRequiredConfig();
					//(see: https://msdn.microsoft.com/zh-cn/library/w4atty68%28v=vs.100%29.aspx,  The .NET Framework version 3.0 and 3.5 use version 2.0.50727 of the CLR)
					{
						config = config.replace(config.find(L"{0}"), sizeof(L"{0}")/sizeof(wchar_t) -1, parameters.GetDotnetV2());
					}
					ofs << config;
				}

				ofs.close();
				//copy the case file
				std::wstring tempCase = tempPath;
				FileUtil::PathCombine(tempCase, FileUtil::GetFileName(caseName.c_str()).c_str());
				FileUtil::CopyFile(tempPath.c_str(), caseName.c_str());
				if(CValidateDotnet::DebugDotnetProgram(tempCase))
				{
					net3_5->canRunCase = true;
				}
			}
		}

		{//run in .net 4.0
			VersionInfo * net4 = FindNet(versions, 4.0);
			if (NULL != net4 && net4->isFullInstall)
			{
				std::wstring tempPath = parameters.GetWorkPath();
				FileUtil::PathCombine(tempPath, parameters.GetTempName().c_str());
				FileUtil::MakeDirs(tempPath.c_str());

				std::wstring tempConfig = tempPath;
				std::wstring caseName = parameters.GetCase();
				//create the config file
				FileUtil::PathCombine(tempConfig, FileUtil::GetFileName(caseName.c_str()).c_str());
				tempConfig.append(L".config");
				std::basic_ofstream<wchar_t> ofs(tempConfig);
				std::locale loc(std::locale(), new std::codecvt_utf8<wchar_t>);
				ofs.imbue(loc);
				{
					std::wstring config = parameters.GetRequiredConfig();
					config = config.replace(config.find(L"{0}"), sizeof(L"{0}")/sizeof(wchar_t) - 1, parameters.GetDotnetV4());
					ofs << config;
				}

				ofs.close();
				//copy the case file
				std::wstring tempCase = tempPath;
				FileUtil::PathCombine(tempCase, FileUtil::GetFileName(caseName.c_str()).c_str());
				FileUtil::CopyFile(tempPath.c_str(), caseName.c_str());

				if(CValidateDotnet::DebugDotnetProgram(tempCase))
				{
					net4->canRunCase = true;
				}
			}
		}

		//clean the temp 
		{
			std::wstring tempPath = parameters.GetWorkPath();
			FileUtil::PathCombine(tempPath, parameters.GetTempName().c_str());
#ifndef _DEBUG
			FileUtil::RemoveDirOrFile(tempPath.c_str());
#endif
			
		}
	}
#ifdef _DEBUG
	std::cout  << "\r\nTryRunDotnetProgram\r\n\r\n";
#endif
	return result;
}

VersionInfo* CValidateDotnet::FindNet(std::vector<VersionInfo> & versions, double dversion)
{
	const double epsinon = 0.00000001;
	VersionInfo * pversion = NULL;
	{
		for(std::vector<VersionInfo>::iterator it = versions.begin(); it != versions.end(); ++it)
		{
			VersionInfo & temp = *it;
			double x = dversion - temp.dVersion;
			if (x >= -epsinon && x <= epsinon)
			{
				pversion = &temp;
				break;
			}
		}
	}
	return pversion;
}

bool CValidateDotnet::GetLastErrorText(std::wstring & text)
{
	DWORD retSize;
	LPWSTR pTemp=NULL;
	DWORD errCode = GetLastError();
	retSize=FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER|
		FORMAT_MESSAGE_FROM_SYSTEM|
		FORMAT_MESSAGE_ARGUMENT_ARRAY,
		NULL,
		errCode,
		LANG_NEUTRAL,
		(LPTSTR)&pTemp,
		0,
		NULL );
	if (!retSize || pTemp == NULL) {
		text.clear();
	}
	else {
		pTemp[wcslen(pTemp)-2]='\0'; //remove cr and newline character
		text = pTemp;
		LocalFree((HLOCAL)pTemp);
		pTemp = NULL;
	}
	return true;
}

bool CValidateDotnet::TryRepairDotnet(Parameters& pamas, std::vector<VersionInfo>& versions)
{
	bool result = false;

	bool info = pamas.GetInfo();
	if (info)
	{
		std::cout  <<"\r\n\r\nTryRepairDotnet";
	}
	do{

		//if do not install any .net framework, then install the .net framework 4.0
		if(versions.empty())
		{
			if (info)
			{
				std::cout << "\r\nnot install .net 2.0 and 4.0";
			}
			if (SystemUtil::IsWindowsXPSP3OrGreater())// if xp and sp3 or creater, 
			{
				result = TryRepairDotnet4(pamas,NULL);
			}else
			{
				result = TryRepairDotnet2(pamas,NULL);
			}
			
			if(result)//has done, so do not do next step
			{
				break;
			}
		}
		VersionInfo * vnet4 = FindNet(versions, 4.0);
		VersionInfo * vnet2 = FindNet(versions, 2.0);
		
		{//just installed the .net 2.0
			if (NULL != vnet2 && (NULL == vnet4))
			{
				if (info)
				{
					std::cout << "\r\ninstall .net 2.0, not install 4.0";
				}
				result = TryRepairDotnet2(pamas, vnet2);
				if(result)
				{
					break;
				}
			}
		}

		{//just installed the .net 4.0
			if (NULL != vnet4 && (NULL == vnet2))
			{
				if (info)
				{
					std::cout << "\r\ninstall .net 4.0, not install 2.0";
				}
				result = TryRepairDotnet4(pamas, vnet4);
				if(result)
				{
					break;
				}
			}
		}

		{//if install .net 2.0 and 4.0, the both do not work

			if(NULL != vnet2 && !vnet2->canRunCase && NULL != vnet4 && !vnet4->canRunCase)
			{
				if (info)
				{
					std::cout << "\r\ninstall .net 2.0 and 4.0, the both do not work";
				}
				result = TryRepairDotnet4(pamas, vnet4);
				if(result)
				{
					break;
				}
			}
		}

		{//if install .net 2.0 and 4.0, the 4.0 is not work

			if(NULL != vnet2 && vnet2->canRunCase && NULL != vnet4 && !vnet4->canRunCase)
			{
				if (info)
				{
					std::cout << "\r\ninstall .net 2.0 and 4.0, the 4.0 is not work";
				}
				result = TryRepairDotnet4(pamas, vnet4);
				if(result)
				{
					break;
				}
			}
		}

		{//if install .net 2.0 and 4.0, the 2.0 is not work

			if(NULL != vnet2 && !vnet2->canRunCase && NULL != vnet4 && vnet4->canRunCase)
			{
				if (info)
				{
					std::cout << "\r\ninstall .net 2.0 and 4.0, the 2.0 is not work";
				}
				result = TryRepairDotnet2(pamas, vnet4);
				if(result)
				{
					break;
				}
			}
		}

		{//use the repair tool
			
			result = TryUseRepairTool(pamas);
			if(result)
			{
				break;
			}
		}

	}while(false);

	if (info)
	{
		std::cout  <<"TryRepairDotnet\r\n\r\n" << result;
	}
	return result;
}

bool CValidateDotnet::TryRepairDotnet2(Parameters& pamas, const VersionInfo * pversion)
{
	bool result = false;
	bool info = pamas.GetInfo();
	if (info)
	{
		std::cout  <<"\r\n\r\nTryRepairDotnet2";
	}
	
	do 
	{
		////first .net framework 2.0....... the sp2 will install the .net 2.0, so do not do this

		//second, repair or install .net framework 2.0 sp2
		{
			std::wstring spPath = pamas.GetNet2Sp2();
			
			//repair and install sp2
			
			if (this->installNet2sp2 >= this->MAX_TIME)
			{
				if (info)
				{
					std::cout << "\r\n .net framework 2.0 sp2 has been fail installed more than " << MAX_TIME << " times";
				}
				break;
			}

			if(!FileUtil::FileExists(spPath.c_str()))
			{
				if (info)
				{
					std::cout << "\r\n the file .net framework 2.0 sp2 not exit, file:"; 
					std::wcout << spPath.c_str();
				}
				if (!DownloadNet2Sp2(pamas, spPath.c_str()))
				{
					break;
				}
			}

			spPath.append(L" ");
			spPath.append(pamas.GetInstallParamOfNet2Sp2());
			DWORD exitCode = 0;
			bool tresult = FileUtil::RunProcessAndWaitforExit(info,spPath.c_str(),exitCode);
			if(!result)
			{
				result = tresult;
			}
			this->installNet2sp2 += 1;
		}
	} while (false);
	if (info)
	{
		std::cout  <<"TryRepairDotnet2\r\n\r\n";
	} 
	return result;
}

bool CValidateDotnet::TryRepairDotnet4(Parameters& pamas, const VersionInfo* pversion)
{
	bool result = false;
	bool info = pamas.GetInfo();

	if (info)
	{
		std::cout  <<"\r\n\r\nTryRepairDotnet4";
	}
	
	do 
	{
		std::wstring netPath = pamas.GetNet4();
		if (NULL == pversion || !pversion->isFullInstall)//install .net 4.0
		{

			if (this->installNet4 >= this->MAX_TIME)
			{
				if (info)
				{
					std::cout << "\r\n .net framework 4.0 has been fail installed more than " << MAX_TIME << " times";
				}
				break;
			}

			if(!FileUtil::FileExists(netPath.c_str()))
			{
				if (info)
				{
					std::cout << "\r\n the file .net framework 4.0 not exit, file:";
					std::wcout << netPath;
				}

				if (!DownloadNet4(pamas, netPath.c_str()))
				{
					break;
				}
			}

			netPath.append(L" ");
			netPath.append(pamas.GetInstallParamOfNet4());
			DWORD exitCode = 0;
			if (info)
			{
				std::cout << "\r\nrun the  " ;
				std::wcout << netPath;
			}
			result = FileUtil::RunProcessAndWaitforExit(info,netPath.c_str(),exitCode);
			this->installNet4 += 1;
		}else
		{
			//repair .net 4.0
			if (this->repairNet4 >= this->MAX_TIME)
			{
				if (info)
				{
					std::cout << "\r\n .net framework 4.0 has been fail repaired more than " << MAX_TIME << " times";
				}
				break;
			}

			if(!FileUtil::FileExists(netPath.c_str()))
			{
				if (info)
				{
					std::cout << "\r\n the file .net framework 4.0 not exit, file:"; 
					std::wcout << netPath.c_str();
				}

				if (!DownloadNet4(pamas, netPath.c_str()))
				{
					break;
				}
			}
			netPath.append(L" ");
			netPath.append(pamas.GetRepairParamOfNet4());
			DWORD exitCode = 0;
			result = FileUtil::RunProcessAndWaitforExit(info,netPath.c_str(),exitCode);
			this->repairNet4 += 1;
		}

	} while (false);
	if (info)
	{
		std::cout  <<"TryRepairDotnet4\r\n\r\n";
	}
	return result;
}

bool CValidateDotnet::TryUseRepairTool(Parameters pamas)
{
	bool result = false;
	bool info = pamas.GetInfo();
	if (info)
	{
		std::cout  <<"\r\n\r\nTryUseRepairTool";
	}
	do{
		if (this->runRepairTool >= this->MAX_TIME)
		{
			if (info)
			{
				std::cout << "\r\n .net framework 2.0 has been fail installed more than " << MAX_TIME << " times";
			}
			break;
		}
		std::wstring netPath = pamas.GetTool();
		if(!FileUtil::FileExists(netPath.c_str()))
		{
			if (info)
			{
				std::cout << "\r\n the file Repair tool not exit, file:"; 
				std::wcout << netPath.c_str();
			}

			if (!DownloadRepairTool(pamas, netPath.c_str()))
			{
				break;
			}
		}
		DWORD exitCode = 0;
		netPath.append(L" ");
		netPath.append(pamas.GetParamOfRepairTool());
		result = FileUtil::RunProcessAndWaitforExit(info,netPath.c_str(),exitCode);
		this->runRepairTool += 1;
	}while(false);

	if (info)
	{
		std::cout  <<"TryUseRepairTool\r\n\r\n";
	}
	return result;
}

bool CValidateDotnet::DownloadNet2Sp2(Parameters& parameters,const wchar_t * outFile)
{
	if (parameters.GetDownload())
	{
		if (parameters.GetInfo())
		{
			std::cout << "\r\n download .net framework 2.0 sp2 ";
		}
		std::wstring url = parameters.GetUrlNet2Sp2();
		return NetUtlil::HttpGet(url.c_str(), outFile);
	}
	return false;
}


bool CValidateDotnet::DownloadNet4(Parameters& parameters,const wchar_t * outFile)
{
	if (parameters.GetDownload())
	{
		if (parameters.GetInfo())
		{
			std::cout << "\r\n download .net framework 4.0. ";
		}
		std::wstring url = parameters.GetUrlNet4();
		return NetUtlil::HttpGet(url.c_str(), outFile);
	}
	return false;
}

bool CValidateDotnet::DownloadRepairTool(Parameters& parameters,const wchar_t * outFile)
{
	if (parameters.GetDownload())
	{
		if (parameters.GetInfo())
		{
			std::cout << "\r\n download repair tool. ";
		}
		std::wstring url = parameters.GetUrlRepairTool();
		return NetUtlil::HttpGet(url.c_str(), outFile);
	}
	return false;
}
