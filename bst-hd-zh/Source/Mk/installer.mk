
all: preparezip $(PROG)_$(ANGLE)native.exe

prepareZip:
	rm -rf DataDir
	rm -f DataDir.zip
	rm -f $(S)GameManager\HTML\GameManager\HTML\Thumbs.db
	rm -f $(S)GameManager\HTML\GameManager\HTML\themes\default_theme\js\Thumbs.db
	rm -f $(S)GameManager\HTML\GameManager\HTML\themes\default_theme\img\Thumbs.db
	rm -f $(S)GameManager\HTML\GameManager\HTML\themes\default_theme\css\Thumbs.db
	mkdir DataDir
	mkdir DataDir\Assets
	mkdir DataDir\Assets\Em
	mkdir DataDir\Assets\Em\Default
	mkdir DataDir\Assets\Toob
	mkdir DataDir\Assets\Toob\Default
	mkdir DataDir\Assets\Common
	mkdir DataDir\UserData
	mkdir DataDir\UserData\Home
	copy /y $(C)\PinUnpinExe\PinUnpinShortcut.exe DataDir
	copy /y $(C)\Geckofx\Geckofx-Core.dll DataDir
	copy /y $(C)\Geckofx\Geckofx-Winforms.dll DataDir
	copy /y $(SUL)\Tool\RPCErrorTroubleShooter\HD-RPCErrorTroubleShooter.exe DataDir
	copy /y $(SUL)\Tool\StuckInitializationTroubleShooter\HD-StuckInitializationTroubleShooter.exe DataDir
	mkdir DataDir\UserData\Cache
	mkdir DataDir\UserData\Home\themes
	mkdir DataDir\UserData\Home\themes\setup
	mkdir DataDir\UserData\Home\themes\default_theme
	mkdir DataDir\xulrunner-sdk
	mkdir DataDir\xulrunner-sdk\Plugins
	mkdir DataDir\xulrunner-sdk\dictionaries
	mkdir DataDir\chrome
	copy /y $(C)\xulrunner-sdk\*.* DataDir\xulrunner-sdk
	copy /y $(C)\xulrunner-sdk\Plugins\*.* DataDir\xulrunner-sdk\Plugins
	copy /y $(C)\xulrunner-sdk\dictionaries\*.* DataDir\xulrunner-sdk\dictionaries
	copy /y $(C)\chrome\*.* DataDir\chrome
	xcopy /y /E $(S)GameManager\HTML\GameManager\HTML\themes\setup DataDir\UserData\Home\themes\setup
	xcopy /y /E $(S)GameManager\HTML\GameManager\HTML\themes\default_theme DataDir\UserData\Home\themes\default_theme
	copy /y $(S)GameManager\HTML\GameManager\HTML\themes.json DataDir\UserData\Home
!IF "$(TYPE)" == "BTV"
	mkdir DataDir\OBS
	xcopy /y /E $(C)\OBS_Release DataDir\OBS
	copy /y $(C)\OBS\Debug\OBS.exe DataDir\OBS\HD-OBS.exe
	copy /y $(C)\OBS\OBSApi\Debug\OBSApi.dll DataDir\OBS\OBSApi.dll
