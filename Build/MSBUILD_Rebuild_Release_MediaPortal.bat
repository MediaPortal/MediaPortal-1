@ECHO OFF

rem build init
set project=MediaPortal
call BuildInit.bat %1

rem build
echo.
echo Writing GIT revision assemblies...
rem %DeployVersionGIT% /git="%MediaPortal%" >> %log%
%DeployVersionGIT% /git="%CommonMPTV%" >> %log%

echo.
echo Building MediaPortal...
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=%BUILD_TYPE%;Platform=x86 "%MediaPortal%\MediaPortal.sln" >> %log%

echo.
echo Reverting assemblies...
rem %DeployVersionGIT% /git="%MediaPortal%" /revert >> %log%
%DeployVersionGIT% /git="%CommonMPTV%" /revert >> %log%

echo.
echo Reading the git revision...
%DeployVersionGIT% /git="%CommonMPTV%" /GetVersion >> %log%
rem SET /p version=<version.txt >> %log%
SET version=%errorlevel%
DEL version.txt >> %log%

echo.
echo Building Installer...
"%progpath%\NSIS\makensis.exe" /DBUILD_TYPE=%BUILD_TYPE% /DVER_BUILD=%version% "%MediaPortal%\Setup\setup.nsi" >> %log%
