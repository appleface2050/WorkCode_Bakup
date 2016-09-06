all : $(PROG)_native.exe

7zSDcfg.txt: $(SUL)\Sfx\7zSDcfg.txt.in
	-DEL $@
	cscript.exe /nologo $(SUL)\Sfx\Generate7zSDcfg.js MicroDownloader_native.exe \
	    < $(SUL)\Sfx\7zSDcfg.txt.in > 7zSDcfg.txt.tmp
	MOVE 7zSDcfg.txt.tmp 7zSDcfg.txt

$(PROG).exe.config: $(SUL)\Sfx\exe.config
	-DEL $@
	COPY $(SUL)\Sfx\exe.config $@

$(PROG).7z: $(PROG).exe $(PROG).exe.config
	-DEL $@
	$(C)\7z920_extra\7zr.exe a $@ -m0=LZMA:a=2 $(PROG).exe $(PROG).exe.config \
	$(S)\Core\Logger\HD-Logger-Native.dll $(SUL)Package\Resources\BlueStacks.ico \
	$(SUL)\Locale\Strings\* \
	$(SUL)\Locale\ProblemCategories\* \
	$(S)\Tool\BlueStacksDownloader\Native\MicroDownloader_native.exe \

clean:
	del 7zSDcfg.txt $(PROG).exe $(PROG).7z $(PROG)_native.exe $(PROG).7z.exe $(PROG).exe.config $(PROG).pdb
	del /Q *.7z
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
