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

!IF "$(ICON_FILE)" == ""
CSFLAGS = $(CSFLAGS) /win32icon:$(SUL)\Package\Resources\BlueStacks.ico
!ELSE
CSFLAGS = $(CSFLAGS) /win32icon:$(ICON_FILE)
!ENDIF

SIGNTOOL = $(DDKDIR)\bin\x86\SignTool.exe

!IF "$(BUILD_TYPE)" == "DBG"
CSFLAGS = $(CSFLAGS) /optimize-
!ELSE
CSFLAGS = $(CSFLAGS) /optimize+
!ENDIF

!IFDEF BUILD_HYBRID
CSFLAGS = $(CSFLAGS) /define:BUILD_HYBRID
!ENDIF

!IF "$(DOTNETVER)" == "35"
CSFLAGS = $(CSFLAGS) /define:BUGSNAG
!ENDIF

!IFDEF MULTI_INS
CSFLAGS = $(CSFLAGS) /define:MULTI_INS
!ENDIF

all: $(PROG).exe

fxcop: $(PROG).exe.fxcop.xml


$(PROG).exe: $(SRCS)
	$(CSC) $(RESOURCES) /nologo $(CSFLAGS) /out:$@ $(SRCS)
	if exist App.manifest $(C)\Tools\mt -manifest App.manifest -outputresource:$(PROG).exe
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

$(PROG).exe.fxcop.xml: $(PROG).exe
	"$(FXCOP)" /f:$(PROG).exe $(FXCOPFLAGS) /o:$@

clean:
        del /f $(PROG).exe $(PROG).pdb $(PROG).exe.fxcop.xml
