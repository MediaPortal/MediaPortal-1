@ECHO OFF

set project=dshowhelper - Owlsroost
set log=build.log

call BuildInit.bat %1

echo.
echo Building dshowhelper - Owlsroost...
"%progpath%\Microsoft Visual Studio 9.0\Common7\IDE\devenv.com" ".\dshowhelper\dshowhelper.sln" /Rebuild "%BUILD_TYPE%" >> %log%
