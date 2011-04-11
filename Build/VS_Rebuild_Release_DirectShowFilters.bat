@ECHO OFF

rem build init
set project=DirectShowFilters
call BuildInit.bat %1

rem build
echo.
echo Building %project%
"%progpath%\Microsoft Visual Studio %vsver%\Common7\IDE\devenv.com" "..\DirectShowFilters\Filters.sln" /Rebuild "%BUILD_TYPE%" >> %log%
