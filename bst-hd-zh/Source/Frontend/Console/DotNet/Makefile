DLL = HD-Frontend-UserControl
SRCS = \
    $(SUL)\Version.cs \
    $(SUL)\Common\DotNet\Strings.cs \
    $(SUL)\Common\DotNet\WindowMessages.cs \
    $(SUL)\Locale\DotNet\Strings.cs \
    $(C)\MTScratchpad_HOL\Finished\WMTouchForm.cs \
    $(SUL)\Cloud\DotNet\Services.cs \
    $(SUL)\Common\DotNet\Logger.cs \
    $(SUL)\Common\DotNet\MonitorLocator.cs \
    $(SUL)\Common\DotNet\User.cs \
    $(SUL)\Common\DotNet\Agent.cs \
    $(SUL)\Common\DotNet\HTTPHandlerSetup.cs \
    $(SUL)\Common\DotNet\GuestNetwork.cs \
    $(SUL)\Common\DotNet\Features.cs \
    $(SUL)\Common\DotNet\ApkStrings.cs \
    $(SUL)\Common\DotNet\Downloader.cs \
    $(SUL)\Common\DotNet\SplitDownloader.cs \
    $(SUL)\Common\DotNet\SplitFile.cs \
    $(SUL)\Common\DotNet\SerialWorkQueue.cs \
    $(SUL)\Common\DotNet\Interop_Window.cs \
    $(SUL)\Common\DotNet\Secure.cs \
    $(SUL)\Common\DotNet\HTTP.cs \
    $(SUL)\Common\DotNet\Oem.cs \
    $(SUL)\Common\DotNet\FrontendBanner.cs \
    $(SUL)\Common\DotNet\Utils.cs \
    $(SUL)\Common\DotNet\MessageBox.cs \
    $(SUL)\Common\DotNet\VmCmdHandler.cs \
    $(SUL)\Common\DotNet\JsonParser.cs \
    $(SUL)\Common\DotNet\Stats.cs \
    $(SUL)\Common\DotNet\RandomGenerator.cs \
    $(SUL)\Common\DotNet\HTTPUtils.cs \
    $(SUL)\Common\DotNet\GMAppsManager.cs \
    $(SUL)\Common\DotNet\Interop_SparseFile.cs \
    $(SUL)\Common\DotNet\IniFile.cs \
    $(SUL)\Common\DotNet\Toast.cs \
    $(SUL)\Common\DotNet\UIHelper.cs \
    $(SUL)\Common\DotNet\GraphicsDriverData.cs \
    $(SUL)\Common\DotNet\KeyboardSend.cs \
    $(SUL)\Common\DotNet\LoadingScreen.cs \
    $(SUL)\Common\DotNet\GetOpt.cs \
    $(SUL)\Common\DotNet\InstallerCodes.cs \
    $(SUL)\Core\VMCommand\DotNet\Command.cs \
    $(SUL)\Device\DotNet\Profile.cs \
    $(SUL)\Frontend\Audio\DotNet\Audio.cs \
    $(SUL)\Frontend\Gps\DotNet\Gps_HD.cs \
    $(SUL)\Frontend\Camera\DotNet\VideoCapture.cs \
    $(SUL)\Frontend\Camera\DotNet\Camera.cs \
    $(SUL)\Frontend\Camera\DotNet\Guids.cs \
    $(SUL)\Frontend\Camera\DotNet\Errors.cs \
    Interop_Animate.cs \
    Interop_Common.cs \
    Interop_Monitor.cs \
    Interop_Manager.cs \
    Monitor.cs \
    Console.cs \
    Interop_Video.cs \
    Interop_DWM.cs \
    Interop_Input.cs \
    Interop_MouseHWheel.cs \
    StateMachine.cs \
    Modules.cs \
    FrontendHandler.cs \
    Interop_Opengl.cs \
    Keyboard.cs \
    Mouse.cs \
    FrontendButton.cs \
    StatusMessage.cs \
    Cursor.cs \
    IControlHandler.cs \
    HTTPHandler.cs \
    OpenSensor.cs \
    InputMapper.cs \
    InputMapperForm.cs \
    GamePad.cs \
    FullScreenToast.cs \
    FileImporter.cs \
    VmxChecker.cs \
    SensorDevice.cs \
    InputMapperTool.cs \
    GraphicsDriverUpdater.cs \
    AssemblyInfo.cs \
    Id.cs

CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /unsafe
CSFLAGS = $(CSFLAGS) /target:library
CSFLAGS = $(CSFLAGS) /reference:$(C)\NAudio\NAudio.dll /reference:Microsoft.VisualBasic.dll
CSFLAGS = $(CSFLAGS) /reference:$(C)\JSON\JSON.dll /reference:$(C)\DevComponents\bin\DevComponents.DotNetBar2.dll
CSFLAGS = $(CSFLAGS) /reference:WindowsBase.dll
CSFLAGS = $(CSFLAGS) /reference:PresentationCore.dll
!IF "$(DOTNETVER)" == "35"
CSFLAGS = $(CSFLAGS) /reference:$(C)\Bugsnag\Bugsnag.dll
!ENDIF
FXCOPFLAGS = $(FXCOPFLAGS) /reference:$(C)\NAudio\NAudio.dll
FXCOPFLAGS = $(FXCOPFLAGS) /reference:$(C)\JSON\Json.dll

!INCLUDE $(M)\win_csharp_dll.mk
