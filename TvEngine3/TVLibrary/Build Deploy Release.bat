@ECHO OFF

rem IF EXIST DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe GOTO BUILT

"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release DeployVersionSVN\DeployVersionSVN.sln

:BUILT

DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD%

"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release TvLibrary.sln
"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release TvPlugin\TvPlugin.sln

DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe /svn=%CD% /revert