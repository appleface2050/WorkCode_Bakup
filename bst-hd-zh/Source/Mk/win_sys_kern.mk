!INCLUDE win_env_kern.mk

LDFLAGS = $(LDFLAGS) /MERGE:_PAGE=PAGE
LDFLAGS = $(LDFLAGS) /MERGE:_TEXT=.text
LDFLAGS = $(LDFLAGS) /SECTION:INIT,d
LDFLAGS = $(LDFLAGS) /OPT:REF
LDFLAGS = $(LDFLAGS) /OPT:ICF
LDFLAGS = $(LDFLAGS) /INCREMENTAL:NO
LDFLAGS = $(LDFLAGS) /release
LDFLAGS = $(LDFLAGS) /NODEFAULTLIB
LDFLAGS = $(LDFLAGS) /WX
LDFLAGS = $(LDFLAGS) /debugtype:cv,fixup,pdata
LDFLAGS = $(LDFLAGS) /functionpadmin:5
#LDFLAGS = $(LDFLAGS) /safeseh
LDFLAGS = $(LDFLAGS) /pdbcompress
LDFLAGS = $(LDFLAGS) /pdb:$(SYS).pdb
LDFLAGS = $(LDFLAGS) /map:$(SYS).map
LDFLAGS = $(LDFLAGS) /STACK:0x40000,0x1000
LDFLAGS = $(LDFLAGS) /driver
LDFLAGS = $(LDFLAGS) /base:0x10000
LDFLAGS = $(LDFLAGS) /entry:GsDriverEntry

LDFLAGS = $(LDFLAGS) /ignore:4078

!IF "$(TARGET_ARCH)" == "amd64"
LDFLAGS = $(LDFLAGS) /machine:X64
LDFLAGS = $(LDFLAGS) /osversion:5.2
LDFLAGS = $(LDFLAGS) /subsystem:native,5.02
!ELSE
LDFLAGS = $(LDFLAGS) /machine:X86
LDFLAGS = $(LDFLAGS) /osversion:5.1
LDFLAGS = $(LDFLAGS) /subsystem:native,5.01
!ENDIF

all: $(SYS).sys

fxcop:

$(SYS).sys: $(OBJS) Version.res $(LDADD)
	$(LD) /nologo /out:$@ $(LDFLAGS) $(OBJS) Version.res \
	    $(LDADD) $(LDADD_OS)
	FOR /L %%A IN (1,1,5) DO @(\
		IF DEFINED SIGN_PACKAGE ( IF DEFINED USE_SHA2_SIGNING (\
			$(SIGNTOOL81) sign /s MY /a /ac $(S)\Certs\MSCV-VSClass3.cer /n "BlueStack Systems, Inc." \
			/fd sha1 /t "http://timestamp.verisign.com/scripts/timestamp.dll" /v $@ \
			&& $(SIGNTOOL81) sign /s MY /ac $(S)\Certs\MSCV-VSClass3.cer /n "BlueStack Systems, Inc." \
			/as /fd sha256 /sha1 F1AECC2B5B251589B4B4009D30201AC01ABBA7DC /tr "http://timestamp.comodoca.com/rfc3161" /td sha256 /v $@ \
			&& IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ELSE (\
			$(SIGNTOOL) sign /s "my" /ac $(S)\Certs\MSCV-VSClass3.cer /sha1 "A81A3732D08E0499A392A5E921816D4A60AB9374" /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			$@ && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ) \
			)

CLEAN = $(CLEAN) $(SYS).lib vc90.pdb

clean:
	DEL /F $(SYS).sys $(SYS).pdb $(SYS).map $(OBJS) Version.res
	DEL /F $(CLEAN)
