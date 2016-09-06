
#include "CMultiData.h"
#include "CFileUtil.h"
#include "CSystemUtil.h"
#include "CConfig.h"

const wchar_t * Parameters::HELP    = L"help";
const wchar_t * Parameters::REG     = L"reg";
const wchar_t * Parameters::REPAIR  = L"repair";
const wchar_t * Parameters::WORKPATH= L"workpath";
const wchar_t * Parameters::CASE    = L"case";
const wchar_t * Parameters::TOOL    = L"tool";
const wchar_t * Parameters::NET2SP2 = L"net2sp2";
const wchar_t * Parameters::NET4    = L"net4";
const wchar_t * Parameters::INFO    = L"info";
const wchar_t * Parameters::MODE    = L"mode";
const wchar_t * Parameters::DOWNLOAD= L"download";


void Parameters::OutputHelp()
{
	std::cout << "\r\nValidate the App Player whether can run in current .net framework \r\n\r\n";

	std::cout << "Parameters\r\n";
	std::cout << "   help     : show the parameters information\r\n";
	std::cout << "   reg      : write the result into registry, the item is HKEY_CURRENT_USER\\SOFTWARE\\BlueStacks [ValidateDotnet] \r\n";
	std::cout << "   repair   : repair .net framework. default do not repair .net framework.\r\n";
	std::cout << "   download : if need, download .net framework from internet. default do not download\r\n";
	//std::cout << "   workpath : work path, default value is path of current process path\r\n";
	//std::cout << "   case     : sample code of App Player, default value is {workpath}\\data\\HD-CaseDotnet.exe\r\n";
	//std::cout << "   tool     : file(full path) of repair tool, default value is {workpath}\\data\\NetFxRepairTool.exe\r\n";
	//std::cout << "   net2sp2  : .net2 sp2 install package, default value is {workpath}\\data\NetFx20SP2_x86.exe\r\n";
	//std::cout << "   net4     : .net4 install package, default value is {workpath}\\data\\dotNetFx40_Full_x86_x64.exe\r\n";

	std::cout << "   info     : out put info to console.\r\n";
	//std::cout << "  mode     : value 0 1 2 3, default value is 0. 1: check the registry,not run the case 2: not check registry,run the case, 3, check the registry and run the case, 0: same as 3\r\n";

	
	////////////////////
	std::cout << "\r\nResult\r\n";
	std::cout << "   0x0    : success \r\n";
	std::cout << "   0x1    : not installed the .net 2.0\r\n";
	std::cout << "   0x2    : not installed the .net sp2\r\n";
	std::cout << "   0x4    : .net 2.0 is not validate\r\n";
	std::cout << "   0x10   : installed the .net4 client \r\n";
	std::cout << "   0x20   : not installed the .net4(4.0,4.5,<5.0) full\r\n";
	std::cout << "   0x40   : .net 4.0(4.0,4.5,<5.0) is not validate\r\n";
	std::cout << "   0x2000 : fail run the case\r\n";
	std::cout << "   0x4000 : fail repair\r\n";

	////
	std::cout << "\r\nSample1, show help info\r\n";
	std::cout << "   HD-ValidateDotnetExe-Native.exe help\r\n";
	std::cout << "\r\nSample2, validate whether can run App Player\r\n";
	std::cout << "   HD-ValidateDotnetExe-Native.exe\r\n";
	std::cout << "\r\nSample1, validate,repair and download .net framework(if need)\r\n";
	std::cout << "   HD-ValidateDotnetExe-Native.exe repair download\r\n";

}

Parameters::ParameterData & Parameters::GetParameterMap()
{
	return paMap;
}

void Parameters::AddParamter(const wchar_t * key, wchar_t * value)
{
	if(NULL != key && std::wcslen(key) > 0)
	{
		std::wstring temp = key;
		if(L'-' == temp[0])
		{
			temp = temp.substr(1);
		}

		paMap[temp] = value;
	}
}

void Parameters::AddParamter(const wchar_t * keyValue)
{
	if(NULL != keyValue && std::wcslen(keyValue) > 0)
	{
		std::wstring temp = keyValue;
		if(L'-' == temp[0])
		{
			temp = temp.substr(1);
		}
		if(!temp.empty())
		{
			std::wstring key = temp;
			std::wstring value;
			std::size_t index = temp.find(L"=");
			if(index !=  std::wstring::npos)
			{
				key = temp.substr(0,index);
				if(index + 1 < temp.length())
				{
					value = temp.substr(index + 1);
				}
			}
			paMap[key] = value;
		}
	}
}

void Parameters::AddParamters(const wchar_t * keyValues[])
{
	//const wchar_t ** keyValue = keyValues;
	for (const wchar_t ** keyValue = keyValues; NULL != keyValue; ++keyValue)
	{
		AddParamter(keyValue[0]);
	}
}

