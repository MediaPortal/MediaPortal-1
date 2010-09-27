@ECHO OFF

rem build init
set project=dshowhelper
call BuildInit.bat %1

rem build
echo.
echo Building dshowhelper - Owlsroost...
"%progpath%\Microsoft Visual Studio 9.0\Common7\IDE\devenv.com" "..\dshowhelper\dshowhelper.sln" /Rebuild "%BUILD_TYPE%" >> %log%
