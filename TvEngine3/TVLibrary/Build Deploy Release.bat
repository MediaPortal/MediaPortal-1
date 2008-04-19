@ECHO OFF

echo.
echo -= TV Server / Client plugin : Build Deploy Release.bat =-

echo.
echo Building DeployVersionSVN...
"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release DeployVersionSVN\DeployVersionSVN.sln > build.log

echo.
echo Writing SVN revision assemblies...
DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% >> build.log

echo.
echo Building TV Server...
"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" TvLibrary.sln >> build.log
echo.
echo Building TV Client plugin...
"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" TvPlugin\TvPlugin.sln >> build.log

echo.
echo Reverting assemblies...
DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% /revert >> build.log

echo.
echo Building Installer...
DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% /GetVersion >> build.log
IF NOT EXIST version.txt EXIT >> build.log
SET /p version=<version.txt >> build.log
DEL version.txt >> build.log
"%ProgramFiles%\NSIS\makensis.exe" /DVER_BUILD=%version% Setup\setup.nsi >> build.log