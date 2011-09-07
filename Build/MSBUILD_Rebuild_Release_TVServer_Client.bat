@ECHO OFF

rem build init
set project=TVServer_Client
call BuildInit.bat %1

rem build
echo.
echo Writing GIT revision assemblies...
rem %DeployVersionGIT% /git="%TVLibrary%" >> %log%
%DeployVersionGIT% /git="%CommonMPTV%" >> %log%

echo.
echo Building TV Server...
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=%BUILD_TYPE%;Platform=x86 "%TVLibrary%\TvLibrary.sln" >> %log%
echo.
echo Building TV Client plugin...
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=%BUILD_TYPE%;Platform=x86 "%TVLibrary%\TvPlugin\TvPlugin.sln" >> %log%

echo.
echo Reverting assemblies...
rem %DeployVersionGIT% /git="%TVLibrary%" /revert >> %log%
%DeployVersionGIT% /git="%CommonMPTV%" /revert >> %log%

echo.
echo Reading the GIT revision...
%DeployVersionGIT% /git="%CommonMPTV%" /GetVersion >> %log%
rem SET /p version=<version.txt >> %log%
SET version=%errorlevel%
DEL version.txt >> %log%

echo.
echo Building Installer...
"%progpath%\NSIS\makensis.exe" /DBUILD_TYPE=%BUILD_TYPE% /DVER_BUILD=%version% "%TVLibrary%\Setup\setup.nsi" >> %log%
