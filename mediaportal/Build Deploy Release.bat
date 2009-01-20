@ECHO OFF


REM detect if BUILD_TYPE should be release or debug
if not %1!==Debug! goto RELEASE
:DEBUG
set BUILD_TYPE=Debug
goto START
:RELEASE
set BUILD_TYPE=Release
goto START


:START

echo.
echo -= MediaPortal =-
echo -= build mode: %BUILD_TYPE% =-
echo.

echo.
echo Building DeployVersionSVN...
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild "..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN.sln" > build.log

echo.
echo Writing SVN revision assemblies...
"..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD% >> build.log

echo.
echo Building MediaPortal...
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=%BUILD_TYPE%;Platform=x86 MediaPortal.sln >> build.log


echo.
echo Reverting assemblies...
"..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD% /revert >> build.log

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