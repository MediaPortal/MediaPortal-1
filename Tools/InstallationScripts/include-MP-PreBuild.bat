@echo off

rem That file is a workaround to use the        %errorlevel%   returned by DeployVersionGIT.exe in NSIS.
rem       It is necessary, because NSIS' own compile time command 
rem                 !system '"Debug\DeployVersionGIT.exe" /GetVersion /path=%cd%'
rem       is not able to write the exit code  (%errorlevel%) to a variable.
rem       It is only available to compare the exit code.

"..\Script & Batch tools\DeployVersionGIT\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /GetVersion=version.template.txt /path=%cd%
rem echo !define VER_BUILD %errorlevel% > version.txt
