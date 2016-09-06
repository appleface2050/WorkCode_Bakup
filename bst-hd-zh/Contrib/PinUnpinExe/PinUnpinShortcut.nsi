OutFile "C:\Users\Vivek\Documents\PinUnpinShortcut.exe"

!include 'StdUtils.nsh'
!include FileFunc.nsh

SilentInstall silent
RequestExecutionLevel user ;no elevation needed
ShowInstDetails hide

var inputParam
Section
    ${GetParameters} $inputParam
	System::Call "kernel32::GetCurrentDirectory(i ${NSIS_MAX_STRLEN}, t .r0)"
	${StdUtils.InvokeShellVerb} $0 "$0" "BlueStacks.exe" $inputParam
SectionEnd


# "StdUtils.Const.ShellVerb.PinToTaskbar"="0"
# "StdUtils.Const.ShellVerb.UnpinFromTaskbar"="1"
# "StdUtils.Const.ShellVerb.PinToStart"="2"
# "StdUtils.Const.ShellVerb.UnpinFromStart"="3"