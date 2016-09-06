!INCLUDE win_env_user.mk

LDFLAGS = $(LDFLAGS) /WX
LDFLAGS = $(LDFLAGS) /osversion:5.1
LDFLAGS = $(LDFLAGS) /pdbcompress
LDFLAGS = $(LDFLAGS) /pdb:$(PROG).pdb
LDFLAGS = $(LDFLAGS) /map:$(PROG).map
!IF "$(TARGET_ARCH)" == "amd64"
!IFDEF SUBSYSTEM_WIN
LDFLAGS = $(LDFLAGS) /subsystem:windows,5.02
!ELSE
LDFLAGS = $(LDFLAGS) /subsystem:console,5.02
!ENDIF
!ELSE
!IFDEF SUBSYSTEM_WIN	
LDFLAGS = $(LDFLAGS) /subsystem:windows,5.01
!ELSE
LDFLAGS = $(LDFLAGS) /subsystem:console,5.01
!ENDIF
!ENDIF
LDFLAGS = $(LDFLAGS) /libpath:$(DDKDIR)\lib\win7\i386

!IF "$(BUILD_TYPE)" == "DBG"
CFLAGS = $(CFLAGS) /MTd
!ELSE
CFLAGS = $(CFLAGS) /MT
!ENDIF

all: $(PROG).exe

$(PROG).exe: $(OBJS) $(LDADD)
	$(LD) /nologo /out:$@ $(LDFLAGS) $(OBJS) $(LDADD) $(LDADD_OS)
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


CLEAN = $(CLEAN) vc90.pdb

clean:
	DEL /F $(PROG).exe $(PROG).ilk $(PROG).exe.manifest $(OBJS)
	DEL /F $(PROG).pdb $(PROG).map
	DEL /F $(CLEAN)
