/********************************************************************************
 Copyright (C) 2001-2012 Hugh Bailey <obs.jim@gmail.com>

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


#include "XT.h"

#include <memory>
#include "windows.h"
#include <winhttp.h>

namespace
{
    struct XStringLog
    {
        XStringLog() : stopped(false) { Reset(); }

        void Append(String const &string, bool linefeed=true);
        void Append(CTSTR str, UINT len, bool linefeed=true);

        void Read(String &str);
        void Read(String &str, unsigned &start, unsigned length);

        void Stop();
        void Clear();
        void Reset();
    private:
        void Process();

        String log;
        StringList unprocessed, processing;
        std::unique_ptr<void, MutexDeleter> append_mutex, process_mutex, read_mutex;
        bool stopped;
    };
}

////////////////
//globals (oh deary me.  if there were a convention convention, I most certainly wouldn't be invited.)
SafeList<DWORD>         TickList;

Alloc                  *MainAllocator   = NULL;
BOOL                    bBaseLoaded     = FALSE;
BOOL                    bDebugBreak     = TRUE;
TCHAR                   lpLogFileName[260] = TEXT("XT.log");
XFile                   LogFile;
XStringLog              StringLog;
LogUpdateCallback       LogUpdateProc;
StringList              TraceFuncList;


BOOL bLogStarted = FALSE;

void STDCALL CriticalExit();
void STDCALL OpenLogFile();
void STDCALL CloseLogFile();

void STDCALL OSInit();
void STDCALL OSExit();

BOOL STDCALL InitXT(CTSTR logFile, CTSTR allocatorName)
{
    if(!bBaseLoaded)
    {
        if(logFile)
            scpy(lpLogFileName, logFile);

        OSInit();

        ResetXTAllocator(allocatorName);
        bBaseLoaded = 1;
    }

    return TRUE;
}


void STDCALL InitXTLog(CTSTR logFile)
{
    if(logFile)
        scpy(lpLogFileName, logFile);
}

void STDCALL ResetXTAllocator(CTSTR lpAllocator)
{
    StringLog.Stop();
    StringLog.Clear();

    delete locale;
    delete MainAllocator;

    if(scmpi(lpAllocator, TEXT("DebugAlloc")) == 0)
        MainAllocator = new DebugAlloc;
    else if (scmpi(lpAllocator, TEXT("DefaultAlloc")) == 0)
        MainAllocator = new DefaultAlloc;
    else if (scmpi(lpAllocator, TEXT("SeriousMemoryDebuggingAlloc")) == 0)
        MainAllocator = new SeriousMemoryDebuggingAlloc;
    else
#if defined(_M_X64) || defined(__amd64__)
        MainAllocator = new DefaultAlloc;
#else
        MainAllocator = new FastAlloc;
#endif

    locale = new LocaleStringLookup;

    StringLog.Reset();
}

void STDCALL TerminateXT()
{
    if(bBaseLoaded)
    {
        StringLog.Stop();

        FreeProfileData();

        delete locale;
        locale = NULL;

        StringLog.Clear();

        if(MainAllocator)
        {
            Alloc *curAlloc = MainAllocator;
            MainAllocator = new DefaultAlloc;

            delete curAlloc;

            delete MainAllocator;
            MainAllocator = NULL;
        }

        bBaseLoaded = 0;

        if(LogFile.IsOpen())
            LogFile.Close();

        OSExit();
    }
}

void STDCALL CriticalExit()
{
    if(bBaseLoaded)
    {
        bBaseLoaded = 0;

        if(LogFile.IsOpen())
            LogFile.Close();

        OSCriticalExit();
    }
}

#define MAX_STACK_TRACE 1000

void STDCALL TraceCrash(const TCHAR *trackName)
{
    TraceFuncList.Insert(0, trackName);

    if(TraceFuncList.Num() > MAX_STACK_TRACE)
        TraceFuncList.SetSize(MAX_STACK_TRACE);
}

void STDCALL TraceCrashEnd()
{
    String strStackTrace = TEXT("\r\nException Fault - Stack Trace:");

    for(unsigned int i=0; i<TraceFuncList.Num(); i++)
    {
        if(i) strStackTrace << TEXT(" -> ");
        if(!(i%10)) strStackTrace << TEXT("\r\n    ");
        strStackTrace << TraceFuncList[i];
    }

    if(TraceFuncList.Num() == MAX_STACK_TRACE)
        strStackTrace << TEXT(" -> ...");

    String strOut = FormattedString(TEXT("%s\r\n"), strStackTrace.Array());

    OpenLogFile();
    LogFile.WriteAsUTF8(strOut, strOut.Length());
    LogFile.WriteAsUTF8(TEXT("\r\n"));
    CloseLogFile();

    OSMessageBox(TEXT("Error: Exception fault - More info in the log file.\r\n\r\nMake sure you're using the latest verison, otherwise send your log to obs.jim@gmail.com"));

    TraceFuncList.Clear();
    CriticalExit();
}


void STDCALL OpenLogFile()
{
    if(LogFile.IsOpen())
        return;

    if(!bLogStarted)
    {
        LogFile.Open(lpLogFileName, XFILE_READ|XFILE_WRITE|XFILE_SHARED, XFILE_CREATEALWAYS);
        LogFile.Write("\xEF\xBB\xBF", 3);
        bLogStarted = TRUE;
    }
    else
    {
        //LogFile.Open(lpLogFileName, XFILE_WRITE, XFILE_OPENEXISTING);
        //LogFile.SetPos(0, XFILE_END);
    }
}

void STDCALL CloseLogFile()
{
    //LogFile.Close();
}



void __cdecl LogRaw(const TCHAR *text, UINT len)
{
    if(!text) return;

    if (!len)
        len = slen(text);

    OpenLogFile();
    LogFile.WriteAsUTF8(text, len);
    LogFile.WriteAsUTF8(TEXT("\r\n"));
    CloseLogFile();

    StringLog.Append(text, len);
}

void __cdecl Logva(const TCHAR *format, va_list argptr)
{
    if(!format) return;

#ifdef _DEBUG
    OSDebugOutva(format, argptr);
    OSDebugOut(L"\n");
#endif

    String strCurTime = CurrentTimeString();
    strCurTime << TEXT(": ");
    String strOut = strCurTime;
    strOut << FormattedStringva(format, argptr);

    strOut.FindReplace(TEXT("\n"), String() << TEXT("\n") << strCurTime);

    OpenLogFile();
    LogFile.WriteAsUTF8(strOut, strOut.Length());
    LogFile.WriteAsUTF8(TEXT("\r\n"));
    CloseLogFile();

    StringLog.Append(strOut);
}

void __cdecl Log(const TCHAR *format, ...)
{
    if(!format) return;

    va_list arglist;

    va_start(arglist, format);

    Logva(format, arglist);
}


void __cdecl AppWarning(const TCHAR *format, ...)
{
    if(!format) return;

    va_list arglist;

    va_start(arglist, format);

    String strOut(L"Warning -- ");
    strOut << FormattedStringva(format, arglist);

    if(bLogStarted)
    {
        OpenLogFile();
        LogFile.WriteAsUTF8(strOut, strOut.Length());
        LogFile.WriteAsUTF8(TEXT("\r\n"));
        CloseLogFile();
    }

    OSDebugOut(TEXT("Warning -- "));
    OSDebugOutva(format, arglist);
    OSDebugOut(TEXT("\r\n"));

    //------------------------------------------------------
    // NOTE:
    // If you're seeting this, you can safely continue running, but I recommend fixing whatever's causing this warning.
    //
    // The debug output window contains the warning that has occured.
    //------------------------------------------------------

#if defined(_DEBUG) && defined(_WIN32)
    if(bDebugBreak && OSDebuggerPresent())
    {
        ProgramBreak();
    }
#endif

    StringLog.Append(strOut);
}

String HTTPGetString (CTSTR url, int port, CTSTR extraHeaders, int *responseCode)
{
    HINTERNET hSession = NULL;
    HINTERNET hConnect = NULL;
    HINTERNET hRequest = NULL;
    URL_COMPONENTS  urlComponents;
    BOOL secure = FALSE;
	String result = "";

    String hostName, path;

    const TCHAR *acceptTypes[] = {
        TEXT("*/*"),
        NULL
    };

    hostName.SetLength(256);
    path.SetLength(1024);

    zero(&urlComponents, sizeof(urlComponents));

    urlComponents.dwStructSize = sizeof(urlComponents);
    
    urlComponents.lpszHostName = hostName;
    urlComponents.dwHostNameLength = hostName.Length();

    urlComponents.lpszUrlPath = path;
    urlComponents.dwUrlPathLength = path.Length();

    WinHttpCrackUrl(url, 0, 0, &urlComponents);

    if (urlComponents.nPort == 443)
        secure = TRUE;

    hSession = WinHttpOpen(TEXT("1.1.1.1"), WINHTTP_ACCESS_TYPE_DEFAULT_PROXY, WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);
    if (!hSession)
        goto failure;

    if (port == 0)
    {
        port = secure ? INTERNET_DEFAULT_HTTPS_PORT : INTERNET_DEFAULT_HTTP_PORT;
    }

    hConnect = WinHttpConnect(hSession, hostName, port, 0);
    if (!hConnect)
        goto failure;

    hRequest = WinHttpOpenRequest(hConnect, TEXT("GET"), path, NULL, WINHTTP_NO_REFERER, acceptTypes, secure ? WINHTTP_FLAG_SECURE|WINHTTP_FLAG_REFRESH : WINHTTP_FLAG_REFRESH);
    if (!hRequest)
        goto failure;

    BOOL bResults = WinHttpSendRequest(hRequest, extraHeaders, extraHeaders ? -1 : 0, WINHTTP_NO_REQUEST_DATA, 0, 0, 0);

    // End the request.
    if (bResults)
        bResults = WinHttpReceiveResponse(hRequest, NULL);
    else
        goto failure;

    TCHAR statusCode[8];
    DWORD statusCodeLen;

    statusCodeLen = sizeof(statusCode);
    if (!WinHttpQueryHeaders (hRequest, WINHTTP_QUERY_STATUS_CODE, WINHTTP_HEADER_NAME_BY_INDEX, &statusCode, &statusCodeLen, WINHTTP_NO_HEADER_INDEX))
        goto failure;

    *responseCode = wcstoul(statusCode, NULL, 10);	

    if (bResults && *responseCode == 200)
    {
        CHAR buffer[16384];
        DWORD dwSize, dwOutSize;

        do 
        {
            // Check for available data.
            dwSize = 0;
            if (!WinHttpQueryDataAvailable(hRequest, &dwSize))
                goto failure;

            if (!WinHttpReadData(hRequest, (LPVOID)buffer, dwSize, &dwOutSize))
            {
                goto failure;
            }
            else
            {
                if (!dwOutSize)
                    break;

				// Ensure the string is terminated.
				buffer[dwOutSize] = 0;

				String b = String((LPCSTR)buffer);
				result.AppendString(b);
            }
        } while (dwSize > 0);
    }

