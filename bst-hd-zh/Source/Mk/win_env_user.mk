VSDIR = "C:\Program Files (x86)\Microsoft Visual Studio 10.0"
#SDKDIR = "C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1a"
SDKDIR = "C:\Program Files\Microsoft SDKs\Windows\v7.1"

CFLAGS = $(CFLAGS) /nologo /W3 /WX
CFLAGS = $(CFLAGS) /I$(S)
!IF "$(USE_SDK)" == "YES"
CFLAGS = $(CFLAGS) /I$(SDKDIR)\include
!ELSE
CFLAGS = $(CFLAGS) /I$(DDKDIR)\inc
CFLAGS = $(CFLAGS) /I$(DDKDIR)\inc\api
CFLAGS = $(CFLAGS) /I$(DDKDIR)\inc\crt
CFLAGS = $(CFLAGS) /I$(DDKDIR)\inc\ddk
!ENDIF

CFLAGS = $(CFLAGS) /DWIN32_LEAN_AND_MEAN=1
CFLAGS = $(CFLAGS) /D_WIN32_WINNT=0x0501 /DWINVER=0x0501

CFLAGS = $(CFLAGS) /Zl /Zp8 /Gy /Gm-
CFLAGS = $(CFLAGS) /GF /GS /Zi /Oy-

CFLAGS = $(CFLAGS) /wd4018
#CFLAGS = $(CFLAGS) /wd4055
#CFLAGS = $(CFLAGS) /wd4100
#CFLAGS = $(CFLAGS) /wd4127
#CFLAGS = $(CFLAGS) /wd4200
#CFLAGS = $(CFLAGS) /wd4201

ASFLAGS = $(ASFLAGS) /nologo
#ASFLAGS = $(ASFLAGS) /I$(S)\include

LDFLAGS = $(LDFLAGS) /debug

!IF "$(PROFILING_ENABLED)" == "YES"
LDFLAGS = $(LDFLAGS) /profile
!ENDIF

!IF "$(USE_SDK)" == "YES"
!ELSE
RCFLAGS = $(RCFLAGS) /i$(DDKDIR)\inc\api
!ENDIF
RCFLAGS = $(RCFLAGS) /i$(S)

RCFLAGS = $(RCFLAGS) /dVERSION_MAJOR=$(VERSION_MAJOR)
RCFLAGS = $(RCFLAGS) /dVERSION_MINOR=$(VERSION_MINOR)
RCFLAGS = $(RCFLAGS) /dVERSION_PATCH=$(VERSION_PATCH)
RCFLAGS = $(RCFLAGS) /dVERSION_BUILD=$(VERSION_BUILD)

RCFLAGS = $(RCFLAGS) /dWEB_PLUGIN_VERSION_MAJOR=$(WEB_PLUGIN_VERSION_MAJOR)
RCFLAGS = $(RCFLAGS) /dWEB_PLUGIN_VERSION_MINOR=$(WEB_PLUGIN_VERSION_MINOR)
RCFLAGS = $(RCFLAGS) /dWEB_PLUGIN_VERSION_PATCH=$(WEB_PLUGIN_VERSION_PATCH)
RCFLAGS = $(RCFLAGS) /dWEB_PLUGIN_VERSION_BUILD=$(WEB_PLUGIN_VERSION_BUILD)

!IF "$(BUILD_TYPE)" == "DBG"
CFLAGS  = $(CFLAGS) /Od /Oi- /D_DEBUG
!IF "$(USE_SDK)" == "YES"
MSVCRTLIB = libcmtd.lib
MSVCPRTLIB = libcpmtd.lib
!ELSE
MSVCRTLIB = libcmtd.lib
MSVCPRTLIB = ntstc_libcmt.lib
!ENDIF
!ELSE
CFLAGS = $(CFLAGS) /Oxt /Oi /DNDEBUG
!IF "$(USE_SDK)" == "YES"
MSVCRTLIB = libcmt.lib
MSVCPRTLIB = libcpmt.lib
!ELSE
MSVCRTLIB = libcmt.lib
MSVCPRTLIB = ntstc_libcmt.lib
!ENDIF
!ENDIF

