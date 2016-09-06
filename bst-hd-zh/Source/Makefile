VERSION_STRING = \
    $(VERSION_MAJOR).$(VERSION_MINOR).$(VERSION_PATCH).$(VERSION_BUILD)

PARTIAL_UPDATE_ROOT = PartialUpdate

OEM = $(OEM)

#
# These are prerequisites for other pieces of the product and must be built
# first.
#

SUBDIR = \
    $(SUBDIR) \
    $(C)\JSON \
    $(S)\Locale\Resources

#
# Build our core components, which provide the hypervisor and support for
# required virtual devices.
#

SUBDIR = \
    $(SUBDIR) \
    Core\Shared\Win32 \
    Core\Private\Win32 \
    Core\Network\Win32 \
    Core\BlockDevice\Win32 \
    Core\BlockDeviceTool\Win32 \
    Core\SharedFolder\Win32 \
    Core\Service\Dotnet \
    Core\Service\Native \
    Core\VTxCheck\Native \
    Core\Hypervisor\x86 \
    Core\Hypervisor\amd64 \
    Core\VMTool\Native \
    Core\VMTool\DotNet \
    Core\VMCommand\Native \
    Core\VMCommand\DotNet \
    Core\Logger \
    Core\LogRotator\Native \
    Core\LogRotator\DotNet \
    Core\LogRotatorService\DotNet

#
# Next, build the frontend prerequisites and the frontend itself.
#

SUBDIR = \
    $(SUBDIR) \
    Opengl\Host \
    Frontend\InputMapper\InputMapper\Native \
    Frontend\InputMapper\OpenSensor\Native \
    Frontend\Audio\Native \
    Frontend\Gps\Native \
    Frontend\Camera\Native \
    Frontend\Sensor\Native \
    Frontend\Console\Native \
    Frontend\Console\DotNet \
    Frontend\WebPlugin

#
# Build the agent and other assorted components that make up the product.
#

SUBDIR = \
    $(SUBDIR) \
    Opengl\glcheck \
    Tool\Package\SetupPatcher\DotNet \
    Agent\DotNet \
    Restart\DotNet \
    ApkInstaller\DotNet \
#    AppLauncher-Win8\DotNet \
    LogCollector\DotNet \
    StartLauncher\DotNet \
    Tool\RunApp\DotNet \
    Tool\Win8TileCreator\DotNet \
#    Tool\InstallerInfoUploader\DotNet \
    Tool\QuitBlueStacks\DotNet \
    Tool\QuitMultiInstance\DotNet \
    Tool\RuntimeUninstaller\DotNet \
    Tool\CreateSymlink\DotNet \
    Tool\SplitFile\DotNet \
    Common\Native\ShortcutHandler \
    Common\Native\SystemDeviceInfo \
    Common\Native\LibraryHandler \
    Frontend\InputMapper\InputMapper\Profiles \
    Tool\Signer\DotNet

# userland test utilities
SUBDIR = \
    $(SUBDIR) \
#    Test\Input\DotNet \
#    Test\Buttons\DotNet \
#    Test\Pipe\Server\DotNet \
#    Test\Pipe\Client\DotNet \
#    Test\Pipe\Client2\DotNet \
#    Test\CloudConnect\DotNet \
#    Test\Json\DotNet \
#    Test\Cloud\DotNet \
#    Test\GlEventTest\DotNet \
#    Test\HTTP\Client\DotNet \
#    Test\HTTP\Server\DotNet \
#    Test\Power\DotNet \
#    Tool\MSITweak\DotNet \
#    Tool\ResizeAppIcon\DotNet \
#    Test\AsyncIO\DotNet \
#    Test\RangeDownload\DotNet \
#    Test\WebPlugin\Log \
#    Test\WebPlugin\Wnd \
#    Test\WebPlugin\Thread


# Updater Service
SUBDIR = \
    $(SUBDIR) \
    UpdaterService\DotNet \

# Gps Locator
SUBDIR = \
    $(SUBDIR) \
    GpsLocator\Native \

# Guest Command Runner
SUBDIR = \
    $(SUBDIR) \
    Tool\GuestCommandRunner\Dotnet \

# Micro installer native
MICRO_SUBDIR = \
    $(SUBDIR) \
    MicroInstaller\Native \

# Thin installer stuff. Make sure to keep this in the end
THIN_SUBDIR = \
    ThinInstaller\Resources \
    ThinInstaller\DotNet

# Thin installer for third party apps
APK_TO_EXE_SUBDIR = \
    ApkThinInstaller\3rdPartyApps

all: Version.cs
	@FOR %I IN ($(SUBDIR)) DO @( \
	    ECHO ==^> %I & \
	    CMD /C "CD %I & $(MAKE) /$(MAKEFLAGS) all" || EXIT /B 1 \
	)

apktoexe:
	@FOR %I IN ($(APK_TO_EXE_SUBDIR)) DO @( \
	    ECHO ==^> %I & \
	    CMD /C "CD %I & $(MAKE) /$(MAKEFLAGS) apktoexe" || EXIT /B 1 \
	)

fxcop: force 
	@FOR %I IN ($(SUBDIR)) DO @( \
	    ECHO ==^> %I & \
	    CMD /C "CD %I & $(MAKE) /$(MAKEFLAGS) fxcop" || EXIT /B 1 \
	)

clean-deploy: force
	CMD /C "CD $(SUL)Tool\DeployTool\Dotnet & $(MAKE) /$(MAKEFLAGS) clean" || EXIT /B 1 \
	CMD /C "CD $(SUL)Tool\DeployTool\Uninstaller\Dotnet & $(MAKE) /$(MAKEFLAGS) clean" || EXIT /B 1 \
	CMD /C "CD $(SUL) $(MAKE) /$(MAKEFLAGS) clean" || EXIT /B 1

