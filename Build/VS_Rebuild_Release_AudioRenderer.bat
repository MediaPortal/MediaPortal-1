@ECHO OFF

rem build init
set project=AudioRenderer
call BuildInit.bat %1

rem build
echo.
echo Building MPAudioRenderer...
"%progpath%\Microsoft Visual Studio %vsver%\Common7\IDE\devenv.com" "..\DirectShowFilters\MPAudioRenderer\AudioRenderer.sln" /ReBuild "%BUILD_TYPE% unicode" >> %log%
