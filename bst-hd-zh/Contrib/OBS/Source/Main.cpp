/********************************************************************************
 Copyright (C) 2012 Hugh Bailey <obs.jim@gmail.com>

 This program is free software; you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation; either version 2 of the License, or
 (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program; if not, write to the Free Software
 Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307, USA.
********************************************************************************/


#include "Main.h"

#include <shellapi.h>
#include <shlobj.h>
#include <dwmapi.h>

#include <intrin.h>
#include <inttypes.h>

#include <gdiplus.h>

#include <string.h>
#include "Server.h"

//----------------------------

HWND        hwndMain        = NULL;
HWND        hwndRenderFrame = NULL;
HWND        hwndLogWindow   = NULL;
HWND        hwndLog         = NULL;
HINSTANCE   hinstMain       = NULL;
ConfigFile  *GlobalConfig   = NULL;
ConfigFile  *AppConfig      = NULL;
OBS         *App            = NULL;
bool        bIsPortable     = false;
bool        bStreamOnStart  = false;
LPWSTR		oemName			= NULL;
TCHAR       lpAppPath[MAX_PATH];
TCHAR       lpAppDataPath[MAX_PATH];
bool        bDebugMode = false;
bool        stopOBS = false;
int			serverPort = -1;

//----------------------------

void OBSLog(const char *log, ...)
{

    FILE* file = fopen("OBS.log", "a+");
    va_list ap;
    va_start(ap, log);

    time_t ltime;
    ltime = time(NULL);
    struct tm *tm;
    tm = localtime(&ltime);
    fprintf(file, "%02d:%02d:%02d %02d-%02d-%04d: ",
            tm->tm_hour,
            tm->tm_min,
            tm->tm_sec,
            tm->tm_mday,
            tm->tm_mon,
            tm->tm_year+1900
            );

    vfprintf(file, log, ap);
    fclose(file);
    va_end(ap);
}

void OBSLog(const wchar_t *log, ...)
{
    FILE* file = fopen("OBS.log", "a+");
    va_list ap;
    va_start(ap, log);

    time_t ltime;
    ltime = time(NULL);
    struct tm *tm;
    tm = localtime(&ltime);
    fwprintf(file, L"%02d:%02d:%02d %02d-%02d-%04d: ",
            tm->tm_hour,
            tm->tm_min,
            tm->tm_sec,
            tm->tm_mday,
            tm->tm_mon + 1,
            tm->tm_year + 1900
            );

    vfwprintf(file, log, ap);
    fclose(file);
    va_end(ap);
}

void InitSockets();
void TerminateSockets();

void LogVideoCardStats();

HANDLE hOBSMutex = NULL;

BOOL LoadSeDebugPrivilege()
{
    DWORD   err;
    HANDLE  hToken;
    LUID    Val;
    TOKEN_PRIVILEGES tp;

    if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken))
    {
        err = GetLastError();
        return FALSE;
    }

    if (!LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &Val))
    {
        err = GetLastError();
        CloseHandle(hToken);
        return FALSE;
    }

    tp.PrivilegeCount = 1;
    tp.Privileges[0].Luid = Val;
    tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

    if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof (tp), NULL, NULL))
    {
        err = GetLastError();
        CloseHandle(hToken);
        return FALSE;
    }

    CloseHandle(hToken);

    return TRUE;
}

static void LogModule(CTSTR lpModuleName, HMODULE addr)
{
#ifdef _WIN64
    Log(TEXT("%016X %s"), addr, lpModuleName);
#else
    Log(TEXT("%08X         %s"), addr, lpModuleName);
#endif
}

void LogSystemStats()
{
    HKEY key;
    TCHAR data[1024];
    DWORD dwSize, dwSpeed;

    zero(data, 1024);

    if(RegOpenKey(HKEY_LOCAL_MACHINE, TEXT("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0"), &key) != ERROR_SUCCESS)
    {
        AppWarning(TEXT("Could not open system information registry key"));
        return;
    }

#ifdef _WIN64
    Log(TEXT("%s - 64bit (　^ω^)"), OBS_VERSION_STRING);
#else
    Log(TEXT("%s - 32bit (´・ω・｀)"), OBS_VERSION_STRING);
#endif

    Log(TEXT("-------------------------------"));

    dwSize = 1024;
    RegQueryValueEx(key, TEXT("ProcessorNameString"), NULL, NULL, (LPBYTE)data, &dwSize);
    Log(TEXT("CPU Name: %s"), sfix(data));

    dwSize = 4;
    RegQueryValueEx(key, TEXT("~MHz"), NULL, NULL, (LPBYTE)&dwSpeed, &dwSize);
    Log(TEXT("CPU Speed: %dMHz"), dwSpeed);

    RegCloseKey(key);

    MEMORYSTATUS ms;
    GlobalMemoryStatus(&ms);

    Log(TEXT("Physical Memory:  %ldMB Total, %ldMB Free"), (ms.dwTotalPhys/1048576), (ms.dwAvailPhys/1048576));

    int cpuInfo[4];
    __cpuid(cpuInfo, 1);
    BYTE cpuSteppingID  = cpuInfo[0] & 0xF;
    BYTE cpuModel       = ((cpuInfo[0]>>4) & 0xF) + ((cpuInfo[0]>>12) & 0xF0);
    BYTE cpuFamily      = ((cpuInfo[0]>>8) & 0xF) + ((cpuInfo[0]>>20) & 0xFF);
    BYTE cpuType        = (cpuInfo[0]>>12) & 0x3;
    BYTE cpuExtModel    = (cpuInfo[0]>>17) & 0xF;
    BYTE cpuExtFamily   = (cpuInfo[0]>>21) & 0xFF;

    BYTE cpuHTT         = (cpuInfo[3]>>28) & 1;

    Log(TEXT("stepping id: %u, model %u, family %u, type %u, extmodel %u, extfamily %u, HTT %u, logical cores %u, total cores %u"), cpuSteppingID, cpuModel, cpuFamily, cpuType, cpuExtModel, cpuExtFamily, cpuHTT, OSGetLogicalCores(), OSGetTotalCores());

    for(UINT i=0; i<App->NumMonitors(); i++)
    {
        const MonitorInfo &info = App->GetMonitor(i);
        Log(TEXT("monitor %u: pos={%d, %d}, size={%d, %d}"), i+1, info.rect.left, info.rect.top, info.rect.right-info.rect.left, info.rect.bottom-info.rect.top);
    }

    OSVERSIONINFO osvi;
    osvi.dwOSVersionInfoSize = sizeof(osvi);
    GetVersionEx(&osvi);
    Log(TEXT("Windows Version: %u.%u Build %u %S"), osvi.dwMajorVersion, osvi.dwMinorVersion, osvi.dwBuildNumber, osvi.szCSDVersion);

    BOOL bComposition;
    DwmIsCompositionEnabled(&bComposition);
    Log(TEXT("Aero is %s"), bComposition ? TEXT("Enabled") : TEXT("Disabled"));

    HMODULE hOBS    = GetModuleHandle(NULL);
    HMODULE hOBSApi = GetModuleHandle(TEXT("OBSApi"));

    Log(TEXT("-------------------------------"));
    Log(TEXT("OBS Modules:"));
    Log(TEXT("Base Address     Module"));

    LogModule(TEXT("OBS.exe"), hOBS);
    LogModule(TEXT("OBSApi.dll"), hOBSApi);

    for (UINT i=0; i<App->NumPlugins(); i++) {
        const PluginInfo *info = App->GetPluginInfo(i);
        LogModule(info->strFile, info->hModule);
    }

    LogVideoCardStats();
}