OLE32LIB = ole32.lib

!IF "$(TARGET_ARCH)" == "amd64"
CFLAGS = $(CFLAGS) /D_WIN64 /D_AMD64_=1 /DAMD64
!IF "$(USE_SDK)" == "YES"
LDFLAGS = $(LDFLAGS) /LIBPATH:$(VSDIR)\vc\lib\amd64
LDFLAGS = $(LDFLAGS) /LIBPATH:$(VSDIR)\vc\atlmfc\lib\amd64
LDFLAGS = $(LDFLAGS) /LIBPATH:$(SDKDIR)\lib\x64
!ELSE
LDFLAGS = $(LDFLAGS) /LIBPATH:$(DDKDIR)\lib\crt\amd64
LDFLAGS = $(LDFLAGS) /LIBPATH:$(DDKDIR)\lib\win7\amd64
!ENDIF
!ELSE
CFLAGS = $(CFLAGS) /D_X86_=1 /Di386=1
!IF "$(USE_SDK)" == "YES"
LDFLAGS = $(LDFLAGS) /LIBPATH:$(VSDIR)\vc\lib
LDFLAGS = $(LDFLAGS) /LIBPATH:$(SDKDIR)\lib
!ELSE
LDFLAGS = $(LDFLAGS) /LIBPATH:$(DDKDIR)\lib\crt\i386
LDFLAGS = $(LDFLAGS) /LIBPATH:$(C)\VS10_x86\lib
!ENDIF
!ENDIF

###

!IF "$(TARGET_ARCH)" == "amd64"
CC = $(VSDIR)\vc\bin\x86_amd64\cl.exe
CXX = $(VSDIR)\vc\bin\x86_amd64\cl.exe
LD = $(VSDIR)\vc\bin\x86_amd64\link.exe
AS = $(VSDIR)\vc\bin\x86_amd64\ml.exe
!IF "$(USE_SDK)" == "YES"
!ELSE
CC = %DDKDIR%\bin\x86\amd64\cl.exe
CXX = %DDKDIR%\bin\x86\amd64\cl.exe
LD = %DDKDIR%\bin\x86\amd64\link.exe
AS = %DDKDIR%\bin\x86\amd64\ml.exe
!ENDIF
!ELSE
!IF "$(USE_SDK)" == "YES"
CC = $(VSDIR)\vc\bin\cl.exe
CXX = $(VSDIR)\vc\bin\cl.exe
LD = $(VSDIR)\vc\bin\link.exe
AS = $(VSDIR)\vc\bin\ml.exe
!ELSE
CC = %DDKDIR%\bin\x86\x86\cl.exe
CXX = %DDKDIR%\bin\x86\x86\cl.exe
LD = %DDKDIR%\bin\x86\x86\link.exe
AS = %DDKDIR%\bin\x86\ml.exe
!ENDIF
!ENDIF

!IF "$(USE_SDK)" == "YES"
SIGNTOOL = $(DDKDIR)\bin\x86\SignTool.exe
!ELSE
RC = %DDKDIR%\bin\x86\rc.exe
SIGNTOOL = $(DDKDIR)\bin\x86\SignTool.exe
!ENDIF

OBJS = $(SRCS:.c=.obj) $(CPPS:.cpp=.obj) $(CXXS:.cxx=.obj) \
    $(ASMS:.asm=.obj) $(RCS:.rc=.res)

!IFDEF BUILD_HYBRID
CFLAGS  = $(CFLAGS) /DBUILD_HYBRID
!ENDIF

.SUFFIXES: .c .cpp .asm .obj .rc

.c.obj:
	$(CC) $(CFLAGS) /c /Fo$@ $<

.cpp.obj:
	$(CXX) $(CFLAGS) $(CXXFLAGS) /c /Fo$@ $<

.cxx.obj:
	$(CXX) $(CFLAGS) $(CXXFLAGS) /c /Fo$@ $<

.asm.obj:
	$(AS) $(ASFLAGS) /c /Fo$@ $<

.rc.res:
	$(RC) $(RCFLAGS) /fo$@ $<
