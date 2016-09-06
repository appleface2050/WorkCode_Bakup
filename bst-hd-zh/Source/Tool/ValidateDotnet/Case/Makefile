PROG = HD-CaseDotnet

SRCS = \
       CaseData.cs \
       CaseDotnetAttribute.cs \
       CaseManagement.cs \
       CaseSecurity.cs \
       CaseServiceProcess.cs \
       CaseUtils.cs \
       CaseWeb.cs \
       CaseWindows.cs \
       CaseXml.cs \
       Program.cs

FXCOPFLAGS = $(FXCOPFLAGS)
CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe
CSFLAGS = $(CSFLAGS) /main:CaseDotnet.Program


!INCLUDE $(M)\win_csharp.mk
