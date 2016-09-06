PROG = HD-SetUserGuid
SRCS = \
    $(SUL)\Version.cs \
	$(SUL)\Common\DotNet\LoggerManaged.cs \
    $(SUL)\Common\DotNet\Strings.cs \
    $(SUL)\Common\DotNet\User.cs \
    Program.cs

CSFLAGS = $(CSFLAGS)
FXCOPFLAGS = $(FXCOPFLAGS)
CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe

!INCLUDE $(M)\win_csharp.mk
