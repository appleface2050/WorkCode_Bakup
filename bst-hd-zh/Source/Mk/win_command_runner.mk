
all: $(PROG)_native.exe

$(PROG).exe: $(VM_COMMAND_SETTER).in $(VM_COMMAND_DIR)\GenerateVMCommandSetterCs.js

$(VM_COMMAND_SETTER):
	-DEL $(VM_COMMAND_SETTER) $(VM_COMMAND_SETTER).tmp
	cscript.exe /nologo $(VM_COMMAND_DIR)\GenerateVMCommandSetterCs.js $(VM_COMMAND) \
	    < $(VM_COMMAND_SETTER).in > $(VM_COMMAND_SETTER).tmp
	MOVE $(VM_COMMAND_SETTER).tmp $(VM_COMMAND_SETTER)

7zSDcfg.txt: $(SUL)\Sfx\7zSDcfg.txt.in
	-DEL $@
	cscript.exe /nologo $(SUL)\Sfx\Generate7zSDcfg.js $(PROG).exe \
	    < $(SUL)\Sfx\7zSDcfg.txt.in > 7zSDcfg.txt.tmp
	MOVE 7zSDcfg.txt.tmp 7zSDcfg.txt

$(PROG).exe.config: $(SUL)\Sfx\exe.config
	-DEL $@
	COPY $(SUL)\Sfx\exe.config $@

$(PROG).7z: $(PROG).exe $(PROG).exe.config
	-DEL $@
!IFDEF OLD_USERS
	$(C)\7z920_extra\7zr.exe a $@ -m0=LZMA:a=2 $(PROG).exe $(PROG).exe.config $(S)Core\VMCommand\Native\HD-VMCommand-Native.dll $(S)\Tool\GuestCommandRunner\Dotnet\BstCommandProcessor.apk
!ELSE
	$(C)\7z920_extra\7zr.exe a $@ -m0=LZMA:a=2 $(PROG).exe $(PROG).exe.config $(S)Core\VMCommand\Native\HD-VMCommand-Native.dll
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
       $(SUL)\Common\DotNet\GraphicsDriverData.cs \
       $(SUL)\Common\DotNet\MonitorLocator.cs \
	   $(SUL)\Locale\DotNet\Strings.cs \
	   $(VM_COMMAND_SETTER)

CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe

!IFDEF OLD_USERS
CSFLAGS = $(CSFLAGS) /define:OLD_USERS
!ENDIF

clean:
	del 7zSDcfg.txt $(PROG).exe $(PROG).7z $(PROG)_native.exe $(PROG).7z.exe $(PROG).exe.config $(PROG).pdb
	del $(VM_COMMAND_SETTER) $(VM_COMMAND_SETTER).tmp
	del /Q *.exe
	del /Q *.pdb

!INCLUDE $(M)\win_prod_version.mk
!INCLUDE $(M)\win_csharp.mk

$(PROG)_native.exe: $(PROG).7z 7zSDcfg.txt
	-DEL $@
	$(C)\Tools\mt -manifest App.manifest -outputresource:$(SUL)\Sfx\BlueStacks.sfx
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
