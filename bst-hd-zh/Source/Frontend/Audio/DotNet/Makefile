PROG = HD-Audio
SRCS = \
    $(SUL)\Version.cs \
    $(SUL)\Common\DotNet\Logger.cs \
    $(SUL)\Common\DotNet\MonitorLocator.cs \
    $(SUL)\Frontend\DotNet\Console.cs \
    Audio.cs \
    AssemblyInfo.cs

CSFLAGS = $(CSFLAGS) /debug:full /unsafe /reference:$(C)\NAudio\NAudio.dll
CSFLAGS = $(CSFLAGS) /platform:x86

!INCLUDE $(M)\win_csharp.mk
