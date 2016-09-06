PROG = HD-Frontend
SRCS = \
    Program.cs \
    FrontendForm.cs \
    FrontendForm.Designer.cs \
    AssemblyInfo.cs

CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /unsafe
CSFLAGS = $(CSFLAGS) /target:winexe
CSFLAGS = $(CSFLAGS) /reference:$(SUL)\Frontend\Console\Dotnet\HD-Frontend-UserControl.dll
!IF "$(DOTNETVER)" == "35"
CSFLAGS = $(CSFLAGS) /reference:$(C)\Bugsnag\Bugsnag.dll
!ENDIF

!INCLUDE $(M)\win_csharp.mk
