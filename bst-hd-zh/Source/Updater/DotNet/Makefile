PROG = HD-Test-Updater
SRCS = \
       $(SUL)\Common\DotNet\Logger.cs \
       $(SUL)\Common\DotNet\IniFile.cs \
       $(SUL)\Common\DotNet\User.cs \
       $(SUL)\Common\DotNet\Strings.cs \
       $(SUL)\Common\DotNet\Utils.cs \
	   $(SUL)\Locale\DotNet\Strings.cs \
	   $(SUL)\Common\DotNet\Oem.cs \
       $(SUL)\Version.cs \
       $(SUL)\Common\DotNet\MessageBox.cs \
       Manager.cs \
       $(SUL)\Common\DotNet\Downloader.cs \
       Manifest.cs \
       Test.cs \
       AssemblyInfo.cs

CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /unsafe
CSFLAGS = $(CSFLAGS) /target:exe

!INCLUDE $(M)\win_csharp.mk
