@ECHO OFF

rem build init
set project=DirectShowFilters
call BuildInit.bat %1

rem build
echo.
echo Building %project%
"%progpath%\Microsoft Visual Studio 9.0\Common7\IDE\devenv.com" "..\DirectshowFilters\Filters.sln" /Rebuild "%BUILD_TYPE%" >> %log%
