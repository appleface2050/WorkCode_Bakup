#ifndef TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CFILEUTIL_H
#define TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CFILEUTIL_H

#include <Windows.h>
#include <string>


class FileUtil
{
private:
	FileUtil(void);
	~FileUtil(void);

public:
	const static wchar_t FILE_SEPARATE;
public:

	//************************************
	// Method:    PathCombine
	// FullName:  FileUtil::PathCombine
	// Access:    public static 
	// Returns:   void
	// Qualifier: ("c:\test", "file.txt")  ==> "c:\text\file.txt"
	// Parameter: std::wstring & path
	// Parameter: const wchar_t * path1
	//************************************
	static std::wstring & PathCombine(std::wstring & path, const wchar_t * path1);

	static std::wstring & PathCombine(std::wstring & path, const wchar_t * path1, const wchar_t * path2);


	//
	static bool RunProcessAndWaitforExit(bool info, const wchar_t * cmdLine,DWORD & exitCode, DWORD dwMilliseconds = INFINITE);

	static bool FileExists(const wchar_t * fileName);

	static bool MakeDirs(const wchar_t * dirs);

	static bool RemoveDirOrFile(const wchar_t * dirFile);

	static bool CopyFile(const wchar_t * destFile, const wchar_t * srcFile); 

	static std::wstring GetFileName(const wchar_t * fullFile);

	static bool GetProcessPathName(std::wstring& path, std::wstring& name);

};

#endif //TOOL_VALIDATEDOT_DLL_VALIDATEDOTNETDLL_CFILEUTIL_H
