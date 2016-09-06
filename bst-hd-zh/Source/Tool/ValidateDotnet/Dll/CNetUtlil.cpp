#include "CNetUtlil.h"
#include <cstdlib>
#include <fstream>
#include <windows.h>
#include <wininet.h>
#include <vector>

#include "CFileUtil.h"

NetUtlil::NetUtlil(void)
{
}


NetUtlil::~NetUtlil(void)
{
}

bool NetUtlil::HttpGet(const wchar_t * url, const wchar_t * fileName)
{
	bool result = false;
	HINTERNET hOpen = NULL;
	HINTERNET hFile = NULL;	
	hOpen = InternetOpen(L"t", NULL, NULL, NULL, NULL);
	if(NULL != hOpen)
	{
		hFile = InternetOpenUrl(hOpen, url, NULL, NULL, INTERNET_FLAG_RELOAD | INTERNET_FLAG_DONT_CACHE, NULL);
		if(NULL != hFile) {
			const int BLEN = 1024*1024;
			char* buffer = new char[BLEN];
			DWORD bytesRead = 0;
			std::filebuf outFile;
			outFile.open(fileName, std::ios_base::out | std::ios_base::binary | std::ios_base::trunc);
			if (outFile.is_open())
			{
				bool readSuccess = false;
				do {
					bytesRead = 0;
					readSuccess = (TRUE == InternetReadFile(hFile, buffer, BLEN, &bytesRead));
					if (bytesRead > 0)
					{
						std::streamsize bytesWrite = outFile.sputn(buffer, bytesRead);
						if (bytesWrite != bytesRead)
						{
							readSuccess = false;
							break;
						}
					}
				} while (readSuccess && (bytesRead > 0));
				delete buffer;
				buffer = NULL;
				outFile.close();
				result = readSuccess;
			}
		}
	}
	if (NULL != hOpen)
	{
		InternetCloseHandle(hOpen);
		hOpen = NULL;
	}
	if (NULL != hFile)
	{
		InternetCloseHandle(hFile);
		hFile = NULL;
	}

	if (!result && FileUtil::FileExists(fileName))
	{
		FileUtil::RemoveDirOrFile(fileName);
	}

	return result;
}
