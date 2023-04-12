@ECHO OFF

ECHO.
ECHO Building Installer... %2

SET progpath=%ProgramFiles%
IF NOT "%ProgramFiles(x86)%".=="". SET progpath=%ProgramFiles(x86)%

SET GIT_ROOT=..

IF "%1"=="" (
  SET "OUTF=>> BuildFullInstaller.log"
) ELSE ( 
  IF "%1"=="LOG" ( 
    SET "OUTF=>> BuildFullInstaller.log"
  ) ELSE (
    SET "OUTF="
  )
)

IF NOT "%2"=="" (
  SET ARCH=/DArchitecture=%2
) ELSE (
  SET ARCH=
)

"%progpath%\NSIS\makensis.exe" %ARCH% "%GIT_ROOT%\Tools\InstallationScripts\DeployToolUnPacker-x64.nsi" %OUTF%
