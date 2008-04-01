@ECHO OFF

rem IF EXIST DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe GOTO BUILT

"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release DeployVersionSVN\DeployVersionSVN.sln

:BUILT

DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD%

"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release MediaPortal.sln

DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% /revert

rem these commands are necessary to get the svn revision, to enable them just remove the EXIT one line above
DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% /GetVersion
IF NOT EXIST version.txt EXIT
SET /p version=<version.txt
DEL version.txt
"%ProgramFiles%\NSIS\makensis.exe" /DVER_BUILD=%version% Setup\setup.nsi