void InvertPre47Scenes()
{
    String strScenesPath;
    strScenesPath << lpAppDataPath << TEXT("\\scenes.xconfig");

    XConfig scenesConfig;
    if(scenesConfig.Open(strScenesPath))
    {
        XElement *scenes = scenesConfig.GetElement(TEXT("scenes"));
        if(!scenes)
            return;

        UINT numScenes = scenes->NumElements();
        for(UINT i=0; i<numScenes; i++)
        {
            XElement *scene = scenes->GetElementByID(i);
            XElement *sources = scene->GetElement(TEXT("sources"));
            if(!sources)
                continue;

            sources->ReverseOrder();
        }

        scenesConfig.Close(true);
    }
}

String FindSceneCollection(String scenecollection)
{
    String result = FormattedString(L"%s\\sceneCollection\\%s.xconfig", lpAppDataPath, scenecollection.Array());
    if (OSFileExists(result))
        return result;

    return String();
}

void SetupSceneCollection(CTSTR scenecollection)
{
    String strSceneCollection = scenecollection ? scenecollection : GlobalConfig->GetString(TEXT("General"), TEXT("SceneCollection"));
    String strXconfig;

    if (scenecollection)
        GlobalConfig->SetString(TEXT("General"), TEXT("SceneCollection"), scenecollection);

    if (!strSceneCollection.IsValid() || FindSceneCollection(strSceneCollection).IsEmpty())
    {
        OSFindData ofd;

        strXconfig.Clear() << lpAppDataPath << TEXT("\\sceneCollection\\*.xconfig");
        HANDLE hFind = OSFindFirstFile(strXconfig, ofd);
        if (hFind)
        {
            do
            {
                if (ofd.bDirectory) continue;

                strSceneCollection = GetPathWithoutExtension(ofd.fileName);
                GlobalConfig->SetString(TEXT("General"), TEXT("SceneCollection"), strSceneCollection);
                break;

            } while (OSFindNextFile(hFind, ofd));
            OSFindClose(hFind);
        }

        if (strSceneCollection.IsEmpty())
        {
            CopyFile(String() << lpAppDataPath << L"\\scenes.xconfig", String() << lpAppDataPath << L"\\sceneCollection\\scenes.xconfig", true);
            strSceneCollection = L"scenes";
            GlobalConfig->SetString(L"General", L"SceneCollection", strSceneCollection);
        }
    }
}

void SetupIni(CTSTR profile)
{
    //first, find out which profile we're using

    String strProfile = profile ? profile : GlobalConfig->GetString(TEXT("General"), TEXT("Profile"));
    DWORD lastVersion = GlobalConfig->GetInt(TEXT("General"), TEXT("LastAppVersion"));
    String strIni;

    if (profile)
        GlobalConfig->SetString(TEXT("General"), TEXT("Profile"), profile);

    //--------------------------------------------
    // 0.47a fix (invert sources in all scenes)

    if(lastVersion < 0x470)
        InvertPre47Scenes();

    //--------------------------------------------
    // try to find and open the file, otherwise use the first one available

    bool bFoundProfile = false;

    if(strProfile.IsValid())
    {
        strIni << lpAppDataPath << TEXT("\\profiles\\") << strProfile << TEXT(".ini");
        bFoundProfile = OSFileExists(strIni) != 0;
    }

    if(!bFoundProfile)
    {
        OSFindData ofd;

        strIni.Clear() << lpAppDataPath << TEXT("\\profiles\\*.ini");
        HANDLE hFind = OSFindFirstFile(strIni, ofd);
        if(hFind)
        {
            do
            {
                if(ofd.bDirectory) continue;

                strProfile = GetPathWithoutExtension(ofd.fileName);
                GlobalConfig->SetString(TEXT("General"), TEXT("Profile"), strProfile);
                bFoundProfile = true;

                break;

            } while(OSFindNextFile(hFind, ofd));

            OSFindClose(hFind);
        }
    }

    //--------------------------------------------
    // open, or if no profile found, create one

    if(bFoundProfile)
    {
        strIni.Clear() << lpAppDataPath << TEXT("\\profiles\\") << strProfile << TEXT(".ini");

        if(AppConfig->Open(strIni))
            return;
    }

    strProfile = TEXT("Untitled");
    GlobalConfig->SetString(TEXT("General"), TEXT("Profile"), strProfile);

    strIni.Clear() << lpAppDataPath << TEXT("\\profiles\\") << strProfile << TEXT(".ini");

    if(!AppConfig->Create(strIni))
        CrashError(TEXT("Could not create '%s'"), strIni.Array());

    AppConfig->SetString(TEXT("Audio"),          TEXT("Device"),        TEXT("Default"));
    AppConfig->SetFloat (TEXT("Audio"),          TEXT("MicVolume"),     1.0f);
    AppConfig->SetFloat (TEXT("Audio"),          TEXT("DesktopVolume"), 1.0f);

    AppConfig->SetInt   (TEXT("Video"),          TEXT("Monitor"),       0);
    AppConfig->SetInt   (TEXT("Video"),          TEXT("FPS"),           30);
    AppConfig->SetFloat (TEXT("Video"),          TEXT("Downscale"),     1.0f);
    AppConfig->SetInt   (TEXT("Video"),          TEXT("DisableAero"),   0);

    AppConfig->SetInt   (TEXT("Video Encoding"), TEXT("BufferSize"),    1000);
    AppConfig->SetInt   (TEXT("Video Encoding"), TEXT("MaxBitrate"),    1000);
    AppConfig->SetString(TEXT("Video Encoding"), TEXT("Preset"),        TEXT("veryfast"));
    AppConfig->SetInt   (TEXT("Video Encoding"), TEXT("Quality"),       8);

    AppConfig->SetInt   (TEXT("Audio Encoding"), TEXT("Format"),        1);
    AppConfig->SetString(TEXT("Audio Encoding"), TEXT("Bitrate"),       TEXT("128"));
    AppConfig->SetInt   (TEXT("Audio Encoding"), TEXT("isStereo"),      1);

    AppConfig->SetInt   (TEXT("Publish"),        TEXT("Service"),       0);
    AppConfig->SetInt   (TEXT("Publish"),        TEXT("Mode"),          0);
};