!ENDIF
	CMD /C "CD $(SUL)GameManager\DotNet && $(MAKE) /$(MAKEFLAGS) clean all TYPE=$(TYPE) LOCATION=$(LOCATION)" || EXIT /B 1
	copy /y $(SUL)GameManager\DotNet\BlueStacks.exe DataDir
	copy /y $(S)Frontend\Console\Native\HD-Frontend-Native.dll DataDir
	copy /y $(S)\Opengl\glcheck\HD-GLCheck.exe DataDir
	copy /y $(SUL)GameManager\DotNet\BlueStacks.exe.config DataDir
	copy /y $(SUL)GameManager\DotNet\Assets\Em\Default\* DataDir\Assets\Em\Default
	copy /y $(SUL)GameManager\DotNet\Assets\Toob\Default\* DataDir\Assets\Toob\Default
	copy /y $(SUL)GameManager\DotNet\Assets\Common\* DataDir\Assets\Common
	copy /y $(SUL)GameManager\DotNet\Lato.ttf DataDir\Lato.ttf
	copy /y $(SUL)GameManager\DotNet\Roboto-Medium.ttf DataDir\Roboto-Medium.ttf
	copy /y $(S)GameManager\HTML\GameManager\HTML\installedApps.json DataDir\UserData\Home
	copy /y $(S)GameManager\HTML\GameManager\HTML\*.png DataDir\UserData\Home
	copy /y $(S)Common\Native\ShortcutHandler\HD-ShortcutHandler.dll DataDir
	copy /y $(S)Common\Native\SystemDeviceInfo\HD-SystemDeviceInfo.dll DataDir
	copy /y $(S)Core\Logger\HD-Logger-Native.dll DataDir
	copy /y $(SUL)Package\Resources\BlueStacks.ico DataDir
	copy /y $(SUL)Package\Resources\ProductLogo.png DataDir\UserData\Home\BlueStacks.png
	"$(C)\Tools\HD-zip.exe" -r DataDir.zip DataDir

7zSDcfg.txt: $(SUL)\Sfx\7zSDcfg.txt.in
	-DEL $@
	cscript.exe /nologo $(SUL)\Sfx\Generate7zSDcfg.js MicroInstallerNative.exe \
	    < $(SUL)\Sfx\7zSDcfg.txt.in > 7zSDcfg.txt.tmp
	MOVE 7zSDcfg.txt.tmp 7zSDcfg.txt

$(PROG).exe.config: $(SUL)\Sfx\exe.config
	-DEL $@
	COPY $(SUL)\Sfx\exe.config $@

$(PROG).7z: $(PROG).exe $(PROG).exe.config
	-DEL $@
!IF "$(TYPE)" == "BTV"
	$(C)\7z920_extra\7zr.exe a $@ -m0=LZMA:a=2 $(PROG).exe $(PROG).exe.config $(INSTALLER_FILES) \
	DataDir.zip $(S)\Core\Logger\HD-Logger-Native.dll $(SUL)Package\Resources\ProductLogo.png \
	$(C)\Tools\HD-zip.exe \
	$(S)\Installer\DotNet\Assets\* \
	$(SUL)Locale\Strings\* \
	$(SUL)Locale\ProblemCategories\* \
	$(S)\MicroInstaller\Native\MicroInstallerNative.exe \
	$(SUL)\Package\Resources\$(OEM)\Oem.cfg
!ELSE
	$(C)\7z920_extra\7zr.exe a $@ -m0=LZMA:a=2 $(PROG).exe $(PROG).exe.config $(INSTALLER_FILES) \
	DataDir.zip $(S)\Core\Logger\HD-Logger-Native.dll $(SUL)Package\Resources\ProductLogo.png \
	$(C)\Tools\HD-zip.exe \
	$(S)\Installer\DotNet\Assets\* \
	$(SUL)Locale\Strings\* \
	$(SUL)Locale\ProblemCategories\* \
	$(S)\MicroInstaller\Native\MicroInstallerNative.exe \
	$(SUL)\Package\Resources\$(OEM)\Oem.cfg
!ENDIF

