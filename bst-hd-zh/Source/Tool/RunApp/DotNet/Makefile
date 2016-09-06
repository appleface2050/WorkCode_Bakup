PROG = HD-RunApp
SRCS = \
    $(SUL)\Version.cs \
    $(SUL)\Common\DotNet\Agent.cs \
    $(SUL)\Common\DotNet\Logger.cs \
    $(SUL)\Common\DotNet\Strings.cs \
    $(SUL)\Locale\DotNet\Strings.cs \
    $(SUL)\Common\DotNet\Oem.cs \
    $(SUL)\Common\DotNet\Utils.cs \
    $(SUL)\Common\DotNet\Interop_Window.cs \
    $(SUL)\Common\DotNet\HTTP.cs \
    $(SUL)\Common\DotNet\JsonParser.cs \
    $(SUL)\Common\DotNet\User.cs \
    $(SUL)\Common\DotNet\RandomGenerator.cs \
    $(SUL)\Common\DotNet\Features.cs \
    $(SUL)\Device\DotNet\Profile.cs \
    $(SUL)\Common\DotNet\GuestNetwork.cs \
    $(SUL)\Common\DotNet\VmCmdHandler.cs \
    $(SUL)\Common\DotNet\UIHelper.cs \
    $(SUL)\Common\DotNet\GraphicsDriverData.cs \
    $(SUL)\Common\DotNet\ProgressBar.cs \
    $(SUL)\Cloud\DotNet\Services.cs \
    $(SUL)\Common\DotNet\Secure.cs \
    $(SUL)\Common\DotNet\GetOpt.cs \
    $(SUL)\Common\DotNet\IniFile.cs \
    RunApp.cs \
    AssemblyInfo.cs


CSFLAGS = $(CSFLAGS) /reference:$(C)\JSON\JSON.dll
FXCOPFLAGS = $(FXCOPFLAGS) /reference:$(C)\JSON\JSON.dll
CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe

!INCLUDE $(M)\win_csharp.mk