void LoadGlobalIni()
{
    GlobalConfig = new ConfigFile;

    String strGlobalIni;
    strGlobalIni << lpAppDataPath << TEXT("\\global.ini");

    if(!GlobalConfig->Open(strGlobalIni))
    {
        if(!GlobalConfig->Create(strGlobalIni))
            CrashError(TEXT("Could not create '%s'"), strGlobalIni.Array());

        //----------------------
        // first, try to set the app to the system language, defaulting to english if the language doesn't exist

        DWORD bufSize = GetLocaleInfoEx(LOCALE_NAME_USER_DEFAULT, LOCALE_SISO639LANGNAME, NULL, 0);

        String str639Lang;
        str639Lang.SetLength(bufSize);

        GetLocaleInfoEx(LOCALE_NAME_USER_DEFAULT, LOCALE_SISO639LANGNAME, str639Lang.Array(), bufSize+1);

        String strLangFile;
        strLangFile << TEXT("locale/") << str639Lang << TEXT(".txt");

        if(!OSFileExists(strLangFile))
            str639Lang = TEXT("en");

        //----------------------

        GlobalConfig->SetString(TEXT("General"), TEXT("Language"), str639Lang);
        GlobalConfig->SetInt(TEXT("General"), TEXT("MaxLogs"), 20);
    }
}

void WINAPI ProcessEvents()
{
    MSG msg;
    while(PeekMessage(&msg, NULL, 0, 0, 1))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
}

int HasSSE2Support ()
{
    int cpuInfo[4];

    __cpuid(cpuInfo, 1);

    return (cpuInfo[3] & (1<<26)) != 0;
}

typedef BOOL (WINAPI *getUserModeExceptionProc)(LPDWORD);
typedef BOOL (WINAPI *setUserModeExceptionProc)(DWORD);

void InitializeExceptionHandler()
{
    HMODULE k32;

    //standard app-wide unhandled exception filter
    SetUnhandledExceptionFilter(OBSExceptionHandler);

    //fix for exceptions being swallowed inside callbacks (see KB976038)
    k32 = GetModuleHandle(TEXT("KERNEL32"));
    if (k32)
    {
        DWORD dwFlags;
        getUserModeExceptionProc procGetProcessUserModeExceptionPolicy;
        setUserModeExceptionProc procSetProcessUserModeExceptionPolicy;

        procGetProcessUserModeExceptionPolicy = (getUserModeExceptionProc)GetProcAddress(k32, "GetProcessUserModeExceptionPolicy");
        procSetProcessUserModeExceptionPolicy = (setUserModeExceptionProc)GetProcAddress(k32, "SetProcessUserModeExceptionPolicy");

        if (procGetProcessUserModeExceptionPolicy && procSetProcessUserModeExceptionPolicy)
        {
            if (procGetProcessUserModeExceptionPolicy(&dwFlags))
                procSetProcessUserModeExceptionPolicy(dwFlags & ~1);
        }
    }
}

void SetWorkingFolder(void)
{
    String modulePath;

    if (GetFileAttributes(TEXT("locale\\en.txt")) != INVALID_FILE_ATTRIBUTES)
        return;

    modulePath.SetLength(MAX_PATH);

    if (GetModuleFileName(NULL, modulePath, modulePath.Length()-1))
    {
        TCHAR *p;

        p = srchr(modulePath, '\\');
        if (p)
            *p = 0;

        SetCurrentDirectory(modulePath);
    }
}

int AddHttpPath(HANDLE hReqQueue, wchar_t* path)
{
	int retCode = -1;
	int basePort = 2891;
	int endPort = 2900;
	if (serverPort != -1)
	{
		basePort = endPort = serverPort;
	}
	for (int port = basePort; port <= endPort; ++port)
	{
		wchar_t* url = new wchar_t[80];
		wchar_t* baseUrl = new wchar_t[80];
		wcscpy(baseUrl, L"http://localhost:");
		wchar_t* portChar = new wchar_t[10];
		swprintf(portChar, 10, L"%d/", port);
		baseUrl = wcscat(baseUrl, portChar);
		url = wcscpy(url, baseUrl);
		url = wcscat(url, path);
		OBSLog(L"listening for requests on the following url: %s\n", url);

		int retCode = HttpAddUrl(
			hReqQueue,
			url,
			NULL
			);

		if (retCode == NO_ERROR)
		{
			OBSLog(L"AddHttpPath for %ws success with %lu \n", path, retCode);

			if (basePort != endPort)
			{
				serverPort = port;
				HKEY key;
				wchar_t* keyPath = new wchar_t[256];
				wcscpy(keyPath, L"Software\\BlueStacks");
				if (oemName != NULL)
				{
					keyPath = wcscat(keyPath, oemName);
				}
				keyPath = wcscat(keyPath, L"\\BluestacksGamemanager\\Config");
				LONG retval = RegOpenKeyEx(HKEY_LOCAL_MACHINE,
					keyPath,
					0,
					KEY_WRITE,
					&key);
				if (ERROR_SUCCESS == retval)
				{
					DWORD value = serverPort;
					LONG writeStatus = RegSetValueEx(key,
						TEXT("OBSServerPort"),
						0,
						REG_DWORD,
						(const BYTE*)&value,
						sizeof(value));
					if (ERROR_SUCCESS == writeStatus)
					{
						OBSLog(L"Successfully written OBSServerPort in registry\n");
					}
					else
					{
						OBSLog(L"Failed to write OBSServerPort in registry\n");
					}
					RegCloseKey(key);
				}
			}
			break;
		}

		if (port == 2900)
		{
			OBSLog(L"AddHttpPath for %ws failed with %lu \n", path, retCode);
		}
	}
    return retCode;
}

void SetupHTTPServer()
{
    int retCode;

    HANDLE          hReqQueue      = NULL;

    //
    // Initialize HTTP Server APIs
    //
    HTTPAPI_VERSION HttpApiVersion = HTTPAPI_VERSION_1;
    retCode = HttpInitialize(
                HttpApiVersion,
                HTTP_INITIALIZE_SERVER,    // Flags
                NULL                       // Reserved
                );

    if (retCode != NO_ERROR)
    {
        OBSLog(L"HttpInitialize failed with %lu \n", retCode);
        SendMessage(hwndMain, WM_CLOSE, 0, 0);
        return;
    }

    //
    // Create a Request Queue Handle
    //
    retCode = HttpCreateHttpHandle(
                &hReqQueue,        // Req Queue
                0                  // Reserved
                );

    if (retCode != NO_ERROR)
    {
        OBSLog(L"HttpCreateHttpHandle failed with %lu \n", retCode);
        SendMessage(hwndMain, WM_CLOSE, 0, 0);
        return;
    }

    AddHttpPath(hReqQueue, L"ping");
    AddHttpPath(hReqQueue, L"getstatus");
    AddHttpPath(hReqQueue, L"setstreamsettings");
    AddHttpPath(hReqQueue, L"setconfig");
    AddHttpPath(hReqQueue, L"startstream");
    AddHttpPath(hReqQueue, L"startrecord");
    AddHttpPath(hReqQueue, L"stopstream");
    AddHttpPath(hReqQueue, L"stoprecord");
    AddHttpPath(hReqQueue, L"startreplaybuffer");
    AddHttpPath(hReqQueue, L"stopreplaybuffer");
    AddHttpPath(hReqQueue, L"setmicvolume");
    AddHttpPath(hReqQueue, L"getmicvolume");
    AddHttpPath(hReqQueue, L"setsystemvolume");
    AddHttpPath(hReqQueue, L"getsystemvolume");
    AddHttpPath(hReqQueue, L"sethwnd");
    AddHttpPath(hReqQueue, L"setsavepath");
    AddHttpPath(hReqQueue, L"setreplaybuffersavepath");
    AddHttpPath(hReqQueue, L"windowresized");
    AddHttpPath(hReqQueue, L"show");
    AddHttpPath(hReqQueue, L"resetflvstream");
    AddHttpPath(hReqQueue, L"executeinclr");
    AddHttpPath(hReqQueue, L"disableclrbrowser");
    AddHttpPath(hReqQueue, L"enableclrbrowser");
    AddHttpPath(hReqQueue, L"setclrbrowserconfig");
    AddHttpPath(hReqQueue, L"resettooriginalscene");
    AddHttpPath(hReqQueue, L"setsourceposition");
    AddHttpPath(hReqQueue, L"setrender");
    AddHttpPath(hReqQueue, L"setfrontendposition");
    AddHttpPath(hReqQueue, L"setorderandlogo");
    AddHttpPath(hReqQueue, L"setcameraposition");
    AddHttpPath(hReqQueue, L"hide");
    AddHttpPath(hReqQueue, L"close");
    AddHttpPath(hReqQueue, L"enablewebcam");
    AddHttpPath(hReqQueue, L"disablewebcam");
    AddHttpPath(hReqQueue, L"movewebcam");
    AddHttpPath(hReqQueue, L"savereplaybuffer");

	OBSLog(L"omiting getstatus request log as was populating file\n");
    DoReceiveRequests(hReqQueue);

    if (retCode != NO_ERROR)
    {
        OBSLog(L"AddHttpPath failed with %lu \n", retCode);
        SendMessage(hwndMain, WM_CLOSE, 0, 0);
    }
}

