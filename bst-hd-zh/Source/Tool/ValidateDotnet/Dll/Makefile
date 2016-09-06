
vspath=$(VS100COMNTOOLS)..\..
!if "$(BUILD_TYPE)" == "DBG"
DBGP=DBG=1 
!endif

all:
	SETLOCAL
	call "$(vspath)\VC\vcvarsall.bat" x86 & nmake -f VsMakefile all $(DBGP)
	ENDLOCAL
clean:
	SETLOCAL
	call "$(vspath)\VC\vcvarsall.bat" x86 & nmake -f VsMakefile clean $(DBGP)
	ENDLOCAL
