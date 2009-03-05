@echo off

rem That file is a workaround to use the        %errorlevel%   returned by DeployVersionSVN.exe in NSIS.
rem       It is necessary, because NSIS' own compile time command 
rem                 !system '"Debug\DeployVersionSVN.exe" /GetVersion /svn=%cd%'
rem       is not able to write the exit code  (%errorlevel%) to a variable.
rem       It is only available to compare the exit code.

"..\Script & Batch tools\DeployVersionSVN\DeployVersionSVN\bin\Debug\DeployVersionSVN.exe" /GetVersion /svn=%cd%
echo !define SVN_REVISION %errorlevel% > version.txt
