@ECHO OFF

set project=MP Audio Renderer
set log=build.log

call BuildInit.bat %1

echo.
echo Building MPAudioRenderer...
"%progpath%\Microsoft Visual Studio 9.0\Common7\IDE\devenv.com" ".\audio renderer\AudioRenderer.sln" /ReBuild "%BUILD_TYPE% unicode" >> %log%
