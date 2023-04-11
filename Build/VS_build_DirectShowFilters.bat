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

if [%3]==[] (set ARCH=x86) ELSE (set ARCH=%3)

if [%ARCH%]==[x86] (set FARCH=Win32) ELSE (set FARCH=x64)

if not [%4]==[] set PRJ=/project %4

REM build init
set project=DirectShowFilters
call BuildInit.bat %BUILD_TYPE%

REM build
echo.
echo Building %project%


if not [%4]==[] goto BUILD_PRJ
goto BUILD rem full build


:BUILD_PRJ
set xml=Build_Report_%BUILD_TYPE%_Filters_%PRJ%.xml
set html=Build_Report_%BUILD_TYPE%_Filters_%PRJ%.html
set logger=/l:XmlFileLogger,"BuildReport\MSBuild.ExtensionPack.Loggers.dll";logfile=%xml%

"%MSBUILD_PATH%" %logger% /m /target:%BUILD_MODE% /property:Configuration=%BUILD_TYPE%;Platform=%FARCH% "..\DirectShowFilters\Filters.sln" %PRJ% >> %log%
BuildReport\msxsl %xml% _BuildReport_Files\BuildReport.xslt -o %html%

goto DONE
 
:BUILD
set xml=Build_Report_%BUILD_TYPE%_Filters.xml
set html=Build_Report_%BUILD_TYPE%_Filters.html
set logger=/l:XmlFileLogger,"BuildReport\MSBuild.ExtensionPack.Loggers.dll";logfile=%xml%

"%MSBUILD_PATH%" %logger% /m /target:%BUILD_MODE% /property:Configuration=%BUILD_TYPE%;Platform=%FARCH% "..\DirectShowFilters\Filters.sln" >> %log%
BuildReport\msxsl %xml% _BuildReport_Files\BuildReport.xslt -o %html%

REM BUILD LIBBLURAY PROJECT
IF EXIST "..\libbluray\libbluray.vcxproj" (
set xml=Build_Report_%BUILD_TYPE%_libbluray.xml
set html=Build_Report_%BUILD_TYPE%_libbluray.html
set logger=/l:XmlFileLogger,"BuildReport\MSBuild.ExtensionPack.Loggers.dll";logfile=%xml%

"%MSBUILD_PATH%" %logger% /m /target:%BUILD_MODE% /property:Configuration=%BUILD_TYPE%_libbluray;Platform=%FARCH% "..\DirectShowFilters\Filters.sln" >> %log%
BuildReport\msxsl %xml% _BuildReport_Files\BuildReport.xslt -o %html%
)

goto DONE

:ERROR_IN_PARAMETERS
echo.
echo "Error in given parameters. Valid options [build|rebuild] [release|debug] [x86|x64] and optional [project name]. For example to rebuild release mode binaries use 'rebuild release' or to build only TsReader in debug mode 'build debug TsReader'"
echo.

:DONE
