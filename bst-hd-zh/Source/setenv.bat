@ECHO OFF

IF "X%1" == "X" GOTO :USAGE
IF NOT "X%2" == "X" GOTO :USAGE

SET OEM_NAME=gamemanager
SET THREEBT=1

SET BUILD_TYPE=
IF "X%1" == "Xrel" SET BUILD_TYPE=REL
IF "X%1" == "Xdbg" SET BUILD_TYPE=DBG
IF "X%BUILD_TYPE%" == "X" GOTO :USAGE

IF NOT "X%IFSKIT_INC_PATH%" == "X" (
	ECHO ERROR: You are currently using a Windows DDK build environment
	ECHO        window.  Please try again from vanilla cmd.exe window.
	EXIT /B 1
)

SET DDKDIR=c:\WinDDK\7600.16385.1
DIR %DDKDIR%\bin\setenv.bat > NUL 2> NUL
IF ERRORLEVEL 1 (
	ECHO ERROR: Cannot find a DDK at %DDKDIR%
	EXIT /B 1
)

IF "X%WIX%" == "X" (
	ECHO ERROR: Cannot find WIX
	EXIT /B 1
)

DIR "%WIX%" > NUL 2> NUL
IF ERRORLEVEL 1 (
	ECHO ERROR: Cannot find WIX at "%WIX%"
	EXIT /B 1
)

SET S=%~dp0
SET SUL=%~dp0
SET C=%S%..\Contrib
SET GL=%S%..\..\..\suman\gl
SET M=%S%Mk
CALL %S%version.bat
CALL %S%Tool\GuestCommandRunner\Dotnet\vm_command.bat

doskey bstmake=%DDKDIR%\bin\x86\nmake.exe /nologo $*

ECHO Build Configuration
ECHO.
ECHO     DDKDIR                 = %DDKDIR%
ECHO     WIX                    = %WIX%
ECHO     BUILD_TYPE             = %BUILD_TYPE%
ECHO     PROFILING_ENABLED      = %PROFILING_ENABLED%
ECHO.
ECHO     VERSION_MAJOR          = %VERSION_MAJOR%
ECHO     VERSION_MINOR          = %VERSION_MINOR%
ECHO     VERSION_PATCH          = %VERSION_PATCH%
ECHO     VERSION_BUILD          = %VERSION_BUILD%
ECHO     THREEBT                = %THREEBT%
ECHO.
ECHO Run 'bstmake all package' to build everything.

GOTO END

:USAGE
	ECHO.
	ECHO Usage: setenv.bat ^<rel^|dbg^>
	ECHO.
	ECHO Environment Variables:
	ECHO.
	ECHO     PROFILING_ENABLED - YES to enable profiling
	EXIT /B 1
GOTO END

:END
