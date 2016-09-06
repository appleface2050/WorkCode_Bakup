FULL_MSI = $(S)Package\Build\BlueStacks_HD_AppPlayerKK_GameManager_setup_$(VERSION_STRING)_REL.msi
GAMEMANAGER_INSTALLER = $(S)MicroInstaller\DotNet\GameManagerInstaller_native.exe
all: $(PROG)_$(ANGLE)native.exe

7zSDcfg.txt: $(SUL)\Sfx\7zSDcfg.txt.in
	-DEL $@
	cscript.exe /nologo $(SUL)\Sfx\Generate7zSDcfg.js HD-FullInstaller.exe \
	    < $(SUL)\Sfx\7zSDcfg.txt.in > 7zSDcfg.txt.tmp
	MOVE 7zSDcfg.txt.tmp 7zSDcfg.txt

$(PROG).exe.config: $(SUL)\Sfx\exe.config
	-DEL $@
	COPY $(SUL)\Sfx\exe.config $@

$(PROG).7z: $(PROG).exe $(PROG).exe.config
	-DEL $@
	$(C)\7z920_extra\7zr.exe a $@ -m0=LZMA:a=2 $(S)\Tool\FullInstaller\HD-FullInstaller.exe $(S)\Tool\FullInstaller\HD-FullInstaller.exe.config $(FULL_MSI) $(GAMEMANAGER_INSTALLER)

CSFLAGS = $(CSFLAGS) /debug:full
CSFLAGS = $(CSFLAGS) /platform:x86
CSFLAGS = $(CSFLAGS) /target:winexe
CSFLAGS = $(CSFLAGS) /define:FullInstaller

clean:
!IFDEF ANGLE
	-RD /q /s ANGLE
	del 7zSDcfg.txt $(PROG).exe $(PROG).7z $(PROG).7z.exe $(PROG).exe.config $(PROG).pdb
!ELSE
	del 7zSDcfg.txt $(PROG).exe $(PROG).7z $(PROG)_$(ANGLE)native.exe $(PROG).7z.exe $(PROG).exe.config $(PROG).pdb
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
!IFDEF SIGN_PACKAGE
	$(SIGNTOOL) sign /s "my" /ac $(SUL)\Certs\MSCV-VSClass3.cer /n "BlueStack Systems, Inc." \
	/t "http://timestamp.comodoca.com/authenticode" ANGLE\$@
!ENDIF
!ELSE
	COPY /Y /B $(SUL)\Sfx\BlueStacks.sfx + 7zSDcfg.txt + $(PROG).7z $@
!IFDEF SIGN_PACKAGE
	$(SIGNTOOL) sign /s "my" /ac $(S)\Certs\MSCV-VSClass3.cer /n "BlueStack Systems, Inc." \
	/t "http://timestamp.comodoca.com/authenticode" $@
!ENDIF
!ENDIF
