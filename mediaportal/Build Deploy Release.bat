@ECHO OFF

REM set the path to svn root
set SVN_ROOT=..

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


echo.
echo -= MediaPortal =-
echo -= build mode: %BUILD_TYPE% =-
echo.


echo.
echo Building DeployVersionSVN...
"%SVN_ROOT%\Tools\Script & Batch tools\DeployVersionSVN\Build.bat" > build.log

echo.
echo Writing SVN revision assemblies...
"%SVN_ROOT%\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD% >> build.log
"%SVN_ROOT%\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD%\%SVN_ROOT%\Common-MP-TVE3 >> build.log

echo.
echo Building MediaPortal...
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=%BUILD_TYPE%;Platform=x86 MediaPortal.sln >> build.log

echo.
echo Reverting assemblies...
"%SVN_ROOT%\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD% /revert >> build.log
"%SVN_ROOT%\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD%\%SVN_ROOT%\Common-MP-TVE3 /revert >> build.log


echo.
echo Reading the svn revision...
echo $WCREV$>template.txt
"%ProgramFiles%\TortoiseSVN\bin\SubWCRev.exe" ".." template.txt version.txt >> build.log
SET /p version=<version.txt >> build.log
DEL template.txt >> build.log
DEL version.txt >> build.log

echo.
echo Building Installer...
"%progpath%\NSIS\makensis.exe" /DBUILD_TYPE=%BUILD_TYPE% /DVER_BUILD=%version% Setup\setup.nsi >> build.log