# JSON files
SOURCE_FILES = \
       $(C)\JSON\Core@CodeTitans\IStringReader.cs \
       $(C)\JSON\Core@CodeTitans\NumericHelper.cs \
       $(C)\JSON\Core@CodeTitans\SerializationHelper.cs \
       $(C)\JSON\Core@CodeTitans\StringHelper.cs \
       $(C)\JSON\JSON@CodeTitans\IJSonObject.cs \
       $(C)\JSON\JSON@CodeTitans\IJSonReader.cs \
       $(C)\JSON\JSON@CodeTitans\IJSonSerializable.cs \
       $(C)\JSON\JSON@CodeTitans\IJSonWriter.cs \
       $(C)\JSON\JSON@CodeTitans\IJSonWriterArrayItem.cs \
       $(C)\JSON\JSON@CodeTitans\IJSonWriterItem.cs \
       $(C)\JSON\JSON@CodeTitans\IJSonWriterObjectItem.cs \
       $(C)\JSON\JSON@CodeTitans\JSonException.cs \
       $(C)\JSON\JSON@CodeTitans\JSonIgnoreAttribute.cs \
       $(C)\JSON\JSON@CodeTitans\JSonMemberAttribute.cs \
       $(C)\JSON\JSON@CodeTitans\JSonMemberMissingException.cs \
       $(C)\JSON\JSON@CodeTitans\JSonReader.cs \
       $(C)\JSON\JSON@CodeTitans\JSonReaderException.cs \
       $(C)\JSON\JSON@CodeTitans\JSonReaderTokenInfo.cs \
       $(C)\JSON\JSON@CodeTitans\JSonReaderTokenType.cs \
       $(C)\JSON\JSON@CodeTitans\JSonSerializableAttribute.cs \
       $(C)\JSON\JSON@CodeTitans\JSonWriter.cs \
       $(C)\JSON\JSON@CodeTitans\JSonWriterException.cs \
       $(C)\JSON\JSON@CodeTitans\JSonWriterTokenInfo.cs \
       $(C)\JSON\JSON@CodeTitans\JSonWriterTokenType.cs \
       $(C)\JSON\JSON@CodeTitans\Objects\JSonArray.cs \
       $(C)\JSON\JSON@CodeTitans\Objects\JSonBooleanObject.cs \
       $(C)\JSON\JSON@CodeTitans\Objects\JSonDecimalObject.cs \
       $(C)\JSON\JSON@CodeTitans\Objects\JSonDictionary.cs \
       $(C)\JSON\JSON@CodeTitans\Objects\JSonObjectConverter.cs \
       $(C)\JSON\JSON@CodeTitans\Objects\JSonStringObject.cs \
       $(C)\JSON\JSON@CodeTitans\ReaderHelpers\FclObjectFactory.cs \
       $(C)\JSON\JSON@CodeTitans\ReaderHelpers\IObjectFactory.cs \
       $(C)\JSON\JSON@CodeTitans\ReaderHelpers\JSonObjectFactory.cs \
       $(C)\JSON\JSON@CodeTitans\ReaderHelpers\TokenData.cs \
       $(C)\JSON\JSON@CodeTitans\ReaderHelpers\TokenDataChar.cs \
       $(C)\JSON\JSON@CodeTitans\ReaderHelpers\TokenDataString.cs \
       $(C)\JSON\JSON@CodeTitans\WriterHelpers\ArrayWriter.cs \
       $(C)\JSON\JSON@CodeTitans\WriterHelpers\ObjectWriter.cs \

SOURCE_FILES = $(SOURCE_FILES) \
       $(SUL)\Version.cs \
       $(SUL)\Common\DotNet\Utils.cs \
       $(SUL)\Common\DotNet\Oem.cs \
       $(SUL)\Locale\DotNet\Strings.cs \
       $(SUL)\Common\DotNet\User.cs \
       $(SUL)\Common\DotNet\Logger.cs \
       $(SUL)\Common\DotNet\Strings.cs \
       $(SUL)\Common\DotNet\HTTP.cs \
       $(SUL)\Common\DotNet\Interop_Window.cs \
       $(SUL)\Common\DotNet\Downloader.cs \
       $(SUL)\Common\DotNet\SplitDownloader.cs \
       $(SUL)\Common\DotNet\SplitFile.cs \
       $(SUL)\Common\DotNet\SerialWorkQueue.cs \
       $(SUL)\Common\DotNet\RandomGenerator.cs \
       $(SUL)\Common\DotNet\Features.cs \
       $(SUL)\Device\DotNet\Profile.cs \
       $(SUL)\Cloud\DotNet\Services.cs \
       $(SUL)\Common\DotNet\Secure.cs \
       $(SUL)\Common\DotNet\IniFile.cs \
       $(SUL)\Common\DotNet\ApkStrings.cs \
       $(SUL)\Common\DotNet\GuestNetwork.cs \
       $(SUL)\Common\DotNet\VmCmdHandler.cs \
       $(SUL)\Common\DotNet\UIHelper.cs \
       $(SUL)\Common\DotNet\JsonParser.cs \
       $(SUL)\Common\DotNet\ProgressBar.cs \
       $(SUL)\Common\DotNet\Interop_UUID.cs \
       $(SUL)\Common\DotNet\GraphicsDriverData.cs \
       $(SUL)\Common\DotNet\CustomPopupMessageBox.cs

CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe
CSFLAGS = $(CSFLAGS) /define:ThinInstaller

clean:
!IFDEF ANGLE
	-RD /q /s ANGLE
	del 7zSDcfg.txt $(PROG).exe $(PROG).7z $(PROG).7z.exe $(PROG).exe.config $(PROG).pdb
	del /Q *.7z
!ELSE
	del 7zSDcfg.txt $(PROG).exe $(PROG).7z $(PROG)_$(ANGLE)native.exe $(PROG).7z.exe $(PROG).exe.config $(PROG).pdb
	del /Q *.7z
	del /Q *.exe
	del /Q *.pdb
!ENDIF

!INCLUDE $(M)\win_prod_version.mk
!INCLUDE $(M)\win_csharp.mk

$(PROG)_$(ANGLE)native.exe: $(PROG).7z 7zSDcfg.txt
	-DEL $@
	$(C)\Tools\mt -manifest App.manifest -outputresource:$(SUL)\Sfx\BlueStacks.sfx
!IFDEF ANGLE
	if not exist ANGLE mkdir ANGLE
	COPY /Y /B $(SUL)\Sfx\BlueStacks.sfx + 7zSDcfg.txt + $(PROG).7z ANGLE\$@
	FOR /L %%A IN (1,1,5) DO @(\
		IF DEFINED SIGN_PACKAGE ( IF DEFINED USE_SHA2_SIGNING ( \
			$(SIGNTOOL) sign /s "my" /ac $(S)\Certs\MSCV-VSClass3.cer /sha1 "F1AECC2B5B251589B4B4009D30201AC01ABBA7DC" /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			ANGLE\$@ && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ELSE (\
			IF DEFINED JENKINS_BUILD ( \
			$(SIGNTOOL) sign /sm /s Root /ac $(S)\Certs\MSCV-VSClass3.cer /a /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			ANGLE\$@ && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ELSE (\
			$(SIGNTOOL) sign /s "my" /ac $(S)\Certs\MSCV-VSClass3.cer /a /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			ANGLE\$@ && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ) ) \
	)
!ELSE
	COPY /Y /B $(SUL)\Sfx\BlueStacks.sfx + 7zSDcfg.txt + $(PROG).7z $@
	FOR /L %%A IN (1,1,5) DO @(\
		IF DEFINED SIGN_PACKAGE ( IF DEFINED USE_SHA2_SIGNING ( \
			$(SIGNTOOL) sign /s "my" /ac $(S)\Certs\MSCV-VSClass3.cer /sha1 "F1AECC2B5B251589B4B4009D30201AC01ABBA7DC" /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			$@ && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ELSE (\
			IF DEFINED JENKINS_BUILD ( \
			$(SIGNTOOL) sign /sm /s Root /ac $(S)\Certs\MSCV-VSClass3.cer /a /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			$@ && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ELSE (\
			$(SIGNTOOL) sign /s "my" /ac $(S)\Certs\MSCV-VSClass3.cer /a /n "BlueStack Systems, Inc." \
			/t "http://timestamp.verisign.com/scripts/timestamp.dll" \
			$@ && IF ERRORLEVEL 0 IF NOT ERRORLEVEL 1 EXIT /B 0 ) ) ) \
	)
!ENDIF

