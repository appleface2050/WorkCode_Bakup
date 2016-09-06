#include "CFileUtil.h"
#include <windows.h>
#include <iostream>
#include <boost/filesystem.hpp>

const wchar_t FileUtil::FILE_SEPARATE = L'\\';

FileUtil::FileUtil(void)
{
}


FileUtil::~FileUtil(void)
{
}

std::wstring & FileUtil::PathCombine(std::wstring & path, const wchar_t * path1)
{
	const wchar_t file_win = L'\\';
	const wchar_t file_lin = L'/';

	if (NULL != path1 && std::wcslen(path1) > 0)
	{
		std::size_t oldLen = path.length();
		if(path.empty())
		{
			path = path1;
		}else
		{
			const wchar_t * ptemp = path1;
			for (wchar_t wc = ptemp[0]; (file_win == wc || file_lin == wc);  ++ptemp)
			{

			}

			if (std::wcslen(ptemp) > 0)
			{
				path.push_back(FileUtil::FILE_SEPARATE);
				path.append(ptemp);
			}
		}

		if(oldLen != path.length())
		{
			wchar_t t = path[path.length()-1];
			if(file_win == t || file_lin == t)
			{
				path = path.substr(0, path.length() -1);
			}
		}
	}
	return path;
}

std::wstring & FileUtil::PathCombine(std::wstring & path, const wchar_t * path1, const wchar_t * path2)
{
	PathCombine(path,path1);
	PathCombine(path,path2);
	return path;
}

bool FileUtil::RunProcessAndWaitforExit(bool info, const wchar_t * cmdLine, DWORD & exitCode, DWORD dwMilliseconds /*= INFINITE*/)
{
	bool result = false;
	STARTUPINFO startupInfo; 
	PROCESS_INFORMATION proInfo;   
	memset(&startupInfo,0x0, sizeof(STARTUPINFO));
	memset(&proInfo,0x0, sizeof(PROCESS_INFORMATION));
	startupInfo.cb = sizeof(STARTUPINFO);   
	//GetStartupInfo(&startupInfo); 
	
	wchar_t * cmdLineParam = _wcsdup(cmdLine);
	if(CreateProcess(NULL, cmdLineParam,NULL,NULL,FALSE,0,NULL,NULL,&startupInfo,&proInfo))
	{   
		::WaitForSingleObject(proInfo.hProcess,dwMilliseconds);
		GetExitCodeProcess(proInfo.hProcess, &exitCode);
		::CloseHandle( proInfo.hProcess );
		::CloseHandle( proInfo.hThread );
		result = true;
		if (info)
		{
			std::wcout << "\r\n run cmd: " << cmdLine;
			std::cout << "\r\n run ExitCode: " << exitCode;
		}
	}else
	{
		if (info)
		{
			std::wcout << "\r\n run fail: " << cmdLine;
		}
	}

	free(cmdLineParam);
	cmdLineParam = NULL;
	return result;

}

bool FileUtil::FileExists(const wchar_t * fileName)
{
	struct _stat64i32 buf;
	return (_wstat(fileName, &buf) != -1);
}

bool FileUtil::MakeDirs(const wchar_t * dirs)
{
	bool result = false;
	if (boost::filesystem::exists(dirs))
	{
		result = true;
	}else
	{
		boost::filesystem::path p(dirs);
		result = boost::filesystem::create_directories(p);
	}

	return result;
}

bool FileUtil::RemoveDirOrFile(const wchar_t * dirFile)
{
	bool result = false;
	boost::filesystem::path p(dirFile);
	if (boost::filesystem::exists(p))
	{
		if (boost::filesystem::is_directory(p))
		{
			boost::system::error_code err;
			boost::filesystem::remove_all(p, err);
			err.clear();
		}else
		{
			boost::filesystem::remove(p);
		}
	}else{
		result = true;
	}
	return result;
}

bool FileUtil::CopyFile(const wchar_t * destFile, const wchar_t * srcFile)
{
	bool result = false;\
	boost::filesystem::path fromFile(srcFile);
	boost::filesystem::path tofile(destFile);
	if (boost::filesystem::exists(fromFile))
	{
		
		if(!boost::filesystem::exists(tofile))
		{
			boost::filesystem::create_directories(tofile.parent_path());
		}else
		{
			if (boost::filesystem::is_directory(tofile))
			{
				tofile /=fromFile.filename().wstring();
				if (boost::filesystem::exists(tofile))
				{
					boost::system::error_code erro;
					boost::filesystem::remove(tofile, erro);
					erro.clear();
				}
			}else
			{
				boost::filesystem::remove(tofile);
			}
		}
		boost::system::error_code erro;
		boost::filesystem::copy_file(fromFile, tofile,erro);
		result = boost::filesystem::is_regular_file(tofile);
	}

	return result;
}

std::wstring FileUtil::GetFileName(const wchar_t * fullFile)
{
	boost::filesystem::path p(fullFile);
	return p.filename().c_str();
}

bool FileUtil::GetProcessPathName(std::wstring& path, std::wstring& name)
{
	bool result = false;
	path.clear();
	name.clear();
	wchar_t pathName[MAX_PATH]= {0};  
	if (GetModuleFileName(NULL, pathName, MAX_PATH) != 0) 
	{
		boost::filesystem::path p(pathName);
		path = p.parent_path().wstring();
		name = p.filename().wstring();
		result = true;
	}
	return result;
}

