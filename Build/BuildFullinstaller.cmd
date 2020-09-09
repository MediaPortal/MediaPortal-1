echo.
echo Building Installer...
set progpath=%ProgramFiles%
if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%
set GIT_ROOT=..

if "%1"=="" (
set "OUTF=>> BuildFullInstaller.log"
) else ( 
set "OUTF="
)

"%progpath%\NSIS\makensis.exe" "%GIT_ROOT%\Tools\InstallationScripts\DeployToolUnPacker-x64.nsi" %OUTF%