@ECHO OFF

rem build init
set project=AudioRenderer
call BuildInit.bat %1

rem build
echo.
echo Building MPAudioRenderer...
"%progpath%\Microsoft Visual Studio 9.0\Common7\IDE\devenv.com" "..\audio renderer\AudioRenderer.sln" /ReBuild "%BUILD_TYPE% unicode" >> %log%
