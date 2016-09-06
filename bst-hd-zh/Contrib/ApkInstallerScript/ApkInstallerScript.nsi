!include LogicLib.nsh
!include WinMessages.nsh


SilentInstall silent
RequestExecutionLevel user ;no elevation needed for this test
ShowInstDetails hide

# this will be the created executable archive
OutFile "ClashRoyale.exe"

InstallDir $EXEDIR


# the executable part
Section

# define the output path for the following files
SetOutPath $TEMP\ApkRoyale

# define what to install and place it in the output path...
# ...your app...
File AppInstaller.exe
# ...and the library.
File Royale.apk

# run application
ExecWait "$TEMP\ApkRoyale\AppInstaller.exe"

SetOutPath $TEMP
# remove temp apk folder recursively
RMDir /r $TEMP\ApkRoyale

# done
SectionEnd