failure:
    if (hSession)
        WinHttpCloseHandle(hSession);
    if (hConnect)
        WinHttpCloseHandle(hConnect);
    if (hRequest)
        WinHttpCloseHandle(hRequest);

    return result;
}

String HTTPGetString (CTSTR url, CTSTR extraHeaders, int *responseCode)
{
    return HTTPGetString(url, 0, extraHeaders, responseCode);
}

void ReportObsError()
{
    char localhost[30] = "http://127.0.0.1/";
    String* localhostStr = new String(localhost);
	int btvPort = 2885;
    char errorPath[30] = "reportobserror";
    String* errorPathStr = new String(errorPath);

    String url = *localhostStr + *errorPathStr;

    int responseCode;
	HTTPGetString(url, btvPort, NULL, &responseCode);
}

//note to self:  do try not to rely on this too much.
__declspec(noreturn) void __cdecl CrashError(const TCHAR *format, ...)
{
    va_list arglist;

    va_start(arglist, format);

    String strOut(L"\r\nError: ");
    strOut << FormattedStringva(format, arglist);

    OpenLogFile();
    LogFile.WriteAsUTF8(strOut);
    LogFile.WriteStr(TEXT("\r\n"));
    CloseLogFile();

//    OSMessageBoxva(format, arglist);

#if defined(_DEBUG) && defined(_WIN32)
    if(bDebugBreak && OSDebuggerPresent())
        ProgramBreak();
#endif

    ReportObsError();
    CriticalExit();
}

