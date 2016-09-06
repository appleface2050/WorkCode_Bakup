PROG = HD-StartLauncher
SRCS = \
       $(SUL)\Version.cs \
       $(SUL)\Common\DotNet\Logger.cs \
	   $(SUL)\Common\DotNet\Interop_Window.cs \
       $(SUL)\Common\DotNet\Features.cs \
       $(SUL)\Common\DotNet\Utils.cs \
       $(SUL)\Common\DotNet\Strings.cs \
       $(SUL)\Locale\DotNet\Strings.cs \
       $(SUL)\Common\DotNet\Oem.cs \
       $(SUL)\Common\DotNet\HTTP.cs \
       $(SUL)\Common\DotNet\User.cs \
       $(SUL)\Device\DotNet\Profile.cs \
       $(SUL)\Common\DotNet\GuestNetwork.cs \
       $(SUL)\Common\DotNet\VmCmdHandler.cs \
       $(SUL)\Common\DotNet\IniFile.cs \
       $(SUL)\Common\DotNet\UIHelper.cs \
       $(SUL)\Common\DotNet\GraphicsDriverData.cs \
       $(SUL)\Cloud\DotNet\Services.cs \
       $(SUL)\Common\DotNet\Secure.cs \
       AssemblyInfo.cs \
       StartLauncher.cs

FXCOPFLAGS = $(FXCOPFLAGS) /reference:$(C)\JSON\JSON.dll
CSFLAGS = $(CSFLAGS) /reference:$(C)\JSON\JSON.dll
CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe

!INCLUDE $(M)\win_csharp.mk
