
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

REM Select Visual Studio version

REM set other MP related paths
set GIT_ROOT=..
set DeployVersionGIT="%GIT_ROOT%\Tools\Script & Batch tools\DeployVersionGIT\DeployVersionGIT\bin\Release\DeployVersionGIT.exe"

set CommonMPTV="%GIT_ROOT%\Common-MP-TVE3"
set DirectShowFilters="%GIT_ROOT%\DirectShowFilters"
set MediaPortal="%GIT_ROOT%\mediaportal"
set TVLibrary="%GIT_ROOT%\TvEngine3\TVLibrary"


REM set log file
set log=%project%_%BUILD_TYPE%.log


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
echo DXSDK_DIR = %DXSDK_DIR% >> %log%
echo. >> %log%

REM copy BuildReport resources
xcopy /I /Y .\BuildReport\_BuildReport_Files .\_BuildReport_Files
