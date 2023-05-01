@ECHO OFF

ECHO.
ECHO Prepare Environment...

SET PROGPATH=%ProgramFiles%
IF NOT "%ProgramFiles(x86)%".=="". SET PROGPATH=%ProgramFiles(x86)%
SET GIT_ROOT=..

IF "%1"=="" (
 SET "OUTF=> BuildFullInstaller.log"
) ELSE ( 
 SET "OUTF="
)

ECHO.
ECHO Building Installer...

"%PROGPATH%\NSIS\makensis.exe" "%GIT_ROOT%\Tools\InstallationScripts\DeployToolUnPacker-x64.nsi" %OUTF%
