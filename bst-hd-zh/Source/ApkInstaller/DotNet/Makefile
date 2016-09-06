PROG = HD-ApkHandler
SRCS = \
       $(SUL)\Version.cs \
       $(SUL)\Locale\DotNet\Strings.cs \
       $(SUL)\Common\DotNet\Logger.cs \
       $(SUL)\Common\DotNet\Agent.cs \
       $(SUL)\Common\DotNet\InstallerCodes.cs \
       $(SUL)\Common\DotNet\Oem.cs \
       $(SUL)\Common\DotNet\Utils.cs \
       $(SUL)\Common\DotNet\Strings.cs \
       $(SUL)\Common\DotNet\Interop_Window.cs \
       $(SUL)\Common\DotNet\GuestNetwork.cs \
       $(SUL)\Common\DotNet\VmCmdHandler.cs \
       $(SUL)\Common\DotNet\HTTP.cs \
       $(SUL)\Common\DotNet\User.cs \
       $(SUL)\Common\DotNet\RandomGenerator.cs \
       $(SUL)\Common\DotNet\Features.cs \
       $(SUL)\Common\DotNet\UIHelper.cs \
       $(SUL)\Common\DotNet\IniFile.cs \
       $(SUL)\Common\DotNet\GraphicsDriverData.cs \
       $(SUL)\Cloud\DotNet\Services.cs \
       $(SUL)\Common\DotNet\Secure.cs \
       $(SUL)\Device\DotNet\Profile.cs \
       $(SUL)\Common\DotNet\GetOpt.cs \
       $(SUL)\Common\DotNet\JsonParser.cs \
       HDApkInstaller.cs \
       AssemblyInfo.cs 

CSFLAGS = $(CSFLAGS) /reference:$(C)\JSON\JSON.dll
FXCOPFLAGS = $(FXCOPFLAGS) /reference:$(C)\JSON\JSON.dll
CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe
CSFLAGS = $(CSFLAGS) /main:BlueStacks.hyperDroid.ApkInstaller.HDApkInstaller

!INCLUDE $(M)\win_csharp.mk
