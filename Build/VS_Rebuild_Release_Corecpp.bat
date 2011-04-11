@ECHO OFF

rem build init
set project=Corecpp
call BuildInit.bat %1

rem build
echo.
echo Building %project%
"%progpath%\Microsoft Visual Studio %vsver%\Common7\IDE\devenv.com" "..\mediaportal\Core.cpp\Core.cpp.sln" /Rebuild "%BUILD_TYPE%" >> %log%
