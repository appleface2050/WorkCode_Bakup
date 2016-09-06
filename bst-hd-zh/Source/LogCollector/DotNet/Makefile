!IFDEF SILENT
PROG = HD-SilentLogCollector
!ELSE
PROG = HD-logCollector
!ENDIF
SRCS = \
    $(SUL)\Version.cs \
    $(SUL)\Locale\Dotnet\Strings.cs \
    $(SUL)\Cloud\DotNet\Services.cs \
    $(SUL)\Common\DotNet\HTTP.cs \
    $(SUL)\Common\DotNet\Interop_Window.cs \
    $(SUL)\Common\DotNet\Utils.cs \
    $(SUL)\Common\DotNet\Oem.cs \
    $(SUL)\Common\DotNet\User.cs \
    $(SUL)\Common\DotNet\IniFile.cs \
    $(SUL)\Common\DotNet\Secure.cs \
    $(SUL)\Common\DotNet\Strings.cs \
    $(SUL)\Common\DotNet\GuestNetwork.cs \
    $(SUL)\Common\DotNet\VmCmdHandler.cs \
    $(SUL)\Common\DotNet\UIHelper.cs \
    $(SUL)\Common\DotNet\Features.cs \
    $(SUL)\Common\DotNet\GraphicsDriverData.cs \
    $(SUL)\Device\DotNet\Profile.cs \
    LogCollector.cs \
    AssemblyInfo.cs

!IFDEF SILENT
SRCS = $(SRCS) \
       $(SUL)\Common\DotNet\LoggerManaged.cs \
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
       $(C)\JSON\JSON@CodeTitans\WriterHelpers\ObjectWriter.cs
!ELSE
SRCS = $(SRCS) \
    $(SUL)\Common\DotNet\Logger.cs
CSFLAGS = $(CSFLAGS) /reference:$(C)\JSON\JSON.dll
FXCOPFLAGS = $(FXCOPFLAGS) /reference:$(C)\JSON\JSON.dll
!ENDIF

CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe
CSFLAGS = $(CSFLAGS) /main:BlueStacks.hyperDroid.LogCollector.LogCollector
!IFDEF SILENT
CSFLAGS = $(CSFLAGS) /define:SILENT
!ENDIF

!INCLUDE $(M)\win_csharp.mk