__declspec(noreturn) void __cdecl DumpError(const TCHAR *format, ...)
{
    va_list arglist;

    va_start(arglist, format);

    String strOut(L"\r\nError: ");
    strOut << FormattedStringva(format, arglist);

    OpenLogFile();
    LogFile.WriteAsUTF8(strOut);
    LogFile.WriteStr(TEXT("\r\n"));
    CloseLogFile();

    OSMessageBoxva(format, arglist);

    OSRaiseException(0xDEAD0B5);
}

String CurrentTimeString()
{
    time_t     now = time(0);
    struct tm  tstruct;
    char       buf[80];
    tstruct = *localtime(&now);
    strftime(buf, sizeof(buf), "%X", &tstruct);
    return buf;
}

String CurrentDateTimeString()
{
    time_t     now = time(0);
    struct tm  tstruct;
    char       buf[80];
    tstruct = *localtime(&now);
    strftime(buf, sizeof(buf), "%Y-%m-%d, %X", &tstruct);
    return buf;
}

String CurrentLogFilename()
{
    return lpLogFileName;
}

void ReadLog(String &data)
{
    StringLog.Read(data);
}

void ReadLogPartial(String &data, unsigned &start, unsigned maxLength)
{
    StringLog.Read(data, start, maxLength);
}

