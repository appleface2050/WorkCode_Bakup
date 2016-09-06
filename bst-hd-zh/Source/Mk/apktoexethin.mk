ICON_FILE = app.ico

RESOURCES = $(RESOURCES) /res:..\..\..\..\installerMsi.resources
RESOURCES = $(RESOURCES) /res:installerImage.resources
RESOURCES = $(RESOURCES) /res:whiteLogo.resources
RESOURCES = $(RESOURCES) /res:xButton.resources
RESOURCES = $(RESOURCES) /res:productLogo.resources
RESOURCES = $(RESOURCES) /res:whiteFullScreen.resources

CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe
CSFLAGS = $(CSFLAGS) /reference:$(C)\JSON\JSON.dll
CSFLAGS = $(CSFLAGS) /define:ApkThinInstaller

SOURCE_FILES = \
       $(SUL)\Version.cs \
       $(SUL)\Common\DotNet\Utils.cs \
	   $(SUL)\Common\DotNet\Oem.cs \
       $(SUL)\Common\DotNet\User.cs \
       $(SUL)\Common\DotNet\Logger.cs \
       $(SUL)\Common\DotNet\Strings.cs \
       $(SUL)\Common\DotNet\HTTP.cs \
       $(SUL)\Common\DotNet\Features.cs \
       $(SUL)\Common\DotNet\SplitDownloader.cs \
       $(SUL)\Common\DotNet\SplitFile.cs \
       $(SUL)\Common\DotNet\SerialWorkQueue.cs \
       $(SUL)\Common\DotNet\GuestNetwork.cs \
       $(SUL)\Common\DotNet\VmCmdHandler.cs \
       $(SUL)\Common\DotNet\GraphicsDriverData.cs \
       $(SUL)\Common\DotNet\UIHelper.cs \
       $(SUL)\Common\DotNet\Interop_Window.cs \
       $(SUL)\Device\DotNet\Profile.cs \
       $(SUL)\Common\DotNet\RandomGenerator.cs \
       $(SUL)\Common\DotNet\IniFile.cs \
       $(SUL)\Common\DotNet\LoadingScreen.cs \
       $(SUL)\Common\DotNet\Secure.cs \
       $(SUL)\Cloud\DotNet\Services.cs \
       $(SUL)\Locale\DotNet\Strings.cs \
       $(S)\ApkThinInstaller\DotNet\ApkThinInstaller.cs \
       $(S)\ApkThinInstaller\DotNet\ApkThinInstallerUi.cs \
       ApkStrings.cs \
       AssemblyInfo.cs

all: $(PROG)_native.exe

clean:
	del 7zSDcfg.txt $(PROG).exe $(PROG).7z $(PROG)_native.exe $(PROG).7z.exe $(PROG).exe.config $(PROG).pdb

!INCLUDE $(M)\win_csharp.mk

$(PROG).exe.config: $(SUL)\Sfx\exe.config
	-DEL $@
	COPY $(SUL)\Sfx\exe.config "$@"

$(PROG).7z: $(PROG).exe $(PROG).exe.config
	-DEL $@
	$(C)\7z920_extra\7zr.exe a $@ -m0=LZMA:a=2 $(PROG).exe $(PROG).exe.config $(S)\Core\Logger\HD-Logger-Native.dll

$(PROG)_native.exe: $(PROG).7z 7zSDcfg.txt
	-DEL $@
	$(C)\Tools\mt -manifest App.manifest -outputresource:$(SUL)\Sfx\BlueStacks.sfx
	COPY /Y /B $(SUL)\Sfx\BlueStacks.sfx + 7zSDcfg.txt + $(PROG).7z "$@"
	FOR /L %%A IN (1,1,5) DO @(\
		IF DEFINED SIGN_PACKAGE ( IF DEFINED USE_SHA2_SIGNING ( \
			$(SIGNTOOL) sign /s "my" /ac $(S)\Certs\MSCV-VSClass3.cer /sha1 "F1AECC2B5B251589B4B4009D30201AC01ABBA7DC" /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			"$@" && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ELSE (\
			IF DEFINED JENKINS_BUILD ( \
			$(SIGNTOOL) sign /sm /s Root /ac $(S)\Certs\MSCV-VSClass3.cer /a /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			"$@" && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ELSE (\
			$(SIGNTOOL) sign /s "my" /ac $(S)\Certs\MSCV-VSClass3.cer /a /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			"$@" && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ) ) \
	)

7zSDcfg.txt: $(SUL)\Sfx\7zSDcfg.txt.in
	-DEL $@
	cscript.exe /nologo $(SUL)\Sfx\Generate7zSDcfg.js $(PROG).exe \
	    < $(SUL)\Sfx\7zSDcfg.txt.in > 7zSDcfg.txt.tmp
	MOVE 7zSDcfg.txt.tmp 7zSDcfg.txt

