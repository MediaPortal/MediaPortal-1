@ECHO OFF

rem build init
set project=MediaPortal
call BuildInit.bat %1

rem build
echo.
echo Writing SVN revision assemblies...
rem %DeployVersionSVN% /svn="%MediaPortal%" >> %log%
%DeployVersionSVN% /svn="%CommonMPTV%" >> %log%

echo.
echo Building MediaPortal...
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=%BUILD_TYPE%;Platform=x86 "%MediaPortal%\MediaPortal.sln" >> %log%

echo.
echo Reverting assemblies...
rem %DeployVersionSVN% /svn="%MediaPortal%" /revert >> %log%
%DeployVersionSVN% /svn="%CommonMPTV%" /revert >> %log%

echo.
echo Reading the svn revision...
%DeployVersionSVN% /svn="%CommonMPTV%" /GetVersion >> %log%
rem SET /p version=<version.txt >> %log%
SET version=%errorlevel%
DEL version.txt >> %log%

echo.
echo Building Installer...
"%progpath%\NSIS\makensis.exe" /DBUILD_TYPE=%BUILD_TYPE% /DVER_BUILD=%version% "%MediaPortal%\Setup\setup.nsi" >> %log%
