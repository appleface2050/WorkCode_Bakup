PROG = HD-Restart
SRCS = \
    $(SUL)\Version.cs \
    $(SUL)\Common\DotNet\Logger.cs \
    $(SUL)\Common\DotNet\Oem.cs \
    $(SUL)\Common\DotNet\Utils.cs \
    $(SUL)\Common\DotNet\Strings.cs \
    $(SUL)\Common\DotNet\Interop_Window.cs \
    $(SUL)\Common\DotNet\HTTP.cs \
    $(SUL)\Common\DotNet\User.cs \
    $(SUL)\Common\DotNet\VmCmdHandler.cs \
    $(SUL)\Common\DotNet\GuestNetwork.cs \
    $(SUL)\Common\DotNet\Features.cs \
    $(SUL)\Common\DotNet\UIHelper.cs \
    $(SUL)\Common\DotNet\IniFile.cs \
    $(SUL)\Common\DotNet\GraphicsDriverData.cs \
    $(SUL)\Cloud\DotNet\Services.cs \
    $(SUL)\Common\DotNet\Secure.cs \
    $(SUL)\Device\DotNet\Profile.cs \
    $(SUL)\Locale\DotNet\Strings.cs \
    Restart.cs \
    AssemblyInfo.cs

CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe
CSFLAGS = $(CSFLAGS) /reference:$(C)\JSON\Json.dll
FXCOPFLAGS = $(FXCOPFLAGS) /reference:$(C)\JSON\Json.dll

!INCLUDE $(M)\win_csharp.mk
