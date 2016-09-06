!INCLUDE win_env_user.mk

LDFLAGS = $(LDFLAGS) /WX
LDFLAGS = $(LDFLAGS) /osversion:5.1
LDFLAGS = $(LDFLAGS) /pdbcompress
LDFLAGS = $(LDFLAGS) /pdb:$(DLL).pdb
LDFLAGS = $(LDFLAGS) /map:$(DLL).map
LDFLAGS = $(LDFLAGS) /libpath:$(DDKDIR)\lib\win7\i386

!IF "$(BUILD_TYPE)" == "DBG"
CFLAGS = $(CFLAGS) /MTd
!ELSE
CFLAGS = $(CFLAGS) /MT
!ENDIF

all: $(DLL).dll

fxcop:

$(DLL).dll: $(OBJS) Version.res $(LDADD)
	$(LD) /nologo /out:$@ /DLL $(LDFLAGS) $(OBJS) Version.res \
	    $(LDADD) $(LDADD_OS)

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

CLEAN = $(CLEAN) vc90.pdb

clean:
	DEL /F $(DLL).dll $(DLL).pdb $(DLL).ilk $(OBJS)
	DEL /F $(DLL).exp $(DLL).lib $(DLL).map Version.res
	DEL /F $(CLEAN)