clean: force
	-DEL Version.cs Version.cs.tmp
	@FOR %I IN ($(SUBDIR)) DO @( \
	    ECHO ==^> %I & \
	    CMD /C "CD %I & $(MAKE) /$(MAKEFLAGS) clean" || EXIT /B 1 \
	)
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) clean" || EXIT /B 1

delayedlogcollector:
	@ECHO ==^> $(S)Tool\SilentLogCollector\Dotnet & \
	CMD /C "CD $(S)Tool\SilentLogCollector\Dotnet & $(MAKE) /$(MAKEFLAGS) clean all" || EXIT /B 1
	@ECHO ==^> $(S)Tool\DelayedLogCollector\Dotnet & \
	CMD /C "CD $(S)Tool\DelayedLogCollector\Dotnet & $(MAKE) /$(MAKEFLAGS) clean all" || EXIT /B 1

silentlogcollector:
	@ECHO ==^> $(S)LogCollector\Dotnet & \
	CMD /C "CD $(S)LogCollector\Dotnet & $(MAKE) /$(MAKEFLAGS) clean all SILENT=TRUE" || EXIT /B 1

package: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) package" || EXIT /B 1 

symbols: force
	@ECHO ==^> Package &\
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) symbolszip" || EXIT /B 1

appplayerkk: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) appplayerkk" || EXIT /B 1

appplayerkkangle: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) appplayerkkangle" || EXIT /B 1

appplayer4399: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) appplayer4399" || EXIT /B 1

appplayer4399angle: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) appplayer4399angle" || EXIT /B 1

deploytoolapi: clean-deploy force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) deploytool FLAVOR=china_api API=1" || EXIT /B 1

deploytoolenterprise: clean-deploy force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) deploytool FLAVOR=enterprise ENTERPRISE=1" || EXIT /B 1

deploytoolanqu: clean-deploy force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) deploytool FLAVOR=anquicafe ANQU=1" || EXIT /B 1

deploytoolucweb: clean-deploy force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) deploytool FLAVOR=ucweb_dt UCWEB=1" || EXIT /B 1

deploytoolyy: clean-deploy force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) deploytool FLAVOR=yy_dt YY=1" || EXIT /B 1

deploytooltc: clean-deploy force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) deploytool FLAVOR=tc_dt TC=1" || EXIT /B 1

deploytoolnt: clean-deploy force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) deploytool FLAVOR=nt_dt NT=1" || EXIT /B 1

deploytool4399: clean-deploy force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) deploytool FLAVOR=four399_dt FOUR399=1" || EXIT /B 1

deploytoolsf: clean-deploy force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) deploytool FLAVOR=sf_dt SF=1" || EXIT /B 1

appplayerkkdefaulthome: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) appplayerkkdefaulthome" || EXIT /B 1

appplayerkkcts: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) appplayerkkcts" || EXIT /B 1

msft: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) msft" || EXIT /B 1

appplayersplitkk: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) appplayersplitkk" || EXIT /B 1 

appplayersplitkkangle: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) appplayersplitkkangle" || EXIT /B 1 

runtime_zip: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) runtime_zip" || EXIT /B 1 

thininstallerkk: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) thininstallerkk" || EXIT /B 1 

thininstallerkkangle: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) thininstallerkkangle" || EXIT /B 1 

thininstallerfullmsi: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) thininstallerfullmsi" || EXIT /B 1 

thininstallergm: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) thininstallergm" || EXIT /B 1 

thininstallergmyahoo: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) thininstallergmyahoo" || EXIT /B 1 

thininstaller360: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) thininstaller360" || EXIT /B 1 

thininstalleramd: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) thininstalleramd" || EXIT /B 1 

thininstalleramdangle: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) thininstalleramdangle" || EXIT /B 1 

fullinstallergm: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) fullinstallergm" || EXIT /B 1

win8: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) win8" || EXIT /B 1 

amdkk: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) amdkk" || EXIT /B 1

amdkkangle: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) amdkkangle" || EXIT /B 1

anqu: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) anqu" || EXIT /B 1

chinakk: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) chinakk" || EXIT /B 1

chinakkangle: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) chinakkangle" || EXIT /B 1

chinaapikk: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) chinaapikk" || EXIT /B 1

chinaapikkangle: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) chinaapikkangle" || EXIT /B 1

ucweb: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) ucweb" || EXIT /B 1

anquicafe: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) anquicafe" || EXIT /B 1

360kk: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) 360kk" || EXIT /B 1

bigfish: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) bigfish" || EXIT /B 1

wildtangent: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) wildtangent" || EXIT /B 1

gamemanager: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) gamemanager" || EXIT /B 1

package-dirs:
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) package-dirs" || EXIT /B 1 

vmcommandrunner:
	@ECHO ==^> Tool\GuestCommandRunner\Dotnet & \
	CMD /C "CD Tool\GuestCommandRunner\Dotnet & $(MAKE) /$(MAKEFLAGS) clean all OLD_USERS=1" || EXIT /B 1

package-clean: force
	@ECHO ==^> Package & \
	CMD /C "CD Package & $(MAKE) /$(MAKEFLAGS) package-clean" || EXIT /B 1 

Version.cs: Version.cs.in GenerateVersionCs.js
	-DEL Version.cs Version.cs.tmp
!IFDEF OEM_NAME
	cscript.exe /nologo GenerateVersionCs.js $(VERSION_STRING) $(OEM_NAME) \
!ELSE 
	cscript.exe /nologo GenerateVersionCs.js $(VERSION_STRING) $(OEM) \
!ENDIF
	    < Version.cs.in > Version.cs.tmp
	MOVE Version.cs.tmp Version.cs

force:
