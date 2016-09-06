!IF "$(TARGET_ARCH)" == "amd64"
CSFLAGS = $(CSFLAGS) /platform:x64
!IF "$(DOTNETVER)" == "20"
DOTNET = C:\Windows\Microsoft.NET\Framework64\v2.0.50727
!ELSE
DOTNET = C:\Windows\Microsoft.NET\Framework64\v3.5
!ENDIF
!ELSE
!IF "$(DOTNETVER)" == "20"
DOTNET = C:\Windows\Microsoft.NET\Framework\v2.0.50727
!ELSE
DOTNET = C:\Windows\Microsoft.NET\Framework\v3.5
!ENDIF
!ENDIF

CSC = $(DOTNET)\csc.exe
FXCOP = C:\Program Files (x86)\Microsoft Fxcop 10.0\FxCopCmd.exe
FXCOPFLAGS = $(FXCOPFLAGS) /v

CSFLAGS = $(CSFLAGS) /win32icon:$(SUL)\Package\Resources\BlueStacks.ico
SIGNTOOL = $(DDKDIR)\bin\x86\SignTool.exe

!IF "$(BUILD_TYPE)" == "DBG"
CSFLAGS = $(CSFLAGS) /optimize-
!ELSE
CSFLAGS = $(CSFLAGS) /optimize+
!ENDIF

!IFDEF BUILD_HYBRID
CSFLAGS = $(CSFLAGS) /define:BUILD_HYBRID
!ENDIF

CSFLAGS = $(CSFLAGS) /target:library

all: $(DLL).dll

fxcop: $(DLL).dll.fxcop.xml

$(DLL).dll: $(SRCS)
        $(CSC) /nologo $(CSFLAGS) /out:$@ $(SRCS)

	FOR /L %%A IN (1,1,5) DO @(\
		IF DEFINED SIGN_PACKAGE ( IF DEFINED USE_SHA2_SIGNING ( \
			$(SIGNTOOL) sign /s "my" /ac $(S)\Certs\MSCV-VSClass3.cer /sha1 "F1AECC2B5B251589B4B4009D30201AC01ABBA7DC" /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			$@ && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ELSE (\
			IF DEFINED JENKINS_BUILD ( \
			$(SIGNTOOL) sign /sm /s Root /ac $(S)\Certs\MSCV-VSClass3.cer /a /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			$@ && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ELSE (\
			$(SIGNTOOL) sign /s "my" /ac $(S)\Certs\MSCV-VSClass3.cer /a /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			$@ && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ) ) \
	)

$(DLL).dll.fxcop.xml: $(DLL).dll
	"$(FXCOP)" /f:$(DLL).dll $(FXCOPFLAGS) /o:$@

clean:
        del /f $(DLL).dll $(DLL).pdb $(DLL).dll.fxcop.xml
