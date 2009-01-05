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
REM Select program path based on current machine environment
set progpath=%ProgramFiles%
if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%


echo.
echo -= TV Server / Client plugin =-
echo -= build mode: %BUILD_TYPE% =-
echo.

echo.
echo Building DeployVersionSVN...
"%progpath%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release "..\..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN.sln" > build.log

echo.
echo Writing SVN revision assemblies...
"..\..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD% >> build.log

echo.
echo Building TV Server...
"%progpath%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "%BUILD_TYPE%|x86" TvLibrary.sln >> build.log
echo.
echo Building TV Client plugin...
"%progpath%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "%BUILD_TYPE%|x86" TvPlugin\TvPlugin.sln >> build.log

echo.
echo Reverting assemblies...
"..\..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD% /revert >> build.log

echo.
echo Reading the svn revision...
echo $WCREV$>template.txt
"%ProgramFiles%\TortoiseSVN\bin\SubWCRev.exe" "..\.." template.txt version.txt >> build.log
SET /p version=<version.txt >> build.log
DEL template.txt >> build.log
DEL version.txt >> build.log

echo.
echo Building Installer...
"%progpath%\NSIS\makensis.exe" /DBUILD_TYPE=%BUILD_TYPE% /DVER_BUILD=%version% Setup\setup.nsi >> build.log