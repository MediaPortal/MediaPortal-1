@ECHO OFF

echo.
echo -= MediaPortal : Build Deploy Release.bat =-

echo.
echo Building DeployVersionSVN...
"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release "..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN.sln" > build.log

echo.
echo Writing SVN revision assemblies...
"..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD% >> build.log

echo.
echo Building MediaPortal...
"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" MediaPortal.sln >> build.log

echo.
echo Reverting assemblies...
"..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD% /revert >> build.log

echo.
echo Building Installer...
"..\Tools\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn=%CD% /GetVersion >> build.log
IF NOT EXIST version.txt EXIT >> build.log
SET /p version=<version.txt >> build.log
DEL version.txt >> build.log
"%ProgramFiles%\NSIS\makensis.exe" /DVER_BUILD=%version% Setup\setup.nsi >> build.log