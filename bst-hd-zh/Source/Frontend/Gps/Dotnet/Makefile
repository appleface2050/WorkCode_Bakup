PROG = HD-Gps
SRCS = \
    $(SUL)\Common\DotNet\Logger.cs \
    $(SUL)\Common\DotNet\MonitorLocator.cs \
!IFDEF BUILD_HD
    $(SUL)\Frontend\Console\DotNet\Console.cs \
    Gps_HD.cs \
!ELSE
    $(SUL)\Frontend\Console\DotNet\Console_VBox.cs \
    Gps_VBox.cs \
!ENDIF
    AssemblyInfo.cs

CSFLAGS = $(CSFLAGS) /debug:full /unsafe

!INCLUDE $(M)\win_csharp.mk
