@ECHO OFF

rem IF EXIST DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe GOTO BUILT

"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release DeployVersionSVN\DeployVersionSVN.sln > build.log

:BUILT

DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% >> build.log

"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release MediaPortal.sln >> build.log

DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% /revert >> build.log

EXIT
rem these commands are necessary to get the svn revision, to enable them just remove the EXIT one line above
DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% /GetVersion >> build.log
IF NOT EXIST version.txt EXIT >> build.log
SET /p version=<version.txt >> build.log
DEL version.txt >> build.log
"%ProgramFiles%\NSIS\makensis.exe" /DVER_BUILD=%version% Setup\setup.nsi >> build.log