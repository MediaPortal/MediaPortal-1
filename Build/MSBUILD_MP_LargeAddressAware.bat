@ECHO OFF
REM set other MP related paths
set GIT_ROOT=..
set MediaPortal="%GIT_ROOT%\mediaportal"

if not defined progpath set progpath=%ProgramFiles%
if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%

if not defined VsDevCmd_path set VsDevCmd_path=%progpath%\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat
if not exist "%VsDevCmd_path%" set VsDevCmd_path=%progpath%\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat
if not exist "%VsDevCmd_path%" set VsDevCmd_path=%progpath%\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\VsDevCmd.bat
if not exist "%VsDevCmd_path%" set VsDevCmd_path=%progpath%\Microsoft Visual Studio\2019\BuildTools\Common7\Tools\VsDevCmd.bat

SET CURRENTPATH=%~dp0
SET CURRENTLETTER=%~d0
cd %~dp0

echo.
echo Make MediaPortal 2GB LARGEADDRESSAWARE...
%CURRENTLETTER%
cd %CURRENTPATH%
REM call "%VS140COMNTOOLS%vsvars32.bat"
call "%VsDevCmd_path%"
EditBin.exe %MediaPortal%\MediaPortal.Application\bin\%1\MediaPortal.exe /LARGEADDRESSAWARE
