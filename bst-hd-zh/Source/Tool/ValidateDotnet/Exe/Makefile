PROG = HD-ValidateDotnetExe-Native

SRCS =  

CPPS = \
    CValidateDotneteExe.cpp \
    ValidateDotnetExe.cpp

CFLAGS = $(CFLAGS) -DUNICODE -D_UNICODE -DWIN32  -D_WINDOWS
CFLAGS = $(CFLAGS) /I$(DDKDIR)\inc\api\crt\stl70 
CFLAGS = $(CFLAGS) /EHsc /D_STL70_ /D_STATIC_CPPLIB 
CFLAGS = $(CFLAGS) /wd4800 /wd4244 
CFLAGS = $(CFLAGS) /I$(S)\Common\Native 

LDADD_OS = \
    $(MSVCRTLIB) \
    $(MSVCPRTLIB) \
	HD-ValidateDotnetDll-Native.lib

!INCLUDE $(M)\win_prog_user.mk