bool Parameters::GetReg()
{
	bool result = false;
	if(paMap.end() != paMap.find(Parameters::REG))
	{
		result = true;
	}
	return result;
}

std::wstring Parameters::GetCase()
{
	std::wstring path;
	ParameterData::iterator m = paMap.find(Parameters::CASE);
	if(paMap.end() != m && !m->second.empty())
	{
		path = m->second;
	}
	if (path.empty())
	{
		path = GetWorkPath();
		FileUtil::PathCombine(path,GetDataName(), L"HD-CaseDotnet.exe");
	}

	return path;
}

std::wstring Parameters::GetNet4()
{
	std::wstring path;
	ParameterData::iterator m = paMap.find(Parameters::NET4);
	if(paMap.end() != m && !m->second.empty())
	{
		path = m->second;
	}
	if (path.empty())
	{
		path = GetWorkPath();
		FileUtil::PathCombine(path,GetDataName(), L"dotNetFx40_Full_x86_x64.exe");
	}
	return path;
}

std::wstring Parameters::GetNet2Sp2()
{
	std::wstring path;
	ParameterData::iterator m = paMap.find(Parameters::NET2SP2);
	if(paMap.end() != m && !m->second.empty())
	{
		path = m->second;
	}
	if (path.empty())
	{
		path = GetWorkPath();
		FileUtil::PathCombine(path,GetDataName(), L"NetFx20SP2_x86.exe");
	}
	return path;
}

std::wstring Parameters::GetTool()
{
	std::wstring path;
	ParameterData::iterator m = paMap.find(Parameters::REPAIR);
	if(paMap.end() != m && !m->second.empty())
	{
		path = m->second;
	}
	if (path.empty())
	{
		path = GetWorkPath();
		FileUtil::PathCombine(path,GetDataName(), L"NetFxRepairTool.exe");
	}
	return path;
}

int Parameters::GetMode()
{
	int mode = 0;
	ParameterData::iterator m = paMap.find(Parameters::REG);
	if(paMap.end() != m && !m->second.empty())
	{
		wchar_t * pend = NULL;
		mode = std::wcstol(m->second.c_str(), &pend,10);
	}
	return mode;
}

const std::wstring Parameters::GetWorkPath()
{
	std::wstring path;
	ParameterData::iterator m = paMap.find(Parameters::WORKPATH);
	if(paMap.end() != m && !m->second.empty())
	{
		path = m->second;
	}

	if (path.empty())
	{
		std::wstring name;
		FileUtil::GetProcessPathName(path, name);
		if (!path.empty())
		{
			paMap[Parameters::WORKPATH] = path;
		}
	}

	return path;
}

bool Parameters::GetRepair()
{
	bool result = false;
	if(paMap.end() != paMap.find(Parameters::REPAIR))
	{
		result = true;
	}
	return result;
}

bool Parameters::GetInfo() const
{
	bool result = false;
	if(paMap.end() != paMap.find(Parameters::INFO))
	{
		result = true;
	}
	return result;
}

bool Parameters::GetHelp()
{
	bool result = false;
	if(paMap.end() != paMap.find(Parameters::HELP))
	{
		result = true;
	}
	return result;
}

bool Parameters::GetDownload() const
{
	bool result = false;
	if(paMap.end() != paMap.find(Parameters::DOWNLOAD))
	{
		result = true;
	}
	return result;
}

std::wstring Parameters::GetUrlNet4() const
{
	std::wstring file = Config::GetConfig().UrlNet4();
	if (file.empty())
	{
		file = L"http://go.microsoft.com/fwlink/?linkid=247962";
	}
	return file;
}

std::wstring Parameters::GetUrlRepairTool()
{
	std::wstring file = Config::GetConfig().UrlRepairTool();
	if (file.empty())
	{
		file = L"http://download.microsoft.com/download/2/B/D/2BDE5459-2225-48B8-830C-AE19CAF038F1/NetFxRepairTool.exe";
	}
	return file;
}

std::wstring Parameters::GetUrlNet2Sp2() const
{
	if (SystemUtil::IsSystem64())
	{
		return this->GetUrlNet2Sp2x64();
	}else
	{
		return this->GetUrlNet2Sp2x86();
	}
}

std::wstring Parameters::GetUrlNet2Sp2x64() const
{
	std::wstring file = Config::GetConfig().UrlNet2Sp2x64();
	if (file.empty())
	{
		file = L"http://go.microsoft.com/fwlink/?LinkId=259767";
	}
	return file;
}

std::wstring Parameters::GetUrlNet2Sp2x86() const
{
	std::wstring file = Config::GetConfig().UrlNet2Sp2x86();
	if (file.empty())
	{
		file = L"http://go.microsoft.com/fwlink/?LinkId=259766";
	}
	return file;
}
