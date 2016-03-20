@ECHO OFF
REM set other MP related paths
set GIT_ROOT=..
set MediaPortal="%GIT_ROOT%\mediaportal"

SET CURRENTPATH=%~dp0
SET CURRENTLETTER=%~d0
cd %~dp0

echo.
echo Make MediaPortal 2GB LARGEADDRESSAWARE...
%CURRENTLETTER%
cd %CURRENTPATH%
call "%VS120COMNTOOLS%vsvars32.bat"
EditBin.exe %MediaPortal%\MediaPortal.Application\bin\%1\MediaPortal.exe /LARGEADDRESSAWARE
