@ECHO OFF
REM check parameters

if /I not %1==debug if /I not %1==rebuild if /I not %1==release if /I not %1==build goto ERROR_IN_PARAMETERS
if /I not %2==debug if /I not %2==rebuild if /I not %2==release if /I not %2==build goto ERROR_IN_PARAMETERS

set BUILD_TYPE=Release
set BUILD_MODE=build

if %1==debug set BUILD_TYPE=Debug
if %2==debug set BUILD_TYPE=Debug

if %1==rebuild set BUILD_MODE=rebuild
if %2==rebuild set BUILD_MODE=rebuild

if not [%3]==[] set PRJ=/project %3

REM build init
set project=DirectShowFilters
call BuildInit.bat %BUILD_TYPE%

REM build
echo.
echo Building %project%

if not [%3]==[] goto BUILD_PRJ
goto BUILD rem full build

:BUILD_PRJ
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /target:%BUILD_MODE% /property:Configuration=%BUILD_TYPE% "..\DirectShowFilters\Filters.sln" %PRJ% >> %log%
goto DONE
 
:BUILD
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /target:%BUILD_MODE% /property:Configuration=%BUILD_TYPE% "..\DirectShowFilters\Filters.sln" >> %log%
goto DONE

:ERROR_IN_PARAMETERS
echo.
echo "Error in given parameters. Valid options [build|rebuild] [release|debug] and optional [project name]. For example to rebuild release mode binaries use 'rebuild release' or to build only TsReader in debug mode 'build debug TsReader'"
echo.

:DONE