void urldecode2(char *dst, const char *src)
{
    char* foo = dst;
    OBSLog("src: %s, dst: %s\n", src, dst);
    OBSLog("len(src): %d, len(dst): %d\n", strlen(src), strlen(dst));
    char a, b;
    while (*src)
    {
            if ((*src == '%') &&
                ((a = src[1]) && (b = src[2])) &&
                (isxdigit(a) && isxdigit(b))) {
                    if (a >= 'a')
                            a -= 'a'-'A';
                    if (a >= 'A')
                            a -= ('A' - 10);
                    else
                            a -= '0';
                    if (b >= 'a')
                            b -= 'a'-'A';
                    if (b >= 'A')
                            b -= ('A' - 10);
                    else
                            b -= '0';
                    *dst++ = 16*a+b;
                    src+=3;
            } else if (*src == '+') {
                    *dst++ = ' ';
                    src++;
            } else {
                    *dst++ = *src++;
            }
    }
    *dst++ = '\0';
    OBSLog("foo: %s\n", foo);
    OBSLog("len(foo): %d\n", strlen(foo));
}

char* GetValFromPostData(char* postData, char* key)
{
    urldecode2(postData, postData);

    char* dataCopy = new char[strlen(postData)];
    strcpy(dataCopy, postData);

    char* data = strtok(dataCopy, "&");
    while (data != NULL)
    {
        if (strncmp(data, key, strlen(key)) == 0)
        {
            int valSize = strlen(data) - strlen(key) - 1;
            char* val = new char[valSize];
            strcpy(val, data + strlen(key) + 1);
            return val;
        }
        data = strtok(NULL, "&");
    }

    return NULL;
}

char* GetValFromPostDataWithoutDecode(char* postData, char* key)
{
	//urldecode2(postData, postData);

	char* dataCopy = new char[strlen(postData)];
	strcpy(dataCopy, postData);

	char* data = strtok(dataCopy, "&");
	while (data != NULL)
	{
		if (strncmp(data, key, strlen(key)) == 0)
		{
			int valSize = strlen(data) - strlen(key) - 1;
			char* val = new char[valSize];
			strcpy(val, data + strlen(key) + 1);
			urldecode2(val, val);
			OBSLog("key: %s value: %s", key, val);
			return val;
		}
		data = strtok(NULL, "&");
	}

	return NULL;
}

