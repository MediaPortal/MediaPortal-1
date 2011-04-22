@ECHO OFF

rem build init
set project=Win7RefreshRateHelper
call BuildInit.bat %1

rem build
echo.
echo Building %project%
"%progpath%\Microsoft Visual Studio %vsver%\Common7\IDE\devenv.com" "..\mediaportal\Core.cpp\Win7RefreshRateHelper\Win7RefreshRateHelper.sln" /Rebuild "%BUILD_TYPE%" >> %log%
