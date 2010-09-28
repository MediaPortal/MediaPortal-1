
REM detect if BUILD_TYPE should be release or debug
if not %1!==Debug! goto RELEASE
:DEBUG
set BUILD_TYPE=Debug
goto START
:RELEASE
set BUILD_TYPE=Release
goto START


:START
REM Select program path based on current machine environment
set progpath=%ProgramFiles%
if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%


REM set other MP related paths
set SVN_ROOT=..
set DeployVersionSVN="%SVN_ROOT%\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe"

set CommonMPTV="%SVN_ROOT%\Common-MP-TVE3"
set DirectShowFilters="%SVN_ROOT%\DirectShowFilters"
set MediaPortal="%SVN_ROOT%\mediaportal"
set TVLibrary="%SVN_ROOT%\TvEngine3\TVLibrary"


REM set log file
set log=%project%.log


REM init log file, write dev env...
echo.
echo. > %log%
echo -= %project% =-
echo -= %project% =- >> %log%
echo -= build mode: %BUILD_TYPE% =-
echo -= build mode: %BUILD_TYPE% =- >> %log%
echo.
echo. >> %log%

echo. >> %log%
echo Using following environment variables: >> %log%
echo DSHOW_BASE = %DSHOW_BASE% >> %log%
echo DXSDK_DIR = %DXSDK_DIR% >> %log%
echo WINDOWS_SDK = %WINDOWS_SDK% >> %log%
echo. >> %log%