void ResetLogUpdateCallback(LogUpdateCallback proc)
{
    LogUpdateProc = proc;
}

void XStringLog::Append(String const &string, bool linefeed)
{
    ScopedLock a(append_mutex);
    if (stopped) return;

    unprocessed << string;
    if (linefeed) unprocessed << L"\r\n";

    if (LogUpdateProc) LogUpdateProc();
}

void XStringLog::Append(CTSTR tstr, UINT len, bool linefeed)
{
    if (stopped) return;

    String str;
    str.AppendString(tstr, len);
    Append(str, linefeed);
}

void XStringLog::Read(String &str)
{
    Process();

    ScopedLock r(read_mutex);
    str = log;
}

void XStringLog::Read(String &str, unsigned &start, unsigned length)
{
    Process();

    ScopedLock r(read_mutex);

    if (log.Length() == 0 && start) start = 0;
    if (start >= log.Length()) return;

    if ((UINT_MAX - start) < length) length = UINT_MAX - start;
    str = log.Mid(start, ((start+length) > log.Length()) ? log.Length() : (start+length));
    start += str.Length();
}

void XStringLog::Process()
{
    ScopedLock p(process_mutex, true);
    if (!p.locked) return;
    if (stopped) return;

    {
        ScopedLock a(append_mutex);

        processing.TransferFrom(unprocessed);
    }

    String str;
    for (unsigned int i = 0; i < processing.Num(); i++)
        str << processing[i];

    {
        ScopedLock r(read_mutex);
        log << str;
    }

    processing.Clear();
}

void XStringLog::Stop()
{
    ScopedLock p(process_mutex, false, false); //stop processing
    ScopedLock a(append_mutex);

    stopped = true;
}

void XStringLog::Clear()
{
    unprocessed.Clear();
    processing.Clear();
    log.Clear();
    append_mutex.reset();
    process_mutex.reset();
    read_mutex.reset();
}

void XStringLog::Reset()
{
    Clear();
    stopped = false;
    append_mutex.reset(OSCreateMutex());
    process_mutex.reset(OSCreateMutex());
    read_mutex.reset(OSCreateMutex());
}
