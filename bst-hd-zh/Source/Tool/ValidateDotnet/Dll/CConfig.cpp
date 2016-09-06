#include "CConfig.h"
#include "CFileUtil.h"

#include <boost/property_tree/ptree.hpp> 
#include <boost/property_tree/xml_parser.hpp> 
#include <boost/program_options/detail/utf8_codecvt_facet.hpp>
Config::Config(void)
{
	init = false;
}


Config::~Config(void)
{
}

Config & Config::GetConfig()
{
	static Config instance;
	if (!instance.init)
	{
		std::wstring name;
		std::wstring path;
		FileUtil::GetProcessPathName(path, name);
		if (!path.empty())
		{
			FileUtil::PathCombine(path, L"HD-ValidateDotnetDll-Native.dll.xml");

			if (FileUtil::FileExists(path.c_str()))
			{
				boost::property_tree::wptree xml;
				std::wifstream f(path.c_str());
				std::locale utf8Locale(std::locale(), new boost::program_options::detail::utf8_codecvt_facet());
				f.imbue(utf8Locale); 
				boost::property_tree::read_xml(f, xml);
				std::wstring temp = xml.get<std::wstring>(L"config.url.repairTool");
				if (temp.empty())
				{
					temp = L"http://download.microsoft.com/download/2/B/D/2BDE5459-2225-48B8-830C-AE19CAF038F1/NetFxRepairTool.exe";
				}
				instance.urlRepairTool = temp;

				temp = xml.get<std::wstring>(L"config.url.net4");
				if (temp.empty())
				{
					temp = L"http://go.microsoft.com/fwlink/?linkid=247962";
				}
				instance.urlNet4 = temp;

				temp = xml.get<std::wstring>(L"config.url.net2Sp2x64");
				if (temp.empty())
				{
					temp = L"http://go.microsoft.com/fwlink/?LinkId=259767";
				}
				instance.urlNet2Sp2x64 = temp;

				temp = xml.get<std::wstring>(L"config.url.net2Sp2x86");
				if (temp.empty())
				{
					temp = L"http://go.microsoft.com/fwlink/?LinkId=259766";
				}
				instance.urlNet2Sp2x86 = temp;

				instance.init = true;

			}
		}
	}


	return instance;
}

std::wstring& Config::UrlNet4()
{
	return urlNet4;
}

std::wstring& Config::UrlRepairTool()
{
	return urlRepairTool;
}

std::wstring& Config::UrlNet2Sp2x64()
{
	return urlNet2Sp2x64;
}

std::wstring& Config::UrlNet2Sp2x86()
{
	return urlNet2Sp2x64;
}