char* HandleHttpRequest(PCWSTR request, char* postData)
{
    const wchar_t* path = wcsrchr(request, L'/');

	if (wcscmp(path, L"/getstatus") != 0)
		OBSLog(L"Got http request for %ws\n", path);

    char* response = new char[400];

    if (wcscmp(path, L"/ping") == 0)
    {
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/getstatus") == 0)
    {
        sprintf(response, "{", "");
        sprintf(response + strlen(response), "\"success\": true", "");
        if (App)
        {
            bool streaming = App->GetStreamingStatus();
            bool reconnecting = App->GetReconnectingStatus();
            if ((streaming || reconnecting) && App->GetStreamReport().IsValid())
                App->ClearStreamReport();
            sprintf(response + strlen(response), ", ", "");
            sprintf(response + strlen(response), "\"streaming\": %s", streaming ? "true" : "false");
            sprintf(response + strlen(response), ", ", "");
            sprintf(response + strlen(response), "\"reconnecting\": %s", reconnecting ? "true" : "false");
            if (!streaming && App->GetStreamReport().IsValid())
            {
                sprintf(response + strlen(response), ", ", "");
                char* streamReport = new char[260];
                wcstombs(streamReport, App->GetStreamReport().Array(), App->GetStreamReport().Length()*sizeof(wchar_t));
                sprintf(response + strlen(response), "\"reason\": \"%s\"", streamReport);
                delete streamReport;
            }
        }
        sprintf(response + strlen(response), "}", "");
        return response;
    }
    else if (wcscmp(path, L"/setstreamsettings") == 0)
    {
        Log(TEXT("Setting streaming settings"));

		char* service = GetValFromPostDataWithoutDecode(postData, "service");
		char* url = GetValFromPostDataWithoutDecode(postData, "url");
		char* playPath = GetValFromPostDataWithoutDecode(postData, "playPath");

		String* serviceStr = new String(service);
        String* urlStr = new String(url);
        String* playPathStr = new String(playPath);
		AppConfig->SetString(TEXT("Publish"), TEXT("Service"), serviceStr->Array());
        AppConfig->SetString(TEXT("Publish"), TEXT("URL"), urlStr->Array());
        AppConfig->SetString(TEXT("Publish"), TEXT("PlayPath"), playPathStr->Array());
        Log(TEXT("URL: %s PlayPath: %s"), urlStr->Array(), playPathStr->Array());

        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/setconfig") == 0)
    {
        Log(TEXT("Setting config"));

        char* widthStr = GetValFromPostData(postData, "width");
        char* heightStr = GetValFromPostData(postData, "height");

        char* startXStr = GetValFromPostData(postData, "startX");
        char* startYStr = GetValFromPostData(postData, "startY");

        int startX = atoi(startXStr);
        int startY = atoi(startYStr);
        int width = atoi(widthStr);
        int height = atoi(heightStr);

        int requiredWidth = width;
        if (width != height)
            requiredWidth = (height * 16) / 9;
        int newWidth = width;

        if (width < requiredWidth)
        {
            Log(TEXT("resetting required width"));
            newWidth = requiredWidth;
        }

        while (AppConfig->GetInt(TEXT("Video"), TEXT("WindowWidth")) != newWidth)
            AppConfig->SetInt(TEXT("Video"), TEXT("WindowWidth"), newWidth);
        while (AppConfig->GetInt(TEXT("Video"), TEXT("WindowHeight")) != height)
            AppConfig->SetInt(TEXT("Video"), TEXT("WindowHeight"), height);
        while (AppConfig->GetInt(TEXT("Video"), TEXT("BaseWidth")) != newWidth)
            AppConfig->SetInt(TEXT("Video"), TEXT("BaseWidth"), newWidth);
        while (AppConfig->GetInt(TEXT("Video"), TEXT("BaseHeight")) != height)
            AppConfig->SetInt(TEXT("Video"), TEXT("BaseHeight"), height);
        AppConfig->SetInt(TEXT("Video"), TEXT("StartX"), startX);

        AppConfig->SetInt(TEXT("Video"), TEXT("StartY"), startY);

        char* x264Profile = GetValFromPostData(postData, "x264Profile");
        String* x264ProfileStr = new String(x264Profile);
        AppConfig->SetString(TEXT("Video Encoding"), TEXT("X264Profile"), x264ProfileStr->Array());

        char* maxBitrateStr = GetValFromPostData(postData, "maxBitrate");
        int maxBitrate = atoi(maxBitrateStr);
        AppConfig->SetInt(TEXT("Video Encoding"), TEXT("MaxBitrate"), maxBitrate);

        char* downscaleStr = GetValFromPostData(postData, "downscale");
        float downscale = atof(downscaleStr);
        AppConfig->SetFloat(TEXT("Video"), TEXT("Downscale"), downscale);
        App->SetFrontendPosition(width, height, startX, startY);
        App->SetCLRStreamConfig(newWidth, height);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/startstream") == 0)
    {
        Log(TEXT("Starting stream"));
        if (App)
        {
            bool streaming = App->GetStreamingStatus();
            if (!streaming && App->GetStreamReport().IsValid())
                App->ClearStreamReport();
        }

        SendMessage(hwndMain, WM_COMMAND, MAKEWPARAM(ID_STARTSTREAM, 0), 0);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/startrecord") == 0)
    {
        Log(TEXT("Starting record"));
        SendMessage(hwndMain, WM_COMMAND, MAKEWPARAM(ID_STARTRECORD, 0), 0);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/stopstream") == 0)
    {
        Log(TEXT("Stopping stream"));
        SendMessage(hwndMain, WM_COMMAND, MAKEWPARAM(ID_STOPSTREAM, 0), 0);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/stoprecord") == 0)
    {
        Log(TEXT("Stopping record"));

        char* immediateStr = GetValFromPostData(postData, "immediate");
        int immediate = atoi(immediateStr);

        SendMessage(hwndMain, WM_COMMAND, MAKEWPARAM(ID_STOPRECORD, immediate), 0);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/startreplaybuffer") == 0)
    {
        Log(TEXT("Stopping stream"));
        SendMessage(hwndMain, WM_COMMAND, MAKEWPARAM(ID_STARTREPLAYBUFFER, 0), 0);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/stopreplaybuffer") == 0)
    {
        Log(TEXT("Stopping record"));
        SendMessage(hwndMain, WM_COMMAND, MAKEWPARAM(ID_STOPREPLAYBUFFER, 0), 0);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/setmicvolume") == 0)
    {
        char* vol = GetValFromPostData(postData, "volume");
        float volume = atof(vol);
        volume /= 100.0;
        App->SetMicVolume(volume, true);

        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/getmicvolume") == 0)
    {
        float volume = AppConfig->GetFloat(TEXT("Audio"), TEXT("MicVolume"), 0.0f);
        volume *= 100.0;

        sprintf(response + strlen(response), "{", "");
        sprintf(response + strlen(response), "\"volume\": %d", (int)volume);
        sprintf(response + strlen(response), "}", "");
        return response;
    }
    else if (wcscmp(path, L"/setsystemvolume") == 0)
    {
        char* vol = GetValFromPostData(postData, "volume");
        float volume = atof(vol);
        volume /= 100.0;
        App->SetDesktopVolume(volume, true);

        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/getsystemvolume") == 0)
    {
        float volume = AppConfig->GetFloat(TEXT("Audio"), TEXT("DesktopVolume"), 0.0f);
        volume *= 100.0;

        sprintf(response + strlen(response), "{", "");
        sprintf(response + strlen(response), "\"volume\": %d", (int)volume);
        sprintf(response + strlen(response), "}", "");
        return response;
    }
    else if (wcscmp(path, L"/sethwnd") == 0)
    {
        char* hwndChar = GetValFromPostData(postData, "hwnd");
        int hwnd = atoi(hwndChar);
        AppConfig->SetInt(TEXT("VIDEO"), TEXT("HWND"), hwnd);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/setsavepath") == 0)
    {
        OBSLog("postData: %s\n", postData);
        char* savePath = GetValFromPostData(postData, "savepath");
        OBSLog("savePath: %s\n", savePath);
        String* savePathStr = new String(savePath);
        AppConfig->SetString(TEXT("Publish"), TEXT("SavePath"), savePathStr->Array());
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/setreplaybuffersavepath") == 0)
    {
        char* savePath = GetValFromPostData(postData, "savepath");
        String* savePathStr = new String(savePath);
        AppConfig->SetString(TEXT("Publish"), TEXT("ReplayBufferSavePath"), savePathStr->Array());
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/windowresized") == 0)
    {
        char* widthStr = GetValFromPostData(postData, "width");
        char* heightStr = GetValFromPostData(postData, "height");
        int width = atoi(widthStr);
        int height = atoi(heightStr);

        int requiredWidth = (height * 16) / 9;

        if (width < requiredWidth)
        {
            Log(TEXT("resetting required width"));
            width = requiredWidth;
        }

        AppConfig->SetInt(TEXT("Video"), TEXT("WindowWidth"), width);
        AppConfig->SetInt(TEXT("Video"), TEXT("WindowHeight"), height);

        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/show") == 0)
    {
        SendMessage(hwndMain, WM_COMMAND, MAKEWPARAM(ID_SHOW, 0), 0);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
	else if (wcscmp(path, L"/resetflvstream") == 0)
	{
		App->ResetFLVStreamRecording();
		sprintf(response + strlen(response), "%s", "{\"success\": true}");
		return response;
	}
    else if (wcscmp(path, L"/executeinclr") == 0)
    {
        char* lines = GetValFromPostData(postData, "lines");

        const size_t cSize = strlen(lines) + 1;
        std::wstring wlines(cSize, L'#');

        mbstowcs(&wlines[0], lines, cSize);

        App->ExecuteJS(wlines);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/enableclrbrowser") == 0)
    {
        App->RenderCLRBrowser(true);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
	else if (wcscmp(path, L"/disableclrbrowser") == 0)
	{
		App->RenderCLRBrowser(false);
		sprintf(response + strlen(response), "%s", "{\"success\": true}");
		return response;
	}
    else if (wcscmp(path, L"/setclrbrowserconfig") == 0)
    {
        char* widthStr = GetValFromPostDataWithoutDecode(postData, "width");
        char* heightStr = GetValFromPostDataWithoutDecode(postData, "height");
		char* url = GetValFromPostDataWithoutDecode(postData, "url");
        int width = atoi(widthStr);
        int height = atoi(heightStr);
		String* urlStr = new String(url);

        int newWidth = width;
        int requiredWidth = height * 16 / 9;

        if (width < requiredWidth)
            newWidth = requiredWidth;

        App->SetCLRBrowserConfig(urlStr->Array(), newWidth, height);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
	}
	else if (wcscmp(path, L"/resettooriginalscene") == 0)
	{
		App->ResetToOriginalScene();
		sprintf(response + strlen(response), "%s", "{\"success\": true}");
		return response;
	}
	else if (wcscmp(path, L"/setorderandlogo") == 0)
	{
		char* sourcesStr = GetValFromPostData(postData, "order");
		App->SetSourceOrder(sourcesStr);

		char* logo = GetValFromPostData(postData, "logo");
		String* logoStr = new String(logo);

		StringList logoSourcesList;
		logoStr->GetTokenList(logoSourcesList, ',', false);

		App->SetRender("watermarkGif", 0);
		App->SetRender("watermark", 0);
		App->SetRender("watermarkFB", 0);
		
		for (int i = 0; i < logoSourcesList.Num(); i++)
		{
			App->SetRender(logoSourcesList.GetElement(i), 1);
		}

		sprintf(response + strlen(response), "%s", "{\"success\": true}");
		return response;
	}
	else if (wcscmp(path, L"/setsourceposition") == 0)
	{
        char* xStr = GetValFromPostData(postData, "x");
        char* yStr = GetValFromPostData(postData, "y");
        char* widthStr = GetValFromPostData(postData, "width");
        char* heightStr = GetValFromPostData(postData, "height");
		char* sourceChar = GetValFromPostData(postData, "source");

        int width = atoi(widthStr);
        int height = atoi(heightStr);
        int x = atoi(xStr);
        int y = atoi(yStr);
		String* sourceStr = new String(sourceChar);

        App->SetSourcePosition(sourceStr->Array(), x, y, width, height);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
	}
	else if (wcscmp(path, L"/setrender") == 0)
	{
        char* sourceStr = GetValFromPostData(postData, "source");
        char* renderStr = GetValFromPostData(postData, "render");

        int render = atoi(renderStr);
		String* source = new String(sourceStr);

        App->SetRender(source->Array(), render);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
	}
	else if (wcscmp(path, L"/setfrontendposition") == 0)
	{
		char* widthStr = GetValFromPostData(postData, "width");
		char* heightStr = GetValFromPostData(postData, "height");
		char* xStr = GetValFromPostData(postData, "x");
		char* yStr = GetValFromPostData(postData, "y");
		int width = atoi(widthStr);
		int height = atoi(heightStr);
		int x = atoi(xStr);
		int y = atoi(yStr);

		App->SetFrontendPosition(width, height, x, y);
		sprintf(response + strlen(response), "%s", "{\"success\": true}");
		return response;
	}
	else if (wcscmp(path, L"/setcameraposition") == 0)
	{
		char* widthStr = GetValFromPostData(postData, "width");
		char* heightStr = GetValFromPostData(postData, "height");
		char* xStr = GetValFromPostData(postData, "x");
		char* yStr = GetValFromPostData(postData, "y");
		char* actualWidthStr = GetValFromPostData(postData, "actualWidth");
		char* actualHeightStr = GetValFromPostData(postData, "actualHeight");
		char* renderStr = GetValFromPostData(postData, "render");
		char* isPercentageStr = GetValFromPostData(postData, "isPercentage");

		int width = atoi(widthStr);
		int height = atoi(heightStr);
		int x = atoi(xStr);
		int y = atoi(yStr);
		int actualWidth = atoi(actualWidthStr);
		int actualHeight = atoi(actualHeightStr);
		int isPercentage = atoi(isPercentageStr);
		int render = atoi(renderStr);

		App->SetCameraPosition(width, height, x, y, actualWidth, actualHeight, isPercentage, render);
		if (render == 1)
			sprintf(response + strlen(response), "%s", "{\"success\": true,\"webcam\":true}");
		else
			sprintf(response + strlen(response), "%s", "{\"success\": true,\"webcam\":false}");

		return response;
	}
    else if (wcscmp(path, L"/hide") == 0)
    {
        SendMessage(hwndMain, WM_COMMAND, MAKEWPARAM(ID_HIDE, 0), 0);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/close") == 0)
    {
        SendMessage(hwndMain, WM_CLOSE, 0, 0);
        sprintf(response + strlen(response), "%s", "{\"success\": true}");
        return response;
    }
    else if (wcscmp(path, L"/enablewebcam") == 0)
    {
        char* widthStr = GetValFromPostData(postData, "width");
        char* heightStr = GetValFromPostData(postData, "height");
        char* positionStr = GetValFromPostData(postData, "position");

        int width = atoi(widthStr);
        int height = atoi(heightStr);
        int position = atoi(positionStr);

        bool ret = false;
        if (App)
            ret = App->EnableWebcam(width, height, position);

        sprintf(response + strlen(response), "{", "");
        sprintf(response + strlen(response), "\"success\": %s", ret ? "true" : "false");
        sprintf(response + strlen(response), ", ", "");
        sprintf(response + strlen(response), "\"webcam\": true");
        sprintf(response + strlen(response), "}", "");

        return response;
    }
    else if (wcscmp(path, L"/disablewebcam") == 0)
    {
        if (App)
        {
            App->DisableWebcam();
        }

        sprintf(response + strlen(response), "{", "");
        sprintf(response + strlen(response), "\"success\": true");
        sprintf(response + strlen(response), ", ", "");
        sprintf(response + strlen(response), "\"webcam\": false");
        sprintf(response + strlen(response), "}", "");

        return response;
    }
    else if (wcscmp(path, L"/movewebcam") == 0)
    {
        char* horizontalStr = GetValFromPostData(postData, "horizontal");
        char* verticalStr = GetValFromPostData(postData, "vertical");
        int horizontal = atoi(horizontalStr);
        int vertical = atoi(verticalStr);

        if (App && App->IsRunning() && App->scene)
        {
            App->MoveWebcamToEdge(horizontal, vertical);
            sprintf(response + strlen(response), "%s", "{\"success\": true}");
        }
        else
        {
            sprintf(response + strlen(response), "%s", "{\"success\": false}");
        }

        return response;
    }
    else if (wcscmp(path, L"/savereplaybuffer") == 0)
    {
        if (App && App->IsRunning())
        {
            App->SaveReplayBufferExt();
            sprintf(response + strlen(response), "%s", "{\"success\": true}");
        }
        else
        {
            sprintf(response + strlen(response), "%s", "{\"success\": false}");
        }

        return response;
    }
    else
    {
        sprintf(response + strlen(response), "%s", "{\"success\": false, \"reason\": \"unknown command\"}");
        return response;
    }
}

void PingGM(void)
{
    char localhost[30] = "http://127.0.0.1/";
    String* localhostStr = new String(localhost);

    int gmPort = 2881;

    char pingPath[30] = "ping";
    String* pingPathStr = new String(pingPath);

    String url = *localhostStr + *pingPathStr;

    int responseCode;

    while (1)
    {
        HTTPGetString(url, gmPort, NULL, &responseCode);
        if (responseCode != 200)
            SendMessage(hwndMain, WM_CLOSE, 0, 0);
        responseCode = 0;

        OSSleep(1000);
    }
}

void PingBTV(void)
{
	char localhost[30] = "http://127.0.0.1/";
	String* localhostStr = new String(localhost);

	int btvPort = 2885;

	char pingPath[30] = "ping";
	String* pingPathStr = new String(pingPath);

	String url = *localhostStr + *pingPathStr;

	int responseCode;

	while (1)
	{
		HTTPGetString(url, btvPort, NULL, &responseCode);
		if (responseCode != 200)
			SendMessage(hwndMain, WM_CLOSE, 0, 0);
		responseCode = 0;

		OSSleep(2000);
	}
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{
    if (!HasSSE2Support())
    {
        OBSMessageBox (NULL, TEXT("OBS requires an SSE2-compatible CPU."), TEXT("Unsupported CPU"), MB_ICONERROR);
        return 1;
    }

#if defined _M_X64 && _MSC_VER == 1800
    //workaround AVX2 bug in VS2013, http://connect.microsoft.com/VisualStudio/feedback/details/811093
    _set_FMA3_enable(0);
#endif

    LoadSeDebugPrivilege();

    int numArgs;
    LPWSTR *args = CommandLineToArgvW(GetCommandLineW(), &numArgs);
    LPWSTR profile = NULL;
    LPWSTR sceneCollection = NULL;
    LPWSTR userService = NULL;

    bool bDisableMutex = false;

    for(int i=1; i<numArgs; i++)
    {
        if(scmpi(args[i], TEXT("-multi")) == 0)
            bDisableMutex = true;
        else if(scmpi(args[i], TEXT("-portable")) == 0)
            bIsPortable = true;
        else if (scmpi(args[i], TEXT("-start")) == 0)
            bStreamOnStart = true;
        else if (scmpi(args[i], L"-profile") == 0)
        {
            if (++i < numArgs)
                profile = args[i];
        }
        else if (scmpi(args[i], L"-scenecollection") == 0)
        {
            if (++i < numArgs)
                sceneCollection = args[i];
        }
        else if (scmpi(args[i], L"-installservice") == 0)
        {
            if (++i < numArgs)
            {
                bDisableMutex = true;
                userService = args[i];
            }
        }
		else if (scmpi(args[i], L"-oem") == 0)
		{
			if (++i < numArgs)
			{
				oemName = args[i];
			}
		}
    }
    //------------------------------------------------------------
    //make sure only one instance of the application can be open at a time

    hOBSMutex = CreateMutex(NULL, TRUE, TEXT("OBSMutex"));
    if(!bDisableMutex && GetLastError() == ERROR_ALREADY_EXISTS)
    {
        hwndMain = FindWindow(OBS_WINDOW_CLASS, NULL);
//        if(hwndMain)
//            SetForegroundWindow(hwndMain);
        CloseHandle(hOBSMutex);
        stopOBS = true;
        //return 0;
    }

    //------------------------------------------------------------

    hinstMain = hInstance;
    
    HeapSetInformation(NULL, HeapEnableTerminationOnCorruption, NULL, 0);
    SetProcessDEPPolicy(PROCESS_DEP_ENABLE | PROCESS_DEP_DISABLE_ATL_THUNK_EMULATION);
    InitializeExceptionHandler();

    ULONG_PTR gdipToken;
    const Gdiplus::GdiplusStartupInput gdipInput;
    Gdiplus::GdiplusStartup(&gdipToken, &gdipInput, NULL);

    if(InitXT(NULL, TEXT("FastAlloc")))
    {
        if (stopOBS)
        {
            HTTPPostString("Error=OBSAlreadyRunning");
            return 0;
        }

        HKEY mykey;
        LONG retval = RegOpenKeyEx(
            HKEY_LOCAL_MACHINE,         // handle to open key
            L"SOFTWARE\\BlueStacks\\BlueStacksGameManager",  // subkey name
            0,   // reserved
            KEY_READ, // security access mask
            &mykey    // handle to open key
            );

        if (ERROR_SUCCESS == retval)
        {
            DWORD devEnv;
            unsigned long size = 1024;
            unsigned long type = REG_DWORD;
            retval = RegQueryValueEx(
                mykey,            // handle to key
                L"OBSDevEnv",  // value name
                NULL,   // reserved
                &type,       // type buffer
                (LPBYTE)&devEnv,        // data buffer
                &size      // size of data buffer
                );
            RegCloseKey(mykey);

            if (ERROR_SUCCESS == retval)
            {
                if (devEnv > 0)
                {
                    Log(TEXT("running in debug mode"));
                    bDebugMode = true;
                }
            }
        }

        InitSockets();
        CoInitializeEx(NULL, COINIT_MULTITHREADED);
//        CoInitialize(0);
        EnableProfiling(TRUE);

        //always make sure we're running inside our app folder so that locale files and plugins work
        SetWorkingFolder();

        //get current working dir
        {
            String strDirectory;
            UINT dirSize = GetCurrentDirectory(0, 0);
            strDirectory.SetLength(dirSize);
            GetCurrentDirectory(dirSize, strDirectory.Array());

            scpy(lpAppPath, strDirectory);
        }

        //if -portable isn't specified in command line check if there's a file named "obs_portable_mode" in current working dir, if so, obs goes into portable mode
        if(!bIsPortable)
        {
            String strPMFileName = lpAppPath;
            strPMFileName += TEXT("\\obs_portable_mode");
            if(OSFileExists(strPMFileName))
                bIsPortable = true;
        }

        TSTR lpAllocator = NULL;

        {
            if(bIsPortable)
                scpy(lpAppDataPath, lpAppPath);
            else
            {
                SHGetFolderPath(NULL, CSIDL_APPDATA, NULL, SHGFP_TYPE_CURRENT, lpAppDataPath);
                scat_n(lpAppDataPath, TEXT("\\OBS"), 4);
            }

            if(!OSFileExists(lpAppDataPath) && !OSCreateDirectory(lpAppDataPath))
                CrashError(TEXT("Couldn't create directory '%s'"), lpAppDataPath);

            String strAppDataPath = lpAppDataPath;
            String strProfilesPath = strAppDataPath + TEXT("\\profiles");
            if(!OSFileExists(strProfilesPath) && !OSCreateDirectory(strProfilesPath))
                CrashError(TEXT("Couldn't create directory '%s'"), strProfilesPath.Array());

            String strSceneCollectionPath = strAppDataPath + TEXT("\\sceneCollection");
            if (!OSFileExists(strSceneCollectionPath) && !OSCreateDirectory(strSceneCollectionPath))
                CrashError(TEXT("Couldn't create directory '%s'"), strSceneCollectionPath.Array());

            String strLogsPath = strAppDataPath + TEXT("\\logs");
            if(!OSFileExists(strLogsPath) && !OSCreateDirectory(strLogsPath))
                CrashError(TEXT("Couldn't create directory '%s'"), strLogsPath.Array());

            String strCrashPath = strAppDataPath + TEXT("\\crashDumps");
            if(!OSFileExists(strCrashPath) && !OSCreateDirectory(strCrashPath))
                CrashError(TEXT("Couldn't create directory '%s'"), strCrashPath.Array());

            String strPluginDataPath = strAppDataPath + TEXT("\\pluginData");
            if(!OSFileExists(strPluginDataPath) && !OSCreateDirectory(strPluginDataPath))
                CrashError(TEXT("Couldn't create directory '%s'"), strPluginDataPath.Array());

            String strUpdatePath = strAppDataPath + TEXT("\\updates");
            if(!OSFileExists(strUpdatePath) && !OSCreateDirectory(strUpdatePath))
                CrashError(TEXT("Couldn't create directory '%s'"), strUpdatePath.Array());

            String servicesPath = strAppDataPath + L"\\services";
            if (!OSFileExists(servicesPath) && !OSCreateDirectory(servicesPath))
                CrashError(TEXT("Couldn't create directory '%s'"), servicesPath.Array());

            LoadGlobalIni();

            String strAllocator = GlobalConfig->GetString(TEXT("General"), TEXT("Allocator"));
            if(strAllocator.IsValid())
            {
                UINT size = strAllocator.DataLength();
                lpAllocator = (TSTR)malloc(size);
                mcpy(lpAllocator, strAllocator.Array(), size);
            }

            RegisterServiceFileHandler();
        }

        if(lpAllocator)
        {
            delete GlobalConfig;

            ResetXTAllocator(lpAllocator);
            free(lpAllocator);

            LoadGlobalIni();
        }

        //EnableMemoryTracking(true, 8961);

        //-----------------------------------------------------
        // load locale

        if (!locale->LoadStringFile(TEXT("locale/en.txt")))
            AppWarning(TEXT("Could not open locale string file '%s'"), TEXT("locale/en.txt"));

        String strLanguage = GlobalConfig->GetString(TEXT("General"), TEXT("Language"), TEXT("en"));
        if (!strLanguage.CompareI(TEXT("en")))
        {
            String langFile;
            langFile << TEXT("locale/") << strLanguage << TEXT(".txt");

            if (!locale->LoadStringFile(langFile))
                AppWarning(TEXT("Could not open locale string file '%s'"), langFile.Array());
        }

        // install user service here after we've loaded XT and locale
        if (userService)
        {
            if (!InstallUserService(userService))
                return 1;
            return 0;
        }

        //--------------------------------------------

        GlobalConfig->SetString(TEXT("General"), TEXT("LastAppDirectory"), lpAppPath);

        //--------------------------------------------

        AppConfig = new ConfigFile;
        SetupIni(profile);
        SetupSceneCollection(sceneCollection);

        //--------------------------------------------

        DWORD colors[16];
        for (int i=0; i<16; i++) {
            String strColorIdx = "Color";
            strColorIdx << IntString(i);
            colors[i] = GlobalConfig->GetInt(TEXT("CustomColors"), strColorIdx, 0xFFFFFF);
        }

        CCSetCustomColors(colors);

        String strLogFileWildcard;
        strLogFileWildcard << lpAppDataPath << TEXT("\\logs\\*.log");

        OSFindData ofd;
        HANDLE hFindLogs = OSFindFirstFile(strLogFileWildcard, ofd);
        if(hFindLogs)
        {
            int numLogs = 0;
            String strFirstLog;

            do
            {
                if(ofd.bDirectory) continue;
                if(!numLogs++)
                    strFirstLog << lpAppDataPath << TEXT("\\logs\\") << ofd.fileName;
            } while(OSFindNextFile(hFindLogs, ofd));

            OSFindClose(hFindLogs);

            if(numLogs >= GlobalConfig->GetInt(TEXT("General"), TEXT("MaxLogs"), 20))
                OSDeleteFile(strFirstLog);
        }

        SYSTEMTIME st;
        GetLocalTime(&st);

        String strLog;
        strLog << lpAppDataPath << FormattedString(TEXT("\\logs\\%u-%02u-%02u-%02u%02u-%02u"), st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond) << TEXT(".log");

        InitXTLog(strLog);

        DWORD serverThreadId;
        CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)SetupHTTPServer, NULL, 0, &serverThreadId);

        DWORD pingThreadId;
        //CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)PingGM, NULL, 0, &pingThreadId);
		CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)PingBTV, NULL, 0, &pingThreadId);

        //--------------------------------------------

        BOOL bDisableComposition = AppConfig->GetInt(TEXT("Video"), TEXT("DisableAero"), 0);

        if(bDisableComposition)
          DwmEnableComposition(DWM_EC_DISABLECOMPOSITION);

        //--------------------------------------------


        String strCaptureHookLog;
        strCaptureHookLog << lpAppDataPath << L"\\pluginData\\captureHookLog.txt";
        
        OSFileChangeData *pGCHLogMF = NULL;
        pGCHLogMF = OSMonitorFileStart (strCaptureHookLog, true);

        App = new OBS;

        HACCEL hAccel = LoadAccelerators(hinstMain, MAKEINTRESOURCE(IDR_ACCELERATOR1));

        MSG msg;
        //Uncomment this for showing window
        //SendMessage(hwndMain, WM_COMMAND, MAKEWPARAM(ID_SHOW, 0), 0);
        while(GetMessage(&msg, NULL, 0, 0))
        {
            if(!TranslateAccelerator(hwndMain, hAccel, &msg) && !IsDialogMessage(hwndMain, &msg))
            {
                TranslateMessage(&msg);
                DispatchMessage(&msg);
            }
        }

        delete App;

        //--------------------------------------------

        CCGetCustomColors(colors);
        for (int i=0; i<16; i++) {
            String strColorIdx = "Color";
            strColorIdx << IntString(i);
            GlobalConfig->SetInt(TEXT("CustomColors"), strColorIdx, colors[i]);
        }

        GlobalConfig->SetInt(TEXT("General"), TEXT("LastAppVersion"), OBS_VERSION);

        delete AppConfig;
        delete GlobalConfig;

        if(bDisableComposition)
            DwmEnableComposition(DWM_EC_ENABLECOMPOSITION);

        TerminateSockets();

        bool skipGCHLog = false;

        if(pGCHLogMF)
        {
            if(!OSFileHasChanged(pGCHLogMF))
                skipGCHLog = true;

            OSMonitorFileDestroy(pGCHLogMF);
        }

        //FIXME: File monitoring needs fixing.  Half the time game capture logs are not
        //getting attached even when users clearly used it.
        if(true) //!skipGCHLog)
        {
            XFile captureHookLog;

            if (captureHookLog.Open(strCaptureHookLog, XFILE_READ|XFILE_SHARED, XFILE_OPENEXISTING))
            {
                String strContents;
                captureHookLog.ReadFileToString(strContents);
                LogRaw(L"\r\n\r\nLast game capture log:");
                LogRaw(strContents.Array(), strContents.Length());
            }
        }
    }

    Gdiplus::GdiplusShutdown(gdipToken);

    TerminateXT();

    //------------------------------------------------------------

    CloseHandle(hOBSMutex);

    LocalFree(args);

    return 0;
}
