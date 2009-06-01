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
echo -= TVEngine2 Plugin =-
echo -= build mode: %BUILD_TYPE% =-
echo.

echo.
echo Building DeployVersionSVN...
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild "..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN.sln" > build.log

echo.
echo Writing SVN revision assemblies...
"..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Debug\DeployVersionSVN.exe" /svn=%CD% >> build.log

echo.
echo Building TVEngine2...
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=%BUILD_TYPE%;Platform=x86 TvEngine2.sln >> build.log

echo.
echo Reverting assemblies...
"..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Debug\DeployVersionSVN.exe" /svn=%CD% /revert >> build.log



rem echo.
rem echo Reading the svn revision...
rem echo $WCREV$>template.txt
rem "%ProgramFiles%\TortoiseSVN\bin\SubWCRev.exe" ".." template.txt version.txt >> build.log
rem SET /p version=<version.txt >> build.log
rem DEL template.txt >> build.log
rem DEL version.txt >> build.log

rem echo.
rem echo Building Installer...
rem "%progpath%\NSIS\makensis.exe" /DBUILD_TYPE=%BUILD_TYPE% /DVER_BUILD=%version% Setup\setup.nsi >> build.log
