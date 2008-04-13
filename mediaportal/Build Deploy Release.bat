@ECHO OFF

rem IF EXIST DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe GOTO BUILT

"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release DeployVersionSVN\DeployVersionSVN.sln > build.log

:BUILT

DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% >> build.log

"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" MediaPortal.sln >> build.log

DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% /revert >> build.log


rem be sure you have installed nsis and the required plugins to compile the installer.exe
DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% /GetVersion >> build.log
IF NOT EXIST version.txt EXIT >> build.log
SET /p version=<version.txt >> build.log
DEL version.txt >> build.log
"%ProgramFiles%\NSIS\makensis.exe" /DVER_BUILD=%version% Setup\setup.nsi >> build.log