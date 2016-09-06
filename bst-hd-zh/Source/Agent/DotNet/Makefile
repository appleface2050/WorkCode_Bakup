PROG = HD-Agent

SRCS = \
       $(SUL)\Version.cs \
       $(SUL)\Locale\DotNet\Strings.cs \
       $(SUL)\Device\DotNet\Profile.cs \
       $(SUL)\Common\DotNet\Logger.cs \
       $(SUL)\Common\DotNet\User.cs \
       $(SUL)\Common\DotNet\IniFile.cs \
       $(SUL)\Common\DotNet\Agent.cs \
       $(SUL)\Common\DotNet\GuestNetwork.cs \
       $(SUL)\Common\DotNet\VmCmdHandler.cs \
       $(SUL)\Common\DotNet\Interop_Window.cs \
       $(SUL)\Common\DotNet\Strings.cs \
       $(SUL)\Common\DotNet\MessageBox.cs \
       $(SUL)\Common\DotNet\HTTP.cs \
       $(SUL)\Cloud\DotNet\Services.cs \
       $(SUL)\Common\DotNet\Secure.cs \
       $(SUL)\Common\DotNet\InstallerCodes.cs \
       $(SUL)\Common\DotNet\Oem.cs \
       $(SUL)\Common\DotNet\Utils.cs \
       $(SUL)\Common\DotNet\FrontendBanner.cs \
       $(SUL)\Common\DotNet\JsonParser.cs \
       $(SUL)\Common\DotNet\CustomTrayAlert.cs \
       $(SUL)\Common\DotNet\Stats.cs \
       $(SUL)\Common\DotNet\Interop_UUID.cs \
       $(SUL)\Common\DotNet\RandomGenerator.cs \
       $(SUL)\Common\DotNet\HTTPUtils.cs \
       $(SUL)\Common\DotNet\Features.cs \
       $(SUL)\Common\DotNet\IniFile.cs \
       $(SUL)\Common\DotNet\GMAppsManager.cs \
       $(SUL)\Common\DotNet\UIHelper.cs \
       $(SUL)\Updater\DotNet\Manifest.cs \
       $(SUL)\Updater\DotNet\Manager.cs \
       $(SUL)\Common\DotNet\SplitDownloader.cs \
       $(SUL)\Common\DotNet\SplitFile.cs \
       $(SUL)\Common\DotNet\SerialWorkQueue.cs \
       $(SUL)\Common\DotNet\GraphicsDriverData.cs \
       $(SUL)\Common\DotNet\KeyboardSend.cs \
       SysTray.cs \
       HDAgent.cs \
       AssemblyInfo.cs \
       AppUninstaller.cs \
       InstallApk.cs \
       HTTPHandler.cs \
       CloudAnnouncement.cs \
       TimelineStats.cs \
       ClipboardMgr.cs

FXCOPFLAGS = $(FXCOPFLAGS) /reference:$(C)\JSON\JSON.dll /reference:$(C)\DevComponents\bin\DevComponents.DotNetBar2.dll
CSFLAGS = $(CSFLAGS) /reference:$(C)\JSON\JSON.dll /reference:$(C)\DevComponents\bin\DevComponents.DotNetBar2.dll
CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe
CSFLAGS = $(CSFLAGS) /main:BlueStacks.hyperDroid.Agent.HDAgent


!INCLUDE $(M)\win_csharp.mk
